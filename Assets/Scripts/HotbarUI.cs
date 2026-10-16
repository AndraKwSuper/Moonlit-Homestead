using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoonlitHomestead
{
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class HotbarUI : MonoBehaviour
    {
        [Header("Slot Configuration")]
        [SerializeField] private List<HotbarSlotUI> slots = new List<HotbarSlotUI>();
        [SerializeField] private HotbarSlotUI slotPrefab;
        [SerializeField] private Transform slotContainer;

        public List<HotbarSlotUI> Slots => slots;

        private void Awake()
        {
            if (slotContainer == null)
            {
                slotContainer = transform;
            }
        }

        private void Start()
        {
            InitializeSlots();
            BindEvents();
            SyncSelectionWithInventory();
        }

        private void OnEnable()
        {
            BindEvents();
            SyncSelectionWithInventory();
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        private void OnDestroy()
        {
            UnbindEvents();
        }

        /// <summary>
        /// Inisialisasi slot-slot UI (mengambil child yang sudah ada atau instansiasi prefab).
        /// </summary>
        public void InitializeSlots()
        {
            // Ambil child yang sudah ada di hierarki jika list kosong
            if (slots == null || slots.Count == 0)
            {
                slots = new List<HotbarSlotUI>(slotContainer.GetComponentsInChildren<HotbarSlotUI>(true));
            }

            var inventory = InventoryHotbar.Instance;
            InventoryItem[] items = inventory != null ? inventory.Items : null;
            int currentSelectedIndex = inventory != null ? inventory.SelectedIndex : 0;

            // Jika belum ada slot dan ada slotPrefab, instansiasi secara dinamis
            if (slots.Count == 0 && slotPrefab != null)
            {
                int count = (items != null && items.Length > 0) ? items.Length : 5;
                for (int i = 0; i < count; i++)
                {
                    HotbarSlotUI slotInstance = Instantiate(slotPrefab, slotContainer);
                    slots.Add(slotInstance);
                }
            }

            // Setup data dan visual tiap slot
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null) continue;

                InventoryItem item = (items != null && i < items.Length) ? items[i] : null;
                bool isSelected = (i == currentSelectedIndex);
                slots[i].Setup(i, item, isSelected);
            }
        }

        private void BindEvents()
        {
            if (InventoryHotbar.Instance != null)
            {
                InventoryHotbar.Instance.OnItemSelected -= HandleItemSelected;
                InventoryHotbar.Instance.OnItemSelected += HandleItemSelected;
            }
        }

        private void UnbindEvents()
        {
            if (InventoryHotbar.Instance != null)
            {
                InventoryHotbar.Instance.OnItemSelected -= HandleItemSelected;
            }
        }

        /// <summary>
        /// Callback saat item dipilih pada InventoryHotbar.
        /// </summary>
        public void HandleItemSelected(int index, InventoryItem item)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].SetSelected(i == index);
                }
            }
        }

        public void SyncSelectionWithInventory()
        {
            if (InventoryHotbar.Instance == null) return;
            HandleItemSelected(InventoryHotbar.Instance.SelectedIndex, InventoryHotbar.Instance.CurrentItem);
        }

        /// <summary>
        /// Memperbarui visual slot tertentu jika item di inventory berubah.
        /// </summary>
        public void UpdateSlotItem(int slotIndex, InventoryItem item)
        {
            if (slotIndex >= 0 && slotIndex < slots.Count && slots[slotIndex] != null)
            {
                slots[slotIndex].UpdateItem(item);
            }
        }
    }
}
