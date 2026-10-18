using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoonlitHomestead
{
    [RequireComponent(typeof(Collider2D))]
    public class ScenePortal : MonoBehaviour
    {
        public static Vector3? NextSpawnPosition { get; set; }
        public static bool IsTransitioning { get; private set; } = false;

        [Header("Target Scene Configuration")]
        [SerializeField] private string targetSceneName = "AltarJam";
        [SerializeField] private Vector3 spawnPositionInTargetScene = new Vector3(-8f, 0f, 0f);
        [SerializeField] private string portalLabel = "Menuju Reruntuhan Altar";

        [Header("Walk-Through Trigger Settings")]
        [SerializeField] private bool allowWalkThrough = true;
        [Tooltip("Arah jalan kaki untuk memicu transisi tanpa harus tekan E (misal: (1, 0) untuk border kanan, (-1, 0) untuk border kiri)")]
        [SerializeField] private Vector2 walkThroughDirection = Vector2.right;

        [Header("Prompt Offset")]
        [SerializeField] private Vector3 promptOffset = new Vector3(0f, 1.4f, 0f);

        private bool isPlayerInside = false;
        private Transform playerTransform;

        public string TargetSceneName { get => targetSceneName; set => targetSceneName = value; }
        public Vector3 SpawnPositionInTargetScene { get => spawnPositionInTargetScene; set => spawnPositionInTargetScene = value; }
        public string PortalLabel { get => portalLabel; set => portalLabel = value; }
        public Vector2 WalkThroughDirection { get => walkThroughDirection; set => walkThroughDirection = value; }

        private void OnEnable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (NextSpawnPosition.HasValue)
            {
                ApplyPlayerAndCameraPosition(NextSpawnPosition.Value);
            }
        }

        private static void ApplyPlayerAndCameraPosition(Vector3 spawnPos)
        {
            PlayerController2D player = PlayerController2D.Instance ?? Object.FindAnyObjectByType<PlayerController2D>();
            if (player != null)
            {
                player.transform.position = spawnPos;
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                }
            }

            CameraFollow2D cam = CameraFollow2D.Instance ?? Object.FindAnyObjectByType<CameraFollow2D>();
            if (cam != null)
            {
                cam.transform.position = new Vector3(spawnPos.x, spawnPos.y + cam.Offset.y, cam.transform.position.z);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsPlayerCollider(other))
            {
                isPlayerInside = true;
                playerTransform = other.transform;
                ShowPrompt();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsPlayerCollider(other))
            {
                isPlayerInside = false;
                playerTransform = null;
                HidePrompt();
            }
        }

        private void Update()
        {
            if (!isPlayerInside || IsTransitioning) return;

            // Update posisi prompt mengikuti kepala player
            ShowPrompt();

            bool ePressed = Input.GetKeyDown(KeyCode.E);
            bool walkTriggered = allowWalkThrough && CheckWalkDirection();

            if (ePressed || walkTriggered)
            {
                StartCoroutine(ExecuteTransition());
            }
        }

        private bool CheckWalkDirection()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(walkThroughDirection.x) > 0.1f)
            {
                return Mathf.Sign(horizontal) == Mathf.Sign(walkThroughDirection.x) && Mathf.Abs(horizontal) > 0.1f;
            }
            return false;
        }

        private void ShowPrompt()
        {
            if (InteractionPromptUI.Instance != null && playerTransform != null)
            {
                Vector3 promptPos = playerTransform.position + promptOffset;
                InteractionPromptUI.Instance.Show(promptPos, null, portalLabel, "E");
            }
        }

        private void HidePrompt()
        {
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.Hide();
            }
        }

        private IEnumerator ExecuteTransition()
        {
            IsTransitioning = true;
            HidePrompt();

            // Kunci pergerakan player
            PlayerController2D player = PlayerController2D.Instance ?? Object.FindAnyObjectByType<PlayerController2D>();
            if (player != null)
            {
                player.LockMovement(3f);
            }

            NextSpawnPosition = spawnPositionInTargetScene;

            // Fade Out layar
            if (LoopTransitionUI.Instance != null)
            {
                yield return LoopTransitionUI.Instance.FadeOut(0.5f);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }

            // Load target scene
            AsyncOperation op = SceneManager.LoadSceneAsync(targetSceneName);
            while (!op.isDone)
            {
                yield return null;
            }

            // Pasang posisi player dan kamera di scene baru
            ApplyPlayerAndCameraPosition(spawnPositionInTargetScene);

            // Beri jeda 0.1s agar scene settles
            yield return new WaitForSeconds(0.1f);

            // Fade In layar
            if (LoopTransitionUI.Instance != null)
            {
                yield return LoopTransitionUI.Instance.FadeIn(0.5f);
            }

            IsTransitioning = false;
            NextSpawnPosition = null;
        }

        private bool IsPlayerCollider(Collider2D col)
        {
            if (col == null) return false;
            if (col.CompareTag("Player")) return true;
            return col.GetComponent<PlayerController2D>() != null || col.GetComponentInParent<PlayerController2D>() != null;
        }
    }
}
