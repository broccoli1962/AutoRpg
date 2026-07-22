using Backend.GameSystems.Exploration;
using Backend.Object.Management;
using Backend.Object.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.Object.Management.Scene
{
    /// <summary>
    /// 탐험 게임 씬 진입점. HUD를 구성하고 탐험 세션을 시작한다.
    /// </summary>
    public class GameSceneContext : SceneContext
    {
        public const string ExplorationHudAddressableKey = "UI/ExplorationHudPanel.prefab";

        private ExplorationHudPanel _hudPanel;
        private ExplorationRuntimeHud _runtimeHudFallback;

        protected override async UniTask OnEnterAsync()
        {
            _hudPanel = await UIManager.OpenAsync<ExplorationHudPanel>(ExplorationHudAddressableKey);
            if (_hudPanel == null)
            {
                Debug.LogWarning(
                    "[GameSceneContext] Addressable ExplorationHudPanel 로드 실패. RuntimeHud fallback을 사용합니다.");
                var fallbackGo = new GameObject("ExplorationRuntimeHudFallback");
                _runtimeHudFallback = fallbackGo.AddComponent<ExplorationRuntimeHud>();
                fallbackGo.AddComponent<ExplorationStartRuntimePanel>();
            }
        }

        protected override void OnExit()
        {
            if (_hudPanel != null)
            {
                UIManager.CloseDynamic(_hudPanel);
                _hudPanel = null;
            }

            if (_runtimeHudFallback != null)
            {
                Destroy(_runtimeHudFallback.gameObject);
                _runtimeHudFallback = null;
            }
        }
    }
}
