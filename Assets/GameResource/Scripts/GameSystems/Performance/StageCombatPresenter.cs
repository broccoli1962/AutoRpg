using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.GameSystems.Performance
{
    /// <summary>
    /// 시뮬레이션 결과를 풀링된 연출 오브젝트로 재생한다. 결과를 변경하지 않는다.
    /// </summary>
    public sealed class StageCombatPresenter : MonoBehaviour
    {
        [SerializeField] private RectTransform _stageRoot;
        [SerializeField] private float _hitVfxLifetimeSeconds = 0.35f;

        private readonly System.Collections.Generic.List<PooledHitVfx> _activeHitVfx = new();

        /// <summary>
        /// 전투 풀과 프레젠터를 초기화한다.
        /// </summary>
        public async UniTask InitializeAsync(CancellationToken token = default)
        {
            var root = _stageRoot != null ? _stageRoot : transform as RectTransform;
            await CombatVisualPoolService.InitializeAsync(root, token);
        }

        /// <summary>
        /// 몬스터 스프라이트를 스폰한다.
        /// </summary>
        public PooledCombatSprite ShowMonster(Vector2 anchoredPosition)
        {
            var visual = CombatVisualPoolService.SpawnMonster();
            if (visual == null)
                return null;

            visual.RectTransform.SetParent(_stageRoot, false);
            visual.RectTransform.anchoredPosition = anchoredPosition;
            return visual;
        }

        /// <summary>
        /// 데미지 숫자를 표시한다.
        /// </summary>
        public PooledDamageText ShowDamage(double amount, Vector2 anchoredPosition, bool isCritical = false)
        {
            var visual = CombatVisualPoolService.SpawnDamageText();
            if (visual == null)
                return null;

            visual.RectTransform.SetParent(_stageRoot, false);
            visual.RectTransform.anchoredPosition = anchoredPosition;
            visual.Label.text = Mathf.RoundToInt((float)amount).ToString(CultureInfo.InvariantCulture);
            visual.Label.color = isCritical ? new Color(1f, 0.85f, 0.2f) : Color.white;
            return visual;
        }

        /// <summary>
        /// 드롭 아이콘을 표시한다.
        /// </summary>
        public PooledCombatSprite ShowDropIcon(Vector2 anchoredPosition)
        {
            var visual = CombatVisualPoolService.SpawnDropIcon();
            if (visual == null)
                return null;

            visual.RectTransform.SetParent(_stageRoot, false);
            visual.RectTransform.anchoredPosition = anchoredPosition;
            return visual;
        }

        /// <summary>
        /// 히트 VFX 를 재생한다. 절전·밀도 게이트에 의해 생략될 수 있다.
        /// </summary>
        public PooledHitVfx ShowHitVfx(Vector2 anchoredPosition)
        {
            var visual = CombatVisualPoolService.SpawnHitVfx();
            if (visual == null)
                return null;

            visual.RectTransform.SetParent(_stageRoot, false);
            visual.RectTransform.anchoredPosition = anchoredPosition;
            visual.Begin(_hitVfxLifetimeSeconds);
            _activeHitVfx.Add(visual);
            return visual;
        }

        /// <summary>
        /// 만료된 VFX 를 풀에 반환한다.
        /// </summary>
        public void TickExpiredVfx()
        {
            for (var i = _activeHitVfx.Count - 1; i >= 0; i--)
            {
                var vfx = _activeHitVfx[i];
                if (vfx == null || !vfx.IsExpired)
                    continue;

                CombatVisualPoolService.ReleaseHitVfx(vfx);
                _activeHitVfx.RemoveAt(i);
            }
        }

        private void Update()
        {
            TickExpiredVfx();
        }
    }
}
