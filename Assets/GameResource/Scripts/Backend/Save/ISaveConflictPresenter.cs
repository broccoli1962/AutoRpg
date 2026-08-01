using Cysharp.Threading.Tasks;

namespace Backend.Services.Save
{
    /// <summary>
    /// 로컬·클라우드 충돌 시 사용자 선택 UI 훅.
    /// </summary>
    public interface ISaveConflictPresenter
    {
        /// <summary>
        /// 충돌 해결 UI를 표시하고 사용자 선택을 반환한다.
        /// </summary>
        UniTask<SaveConflictChoice> PresentConflictAsync(
            CloudSaveMetadata localMetadata,
            CloudSaveMetadata cloudMetadata);
    }

    /// <summary>
    /// UI 없이 로컬 우선으로 자동 해결하는 no-op 프레젠터.
    /// </summary>
    public sealed class AutoLocalSaveConflictPresenter : ISaveConflictPresenter
    {
        /// <summary>
        /// 항상 로컬 세이브를 선택한다.
        /// </summary>
        public UniTask<SaveConflictChoice> PresentConflictAsync(
            CloudSaveMetadata localMetadata,
            CloudSaveMetadata cloudMetadata)
        {
            return UniTask.FromResult(SaveConflictChoice.UseLocal);
        }
    }
}
