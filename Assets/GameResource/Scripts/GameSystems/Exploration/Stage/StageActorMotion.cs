using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Backend.GameSystems.Exploration.Stage
{
    /// <summary>
    /// LitMotion 기반 스테이지 액터 모션 (idle / attack / hit / death).
    /// </summary>
    public static class StageActorMotion
    {
        public static async UniTask PlayAttackLungeAsync(RectTransform actor, float lungeX, CancellationToken token)
        {
            var start = actor.anchoredPosition;
            var lunge = start + new Vector2(lungeX, 0f);
            await LMotion.Create(start, lunge, 0.1f).WithEase(Ease.OutQuad)
                .Bind(value => actor.anchoredPosition = value).ToUniTask(token);
            await LMotion.Create(lunge, start, 0.12f).WithEase(Ease.InQuad)
                .Bind(value => actor.anchoredPosition = value).ToUniTask(token);
        }

        public static async UniTask PlayHitShakeAsync(RectTransform actor, CancellationToken token)
        {
            await LMotion.Create(Vector3.one, new Vector3(1.08f, 0.92f, 1f), 0.08f)
                .WithEase(Ease.OutQuad)
                .BindToLocalScale(actor)
                .ToUniTask(token);
            await LMotion.Create(actor.localScale, Vector3.one, 0.1f)
                .WithEase(Ease.OutQuad)
                .BindToLocalScale(actor)
                .ToUniTask(token);
        }

        public static async UniTask PlaySpawnScaleAsync(RectTransform actor, float targetScale, CancellationToken token)
        {
            actor.localScale = Vector3.zero;
            await LMotion.Create(Vector3.zero, Vector3.one * targetScale, 0.22f)
                .WithEase(Ease.OutBack)
                .BindToLocalScale(actor)
                .ToUniTask(token);
        }

        public static async UniTask PlayDeathScaleAsync(RectTransform actor, CancellationToken token)
        {
            var start = actor.localScale;
            await LMotion.Create(start, Vector3.zero, 0.25f)
                .WithEase(Ease.InBack)
                .BindToLocalScale(actor)
                .ToUniTask(token);
        }

        public static async UniTask PlaySlashFadeAsync(CanvasGroup group, CancellationToken token)
        {
            if (group == null)
                return;

            group.alpha = 0.95f;
            group.gameObject.SetActive(true);
            await LMotion.Create(0.95f, 0f, 0.14f)
                .Bind(alpha => group.alpha = alpha)
                .ToUniTask(token);
            group.gameObject.SetActive(false);
        }
    }
}
