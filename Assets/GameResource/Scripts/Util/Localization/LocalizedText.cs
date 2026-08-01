using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

namespace Backend.Util.Localization
{
    /// <summary>
    /// UI Text / TMP 에 현지화 키를 바인딩하고 언어 변경 시 자동 갱신한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string _localizationKey;
        [SerializeField] private Text _legacyText;
#if TMP_PRESENT
        [SerializeField] private TMP_Text _tmpText;
#endif

        private void Reset()
        {
            if (!TryGetComponent(out _legacyText))
                _legacyText = GetComponentInChildren<Text>(true);
#if TMP_PRESENT
            if (_tmpText == null)
                TryGetComponent(out _tmpText);
#endif
        }

        private void OnEnable()
        {
            Refresh();
            LocalizeTable.OnChangedLanguage += Refresh;
        }

        private void OnDisable()
        {
            LocalizeTable.OnChangedLanguage -= Refresh;
        }

        /// <summary>
        /// 현지화 키를 설정하고 즉시 갱신한다.
        /// </summary>
        public void SetKey(string localizationKey)
        {
            _localizationKey = localizationKey;
            Refresh();
        }

        private void Refresh()
        {
            if (string.IsNullOrEmpty(_localizationKey))
                return;

            var text = LocalizationService.Get(_localizationKey);
            ApplyText(text);
        }

        private void ApplyText(string text)
        {
            if (_legacyText != null)
                _legacyText.text = text;
#if TMP_PRESENT
            if (_tmpText != null)
                _tmpText.text = text;
#endif
        }
    }
}
