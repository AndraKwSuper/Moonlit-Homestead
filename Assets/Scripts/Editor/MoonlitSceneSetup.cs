#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MoonlitHomestead.Editor
{
    public static class MoonlitSceneSetup
    {
        [MenuItem("Moonlit Homestead/Setup Prototype Scene (2D Side-Scroller)", false, 10)]
        public static void SetupScene()
        {
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Setup Moonlit Side-Scroller Prototype Scene");

            // 1. Load Prototype Sprite
            Sprite boxSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/prototype_square.png");
            if (boxSprite == null)
            {
                boxSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 2. Setup Camera
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camGO = new GameObject("Main Camera");
                cam = camGO.AddComponent<Camera>();
                camGO.AddComponent<AudioListener>();
                camGO.tag = "MainCamera";
            }
            cam.transform.position = new Vector3(0f, 1.2f, -10f);
            cam.orthographic = true;
            cam.orthographicSize = 4.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.12f); // Dark cosmic twilight

            // 3. Setup GameManager
            GameObject gmGO = GameObject.Find("GameManager");
            if (gmGO == null) gmGO = new GameObject("GameManager");
            gmGO.transform.position = Vector3.zero;

            GetOrAdd<TimeLoopManager>(gmGO);
            GetOrAdd<RealityStabilitySystem>(gmGO);
            GetOrAdd<InventoryHotbar>(gmGO);
            GetOrAdd<LoopBreakerManager>(gmGO);

            // 4. Background Environment (Eerie Moon & Homestead Cabin Silhouette)
            GameObject envParent = GameObject.Find("Environment_Decor");
            if (envParent == null) envParent = new GameObject("Environment_Decor");
            envParent.transform.position = Vector3.zero;

            // Eerie Moon in the background
            GameObject moonGO = GetOrCreateChild(envParent, "EerieMoon");
            moonGO.transform.position = new Vector3(5.5f, 4.2f, 5f);
            moonGO.transform.localScale = new Vector3(2.6f, 2.6f, 1f);
            var moonSR = GetOrAdd<SpriteRenderer>(moonGO);
            moonSR.sprite = boxSprite;
            moonSR.color = new Color(0.85f, 0.80f, 0.95f, 0.85f); // Pale lunar glow

            // Cabin Silhouette (Safe Zone at left)
            GameObject cabinGO = GetOrCreateChild(envParent, "Cabin_Silhouette");
            cabinGO.transform.position = new Vector3(-7f, 0.3f, 2f);
            cabinGO.transform.localScale = new Vector3(4f, 3.2f, 1f);
            var cabinSR = GetOrAdd<SpriteRenderer>(cabinGO);
            cabinSR.sprite = boxSprite;
            cabinSR.color = new Color(0.08f, 0.08f, 0.13f);

            // Warm Window Lamp
            GameObject windowGO = GetOrCreateChild(cabinGO, "Lantern_Window");
            windowGO.transform.localPosition = new Vector3(0.2f, 0f, -0.1f);
            windowGO.transform.localScale = new Vector3(0.3f, 0.35f, 1f);
            var windowSR = GetOrAdd<SpriteRenderer>(windowGO);
            windowSR.sprite = boxSprite;
            windowSR.color = new Color(1f, 0.65f, 0.25f, 0.95f); // Cozy amber lantern glow

            // 5. Solid Ground Platform
            GameObject groundParent = GameObject.Find("Ground");
            if (groundParent == null) groundParent = new GameObject("Ground");
            groundParent.transform.position = Vector3.zero;

            GameObject groundMain = GetOrCreateChild(groundParent, "Ground_Main");
            groundMain.transform.position = new Vector3(0f, -1.6f, 0f);
            groundMain.transform.localScale = new Vector3(28f, 1.4f, 1f);
            var groundSR = GetOrAdd<SpriteRenderer>(groundMain);
            groundSR.sprite = boxSprite;
            groundSR.color = new Color(0.12f, 0.11f, 0.15f);

            var groundCol = GetOrAdd<BoxCollider2D>(groundMain);
            groundCol.size = Vector2.one;
            groundCol.isTrigger = false;

            // Moss / Soil Trim
            GameObject mossTrim = GetOrCreateChild(groundParent, "Moss_Trim");
            mossTrim.transform.position = new Vector3(0f, -0.85f, 0f);
            mossTrim.transform.localScale = new Vector3(28f, 0.1f, 1f);
            var mossSR = GetOrAdd<SpriteRenderer>(mossTrim);
            mossSR.sprite = boxSprite;
            mossSR.color = new Color(0.22f, 0.36f, 0.28f);

            // 6. Farm Plots (Sitting on top of the ground)
            GameObject plotsParent = GameObject.Find("FarmPlots");
            if (plotsParent == null) plotsParent = new GameObject("FarmPlots");
            plotsParent.transform.position = Vector3.zero;

            Vector3[] plotPositions = new Vector3[]
            {
                new Vector3(-2.8f, -0.7f, 0f),
                new Vector3(0f, -0.7f, 0f),
                new Vector3(2.8f, -0.7f, 0f)
            };

            for (int i = 0; i < plotPositions.Length; i++)
            {
                string plotName = $"FarmPlot_{i + 1}";
                Transform existing = plotsParent.transform.Find(plotName);
                GameObject plotGO = existing != null ? existing.gameObject : new GameObject(plotName);
                plotGO.transform.SetParent(plotsParent.transform);
                plotGO.transform.position = plotPositions[i];
                plotGO.transform.localScale = new Vector3(1.6f, 0.4f, 1f);

                var plotSR = GetOrAdd<SpriteRenderer>(plotGO);
                plotSR.sprite = boxSprite;

                var boxCol = GetOrAdd<BoxCollider2D>(plotGO);
                boxCol.size = new Vector2(1f, 1.8f); // Taller trigger so player walking beside/over it triggers detection
                boxCol.offset = new Vector2(0f, 0.4f);
                boxCol.isTrigger = true;

                GetOrAdd<FarmPlot>(plotGO);
            }

            // 7. Setup Player (Side-Scroller Platformer)
            GameObject playerGO = GameObject.Find("Player");
            if (playerGO == null) playerGO = new GameObject("Player");
            playerGO.transform.position = new Vector3(-4.5f, 0.2f, 0f);
            playerGO.transform.localScale = new Vector3(0.85f, 1.35f, 1f);

            var playerSR = GetOrAdd<SpriteRenderer>(playerGO);
            playerSR.sprite = boxSprite;
            playerSR.color = new Color(0.3f, 0.85f, 0.8f); // Teal

            var rb = GetOrAdd<Rigidbody2D>(playerGO);
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = GetOrAdd<BoxCollider2D>(playerGO);
            col.size = new Vector2(0.85f, 1.0f);
            col.offset = Vector2.zero;

            var controller = GetOrAdd<PlayerController2D>(playerGO);
            var interaction = GetOrAdd<PlayerInteraction>(playerGO);

            // 8. Setup Canvas UI
            GameObject canvasGO = GameObject.Find("HUD_Canvas");
            if (canvasGO == null) canvasGO = new GameObject("HUD_Canvas");

            Canvas canvas = GetOrAdd<Canvas>(canvasGO);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = GetOrAdd<CanvasScaler>(canvasGO);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            GetOrAdd<GraphicRaycaster>(canvasGO);
            GameHUD gameHUD = GetOrAdd<GameHUD>(canvasGO);

            // EventSystem
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
            }

            // UI Elements: Clock & Loop
            GameObject topPanelGO = GetOrCreateChild(canvasGO, "TopHeaderPanel");
            RectTransform topRect = GetOrAdd<RectTransform>(topPanelGO);
            topRect.anchorMin = new Vector2(0f, 1f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.anchoredPosition = new Vector2(0f, -20f);
            topRect.sizeDelta = new Vector2(0f, 70f);

            // Clock Text
            GameObject clockGO = GetOrCreateChild(topPanelGO, "ClockText");
            Text clockTxt = GetOrAdd<Text>(clockGO);
            clockTxt.font = defaultFont;
            clockTxt.fontSize = 34;
            clockTxt.fontStyle = FontStyle.Bold;
            clockTxt.alignment = TextAnchor.MiddleCenter;
            clockTxt.color = Color.white;
            clockTxt.text = "18:00";
            RectTransform clockRect = GetOrAdd<RectTransform>(clockGO);
            clockRect.anchorMin = new Vector2(0.5f, 0.5f);
            clockRect.anchorMax = new Vector2(0.5f, 0.5f);
            clockRect.pivot = new Vector2(0.5f, 0.5f);
            clockRect.anchoredPosition = new Vector2(-70f, 0f);
            clockRect.sizeDelta = new Vector2(160f, 50f);

            // Loop Text
            GameObject loopGO = GetOrCreateChild(topPanelGO, "LoopText");
            Text loopTxt = GetOrAdd<Text>(loopGO);
            loopTxt.font = defaultFont;
            loopTxt.fontSize = 24;
            loopTxt.fontStyle = FontStyle.Bold;
            loopTxt.alignment = TextAnchor.MiddleCenter;
            loopTxt.color = new Color(0.9f, 0.75f, 0.3f); // Gold
            loopTxt.text = "Loop 00";
            RectTransform loopRect = GetOrAdd<RectTransform>(loopGO);
            loopRect.anchorMin = new Vector2(0.5f, 0.5f);
            loopRect.anchorMax = new Vector2(0.5f, 0.5f);
            loopRect.pivot = new Vector2(0.5f, 0.5f);
            loopRect.anchoredPosition = new Vector2(80f, 0f);
            loopRect.sizeDelta = new Vector2(160f, 50f);

            // Stability Bar (Slider)
            GameObject sliderGO = GetOrCreateChild(topPanelGO, "StabilitySlider");
            RectTransform sliderRect = GetOrAdd<RectTransform>(sliderGO);
            sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
            sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = new Vector2(0f, -40f);
            sliderRect.sizeDelta = new Vector2(360f, 18f);

            Slider slider = GetOrAdd<Slider>(sliderGO);

            // Slider Background
            GameObject bgGO = GetOrCreateChild(sliderGO, "Background");
            RectTransform bgRect = GetOrAdd<RectTransform>(bgGO);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImg = GetOrAdd<Image>(bgGO);
            bgImg.color = new Color(0.12f, 0.12f, 0.18f, 0.9f);

            // Slider Fill Area & Fill
            GameObject fillAreaGO = GetOrCreateChild(sliderGO, "Fill Area");
            RectTransform fillAreaRect = GetOrAdd<RectTransform>(fillAreaGO);
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = Vector2.zero;

            GameObject fillGO = GetOrCreateChild(fillAreaGO, "Fill");
            RectTransform fillRect = GetOrAdd<RectTransform>(fillGO);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            Image fillImg = GetOrAdd<Image>(fillGO);
            fillImg.color = new Color(0.2f, 0.8f, 0.6f);

            slider.fillRect = fillRect;
            slider.targetGraphic = fillImg;
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = 100f;

            // Hotbar Tool Display Text
            GameObject itemFeedbackGO = GetOrCreateChild(canvasGO, "HotbarText");
            Text itemFeedbackTxt = GetOrAdd<Text>(itemFeedbackGO);
            itemFeedbackTxt.font = defaultFont;
            itemFeedbackTxt.fontSize = 22;
            itemFeedbackTxt.alignment = TextAnchor.MiddleCenter;
            itemFeedbackTxt.color = new Color(0.8f, 0.9f, 1f);
            itemFeedbackTxt.text = "[Slot 1] Cangkul (Hoe)";
            RectTransform itemRect = GetOrAdd<RectTransform>(itemFeedbackGO);
            itemRect.anchorMin = new Vector2(0.5f, 0f);
            itemRect.anchorMax = new Vector2(0.5f, 0f);
            itemRect.pivot = new Vector2(0.5f, 0f);
            itemRect.anchoredPosition = new Vector2(0f, 40f);
            itemRect.sizeDelta = new Vector2(500f, 40f);

            // Dialogue / Notification Panel
            GameObject diagPanelGO = GetOrCreateChild(canvasGO, "DialoguePanel");
            RectTransform diagRect = GetOrAdd<RectTransform>(diagPanelGO);
            diagRect.anchorMin = new Vector2(0.5f, 0f);
            diagRect.anchorMax = new Vector2(0.5f, 0f);
            diagRect.pivot = new Vector2(0.5f, 0f);
            diagRect.anchoredPosition = new Vector2(0f, 100f);
            diagRect.sizeDelta = new Vector2(760f, 70f);

            Image diagBg = GetOrAdd<Image>(diagPanelGO);
            diagBg.color = new Color(0.06f, 0.08f, 0.12f, 0.92f);

            GameObject diagTextGO = GetOrCreateChild(diagPanelGO, "DialogueText");
            Text diagTxt = GetOrAdd<Text>(diagTextGO);
            diagTxt.font = defaultFont;
            diagTxt.fontSize = 20;
            diagTxt.alignment = TextAnchor.MiddleCenter;
            diagTxt.color = new Color(0.95f, 0.95f, 0.95f);
            diagTxt.text = "A/D: Jalan | Space: Lompat | 1: Cangkul | 2: Bibit | E: Interaksi Tanah";
            RectTransform diagTextRect = GetOrAdd<RectTransform>(diagTextGO);
            diagTextRect.anchorMin = Vector2.zero;
            diagTextRect.anchorMax = Vector2.one;
            diagTextRect.sizeDelta = new Vector2(-20f, -10f);

            // Wire Serialized Properties on GameHUD via SerializedObject
            SerializedObject hudSerialized = new SerializedObject(gameHUD);
            hudSerialized.FindProperty("clockText").objectReferenceValue = clockTxt;
            hudSerialized.FindProperty("loopText").objectReferenceValue = loopTxt;
            hudSerialized.FindProperty("stabilitySlider").objectReferenceValue = slider;
            hudSerialized.FindProperty("stabilityFill").objectReferenceValue = fillImg;
            hudSerialized.FindProperty("dialoguePanel").objectReferenceValue = diagPanelGO;
            hudSerialized.FindProperty("dialogueText").objectReferenceValue = diagTxt;
            hudSerialized.FindProperty("itemText").objectReferenceValue = itemFeedbackTxt;
            hudSerialized.ApplyModifiedProperties();

            // Wire references on Player
            SerializedObject playerSerialized = new SerializedObject(controller);
            playerSerialized.FindProperty("spriteRenderer").objectReferenceValue = playerSR;
            playerSerialized.ApplyModifiedProperties();

            SerializedObject interactionSerialized = new SerializedObject(interaction);
            interactionSerialized.FindProperty("interactableLayer").intValue = ~0;
            interactionSerialized.ApplyModifiedProperties();

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog(
                "Moonlit Homestead (2D Side-Scroller)",
                "Setup Side-Scroller Prototype Selesai!\n\n" +
                "- Bergerak: A / D atau Panah Kiri/Kanan\n" +
                "- Lompat: Space / W\n" +
                "- Pilih Alat: Angka 1 (Cangkul), 2 (Bibit)\n" +
                "- Interaksi Tanah: Berdiri di dekat petak tanah & tekan E\n" +
                "- Suasana: Latar belakang bulan eerie & kabin di sebelah kiri.\n\n" +
                "Silakan tekan Play di Unity Editor.",
                "Gas Coba!"
            );
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (comp == null)
            {
                comp = go.AddComponent<T>();
            }
            return comp;
        }

        private static GameObject GetOrCreateChild(GameObject parent, string name)
        {
            Transform existing = parent.transform.Find(name);
            if (existing != null) return existing.gameObject;
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            return child;
        }
    }
}
#endif
