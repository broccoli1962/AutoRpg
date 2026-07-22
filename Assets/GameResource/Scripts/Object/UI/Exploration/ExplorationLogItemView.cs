using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Exploration
{
    public class ExplorationLogItemView : CachedMonobehaviour
    {
        [SerializeField] private Text _messageText;
        [SerializeField] private Image _categoryIcon;

        public string EventId { get; private set; }

        public void Bind(LogEntry entry)
        {
            EventId = entry.EventId;

            if (_messageText != null)
            {
                _messageText.text = entry.Text;
                _messageText.fontStyle = entry.IsPending ? FontStyle.Italic : FontStyle.Normal;
            }

            if (_categoryIcon != null)
                _categoryIcon.color = LogDisplayUtil.GetCategoryColor(entry.Category);
        }
    }
}
