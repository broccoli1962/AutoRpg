using Backend.Object.Management;
using Backend.Object.UI.Backend;
using Backend.Object.UI;
using Backend.Services.Save;
using Cysharp.Threading.Tasks;

namespace Backend.Services.Save
{
    /// <summary>
    /// SaveConflictPopup 을 통해 충돌 해결 UI를 표시한다.
    /// </summary>
    public sealed class SaveConflictUiPresenter : ISaveConflictPresenter
    {
        /// <summary>
        /// 충돌 해결 UI를 표시하고 사용자 선택을 반환한다.
        /// </summary>
        public async UniTask<SaveConflictChoice> PresentConflictAsync(
            CloudSaveMetadata localMetadata,
            CloudSaveMetadata cloudMetadata)
        {
            var popup = await UIManager.OpenAsync<SaveConflictPopup>();
            if (popup == null)
                return SaveConflictChoice.UseLocal;

            return await popup.WaitForChoiceAsync(localMetadata, cloudMetadata);
        }
    }
}
