using UnityEngine;

namespace MoonlitHomestead
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Detection Settings (Side-Scroller)")]
        [SerializeField] private float interactRange = 1.2f;
        [SerializeField] private Vector2 groundOffset = new Vector2(0.5f, -0.4f);
        [SerializeField] private LayerMask interactableLayer = ~0;

        private PlayerController2D controller;
        private readonly Collider2D[] hitBuffer = new Collider2D[8];

        private void Awake()
        {
            controller = GetComponent<PlayerController2D>();
        }

        private void Update()
        {
            // Interaksi lingkungan / ladang dengan tombol E
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryInteract();
            }

            // Gunakan item terpilih langsung (misal Pocket Watch atau Journal) dengan tombol F
            if (Input.GetKeyDown(KeyCode.F))
            {
                UseActiveItemDirectly();
            }
        }

        private void UseActiveItemDirectly()
        {
            if (InventoryHotbar.Instance == null || InventoryHotbar.Instance.CurrentItem == null) return;

            ItemType itemType = InventoryHotbar.Instance.CurrentItem.Type;

            if (itemType == ItemType.Watch)
            {
                if (TimeLoopManager.Instance != null && LoopBreakerManager.Instance != null)
                {
                    LoopBreakerManager.Instance.TryBreakLoop(
                        TimeLoopManager.Instance.CurrentHour,
                        TimeLoopManager.Instance.CurrentMinute
                    );
                }
            }
            else if (itemType == ItemType.Journal)
            {
                if (GameHUD.Instance != null)
                {
                    GameHUD.Instance.ShowMessage("Catatan Pengamat: 'Tanam bibit -> biarkan bermutasi di loop baru -> siram dengan kaleng seng -> panen kristal -> pasang di jam saku saat 23:55.'");
                }
            }
        }

        private void TryInteract()
        {
            Vector2 checkDirection = controller != null ? controller.FacingDirection : Vector2.right;
            // Deteksi ke arah depan bawah (posisi tanah di lantai dengan radius yang lebih terjangkau)
            Vector2 origin = (Vector2)transform.position + new Vector2(checkDirection.x * groundOffset.x, groundOffset.y);

            int hitCount = Physics2D.OverlapCircleNonAlloc(origin, interactRange, hitBuffer, interactableLayer);
            bool interacted = false;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = hitBuffer[i];
                if (col.gameObject == gameObject) continue;

                FarmPlot plot = col.GetComponent<FarmPlot>();
                if (plot != null)
                {
                    ItemType selectedItem = ItemType.None;
                    if (InventoryHotbar.Instance != null && InventoryHotbar.Instance.CurrentItem != null)
                    {
                        selectedItem = InventoryHotbar.Instance.CurrentItem.Type;
                    }
                    plot.Interact(selectedItem);
                    interacted = true;
                    break;
                }
            }

            // Jika tidak ada ladang di depan, dan memegang jam saku atau jurnal, gunakan item tersebut
            if (!interacted)
            {
                UseActiveItemDirectly();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 dir = controller != null ? controller.FacingDirection : Vector2.right;
            Vector2 origin = (Vector2)transform.position + new Vector2(dir.x * groundOffset.x, groundOffset.y);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(origin, interactRange);
        }
    }
}
