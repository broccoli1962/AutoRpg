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

        public void Bind(LogEntry entry)
        {
            if (_messageText != null)
                _messageText.text = entry.Text;

            if (_categoryIcon != null)
                _categoryIcon.color = GetCategoryColor(entry.Category);
        }

        private static Color GetCategoryColor(LogCategory category)
        {
            return category switch
            {
                LogCategory.Move => new Color(0.65f, 0.65f, 0.65f),
                LogCategory.Combat => new Color(0.85f, 0.35f, 0.35f),
                LogCategory.Discovery => new Color(0.9f, 0.75f, 0.25f),
                LogCategory.Status => new Color(0.35f, 0.65f, 0.9f),
                LogCategory.Milestone => new Color(0.7f, 0.45f, 0.9f),
                _ => Color.white
            };
        }
    }
}
