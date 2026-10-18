#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace MoonlitHomestead.Editor
{
    public static class LoopTransitionSafeSetup
    {
        private const string OverlayGameObjectName = "Loop_Transition_Overlay";

        [InitializeOnLoadMethod]
        private static void AutoRunOnceOnCompile()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool("LoopTransitionSetupV1Run", false))
                {
                    SessionState.SetBool("LoopTransitionSetupV1Run", true);
                    SetupLoopTransitionSafe();
                }
            };
        }

        [MenuItem("Moonlit Homestead/Setup Loop Transition FX (Safe)")]
        public static void SetupLoopTransitionSafe()
        {
            // 1. Cari HUD_Canvas di Active Scene
            GameObject hudCanvas = GameObject.Find("HUD_Canvas");
            if (hudCanvas == null)
            {
                Canvas anyCanvas = Object.FindAnyObjectByType<Canvas>();
                if (anyCanvas != null) hudCanvas = anyCanvas.gameObject;
            }

            if (hudCanvas == null)
            {
                Debug.LogError("[LoopTransitionSafeSetup] HUD_Canvas tidak ditemukan di scene saat ini!");
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Setup Loop Transition FX (Safe)");
            int undoGroup = Undo.GetCurrentGroup();

            // 2. Buat / Ambil child GameObject Loop_Transition_Overlay
            Transform existingOverlay = hudCanvas.transform.Find(OverlayGameObjectName);
            GameObject overlayGO;
            if (existingOverlay == null)
            {
                overlayGO = new GameObject(OverlayGameObjectName, typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(overlayGO, "Create " + OverlayGameObjectName);
                overlayGO.transform.SetParent(hudCanvas.transform, false);
            }
            else
            {
                overlayGO = existingOverlay.gameObject;
            }

            Undo.RecordObject(overlayGO, "Configure " + OverlayGameObjectName);
            overlayGO.SetActive(true);
            overlayGO.transform.SetAsLastSibling(); // Pastikan render di lapisan terdepan HUD

            // RectTransform: Stretch Full (Anchor Min 0,0 | Anchor Max 1,1 | Offset 0,0,0,0)
            RectTransform overlayRT = overlayGO.GetComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;
            overlayRT.pivot = new Vector2(0.5f, 0.5f);
            overlayRT.localScale = Vector3.one;

            // Image: Color #0A0714 (Ungu-Hitam Kosmik pekat), RaycastTarget = false
            Image overlayImg = overlayGO.GetComponent<Image>();
            if (overlayImg == null)
            {
                overlayImg = Undo.AddComponent<Image>(overlayGO);
            }
            Undo.RecordObject(overlayImg, "Configure Overlay Image");
            overlayImg.color = new Color32(10, 7, 20, 255); // #0A0714
            overlayImg.raycastTarget = false;
            EditorUtility.SetDirty(overlayImg);

            // CanvasGroup: Alpha = 0, BlocksRaycasts = false
            CanvasGroup cg = overlayGO.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = Undo.AddComponent<CanvasGroup>(overlayGO);
            }
            Undo.RecordObject(cg, "Configure Overlay CanvasGroup");
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
            EditorUtility.SetDirty(cg);

            // 3. Buat 3 child TextMeshPro di dalamnya
            // Child 1: Subtitle ("REALITAS RUNTUH...", Y = +40, FontSize = 22, Color = #B088FF, Alignment = Center)
            TextMeshProUGUI subtitleTMP = GetOrCreateTMPChild(
                overlayGO,
                "Subtitle",
                new Vector2(0f, 40f),
                new Vector2(800f, 50f),
                22f,
                FontStyles.Normal,
                new Color32(176, 136, 255, 255), // #B088FF Ungu Pudar
                TextAlignmentOptions.Center,
                "REALITAS RUNTUH..."
            );

            // Child 2: LoopTitle ("LOOP 02", Y = -10, FontSize = 64, FontStyle = Bold, Color = #70FFE5, Alignment = Center)
            TextMeshProUGUI loopTitleTMP = GetOrCreateTMPChild(
                overlayGO,
                "LoopTitle",
                new Vector2(0f, -10f),
                new Vector2(800f, 90f),
                64f,
                FontStyles.Bold,
                new Color32(112, 255, 229, 255), // #70FFE5 Cyan Kosmik
                TextAlignmentOptions.Center,
                "LOOP 02"
            );

            // Child 3: LoreQuote (Y = -70, FontSize = 16, FontStyle = Italic, Color = #C0C0C0, Alignment = Center)
            TextMeshProUGUI loreQuoteTMP = GetOrCreateTMPChild(
                overlayGO,
                "LoreQuote",
                new Vector2(0f, -70f),
                new Vector2(900f, 60f),
                16f,
                FontStyles.Italic,
                new Color32(192, 192, 192, 255), // #C0C0C0
                TextAlignmentOptions.Center,
                "'Waktu berputar kembali ke senja... Namun tanah tetap mengingat.'"
            );

            // 4. Pasang Komponen LoopTransitionUI & Hubungkan Serialized Fields
            LoopTransitionUI transitionUI = overlayGO.GetComponent<LoopTransitionUI>();
            if (transitionUI == null)
            {
                transitionUI = Undo.AddComponent<LoopTransitionUI>(overlayGO);
            }

            Undo.RecordObject(transitionUI, "Configure LoopTransitionUI References");
            SerializedObject so = new SerializedObject(transitionUI);
            var propCG = so.FindProperty("canvasGroup");
            var propImg = so.FindProperty("overlayImage");
            var propSub = so.FindProperty("subtitleText");
            var propLoop = so.FindProperty("loopNumberText");
            var propLore = so.FindProperty("loreQuoteText");

            if (propCG != null) propCG.objectReferenceValue = cg;
            if (propImg != null) propImg.objectReferenceValue = overlayImg;
            if (propSub != null) propSub.objectReferenceValue = subtitleTMP;
            if (propLoop != null) propLoop.objectReferenceValue = loopTitleTMP;
            if (propLore != null) propLore.objectReferenceValue = loreQuoteTMP;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(transitionUI);
            EditorUtility.SetDirty(overlayGO);

            // 5. Simpan Perubahan Scene
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Undo.CollapseUndoOperations(undoGroup);
            Debug.Log("<color=#70FFE5>[LoopTransitionSafeSetup] Berhasil memasang Midnight Collapse & Loop Reset Overlay di HUD_Canvas!</color>");
        }

        private static TextMeshProUGUI GetOrCreateTMPChild(
            GameObject parent,
            string childName,
            Vector2 anchoredPos,
            Vector2 sizeDelta,
            float fontSize,
            FontStyles fontStyle,
            Color color,
            TextAlignmentOptions alignment,
            string defaultText)
        {
            Transform child = parent.transform.Find(childName);
            GameObject go;
            if (child == null)
            {
                go = new GameObject(childName, typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(go, "Create " + childName);
                go.transform.SetParent(parent.transform, false);
            }
            else
            {
                go = child.gameObject;
            }

            Undo.RecordObject(go, "Configure " + childName);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            rt.localScale = Vector3.one;

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp == null)
            {
                tmp = Undo.AddComponent<TextMeshProUGUI>(go);
            }

            Undo.RecordObject(tmp, "Configure " + childName + " TMP");
            tmp.text = defaultText;
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
    }
}
#endif
