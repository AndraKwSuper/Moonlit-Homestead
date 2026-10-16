using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoonlitHomestead
{
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class HotbarUI : MonoBehaviour
    {
        [Header("Slot Configuration")]
        [SerializeField] private HotbarSlotUI slotPrefab;
        [SerializeField] private Transform slotContainer;

        private readonly List<HotbarSlotUI> spawnedSlots = new List<HotbarSlotUI>();
        private bool isInitialized = false;

        private void Awake()
        {
            if (slotContainer == null)
            {
                slotContainer = transform;
            }
        }

        private void Start()
        {
            InitializeHotbar();
            BindEvents();
        }

        private void OnEnable()
        {
            if (isInitialized)
            {
                BindEvents();
                SyncSelectionWithInventory();
            }
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
        /// Inisialisasi slot secara dinamis berdasarkan data InventoryHotbar.
        /// </summary>
        public void InitializeHotbar()
        {
            if (slotPrefab == null)
            {
                Debug.LogWarning("[HotbarUI] Slot Prefab belum di-assign di Inspector!");
                return;
            }

            // Bersihkan slot lama jika ada
            ClearExistingSlots();

            var inventory = InventoryHotbar.Instance;
            InventoryItem[] items = inventory != null ? inventory.Items : null;
            int slotCount = (items != null && items.Length > 0) ? items.Length : 5;
            int currentSelectedIndex = inventory != null ? inventory.SelectedIndex : 0;

            for (int i = 0; i < slotCount; i++)
            {
                HotbarSlotUI slotInstance = Instantiate(slotPrefab, slotContainer);
                InventoryItem item = (items != null && i < items.Length) ? items[i] : null;
                bool isSelected = (i == currentSelectedIndex);

                slotInstance.Setup(i, item, isSelected);
                spawnedSlots.Add(slotInstance);
            }

            isInitialized = true;
        }

        private void ClearExistingSlots()
        {
            foreach (var slot in spawnedSlots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            spawnedSlots.Clear();
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
        private void HandleItemSelected(int selectedIndex, InventoryItem selectedItem)
        {
            for (int i = 0; i < spawnedSlots.Count; i++)
            {
                if (spawnedSlots[i] != null)
                {
                    spawnedSlots[i].SetSelected(i == selectedIndex);
                }
            }
        }

        private void SyncSelectionWithInventory()
        {
            if (InventoryHotbar.Instance == null) return;
            HandleItemSelected(InventoryHotbar.Instance.SelectedIndex, InventoryHotbar.Instance.CurrentItem);
        }

        /// <summary>
        /// Memperbarui visual slot tertentu jika item di inventory berubah.
        /// </summary>
        public void UpdateSlotItem(int slotIndex, InventoryItem item)
        {
            if (slotIndex >= 0 && slotIndex < spawnedSlots.Count && spawnedSlots[slotIndex] != null)
            {
                spawnedSlots[slotIndex].UpdateItem(item);
            }
        }
    }
}
