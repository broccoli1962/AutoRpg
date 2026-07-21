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
        private ExplorationHudPanel _hudPanel;

        protected override async UniTask OnEnterAsync()
        {
            ExplorationManager.ProcessOfflineElapsed();
            ExplorationManager.StartExploration();

            _hudPanel = await UIManager.OpenAsync<ExplorationHudPanel>();
            if (_hudPanel == null)
            {
                Debug.LogWarning("[GameSceneContext] ExplorationHudPanel을 열지 못했습니다. Addressable 등록이 필요합니다.");
            }
        }

        protected override void OnExit()
        {
            if (_hudPanel != null)
            {
                UIManager.CloseDynamic(_hudPanel);
                _hudPanel = null;
            }
        }
    }
}
