using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Backend.Meta.StoreCompliance;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AbyssChronicle.Editor.Build
{
    /// <summary>
    /// 환경변수·CLI 인자에서 빌드 설정을 읽는다.
    /// </summary>
    public sealed class BuildConfiguration
    {
        public const string EnvBuildEnv = "ABYSS_BUILD_ENV";
        public const string EnvVersion = "ABYSS_VERSION";
        public const string EnvBuildNumber = "ABYSS_BUILD_NUMBER";
        public const string EnvAndroidKeystorePath = "ABYSS_ANDROID_KEYSTORE_PATH";
        public const string EnvAndroidKeystorePass = "ABYSS_ANDROID_KEYSTORE_PASS";
        public const string EnvAndroidKeyAlias = "ABYSS_ANDROID_KEY_ALIAS";
        public const string EnvAndroidKeyPass = "ABYSS_ANDROID_KEY_PASS";
        public const string EnvIosTeamId = "ABYSS_IOS_TEAM_ID";
        public const string EnvIosProvisioningProfile = "ABYSS_IOS_PROVISIONING_PROFILE";
        public const string EnvPrivacyPolicyUrl = "ABYSS_PRIVACY_POLICY_URL";
        public const string EnvTermsUrl = "ABYSS_TERMS_URL";
        public const string EnvAccountDeletionUrl = "ABYSS_ACCOUNT_DELETION_URL";

        /// <summary>
        /// 대상 빌드 환경.
        /// </summary>
        public BuildEnvironmentKind Environment { get; private set; } = BuildEnvironmentKind.Development;

        /// <summary>
        /// 시맨틱 버전.
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Android versionCode / iOS build number.
        /// </summary>
        public int BuildNumber { get; private set; }

        /// <summary>
        /// Android 키스토어 경로.
        /// </summary>
        public string AndroidKeystorePath { get; private set; }

        /// <summary>
        /// Android 키스토어 비밀번호.
        /// </summary>
        public string AndroidKeystorePass { get; private set; }

        /// <summary>
        /// Android 키 별칭.
        /// </summary>
        public string AndroidKeyAlias { get; private set; }

        /// <summary>
        /// Android 키 비밀번호.
        /// </summary>
        public string AndroidKeyPass { get; private set; }

        /// <summary>
        /// iOS Team ID.
        /// </summary>
        public string IosTeamId { get; private set; }

        /// <summary>
        /// iOS 프로비저닝 프로파일 UUID/이름.
        /// </summary>
        public string IosProvisioningProfile { get; private set; }

        /// <summary>
        /// 환경변수·CLI에서 설정을 로드한다.
        /// </summary>
        public static BuildConfiguration LoadFromEnvironment()
        {
            var config = new BuildConfiguration();
            config.Environment = ParseEnvironment(
                GetEnvOrArg(EnvBuildEnv, "-abyssBuildEnv", "Development"));

            config.Version = GetEnvOrArg(EnvVersion, "-abyssVersion", PlayerSettings.bundleVersion);
            config.BuildNumber = ParseBuildNumber(
                GetEnvOrArg(EnvBuildNumber, "-abyssBuildNumber", null),
                PlayerSettings.Android.bundleVersionCode);

            config.AndroidKeystorePath = GetEnvOrArg(EnvAndroidKeystorePath, "-androidKeystorePath", null);
            config.AndroidKeystorePass = GetEnvOrArg(EnvAndroidKeystorePass, "-androidKeystorePass", null);
            config.AndroidKeyAlias = GetEnvOrArg(EnvAndroidKeyAlias, "-androidKeyAlias", null);
            config.AndroidKeyPass = GetEnvOrArg(EnvAndroidKeyPass, "-androidKeyPass", null);
            config.IosTeamId = GetEnvOrArg(EnvIosTeamId, "-iosTeamId", null);
            config.IosProvisioningProfile = GetEnvOrArg(EnvIosProvisioningProfile, "-iosProvisioningProfile", null);
            return config;
        }

        /// <summary>
        /// 환경에 맞는 스크립팅 define 심볼을 반환한다.
        /// </summary>
        public IReadOnlyList<string> GetEnvironmentDefineSymbols()
        {
            switch (Environment)
            {
                case BuildEnvironmentKind.Production:
                    return new[] { "ABYSS_ENV_PRODUCTION" };
                case BuildEnvironmentKind.Staging:
                    return new[] { "ABYSS_ENV_STAGING" };
                default:
                    return new[] { "ABYSS_ENV_DEVELOPMENT" };
            }
        }

        private static BuildEnvironmentKind ParseEnvironment(string raw)
        {
            if (Enum.TryParse(raw, true, out BuildEnvironmentKind parsed))
                return parsed;

            return BuildEnvironmentKind.Development;
        }

        private static int ParseBuildNumber(string raw, int fallback)
        {
            if (int.TryParse(raw, out var parsed) && parsed > 0)
                return parsed;

            return fallback + 1;
        }

        private static string GetEnvOrArg(string envKey, string cliPrefix, string fallback)
        {
            var env = System.Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrEmpty(env))
                return env;

            var args = System.Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], cliPrefix, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }

            return fallback;
        }
    }

    /// <summary>
    /// AAB·iOS 배치모드 빌드 진입점.
    /// </summary>
    public static class AbyssChronicleBuildPipeline
    {
        private const string DefaultOutputRoot = "Builds";

        /// <summary>
        /// CLI: -executeMethod AbyssChronicleBuildPipeline.BuildFromCommandLine -buildTarget Android -abyssBuildEnv Production
        /// </summary>
        public static void BuildFromCommandLine()
        {
            var config = BuildConfiguration.LoadFromEnvironment();
            var target = ResolveBuildTarget();
            ApplyConfiguration(config);

            switch (target)
            {
                case BuildTarget.Android:
                    BuildAndroid(config);
                    break;
                case BuildTarget.iOS:
                    BuildIos(config);
                    break;
                default:
                    throw new BuildFailedException($"[AbyssChronicleBuildPipeline] Unsupported build target: {target}");
            }
        }

        /// <summary>
        /// Android App Bundle 을 빌드한다.
        /// </summary>
        public static void BuildAndroid(BuildConfiguration config)
        {
            config ??= BuildConfiguration.LoadFromEnvironment();
            ApplyConfiguration(config);
            ApplyAndroidSigning(config);

            var outputDir = Path.Combine(DefaultOutputRoot, "Android", config.Environment.ToString());
            Directory.CreateDirectory(outputDir);
            var outputPath = Path.Combine(outputDir, $"AbyssChronicle_{config.Version}_{config.BuildNumber}.aab");

            var options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenePaths(),
                locationPathName = outputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.CompressWithLz4HC,
            };

            EditorUserBuildSettings.buildAppBundle = true;
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"[AbyssChronicleBuildPipeline] Android build failed: {report.summary.result}");
        }

        /// <summary>
        /// iOS Xcode 프로젝트를 빌드한다.
        /// </summary>
        public static void BuildIos(BuildConfiguration config)
        {
            config ??= BuildConfiguration.LoadFromEnvironment();
            ApplyConfiguration(config);
            ApplyIosSigning(config);

            var outputDir = Path.Combine(DefaultOutputRoot, "iOS", config.Environment.ToString());
            Directory.CreateDirectory(outputDir);

            var options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenePaths(),
                locationPathName = outputDir,
                target = BuildTarget.iOS,
                targetGroup = BuildTargetGroup.iOS,
                options = BuildOptions.CompressWithLz4HC,
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"[AbyssChronicleBuildPipeline] iOS build failed: {report.summary.result}");
        }

        private static void ApplyConfiguration(BuildConfiguration config)
        {
            PlayerSettings.bundleVersion = config.Version;
            PlayerSettings.Android.bundleVersionCode = config.BuildNumber;
            PlayerSettings.iOS.buildNumber = config.BuildNumber.ToString();

            ApplyEnvironmentDefines(BuildTargetGroup.Android, config);
            ApplyEnvironmentDefines(BuildTargetGroup.iOS, config);
            ApplyStoreComplianceUrls(config);
        }

        private static void ApplyEnvironmentDefines(BuildTargetGroup group, BuildConfiguration config)
        {
            var envSymbols = new HashSet<string>(config.GetEnvironmentDefineSymbols());
            var existing = PlayerSettings.GetScriptingDefineSymbolsForGroup(group)
                .Split(';')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Where(s => !s.StartsWith("ABYSS_ENV_", StringComparison.Ordinal))
                .ToList();

            existing.AddRange(envSymbols);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", existing));
        }

        private static void ApplyStoreComplianceUrls(BuildConfiguration config)
        {
            var configPath = "Assets/GameResource/Data/StoreCompliance/StoreComplianceConfig.asset";
            var asset = AssetDatabase.LoadAssetAtPath<StoreComplianceConfig>(configPath);
            if (asset == null)
                return;

            var privacy = System.Environment.GetEnvironmentVariable(BuildConfiguration.EnvPrivacyPolicyUrl);
            var terms = System.Environment.GetEnvironmentVariable(BuildConfiguration.EnvTermsUrl);
            var deletion = System.Environment.GetEnvironmentVariable(BuildConfiguration.EnvAccountDeletionUrl);

            var so = new SerializedObject(asset);
            if (!string.IsNullOrEmpty(privacy))
                so.FindProperty("_privacyPolicyUrl").stringValue = privacy;
            if (!string.IsNullOrEmpty(terms))
                so.FindProperty("_termsOfServiceUrl").stringValue = terms;
            if (!string.IsNullOrEmpty(deletion))
                so.FindProperty("_accountDeletionUrl").stringValue = deletion;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        private static void ApplyAndroidSigning(BuildConfiguration config)
        {
            if (string.IsNullOrEmpty(config.AndroidKeystorePath))
                return;

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = config.AndroidKeystorePath;
            PlayerSettings.Android.keystorePass = config.AndroidKeystorePass ?? string.Empty;
            PlayerSettings.Android.keyaliasName = config.AndroidKeyAlias ?? string.Empty;
            PlayerSettings.Android.keyaliasPass = config.AndroidKeyPass ?? string.Empty;
        }

        private static void ApplyIosSigning(BuildConfiguration config)
        {
            if (!string.IsNullOrEmpty(config.IosTeamId))
                PlayerSettings.iOS.appleDeveloperTeamID = config.IosTeamId;

            if (!string.IsNullOrEmpty(config.IosProvisioningProfile))
                PlayerSettings.iOS.iOSManualProvisioningProfileID = config.IosProvisioningProfile;
        }

        private static BuildTarget ResolveBuildTarget()
        {
            var args = System.Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], "-buildTarget", StringComparison.OrdinalIgnoreCase) &&
                    Enum.TryParse(args[i + 1], true, out BuildTarget parsed))
                    return parsed;
            }

            return EditorUserBuildSettings.activeBuildTarget;
        }

        private static string[] GetEnabledScenePaths()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
        }
    }
}
