using Backend.GameSystems.Equipment.Data;
using UnityEngine;

namespace Backend.GameSystems.Equipment
{
    /// <summary>
    /// 장비 등급 표시 색상·라벨 (09_성장과경제.md).
    /// </summary>
    public static class EquipmentGradeUtil
    {
        public static string GetLabel(EquipmentGrade grade) =>
            grade switch
            {
                EquipmentGrade.Uncommon => "고급",
                EquipmentGrade.Rare => "희귀",
                EquipmentGrade.Epic => "영웅",
                EquipmentGrade.Legendary => "전설",
                EquipmentGrade.Mythic => "신화",
                _ => "일반"
            };

        public static Color GetColor(EquipmentGrade grade) =>
            grade switch
            {
                EquipmentGrade.Uncommon => new Color(0.35f, 0.85f, 0.45f),
                EquipmentGrade.Rare => new Color(0.35f, 0.65f, 0.95f),
                EquipmentGrade.Epic => new Color(0.65f, 0.45f, 0.95f),
                EquipmentGrade.Legendary => new Color(0.95f, 0.78f, 0.25f),
                EquipmentGrade.Mythic => new Color(0.95f, 0.35f, 0.35f),
                _ => new Color(0.85f, 0.85f, 0.85f)
            };

        public static string FormatRichName(string displayName, EquipmentGrade grade)
        {
            if (string.IsNullOrEmpty(displayName))
                return displayName;

            var hex = ColorUtility.ToHtmlStringRGB(GetColor(grade));
            return $"<color=#{hex}>{displayName}</color>";
        }
    }
}
