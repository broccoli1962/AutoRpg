using System;
using Backend.Object.Management;
using Backend.Object.UI;
using Backend.Services.Save;
using Backend.Util.Localization;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Backend
{
    /// <summary>
    /// 로컬·클라우드 세이브 충돌 해결 팝업 View.
    /// </summary>
    public sealed class SaveConflictPopup : UIPopup<SaveConflictPresenter>
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _localInfoText;
        [SerializeField] private Text _cloudInfoText;
        [SerializeField] private CommonButton _useLocalButton;
        [SerializeField] private CommonButton _useCloudButton;

        public CommonButton UseLocalButton => _useLocalButton;
        public CommonButton UseCloudButton => _useCloudButton;

        /// <summary>
        /// 버튼 라벨을 현지화한다.
        /// </summary>
        public void SetButtonLabels(string useLocalLabel, string useCloudLabel)
        {
            SetButtonLabel(_useLocalButton, useLocalLabel);
            SetButtonLabel(_useCloudButton, useCloudLabel);
        }

        private static void SetButtonLabel(CommonButton button, string text)
        {
            if (button == null)
                return;

            var label = button.GetComponentInChildren<Text>();
            if (label != null)
                label.text = text;
        }

        /// <summary>
        /// 충돌 선택을 대기한다.
        /// </summary>
        public UniTask<SaveConflictChoice> WaitForChoiceAsync(
            CloudSaveMetadata local,
            CloudSaveMetadata cloud)
        {
            return Presenter.WaitForChoiceAsync(local, cloud);
        }

        /// <summary>
        /// 충돌 정보를 표시한다.
        /// </summary>
        public void SetConflictInfo(CloudSaveMetadata local, CloudSaveMetadata cloud)
        {
            if (_titleText != null)
                _titleText.text = "save_conflict.title".GetLocalizeText();

            if (_localInfoText != null)
                _localInfoText.text = FormatMetadata("save_conflict.local".GetLocalizeText(), local);

            if (_cloudInfoText != null)
                _cloudInfoText.text = FormatMetadata("save_conflict.cloud".GetLocalizeText(), cloud);
        }

        private static string FormatMetadata(string label, CloudSaveMetadata metadata)
        {
            if (metadata == null)
                return label;

            return $"{label}\n{metadata.SavedAtUtc:yyyy-MM-dd HH:mm:ss} UTC";
        }
    }

    /// <summary>
    /// SaveConflictPopup Presenter.
    /// </summary>
    public sealed class SaveConflictPresenter : UIPresenter<SaveConflictPopup>
    {
        private UniTaskCompletionSource<SaveConflictChoice> _choiceSource;

        /// <summary>
        /// 충돌 선택 대기를 시작한다.
        /// </summary>
        public UniTask<SaveConflictChoice> WaitForChoiceAsync(
            CloudSaveMetadata local,
            CloudSaveMetadata cloud)
        {
            _choiceSource = new UniTaskCompletionSource<SaveConflictChoice>();
            View.SetConflictInfo(local, cloud);
            View.SetButtonLabels(
                "save_conflict.use_local".GetLocalizeText(),
                "save_conflict.use_cloud".GetLocalizeText());
            BindButtons();
            return _choiceSource.Task;
        }

        private void BindButtons()
        {
            Bind(View.UseLocalButton, () => Resolve(SaveConflictChoice.UseLocal));
            Bind(View.UseCloudButton, () => Resolve(SaveConflictChoice.UseCloud));
        }

        private void Bind(CommonButton button, Action handler)
        {
            if (button == null || handler == null)
                return;

            button.OnClickAsObservable()
                .Subscribe(_ => handler())
                .AddTo(View);
        }

        private void Resolve(SaveConflictChoice choice)
        {
            _choiceSource?.TrySetResult(choice);
            UIManager.CloseDynamic(View);
        }
    }
}
