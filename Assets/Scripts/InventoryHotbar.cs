using System;
using UnityEngine;

namespace MoonlitHomestead
{
    public enum ItemType
    {
        None,
        Hoe,
        Seed,
        WateringCan,
        Watch,
        Journal
    }

    [System.Serializable]
    public class InventoryItem
    {
        public ItemType type = ItemType.None;
        public string id;
        public string displayName;
        public Sprite icon;

        public ItemType Type
        {
            get
            {
                if (type != ItemType.None) return type;
                if (!string.IsNullOrEmpty(id) && Enum.TryParse<ItemType>(id, true, out var parsed))
                {
                    return parsed;
                }
                return ItemType.None;
            }
        }
    }

    public class InventoryHotbar : MonoBehaviour
    {
        public static InventoryHotbar Instance { get; private set; }

        [Header("Hotbar Slots")]
        [SerializeField] private InventoryItem[] items = new InventoryItem[5];
        [SerializeField] private int selectedIndex = 0;

        public event Action<int, InventoryItem> OnItemSelected;

        public int SelectedIndex => selectedIndex;
        public InventoryItem[] Items => items;
        public InventoryItem CurrentItem => (items != null && selectedIndex >= 0 && selectedIndex < items.Length) ? items[selectedIndex] : null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Default items jika belum diisi di Inspector
            if (items == null || items.Length == 0 || items[0] == null || (items[0].type == ItemType.None && string.IsNullOrEmpty(items[0].id)))
            {
                items = new InventoryItem[]
                {
                    new InventoryItem { id = "Hoe", type = ItemType.Hoe, displayName = "Cangkul (Hoe)" },
                    new InventoryItem { id = "Seed", type = ItemType.Seed, displayName = "Bibit Anomali (Seed)" },
                    new InventoryItem { id = "WateringCan", type = ItemType.WateringCan, displayName = "Penyiram (Watering Can)" },
                    new InventoryItem { id = "Watch", type = ItemType.Watch, displayName = "Jam Saku (Pocket Watch)" },
                    new InventoryItem { id = "Journal", type = ItemType.Journal, displayName = "Jurnal (Journal)" }
                };
            }

#if UNITY_EDITOR
            if (items != null)
            {
                string[] defaultSpritePaths = new string[]
                {
                    "Assets/Sprites/UI/Items/item_hoe.png",
                    "Assets/Sprites/UI/Items/item_seed_pouch.png",
                    "Assets/Sprites/UI/Items/item_watering_can.png",
                    "Assets/Sprites/UI/Items/item_pocket_watch.png",
                    "Assets/Sprites/UI/Items/item_journal.png"
                };

                for (int i = 0; i < items.Length && i < defaultSpritePaths.Length; i++)
                {
                    if (items[i] != null && items[i].icon == null)
                    {
                        items[i].icon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(defaultSpritePaths[i]);
                    }
                }
            }
#endif
        }

        private void Update()
        {
            // Number keys 1-5
            for (int i = 0; i < 5; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectItem(i);
                }
            }

            // Scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f)
            {
                SelectItem((selectedIndex - 1 + 5) % 5);
            }
            else if (scroll < 0f)
            {
                SelectItem((selectedIndex + 1) % 5);
            }
        }

        public void SelectItem(int index)
        {
            if (index < 0 || index >= items.Length) return;
            selectedIndex = index;
            OnItemSelected?.Invoke(selectedIndex, CurrentItem);
            Debug.Log($"[Inventory] Selected Slot {selectedIndex + 1}: {CurrentItem?.displayName ?? "Empty"}");
        }
    }
}
