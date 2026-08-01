using System.Threading;
using Backend.AddressableKey;
using Backend.Object.Management;
using Backend.Object.Management.Pool;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.GameSystems.Performance
{
    /// <summary>
    /// 전투 연출(몬스터·데미지 텍스트·드롭·히트 VFX) 오브젝트 풀을 관리한다.
    /// </summary>
    public static class CombatVisualPoolService
    {
        private const int DEFAULT_CAPACITY = 12;
        private const int MAX_SIZE = 64;
        private const int PRELOAD_COUNT = 4;

        private static Transform _root;
        private static bool _initialized;

        /// <summary>
        /// 전투 풀을 비동기로 초기화하고 프리로드한다.
        /// </summary>
        public static async UniTask InitializeAsync(Transform parent = null, CancellationToken token = default)
        {
            if (_initialized)
                return;

            _root = parent != null ? parent : CreateDefaultRoot();
            await EnsurePoolAsync<PooledCombatSprite>(
                CombatVisualKind.Monster,
                AddressableKeys.CombatPool.Get("MonsterSprite"),
                token);
            await EnsurePoolAsync<PooledDamageText>(
                CombatVisualKind.DamageText,
                AddressableKeys.CombatPool.Get("DamageText"),
                token);
            await EnsurePoolAsync<PooledCombatSprite>(
                CombatVisualKind.DropIcon,
                AddressableKeys.CombatPool.Get("DropIcon"),
                token);
            await EnsurePoolAsync<PooledHitVfx>(
                CombatVisualKind.HitVfx,
                AddressableKeys.CombatPool.Get("HitVfx"),
                token);

            _initialized = true;
        }

        /// <summary>
        /// 몬스터 스프라이트를 풀에서 꺼낸다.
        /// </summary>
        public static PooledCombatSprite SpawnMonster()
        {
            return ObjectPoolManager.Get<PooledCombatSprite>(PoolName(CombatVisualKind.Monster));
        }

        /// <summary>
        /// 데미지 텍스트를 풀에서 꺼낸다.
        /// </summary>
        public static PooledDamageText SpawnDamageText()
        {
            return ObjectPoolManager.Get<PooledDamageText>(PoolName(CombatVisualKind.DamageText));
        }

        /// <summary>
        /// 드롭 아이콘을 풀에서 꺼낸다.
        /// </summary>
        public static PooledCombatSprite SpawnDropIcon()
        {
            return ObjectPoolManager.Get<PooledCombatSprite>(PoolName(CombatVisualKind.DropIcon));
        }

        /// <summary>
        /// 히트 VFX 를 풀에서 꺼낸다. 절전·VFX 밀도 게이트를 통과하지 못하면 null.
        /// </summary>
        public static PooledHitVfx SpawnHitVfx()
        {
            if (!PerformanceManager.ShouldPlayVfx || !PerformanceManager.TryConsumeVfxSlot())
                return null;

            return ObjectPoolManager.Get<PooledHitVfx>(PoolName(CombatVisualKind.HitVfx));
        }

        /// <summary>
        /// 몬스터 스프라이트를 풀에 반환한다.
        /// </summary>
        public static void ReleaseMonster(PooledCombatSprite visual)
        {
            ReleaseInternal(CombatVisualKind.Monster, visual);
        }

        /// <summary>
        /// 데미지 텍스트를 풀에 반환한다.
        /// </summary>
        public static void ReleaseDamageText(PooledDamageText visual)
        {
            ReleaseInternal(CombatVisualKind.DamageText, visual);
        }

        /// <summary>
        /// 드롭 아이콘을 풀에 반환한다.
        /// </summary>
        public static void ReleaseDropIcon(PooledCombatSprite visual)
        {
            ReleaseInternal(CombatVisualKind.DropIcon, visual);
        }

        /// <summary>
        /// 히트 VFX 를 풀에 반환한다.
        /// </summary>
        public static void ReleaseHitVfx(PooledHitVfx visual)
        {
            ReleaseInternal(CombatVisualKind.HitVfx, visual);
        }

        /// <summary>
        /// 테스트/도메인 리로드용 초기화 상태를 리셋한다.
        /// </summary>
        public static void ResetForTests()
        {
            _initialized = false;
            _root = null;
        }

        private static async UniTask EnsurePoolAsync<T>(
            CombatVisualKind kind,
            string addressableKey,
            CancellationToken token) where T : Component
        {
            var poolName = PoolName(kind);
            await ObjectPoolManager.GetOrCreatePoolAsync<T>(
                poolName,
                addressableKey,
                PRELOAD_COUNT,
                _root,
                DEFAULT_CAPACITY,
                MAX_SIZE,
                onRelease: OnRelease<T>,
                token: token);
        }

        private static void OnRelease<T>(T component) where T : Component
        {
            if (component is PooledCombatSprite sprite)
                sprite.ResetVisual();
            else if (component is PooledDamageText damageText)
                damageText.ResetVisual();
            else if (component is PooledHitVfx hitVfx)
                hitVfx.ResetVisual();
        }

        private static void ReleaseInternal<T>(CombatVisualKind kind, T visual) where T : Component
        {
            if (visual == null)
                return;

            ObjectPoolManager.Release(PoolName(kind), visual);
        }

        private static string PoolName(CombatVisualKind kind) => $"CombatPool_{kind}";

        private static Transform CreateDefaultRoot()
        {
            var go = new GameObject("CombatVisualPoolRoot");
            UnityEngine.Object.DontDestroyOnLoad(go);
            return go.transform;
        }
    }
}
