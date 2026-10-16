using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace MoonlitHomestead.Editor
{
    public static class TMPMigrationHelper
    {
        [MenuItem("Moonlit Homestead/Migrate Scene UI to TextMeshPro")]
        public static void MigrateSceneUIToTMP()
        {
            GameObject hudCanvas = GameObject.Find("HUD_Canvas");
            if (hudCanvas == null)
            {
                Canvas anyCanvas = Object.FindAnyObjectByType<Canvas>();
                if (anyCanvas != null) hudCanvas = anyCanvas.gameObject;
            }

            if (hudCanvas == null)
            {
                EditorUtility.DisplayDialog("Error", "HUD_Canvas tidak ditemukan di scene saat ini!", "OK");
                return;
            }

            // Cari semua Text legacy di bawah Canvas
            Text[] legacyTexts = hudCanvas.GetComponentsInChildren<Text>(true);
            if (legacyTexts.Length == 0)
            {
                EditorUtility.DisplayDialog("Info", "Semua UI Text di scene sudah menggunakan TextMeshPro (tidak ada Text legacy yang ditemukan).", "OK");
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Migrate UI Text to TextMeshPro");
            int undoGroup = Undo.GetCurrentGroup();

            int convertedCount = 0;
            foreach (Text legacy in legacyTexts)
            {
                if (legacy == null) continue;

                GameObject go = legacy.gameObject;

                // Salin properti
                string content = legacy.text;
                float fontSize = legacy.fontSize;
                FontStyle fontStyle = legacy.fontStyle;
                Color color = legacy.color;
                TextAnchor anchor = legacy.alignment;
                bool raycast = legacy.raycastTarget;

                // Hapus komponen legacy
                Undo.DestroyObjectImmediate(legacy);

                // Tambahkan TextMeshProUGUI
                TextMeshProUGUI tmp = Undo.AddComponent<TextMeshProUGUI>(go);
                tmp.text = content;
                tmp.fontSize = fontSize;
                tmp.color = color;
                tmp.raycastTarget = raycast;

                // Konversi style
                switch (fontStyle)
                {
                    case FontStyle.Bold:
                        tmp.fontStyle = FontStyles.Bold;
                        break;
                    case FontStyle.Italic:
                        tmp.fontStyle = FontStyles.Italic;
                        break;
                    case FontStyle.BoldAndItalic:
                        tmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
                        break;
                    default:
                        tmp.fontStyle = FontStyles.Normal;
                        break;
                }

                // Konversi alignment
                tmp.alignment = MapAlignment(anchor);

                convertedCount++;
            }

            // Wire ulang GameHUD jika ada
            GameHUD gameHUD = hudCanvas.GetComponent<GameHUD>();
            if (gameHUD == null) gameHUD = Object.FindAnyObjectByType<GameHUD>();
            if (gameHUD != null)
            {
                SerializedObject hudSerialized = new SerializedObject(gameHUD);
                WireTMP(hudSerialized, "clockText", "ClockText");
                WireTMP(hudSerialized, "loopText", "LoopText");
                WireTMP(hudSerialized, "dialogueText", "DialogueText");
                WireTMP(hudSerialized, "itemText", "HotbarText");
                WireTMP(hudSerialized, "stabilityText", "StabilityText");
                hudSerialized.ApplyModifiedProperties();
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log($"<color=#70FFE5>[TMPMigrationHelper] Berhasil memigrasi {convertedCount} komponen UI Text ke TextMeshProUGUI!</color>");
            EditorUtility.DisplayDialog(
                "Migrasi TMP Selesai",
                $"Berhasil mengonversi {convertedCount} UI Text ke TextMeshProUGUI secara in-place tanpa merusak layout atau anchor!",
                "Mantap"
            );
        }

        private static void WireTMP(SerializedObject so, string propertyName, string goName)
        {
            GameObject go = GameObject.Find(goName);
            if (go != null)
            {
                TMP_Text tmp = go.GetComponent<TMP_Text>();
                if (tmp != null)
                {
                    so.FindProperty(propertyName).objectReferenceValue = tmp;
                }
            }
        }

        private static TextAlignmentOptions MapAlignment(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.MidlineLeft;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.MidlineRight;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Center;
            }
        }
    }
}
