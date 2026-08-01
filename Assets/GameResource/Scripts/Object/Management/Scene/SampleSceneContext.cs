using Backend.Object.UI;
using Cysharp.Threading.Tasks;

namespace Backend.Object.Management
{
    /// <summary>
    /// SampleScene 진입 시 모바일 HUD 를 오픈한다.
    /// </summary>
    public sealed class SampleSceneContext : SceneContext
    {
        protected override async UniTask OnEnterAsync()
        {
            UIManager.CloseAllUI();
            await UIManager.OpenAsync<ExplorationHudPanel>();
        }
    }
}
