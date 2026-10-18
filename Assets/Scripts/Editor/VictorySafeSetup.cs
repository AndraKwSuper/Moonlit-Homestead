#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace MoonlitHomestead.Editor
{
    public static class VictorySafeSetup
    {
        private const string OverlayName = "Victory_Screen_Overlay";
        private const string FontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        [MenuItem("Moonlit Homestead/Setup Ending Screen (Safe)")]
        public static void SetupEndingScreen()
        {
            GameObject hudCanvas = GameObject.Find("HUD_Canvas");
            if (hudCanvas == null)
            {
                // Coba buka scene Kebun jika sedang di scene lain
                if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Kebun.unity")
                {
                    bool open = EditorUtility.DisplayDialog(
                        "Buka Scene Kebun?",
                        "HUD_Canvas berada di Scene Kebun. Buka scene Kebun untuk memasang Victory Ending Screen?",
                        "Buka Kebun", "Batal"
                    );

                    if (open)
                    {
                        EditorSceneManager.OpenScene("Assets/Scenes/Kebun.unity", OpenSceneMode.Single);
                        hudCanvas = GameObject.Find("HUD_Canvas");
                    }
                }
            }

            if (hudCanvas == null)
            {
                Debug.LogError("[VictorySafeSetup] HUD_Canvas tidak ditemukan di scene saat ini!");
                return;
            }

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);

            // 1. Buat / Ambil child Victory_Screen_Overlay
            Transform existingOverlay = hudCanvas.transform.Find(OverlayName);
            GameObject overlayGO;
            if (existingOverlay == null)
            {
                overlayGO = new GameObject(OverlayName, typeof(RectTransform));
                overlayGO.transform.SetParent(hudCanvas.transform, false);
            }
            else
            {
                overlayGO = existingOverlay.gameObject;
            }

            overlayGO.SetActive(true);
            overlayGO.transform.SetAsLastSibling(); // Pastikan render di lapisan paling depan HUD

            // RectTransform: Stretch Full
            RectTransform overlayRT = overlayGO.GetComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;
            overlayRT.pivot = new Vector2(0.5f, 0.5f);
            overlayRT.localScale = Vector3.one;

            // Image: Navy gelap keemasan fajar (#0B1220, alpha = 0.95)
            Image overlayImg = overlayGO.GetComponent<Image>();
            if (overlayImg == null) overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = new Color32(11, 18, 32, 242);
            overlayImg.raycastTarget = false;

            // CanvasGroup: Default hidden (alpha = 0, blocksRaycasts = false, interactable = false)
            CanvasGroup cg = overlayGO.GetComponent<CanvasGroup>();
            if (cg == null) cg = overlayGO.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;

            // 2. Child Text 1: Title ("FAJAR PERTAMA")
            TextMeshProUGUI titleTMP = GetOrCreateTMP(
                overlayGO,
                "Title",
                new Vector2(0f, 50f),
                new Vector2(800f, 80f),
                56f,
                FontStyles.Bold,
                new Color32(255, 224, 130, 255), // #FFE082 Emas Fajar
                TextAlignmentOptions.Center,
                "FAJAR PERTAMA",
                font
            );

            // 3. Child Text 2: Subtitle
            TextMeshProUGUI subtitleTMP = GetOrCreateTMP(
                overlayGO,
                "Subtitle",
                new Vector2(0f, -10f),
                new Vector2(900f, 60f),
                18f,
                FontStyles.Italic,
                new Color32(224, 224, 224, 255), // #E0E0E0
                TextAlignmentOptions.Center,
                "Siklus Malam Abadi Terpecahkan — Elian Menyambut Fajar",
                font
            );

            // 4. Child Button 1: RestartBtn ("Ulangi Siklus")
            Button restartBtn = GetOrCreateButton(
                overlayGO,
                "RestartBtn",
                new Vector2(0f, -80f),
                new Vector2(220f, 44f),
                "Ulangi Siklus",
                new Color32(255, 224, 130, 255),
                font
            );

            // 5. Child Button 2: QuitBtn ("Keluar Game")
            Button quitBtn = GetOrCreateButton(
                overlayGO,
                "QuitBtn",
                new Vector2(0f, -135f),
                new Vector2(220f, 44f),
                "Keluar Game",
                new Color32(203, 213, 225, 255),
                font
            );

            // 6. Pasang Komponen VictoryUI & Sambungkan Serialized Fields
            VictoryUI victoryUI = overlayGO.GetComponent<VictoryUI>(); if (victoryUI == null) victoryUI = overlayGO.AddComponent<VictoryUI>();
            SerializedObject so = new SerializedObject(victoryUI);
            var propCG = so.FindProperty("canvasGroup");
            var propTitle = so.FindProperty("titleText");
            var propSub = so.FindProperty("subtitleText");
            var propRestart = so.FindProperty("restartButton");
            var propQuit = so.FindProperty("quitButton");

            if (propCG != null) propCG.objectReferenceValue = cg;
            if (propTitle != null) propTitle.objectReferenceValue = titleTMP;
            if (propSub != null) propSub.objectReferenceValue = subtitleTMP;
            if (propRestart != null) propRestart.objectReferenceValue = restartBtn;
            if (propQuit != null) propQuit.objectReferenceValue = quitBtn;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(victoryUI);
            EditorUtility.SetDirty(overlayGO);

            // 7. Simpan Scene
            EditorSceneManager.MarkSceneDirty(overlayGO.scene);
            EditorSceneManager.SaveScene(overlayGO.scene);

            Debug.Log("<color=#FFE082>[VictorySafeSetup] Sukses! Victory / Ending Screen (The First Dawn) terpasang di HUD_Canvas.</color>");
        }

        private static TextMeshProUGUI GetOrCreateTMP(
            GameObject parent,
            string name,
            Vector2 pos,
            Vector2 size,
            float fontSize,
            FontStyles fontStyle,
            Color color,
            TextAlignmentOptions alignment,
            string text,
            TMP_FontAsset font)
        {
            Transform child = parent.transform.Find(name);
            GameObject go;
            if (child == null)
            {
                go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(parent.transform, false);
            }
            else
            {
                go = child.gameObject;
            }

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            rt.localScale = Vector3.one;

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>(); if (tmp == null) tmp = go.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = fontStyle;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            tmp.enableWordWrapping = true;

            EditorUtility.SetDirty(tmp);
            EditorUtility.SetDirty(go);
            return tmp;
        }

        private static Button GetOrCreateButton(
            GameObject parent,
            string name,
            Vector2 pos,
            Vector2 size,
            string labelText,
            Color textColor,
            TMP_FontAsset font)
        {
            Transform child = parent.transform.Find(name);
            GameObject btnGO;
            if (child == null)
            {
                btnGO = new GameObject(name, typeof(RectTransform));
                btnGO.transform.SetParent(parent.transform, false);
            }
            else
            {
                btnGO = child.gameObject;
            }

            RectTransform rt = btnGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            rt.localScale = Vector3.one;

            Image btnImg = btnGO.GetComponent<Image>(); if (btnImg == null) btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color32(30, 41, 59, 255); // #1E293B Slate dark
            btnImg.raycastTarget = true;

            Button btn = btnGO.GetComponent<Button>(); if (btn == null) btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            ColorBlock cb = btn.colors;
            cb.normalColor = new Color32(30, 41, 59, 255);
            cb.highlightedColor = new Color32(51, 65, 85, 255); // #334155
            cb.pressedColor = new Color32(15, 23, 42, 255);     // #0F172A
            cb.selectedColor = new Color32(30, 41, 59, 255);
            cb.fadeDuration = 0.1f;
            btn.colors = cb;

            // Child Text
            Transform lblTransform = btnGO.transform.Find("Text");
            GameObject lblGO;
            if (lblTransform == null)
            {
                lblGO = new GameObject("Text", typeof(RectTransform));
                lblGO.transform.SetParent(btnGO.transform, false);
            }
            else
            {
                lblGO = lblTransform.gameObject;
            }

            RectTransform lblRT = lblGO.GetComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero;
            lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = Vector2.zero;
            lblRT.offsetMax = Vector2.zero;
            lblRT.pivot = new Vector2(0.5f, 0.5f);
            lblRT.localScale = Vector3.one;

            TextMeshProUGUI lblTMP = lblGO.GetComponent<TextMeshProUGUI>(); if (lblTMP == null) lblTMP = lblGO.AddComponent<TextMeshProUGUI>();
            if (font != null) lblTMP.font = font;
            lblTMP.text = labelText;
            lblTMP.fontSize = 18f;
            lblTMP.fontStyle = FontStyles.Bold;
            lblTMP.color = textColor;
            lblTMP.alignment = TextAlignmentOptions.Center;
            lblTMP.raycastTarget = false;

            EditorUtility.SetDirty(lblTMP);
            EditorUtility.SetDirty(lblGO);
            EditorUtility.SetDirty(btn);
            EditorUtility.SetDirty(btnGO);

            return btn;
        }
    }
}
#endif
