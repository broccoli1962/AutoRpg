using Backend.GameSystems.Prestige;
using Backend.GameSystems.Save;

namespace Backend.GameSystems.Exploration.Narration
{
    /// <summary>
    /// 로그 피드 북마크(즐겨찾기 순간)를 메타 진행 데이터에 저장한다.
    /// </summary>
    public static class LogBookmarkSystem
    {
        private const int MaxFavorites = 50;

        public static bool Toggle(string plainText, int floor)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                return false;

            var meta = PrestigeManager.GetMeta();
            if (meta?.FavoriteMoments == null)
                return false;

            var entry = FormatEntry(plainText, floor);
            var index = meta.FavoriteMoments.IndexOf(entry);
            if (index >= 0)
            {
                meta.FavoriteMoments.RemoveAt(index);
                GameSaveManager.Save();
                return false;
            }

            meta.FavoriteMoments.Add(entry);
            while (meta.FavoriteMoments.Count > MaxFavorites)
                meta.FavoriteMoments.RemoveAt(0);

            GameSaveManager.Save();
            return true;
        }

        public static bool IsBookmarked(string plainText, int floor)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                return false;

            var meta = PrestigeManager.GetMeta();
            return meta?.FavoriteMoments != null &&
                   meta.FavoriteMoments.Contains(FormatEntry(plainText, floor));
        }

        public static string FormatEntry(string plainText, int floor) =>
            $"[{floor}층] {plainText.Trim()}";

        public static string ApplyBookmarkPrefix(string richText, bool isBookmarked)
        {
            if (!isBookmarked || string.IsNullOrEmpty(richText))
                return richText;

            return $"<color=#ffd966>★</color> {richText}";
        }
    }
}
