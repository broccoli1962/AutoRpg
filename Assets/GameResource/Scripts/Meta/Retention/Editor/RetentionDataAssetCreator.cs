#if UNITY_EDITOR
using Backend.Meta.Attendance;
using Backend.Meta.Quests;
using UnityEditor;
using UnityEngine;

namespace Backend.Meta.Retention.Editor
{
    /// <summary>
    /// QuestTable·AttendanceTable 에셋이 없을 때 spec 기본값으로 생성한다.
    /// </summary>
    public static class RetentionDataAssetCreator
    {
        private const string QuestTablePath = "Assets/GameResource/Data/Quests/QuestTable.asset";
        private const string AttendanceTablePath = "Assets/GameResource/Data/Quests/AttendanceTable.asset";

        [MenuItem("Tools/Abyss Chronicle/Ensure Retention Data Assets")]
        public static void EnsureAssets()
        {
            EnsureFolder("Assets/GameResource/Data", "Quests");
            EnsureQuestTable();
            EnsureAttendanceTable();
            AssetDatabase.SaveAssets();
        }

        private static void EnsureQuestTable()
        {
            var table = AssetDatabase.LoadAssetAtPath<QuestTable>(QuestTablePath);
            if (table == null)
            {
                table = ScriptableObject.CreateInstance<QuestTable>();
                table.ApplySpecDefaults();
                AssetDatabase.CreateAsset(table, QuestTablePath);
            }
            else
            {
                table.ApplySpecDefaults();
                EditorUtility.SetDirty(table);
            }
        }

        private static void EnsureAttendanceTable()
        {
            var table = AssetDatabase.LoadAssetAtPath<AttendanceTable>(AttendanceTablePath);
            if (table == null)
            {
                table = ScriptableObject.CreateInstance<AttendanceTable>();
                table.ApplySpecDefaults();
                AssetDatabase.CreateAsset(table, AttendanceTablePath);
            }
            else
            {
                table.ApplySpecDefaults();
                EditorUtility.SetDirty(table);
            }
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
            {
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    var slashIndex = parent.LastIndexOf('/');
                    var grandParent = parent.Substring(0, slashIndex);
                    var parentName = parent.Substring(slashIndex + 1);
                    AssetDatabase.CreateFolder(grandParent, parentName);
                }

                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
#endif
