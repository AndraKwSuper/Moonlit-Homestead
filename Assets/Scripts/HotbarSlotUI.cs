using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlitHomestead
{
    public class HotbarSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image selectedHighlight;
        [SerializeField] private Image itemIconImage;
        [SerializeField] private Text keybindText;
        [SerializeField] private TMP_Text keybindTextTMP;

        public int SlotIndex { get; private set; } = -1;

        /// <summary>
        /// Inisialisasi slot dengan index, item awal, dan status seleksi.
        /// </summary>
        public void Setup(int slotIndex, InventoryItem item, bool isSelected)
        {
            SlotIndex = slotIndex;

            string label = (slotIndex + 1).ToString();
            if (keybindTextTMP != null)
            {
                keybindTextTMP.text = label;
            }
            if (keybindText != null)
            {
                keybindText.text = label;
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
    }
}
