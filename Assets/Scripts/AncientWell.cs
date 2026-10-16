using UnityEngine;

namespace MoonlitHomestead
{
    [RequireComponent(typeof(Collider2D))]
    public class AncientWell : MonoBehaviour
    {
        [Header("Prompt Settings")]
        [SerializeField] private string actionName = "Ambil Air";
        [SerializeField] private string hotkeyText = "E";
        [SerializeField] private Vector3 promptOffset = new Vector3(0f, 1.2f, 0f);

        [Header("Dialogue Messages")]
        [TextArea(2, 4)]
        [SerializeField] private string fillSuccessMessage = "Teko terisi air rembulan anomali yang berpendar dingin.";
        [TextArea(2, 4)]
        [SerializeField] private string needCanMessage = "Sumur batu tua. Air di dasarnya memancarkan radiasi kosmik toska... Butuh Teko Penyiram untuk mengambilnya.";

        public string ActionName => actionName;
        public string HotkeyText => hotkeyText;
        public Vector3 PromptOffset => promptOffset;

        public void Interact(ItemType heldItem)
        {
            if (heldItem == ItemType.WateringCan)
            {
                if (GameHUD.Instance != null)
                {
                    GameHUD.Instance.ShowMessage(fillSuccessMessage);
                }
                Debug.Log("[AncientWell] Air rembulan anomali berhasil diambil ke dalam Teko.");
            }
            else
            {
                if (GameHUD.Instance != null)
                {
                    GameHUD.Instance.ShowMessage(needCanMessage);
                }
                Debug.Log("[AncientWell] Membutuhkan Teko Penyiram (WateringCan) untuk mengambil air.");
            }
        }
    }
}
