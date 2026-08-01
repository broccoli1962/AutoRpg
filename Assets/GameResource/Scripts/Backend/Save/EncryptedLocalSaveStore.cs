using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.Services.Save
{
    /// <summary>
    /// AES 암호화 로컬 세이브 저장소.
    /// </summary>
    public sealed class EncryptedLocalSaveStore : ILocalSaveStore
    {
        private const string FILE_NAME = "abyss_save.dat";
        private const string KEY_SALT = "AbyssChronicle.Save.v1";

        private readonly string _filePath;

        public EncryptedLocalSaveStore(string filePath = null)
        {
            _filePath = filePath ?? Path.Combine(Application.persistentDataPath, FILE_NAME);
        }

        /// <summary>
        /// 로컬 세이브가 존재하는지 확인한다.
        /// </summary>
        public bool Exists() => File.Exists(_filePath);

        /// <summary>
        /// 로컬 세이브를 읽는다.
        /// </summary>
        public UniTask<GameSaveSnapshot> LoadAsync()
        {
            if (!Exists())
                return UniTask.FromResult<GameSaveSnapshot>(null);

            try
            {
                var encrypted = File.ReadAllBytes(_filePath);
                var json = Decrypt(encrypted);
                var snapshot = JsonUtility.FromJson<GameSaveSnapshot>(json);
                return UniTask.FromResult(snapshot);
            }
            catch (Exception)
            {
                return UniTask.FromResult<GameSaveSnapshot>(null);
            }
        }

        /// <summary>
        /// 로컬 세이브를 저장한다.
        /// </summary>
        public UniTask<bool> SaveAsync(GameSaveSnapshot snapshot)
        {
            if (snapshot == null)
                return UniTask.FromResult(false);

            try
            {
                snapshot.SavedAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var json = JsonUtility.ToJson(snapshot);
                var encrypted = Encrypt(json);
                File.WriteAllBytes(_filePath, encrypted);
                return UniTask.FromResult(true);
            }
            catch (Exception)
            {
                return UniTask.FromResult(false);
            }
        }

        /// <summary>
        /// 로컬 세이브 메타데이터를 반환한다.
        /// </summary>
        public CloudSaveMetadata GetLocalMetadata(string userId)
        {
            if (!Exists())
                return null;

            var snapshot = LoadAsync().GetAwaiter().GetResult();
            if (snapshot == null)
                return null;

            return new CloudSaveMetadata(
                userId,
                snapshot.SavedAtUnixSeconds,
                snapshot.SchemaVersion,
                ComputeChecksum(snapshot));
        }

        internal static string ComputeChecksum(GameSaveSnapshot snapshot)
        {
            var json = JsonUtility.ToJson(snapshot);
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
            return Convert.ToBase64String(hash);
        }

        private static byte[] Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = DeriveKey();
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
            return result;
        }

        private static string Decrypt(byte[] cipherWithIv)
        {
            using var aes = Aes.Create();
            aes.Key = DeriveKey();

            var iv = new byte[aes.BlockSize / 8];
            var cipher = new byte[cipherWithIv.Length - iv.Length];
            Buffer.BlockCopy(cipherWithIv, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(cipherWithIv, iv.Length, cipher, 0, cipher.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        private static byte[] DeriveKey()
        {
            var seed = SystemInfo.deviceUniqueIdentifier + KEY_SALT;
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(seed));
        }
    }
}
