using Backend.GameSystems.Exploration;
using Backend.Object.Management;
using Backend.Object.UI;
using Backend.AddressableKey;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.Object.Management.Scene
{
    /// <summary>
    /// 탐험 게임 씬 진입점. HUD를 구성하고 탐험 세션을 시작한다.
    /// </summary>
    public sealed class GameSceneContext : SceneContext
    {
        private ExplorationHudPanel _hudPanel;

        protected override async UniTask OnEnterAsync()
        {
            UIManager.CloseAllUI();
            _hudPanel = await UIManager.OpenAsync<ExplorationHudPanel>(AddressableKeys.UI.Get<ExplorationHudPanel>());
            if (_hudPanel == null)
                Debug.LogError("[GameSceneContext] ExplorationHudPanel open failed. Check Addressables and prefab wiring.");
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
