using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace MoonlitHomestead.Editor
{
    public static class InteractionPromptSafeSetup
    {
        [MenuItem("Moonlit Homestead/Add Interaction Prompt (Safe)")]
        public static void AddInteractionPromptSafe()
        {
            // 1. Cek apakah Interaction_Prompt sudah ada di scene
            GameObject existingPrompt = GameObject.Find("Interaction_Prompt");
            if (existingPrompt != null)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Prompt Sudah Ada",
                    "GameObject 'Interaction_Prompt' sudah ada di scene saat ini.\n\nApakah kamu ingin menghapus yang lama dan membuat ulang secara aman?",
                    "Buat Ulang (Aman)",
                    "Batalkan"
                );

                if (!overwrite)
                {
                    Debug.Log("[InteractionPromptSafeSetup] Dibatalkan oleh user. Objek yang ada tidak diubah.");
                    return;
                }

                Undo.DestroyObjectImmediate(existingPrompt);
            }

            // 2. Load Sprite Aset Interaction
            Sprite plaqueSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Interaction/prompt_banner_plaque.png");
            Sprite keyBadgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Interaction/prompt_key_badge_round.png");
            Sprite arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Interaction/prompt_pointer_arrow.png");
            Sprite iconTill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Interaction/icon_action_till.png");
            Sprite iconPlant = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Interaction/icon_action_plant.png");
            Sprite iconWater = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Interaction/icon_action_water.png");
            Sprite iconHarvest = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Interaction/icon_action_harvest.png");
            Sprite iconInspect = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Interaction/icon_action_inspect.png");

            // 3. Buat GameObject Root: Interaction_Prompt (Canvas World Space)
            GameObject promptGO = new GameObject("Interaction_Prompt");
            Undo.RegisterCreatedObjectUndo(promptGO, "Create Interaction_Prompt");

            Canvas canvas = promptGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = 100; // Selalu di atas objek gameplay

            CanvasScaler scaler = promptGO.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100f;

            promptGO.AddComponent<GraphicRaycaster>();

            RectTransform rootRect = promptGO.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(240f, 90f);
            rootRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            rootRect.position = new Vector3(0f, 1000f, 0f); // Default jauh sebelum disorot

            InteractionPromptUI promptUI = promptGO.AddComponent<InteractionPromptUI>();

            // 4. Buat Container (CanvasGroup)
            GameObject containerGO = new GameObject("Container");
            containerGO.transform.SetParent(promptGO.transform, false);

            RectTransform containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(240f, 90f);
            containerRect.anchoredPosition = Vector2.zero;

            CanvasGroup canvasGroup = containerGO.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f; // Default sembunyi
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // Child 4.1: PlaqueBackground
            GameObject plaqueGO = new GameObject("PlaqueBackground");
            plaqueGO.transform.SetParent(containerGO.transform, false);
            RectTransform plaqueRect = plaqueGO.AddComponent<RectTransform>();
            plaqueRect.anchorMin = new Vector2(0.5f, 0.5f);
            plaqueRect.anchorMax = new Vector2(0.5f, 0.5f);
            plaqueRect.pivot = new Vector2(0.5f, 0.5f);
            plaqueRect.sizeDelta = new Vector2(220f, 75f);
            plaqueRect.anchoredPosition = Vector2.zero;

            Image plaqueImg = plaqueGO.AddComponent<Image>();
            if (plaqueSprite != null) plaqueImg.sprite = plaqueSprite;
            plaqueImg.preserveAspect = true;
            plaqueImg.raycastTarget = false;

            // Child 4.2: KeyBadge (Kiri)
            GameObject keyBadgeGO = new GameObject("KeyBadge");
            keyBadgeGO.transform.SetParent(containerGO.transform, false);
            RectTransform keyBadgeRect = keyBadgeGO.AddComponent<RectTransform>();
            keyBadgeRect.anchorMin = new Vector2(0.5f, 0.5f);
            keyBadgeRect.anchorMax = new Vector2(0.5f, 0.5f);
            keyBadgeRect.pivot = new Vector2(0.5f, 0.5f);
            keyBadgeRect.sizeDelta = new Vector2(46f, 46f);
            keyBadgeRect.anchoredPosition = new Vector2(-68f, 2f);

            Image keyBadgeImg = keyBadgeGO.AddComponent<Image>();
            if (keyBadgeSprite != null) keyBadgeImg.sprite = keyBadgeSprite;
            keyBadgeImg.preserveAspect = true;
            keyBadgeImg.raycastTarget = false;

            // Child 4.2.1: KeyText di dalam KeyBadge
            GameObject keyTextGO = new GameObject("KeyText");
            keyTextGO.transform.SetParent(keyBadgeGO.transform, false);
            RectTransform keyTextRect = keyTextGO.AddComponent<RectTransform>();
            keyTextRect.anchorMin = Vector2.zero;
            keyTextRect.anchorMax = Vector2.one;
            keyTextRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI keyText = keyTextGO.AddComponent<TextMeshProUGUI>();
            keyText.text = "E";
            keyText.fontSize = 22f;
            keyText.fontStyle = FontStyles.Bold;
            keyText.alignment = TextAlignmentOptions.Center;
            keyText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            keyText.raycastTarget = false;

            // Child 4.3: ActionIcon (Tengah-Kiri)
            GameObject iconGO = new GameObject("ActionIcon");
            iconGO.transform.SetParent(containerGO.transform, false);
            RectTransform iconRect = iconGO.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(42f, 42f);
            iconRect.anchoredPosition = new Vector2(-18f, 2f);

            Image iconImg = iconGO.AddComponent<Image>();
            if (iconTill != null) iconImg.sprite = iconTill;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Child 4.4: ActionLabel (Kanan)
            GameObject labelGO = new GameObject("ActionLabel");
            labelGO.transform.SetParent(containerGO.transform, false);
            RectTransform labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.sizeDelta = new Vector2(105f, 30f);
            labelRect.anchoredPosition = new Vector2(10f, 2f);

            TextMeshProUGUI labelTxt = labelGO.AddComponent<TextMeshProUGUI>();
            labelTxt.text = "Cangkul";
            labelTxt.fontSize = 20f;
            labelTxt.fontStyle = FontStyles.Bold;
            labelTxt.alignment = TextAlignmentOptions.MidlineLeft;
            labelTxt.color = new Color(0.95f, 0.98f, 1f, 1f);
            labelTxt.raycastTarget = false;

            // Child 4.5: PointerArrow (Panah bawah aksen gotik opsional)
            Image arrowImg = null;
            if (arrowSprite != null)
            {
                GameObject arrowGO = new GameObject("PointerArrow");
                arrowGO.transform.SetParent(containerGO.transform, false);
                RectTransform arrowRect = arrowGO.AddComponent<RectTransform>();
                arrowRect.anchorMin = new Vector2(0.5f, 0f);
                arrowRect.anchorMax = new Vector2(0.5f, 0f);
                arrowRect.pivot = new Vector2(0.5f, 1f);
                arrowRect.sizeDelta = new Vector2(14f, 26f);
                arrowRect.anchoredPosition = new Vector2(0f, -4f);

                arrowImg = arrowGO.AddComponent<Image>();
                arrowImg.sprite = arrowSprite;
                arrowImg.preserveAspect = true;
                arrowImg.raycastTarget = false;
            }

            // 5. Wire referensi ke InteractionPromptUI
            SerializedObject promptSerialized = new SerializedObject(promptUI);
            promptSerialized.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            promptSerialized.FindProperty("plaqueBackground").objectReferenceValue = plaqueImg;
            promptSerialized.FindProperty("keyBadgeImage").objectReferenceValue = keyBadgeImg;
            promptSerialized.FindProperty("keyText").objectReferenceValue = keyText;
            promptSerialized.FindProperty("actionIconImage").objectReferenceValue = iconImg;
            promptSerialized.FindProperty("actionText").objectReferenceValue = labelTxt;
            if (arrowImg != null)
            {
                promptSerialized.FindProperty("pointerArrowImage").objectReferenceValue = arrowImg;
            }
            promptSerialized.ApplyModifiedPropertiesWithoutUndo();

            // 6. Sambungkan ke PlayerInteraction di scene secara aman
            PlayerInteraction playerInteraction = Object.FindAnyObjectByType<PlayerInteraction>();
            if (playerInteraction != null)
            {
                SerializedObject playerSerialized = new SerializedObject(playerInteraction);
                playerSerialized.FindProperty("interactionPrompt").objectReferenceValue = promptUI;
                playerSerialized.FindProperty("positionMode").enumValueIndex = (int)PromptPositionMode.AbovePlayerHead;
                playerSerialized.FindProperty("playerHeadOffset").vector3Value = new Vector3(0f, 1.35f, 0f);
                playerSerialized.FindProperty("targetOffset").vector3Value = new Vector3(0f, 0.8f, 0f);
                if (iconTill != null) playerSerialized.FindProperty("iconTill").objectReferenceValue = iconTill;
                if (iconPlant != null) playerSerialized.FindProperty("iconPlant").objectReferenceValue = iconPlant;
                if (iconWater != null) playerSerialized.FindProperty("iconWater").objectReferenceValue = iconWater;
                if (iconHarvest != null) playerSerialized.FindProperty("iconHarvest").objectReferenceValue = iconHarvest;
                if (iconInspect != null) playerSerialized.FindProperty("iconInspect").objectReferenceValue = iconInspect;
                playerSerialized.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[InteractionPromptSafeSetup] Berhasil menghubungkan InteractionPromptUI & icon aksi ke PlayerInteraction!");
            }

            // 7. Simpan Prefab ke Assets/Prefabs/InteractionPromptPrefab.prefab
            string prefabPath = "Assets/Prefabs/InteractionPromptPrefab.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(promptGO, prefabPath, InteractionMode.UserAction);

            // 8. Tandai scene dirty agar perubahan tersimpan
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("<color=#70FFE5>[InteractionPromptSafeSetup] Berhasil membuat World-Space Interaction Prompt secara non-destructive!</color>");
            EditorUtility.DisplayDialog(
                "Interaction Prompt Sukses",
                "World-Space Interaction Prompt ('Interaction_Prompt') berhasil dibuat dan terhubung ke PlayerInteraction tanpa mengganggu objek scene lainnya!",
                "Mantap"
            );
        }
    }
}
