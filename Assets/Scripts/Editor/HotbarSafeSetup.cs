using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace MoonlitHomestead.Editor
{
    public static class HotbarSafeSetup
    {
        [MenuItem("Moonlit Homestead/Add Hotbar UI (Safe)")]
        public static void AddHotbarUISafe()
        {
            // 1. Cari HUD_Canvas di active scene
            GameObject hudCanvas = GameObject.Find("HUD_Canvas");
            if (hudCanvas == null)
            {
                // Fallback cari Canvas apa pun di scene
                Canvas anyCanvas = Object.FindAnyObjectByType<Canvas>();
                if (anyCanvas != null)
                {
                    hudCanvas = anyCanvas.gameObject;
                }
            }

            if (hudCanvas == null)
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    "GameObject 'HUD_Canvas' tidak ditemukan di scene saat ini! Buka scene yang memiliki HUD_Canvas terlebih dahulu.",
                    "OK"
                );
                return;
            }

            // 2. Periksa apakah Hotbar_Container sudah ada
            Transform existingHotbar = hudCanvas.transform.Find("Hotbar_Container");
            if (existingHotbar != null)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Hotbar Sudah Ada",
                    "GameObject 'Hotbar_Container' sudah ada di dalam HUD_Canvas.\n\nApakah kamu ingin menghapus yang lama dan membuat ulang secara aman, atau batalkan agar tidak mengubah apapun?",
                    "Buat Ulang (Aman)",
                    "Batalkan"
                );

                if (!overwrite)
                {
                    Debug.Log("[HotbarSafeSetup] Dibatalkan oleh user. Objek yang ada tidak diubah.");
                    return;
                }

                Undo.DestroyObjectImmediate(existingHotbar.gameObject);
            }

            // Load Sprite Aset
            Sprite defaultSlotSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Hotbar/hotbar_slot_default.png");
            Sprite selectedSlotSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Hotbar/hotbar_slot_selected.png");
            Sprite[] itemSprites = LoadItemSprites();

            // 3. Buat Hotbar_Container di bawah HUD_Canvas
            GameObject hotbarContainerGO = new GameObject("Hotbar_Container");
            Undo.RegisterCreatedObjectUndo(hotbarContainerGO, "Create Hotbar_Container");
            hotbarContainerGO.transform.SetParent(hudCanvas.transform, false);

            RectTransform containerRect = hotbarContainerGO.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0f); // Bawah Tengah
            containerRect.anchorMax = new Vector2(0.5f, 0f);
            containerRect.pivot = new Vector2(0.5f, 0f);
            containerRect.anchoredPosition = new Vector2(0f, 59f);
            containerRect.sizeDelta = new Vector2(0f, 90f);

            // Pasang HorizontalLayoutGroup
            HorizontalLayoutGroup layoutGroup = hotbarContainerGO.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 35f;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            // Pasang ContentSizeFitter
            ContentSizeFitter sizeFitter = hotbarContainerGO.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Pasang HotbarUI Controller
            HotbarUI hotbarUI = hotbarContainerGO.AddComponent<HotbarUI>();
            List<HotbarSlotUI> slotUIList = new List<HotbarSlotUI>();

            // 4. Buat 5 Child Slot
            for (int i = 0; i < 5; i++)
            {
                GameObject slotGO = new GameObject($"Slot_{i}");
                slotGO.transform.SetParent(hotbarContainerGO.transform, false);

                RectTransform slotRect = slotGO.AddComponent<RectTransform>();
                slotRect.sizeDelta = new Vector2(149.49f, 149.49f);

                // Child 1: Background
                GameObject bgGO = new GameObject("Background");
                bgGO.transform.SetParent(slotGO.transform, false);
                RectTransform bgRect = bgGO.AddComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.sizeDelta = Vector2.zero;

                Image bgImg = bgGO.AddComponent<Image>();
                if (defaultSlotSprite != null) bgImg.sprite = defaultSlotSprite;
                bgImg.preserveAspect = true;
                bgImg.raycastTarget = true; // Aktifkan agar bisa diklik

                // Child 2: ItemIcon (size 100x100, centered)
                GameObject iconGO = new GameObject("ItemIcon");
                iconGO.transform.SetParent(slotGO.transform, false);
                RectTransform iconRect = iconGO.AddComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.sizeDelta = new Vector2(100f, 100f);
                iconRect.anchoredPosition = Vector2.zero;

                Image iconImg = iconGO.AddComponent<Image>();
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
                if (i < itemSprites.Length && itemSprites[i] != null)
                {
                    iconImg.sprite = itemSprites[i];
                    iconGO.SetActive(true);
                }
                else
                {
                    iconGO.SetActive(false);
                }

                // Child 3: SelectedHighlight (default aktif hanya pada slot 0)
                GameObject highlightGO = new GameObject("SelectedHighlight");
                highlightGO.transform.SetParent(slotGO.transform, false);
                RectTransform hlRect = highlightGO.AddComponent<RectTransform>();
                hlRect.anchorMin = Vector2.zero;
                hlRect.anchorMax = Vector2.one;
                hlRect.sizeDelta = Vector2.zero;

                Image hlImg = highlightGO.AddComponent<Image>();
                if (selectedSlotSprite != null) hlImg.sprite = selectedSlotSprite;
                hlImg.preserveAspect = true;
                hlImg.raycastTarget = false;
                highlightGO.SetActive(i == 0);

                // Child 4: KeybindText (Pojok kiri-atas slot)
                GameObject keybindGO = new GameObject("KeybindText");
                keybindGO.transform.SetParent(slotGO.transform, false);
                RectTransform keybindRect = keybindGO.AddComponent<RectTransform>();
                keybindRect.anchorMin = new Vector2(0f, 1f);
                keybindRect.anchorMax = new Vector2(0f, 1f);
                keybindRect.pivot = new Vector2(0f, 1f);
                keybindRect.anchoredPosition = new Vector2(-29f, -6f);
                keybindRect.sizeDelta = new Vector2(25f, 20f);

                TextMeshProUGUI keybindTxt = keybindGO.AddComponent<TextMeshProUGUI>();
                keybindTxt.fontSize = 30f;
                keybindTxt.fontStyle = FontStyles.Bold;
                keybindTxt.alignment = TextAlignmentOptions.Center;
                keybindTxt.color = new Color(0.95f, 0.95f, 0.95f, 0.9f);
                keybindTxt.text = $"{i + 1}";
                keybindTxt.raycastTarget = false;

                // Pasang HotbarSlotUI pada slotGO
                HotbarSlotUI slotUI = slotGO.AddComponent<HotbarSlotUI>();

                // Wire serialized fields on slotUI
                SerializedObject slotSerialized = new SerializedObject(slotUI);
                slotSerialized.FindProperty("backgroundImage").objectReferenceValue = bgImg;
                slotSerialized.FindProperty("itemIconImage").objectReferenceValue = iconImg;
                slotSerialized.FindProperty("selectedHighlight").objectReferenceValue = hlImg;
                slotSerialized.FindProperty("keybindText").objectReferenceValue = keybindTxt;
                slotSerialized.ApplyModifiedPropertiesWithoutUndo();

                slotUIList.Add(slotUI);
            }

            // Wire list slots ke HotbarUI
            SerializedObject hotbarSerialized = new SerializedObject(hotbarUI);
            SerializedProperty slotsProp = hotbarSerialized.FindProperty("slots");
            slotsProp.ClearArray();
            for (int i = 0; i < slotUIList.Count; i++)
            {
                slotsProp.InsertArrayElementAtIndex(i);
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slotUIList[i];
            }
            hotbarSerialized.FindProperty("slotContainer").objectReferenceValue = hotbarContainerGO.transform;
            hotbarSerialized.ApplyModifiedPropertiesWithoutUndo();

            // Setup 5 item pada InventoryHotbar di scene
            SetupInventoryHotbarItems(itemSprites);

            // Mark Scene Dirty agar perubahan tersimpan
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("<color=#70FFE5>[HotbarSafeSetup] Berhasil menambahkan Hotbar_Container dan item ke HUD_Canvas secara non-destructive!</color>");
            EditorUtility.DisplayDialog(
                "Hotbar UI Sukses",
                "Visual UI Hotbar ('Hotbar_Container') dan 5 item berhasil ditambahkan ke HUD_Canvas tanpa mengganggu objek scene lainnya!",
                "Mantap"
            );
        }

        [MenuItem("Moonlit Homestead/Setup Hotbar Items")]
        public static void SetupHotbarItemsMenu()
        {
            Sprite[] itemSprites = LoadItemSprites();
            SetupInventoryHotbarItems(itemSprites);
            UpdateExistingHotbarSlots(itemSprites);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("<color=#70FFE5>[HotbarSafeSetup] Berhasil mengkonfigurasi 5 item dan icon pada Hotbar!</color>");
            EditorUtility.DisplayDialog(
                "Hotbar Items Sukses",
                "Berhasil mengkonfigurasi 5 item (Hoe, Seed, Watering Can, Watch, Journal) beserta icon pada InventoryHotbar dan Hotbar UI!",
                "Mantap"
            );
        }

        private static Sprite[] LoadItemSprites()
        {
            return new Sprite[]
            {
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Items/item_hoe.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Items/item_seed_pouch.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Items/item_watering_can.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Items/item_pocket_watch.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Items/item_journal.png")
            };
        }

        private static void SetupInventoryHotbarItems(Sprite[] itemSprites)
        {
            InventoryHotbar inv = Object.FindAnyObjectByType<InventoryHotbar>();
            if (inv == null)
            {
                Debug.LogWarning("[HotbarSafeSetup] InventoryHotbar tidak ditemukan di scene.");
                return;
            }

            SerializedObject invSerialized = new SerializedObject(inv);
            SerializedProperty itemsProp = invSerialized.FindProperty("items");
            if (itemsProp != null)
            {
                (ItemType type, string id, string name)[] itemDefs = new (ItemType, string, string)[]
                {
                    (ItemType.Hoe, "Hoe", "Cangkul (Hoe)"),
                    (ItemType.Seed, "Seed", "Bibit Anomali (Seed)"),
                    (ItemType.WateringCan, "WateringCan", "Penyiram (Watering Can)"),
                    (ItemType.Watch, "Watch", "Jam Saku (Pocket Watch)"),
                    (ItemType.Journal, "Journal", "Jurnal (Journal)")
                };

                itemsProp.arraySize = 5;
                for (int i = 0; i < 5; i++)
                {
                    SerializedProperty elem = itemsProp.GetArrayElementAtIndex(i);
                    elem.FindPropertyRelative("type").enumValueIndex = (int)itemDefs[i].type;
                    elem.FindPropertyRelative("id").stringValue = itemDefs[i].id;
                    elem.FindPropertyRelative("displayName").stringValue = itemDefs[i].name;
                    if (i < itemSprites.Length)
                    {
                        elem.FindPropertyRelative("icon").objectReferenceValue = itemSprites[i];
                    }
                }
                invSerialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void UpdateExistingHotbarSlots(Sprite[] itemSprites)
        {
            GameObject hudCanvas = GameObject.Find("HUD_Canvas");
            if (hudCanvas == null) return;

            Transform hotbarContainer = hudCanvas.transform.Find("Hotbar_Container");
            if (hotbarContainer == null) return;

            for (int i = 0; i < 5; i++)
            {
                Transform slot = hotbarContainer.Find($"Slot_{i}");
                if (slot == null) continue;

                // Background raycast
                Transform bg = slot.Find("Background");
                if (bg != null && bg.TryGetComponent<Image>(out var bgImg))
                {
                    bgImg.raycastTarget = true;
                }

                // ItemIcon
                Transform icon = slot.Find("ItemIcon");
                if (icon != null)
                {
                    RectTransform iconRect = icon.GetComponent<RectTransform>();
                    if (iconRect != null)
                    {
                        iconRect.sizeDelta = new Vector2(100f, 100f);
                    }
                    if (icon.TryGetComponent<Image>(out var iconImg))
                    {
                        iconImg.preserveAspect = true;
                        iconImg.raycastTarget = false;
                        if (i < itemSprites.Length && itemSprites[i] != null)
                        {
                            iconImg.sprite = itemSprites[i];
                        }
                    }
                    icon.gameObject.SetActive(true);
                }
            }
        }
    }
}
