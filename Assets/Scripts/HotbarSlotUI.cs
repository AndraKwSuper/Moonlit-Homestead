using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace MoonlitHomestead
{
    public class HotbarSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image selectedHighlight;
        [SerializeField] private Image itemIconImage;
        [SerializeField] private TMP_Text keybindText;

        public int SlotIndex { get; private set; } = -1;

        /// <summary>
        /// Inisialisasi slot dengan index, item awal, dan status seleksi.
        /// </summary>
        public void Setup(int slotIndex, InventoryItem item, bool isSelected)
        {
            SlotIndex = slotIndex;

            if (keybindText != null)
            {
                keybindText.text = (slotIndex + 1).ToString();
            }

            UpdateItem(item);
            SetSelected(isSelected);
        }

        /// <summary>
        /// Mengubah status visual seleksi slot (aktifkan highlight).
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            if (selectedHighlight != null)
            {
                selectedHighlight.gameObject.SetActive(isSelected);
            }
        }

        /// <summary>
        /// Memperbarui ikon item di dalam slot. Jika item/ikon kosong, sembunyikan ikon.
        /// </summary>
        public void UpdateItem(InventoryItem item)
        {
            if (itemIconImage == null) return;

            if (item != null && item.icon != null)
            {
                itemIconImage.sprite = item.icon;
                itemIconImage.gameObject.SetActive(true);
            }
            else
            {
                itemIconImage.sprite = null;
                itemIconImage.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Handler klik mouse untuk memilih slot item ini.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (InventoryHotbar.Instance != null && SlotIndex >= 0)
            {
                InventoryHotbar.Instance.SelectItem(SlotIndex);
            }
        }
    }
}
