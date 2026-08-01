using Backend.AddressableKey;
using Backend.Object.Management;
using Backend.Util.Management;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Backend.GameSystems.Performance
{
    /// <summary>
    /// 품질 프리셋·절전 모드·목표 FPS 를 관리한다. 시뮬레이션 틱과 독립적으로 동작한다.
    /// </summary>
    public sealed class PerformanceManager : SingletonGameObject<PerformanceManager>
    {
        private static readonly Subject<PerformanceMode> ModeChanged = new();

        private PerformancePolicyTable _policy;
        private QualityPreset _userPreset;
        private QualityPreset _effectivePreset;
        private PerformanceMode _mode = PerformanceMode.Active;
        private float _lastInputRealtime;
        private bool _isBackground;
        private int _vfxSpawnCounter;

        /// <summary> 현재 렌더링 모드. </summary>
        public static PerformanceMode CurrentMode => Instance._mode;

        /// <summary> 적용 중인 품질 프리셋. </summary>
        public static QualityPreset EffectivePreset => Instance._effectivePreset;

        /// <summary> VFX 스폰 허용 여부(절전 모드에서는 false). </summary>
        public static bool ShouldPlayVfx => Instance._mode == PerformanceMode.Active;

        /// <summary> 모드 변경 Observable. </summary>
        public static Observable<PerformanceMode> OnModeChanged => ModeChanged;

        /// <summary>
        /// 정책 테이블을 로드하고 단말 성능으로 초기 프리셋을 적용한다.
        /// </summary>
        public static async UniTask InitializeAsync()
        {
            var instance = Instance;
            instance._policy = await ResourceManager.LoadResourceAsync<PerformancePolicyTable>(
                AddressableKeys.Performance.Get("PerformancePolicyTable"));

            if (instance._policy == null)
            {
                instance._policy = ScriptableObject.CreateInstance<PerformancePolicyTable>();
                instance._policy.ApplySpecDefaults();
            }

            instance._userPreset = PerformanceSettingsStore.LoadPreset();
            instance._lastInputRealtime = Time.realtimeSinceStartup;
            instance.RefreshEffectivePreset();
            instance.ApplyActiveQuality();
            instance.RegisterInputHooks();
        }

        /// <summary>
        /// 사용자 품질 프리셋을 변경하고 즉시 적용한다.
        /// </summary>
        public static void SetUserPreset(QualityPreset preset)
        {
            Instance._userPreset = preset;
            PerformanceSettingsStore.SavePreset(preset);
            Instance.RefreshEffectivePreset();
            if (Instance._mode == PerformanceMode.Active)
                Instance.ApplyActiveQuality();
        }

        /// <summary>
        /// VFX 밀도 게이트. 저사양·절전 시 일부 VFX 를 건너뛴다.
        /// </summary>
        public static bool TryConsumeVfxSlot()
        {
            var instance = Instance;
            if (!ShouldPlayVfx)
                return false;

            var density = instance._policy.GetVfxDensity(instance._effectivePreset);
            if (density >= 1f)
                return true;

            instance._vfxSpawnCounter++;
            var threshold = Mathf.Max(1, Mathf.RoundToInt(1f / density));
            return instance._vfxSpawnCounter % threshold == 0;
        }

        /// <summary>
        /// 시뮬레이션 틱은 프레임과 무관하게 진행되므로, 절전 모드에서도 정확도가 유지된다.
        /// </summary>
        public static void NotifySimulationTick()
        {
            // 시뮬레이션 레이어가 틱 기반이므로 별도 처리 불필요. 훅만 제공한다.
        }

        private void Update()
        {
            if (_isBackground || _policy == null)
                return;

            if (Time.realtimeSinceStartup - _lastInputRealtime >= _policy.IdleTimeoutSeconds)
                EnterPowerSave_Internal();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            _isBackground = pauseStatus;
            if (pauseStatus)
                EnterPowerSave_Internal();
            else
                ExitPowerSaveIfIdleExpired_Internal();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                _isBackground = true;
                EnterPowerSave_Internal();
            }
            else
            {
                _isBackground = false;
                _lastInputRealtime = Time.realtimeSinceStartup;
                ExitPowerSaveIfIdleExpired_Internal();
            }
        }

        private void RegisterInputHooks()
        {
            Observable.EveryUpdate()
                .Where(_ => Input.anyKeyDown || Input.touchCount > 0)
                .Subscribe(_ => OnUserInput_Internal())
                .AddTo(this);
        }

        private void OnUserInput_Internal()
        {
            _lastInputRealtime = Time.realtimeSinceStartup;
            if (_mode == PerformanceMode.PowerSave && !_isBackground)
                ExitPowerSave_Internal();
        }

        private void RefreshEffectivePreset()
        {
            _effectivePreset = DeviceCapabilityDetector.ResolveEffectivePreset(
                _userPreset,
                SystemInfo.systemMemorySize,
                SystemInfo.processorCount,
                _policy);
        }

        private void ApplyActiveQuality()
        {
            Application.targetFrameRate = _policy.GetTargetFps(_effectivePreset);
            _vfxSpawnCounter = 0;
        }

        private void EnterPowerSave_Internal()
        {
            if (_mode == PerformanceMode.PowerSave)
                return;

            _mode = PerformanceMode.PowerSave;
            Application.targetFrameRate = _policy.PowerSaveTargetFps;
            ModeChanged.OnNext(_mode);
        }

        private void ExitPowerSave_Internal()
        {
            if (_mode != PerformanceMode.PowerSave)
                return;

            _mode = PerformanceMode.Active;
            ApplyActiveQuality();
            ModeChanged.OnNext(_mode);
        }

        private void ExitPowerSaveIfIdleExpired_Internal()
        {
            if (Time.realtimeSinceStartup - _lastInputRealtime < _policy.IdleTimeoutSeconds)
                ExitPowerSave_Internal();
        }
    }
}
