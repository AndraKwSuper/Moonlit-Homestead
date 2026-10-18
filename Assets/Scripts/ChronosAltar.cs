using UnityEngine;

namespace MoonlitHomestead
{
    [RequireComponent(typeof(Collider2D))]
    public class ChronosAltar : MonoBehaviour
    {
        [Header("Prompt Settings")]
        [SerializeField] private Vector3 promptOffset = new Vector3(0f, 2.5f, 0f);
        [SerializeField] private Sprite inspectIcon;
        [SerializeField] private Sprite breakLoopIcon;

        private bool isPlayerNearby = false;
        private Transform playerTransform;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsPlayer(other))
            {
                isPlayerNearby = true;
                playerTransform = other.transform;
                UpdatePrompt();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsPlayer(other))
            {
                isPlayerNearby = false;
                playerTransform = null;
                HidePrompt();
            }
        }

        private void Update()
        {
            if (!isPlayerNearby) return;

            UpdatePrompt();

            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }

        private void UpdatePrompt()
        {
            if (InteractionPromptUI.Instance == null) return;

            bool isBroken = LoopBreakerManager.Instance != null && LoopBreakerManager.Instance.IsLoopBroken;
            if (isBroken)
            {
                Vector3 promptPos = transform.position + promptOffset;
                InteractionPromptUI.Instance.Show(promptPos, null, "Altar Terpecahkan", "", this);
                return;
            }

            bool hasCrystal = LoopBreakerManager.Instance != null && LoopBreakerManager.Instance.HasCrystal;
            float hour = TimeLoopManager.Instance != null ? TimeLoopManager.Instance.CurrentHour : 18f;
            float minute = TimeLoopManager.Instance != null ? TimeLoopManager.Instance.CurrentMinute : 0f;

            // Syarat pecahkan loop: kristal ada & waktu >= 23:55
            bool isTimeReady = (hour == 23 && minute >= 55) || (hour >= 24) || (hour == 0 && minute <= 5);

            string actionLabel = (hasCrystal && isTimeReady) ? "Pecahkan Loop" : "Periksa Altar";
            Sprite actionIcon = (hasCrystal && isTimeReady) ? breakLoopIcon : inspectIcon;

            Vector3 worldPos = transform.position + promptOffset;
            InteractionPromptUI.Instance.Show(worldPos, actionIcon, actionLabel, "E", this);
        }

        private void HidePrompt()
        {
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.Hide(this);
            }
        }

        public void Interact()
        {
            if (LoopBreakerManager.Instance == null) return;

            if (LoopBreakerManager.Instance.IsLoopBroken)
            {
                if (GameHUD.Instance != null)
                {
                    GameHUD.Instance.ShowMessage("Altar Chronos telah terbebas dari jeratan waktu. Fajar abadi kini bersinar.");
                }
                return;
            }

            float hour = TimeLoopManager.Instance != null ? TimeLoopManager.Instance.CurrentHour : 18f;
            float minute = TimeLoopManager.Instance != null ? TimeLoopManager.Instance.CurrentMinute : 0f;

            bool success = LoopBreakerManager.Instance.TryBreakLoop(hour, minute);

            if (!success)
            {
                if (!LoopBreakerManager.Instance.HasCrystal)
                {
                    if (GameHUD.Instance != null)
                    {
                        GameHUD.Instance.ShowMessage("Altar Chronos bergetar dingin... Terdapat rongga kosong berbentuk kristal resonansi anomali.");
                    }
                }
                else
                {
                    if (GameHUD.Instance != null)
                    {
                        GameHUD.Instance.ShowMessage($"Kristal bergetar pelan di tanganmu ({hour:00}:{minute:00}). Altar hanya merespons tepat di ambang tengah malam (23:55+)!");
                    }
                }
            }
        }

        private bool IsPlayer(Collider2D col)
        {
            if (col == null) return false;
            if (col.CompareTag("Player")) return true;
            return col.GetComponent<PlayerController2D>() != null || col.GetComponentInParent<PlayerController2D>() != null;
        }
    }
}
