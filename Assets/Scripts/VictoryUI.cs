using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace MoonlitHomestead
{
    public class VictoryUI : MonoBehaviour
    {
        public static VictoryUI Instance { get; private set; }

        [Header("UI Components")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        [Header("Timing")]
        [SerializeField] private float delayBeforeFade = 1.0f;
        [SerializeField] private float fadeInDuration = 2.0f;

        private Coroutine victoryCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureComponents();
            HideImmediately();
        }

        private void Start()
        {
            BindButtons();
            BindLoopBreaker();
        }

        private void OnEnable()
        {
            BindLoopBreaker();
        }

        private void OnDisable()
        {
            UnbindLoopBreaker();
        }

        private void OnDestroy()
        {
            UnbindLoopBreaker();
        }

        public void EnsureComponents()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            if (titleText == null)
            {
                Transform t = transform.Find("Title");
                if (t != null) titleText = t.GetComponent<TMP_Text>();
            }

            if (subtitleText == null)
            {
                Transform t = transform.Find("Subtitle");
                if (t != null) subtitleText = t.GetComponent<TMP_Text>();
            }

            if (restartButton == null)
            {
                Transform t = transform.Find("RestartBtn");
                if (t != null) restartButton = t.GetComponent<Button>();
            }

            if (quitButton == null)
            {
                Transform t = transform.Find("QuitBtn");
                if (t != null) quitButton = t.GetComponent<Button>();
            }
        }

        private void BindButtons()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartClicked);
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitClicked);
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void BindLoopBreaker()
        {
            if (LoopBreakerManager.Instance != null)
            {
                LoopBreakerManager.Instance.OnLoopBroken -= HandleLoopBroken;
                LoopBreakerManager.Instance.OnLoopBroken += HandleLoopBroken;
            }
        }

        private void UnbindLoopBreaker()
        {
            if (LoopBreakerManager.Instance != null)
            {
                LoopBreakerManager.Instance.OnLoopBroken -= HandleLoopBroken;
            }
        }

        public void HideImmediately()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        private void HandleLoopBroken()
        {
            if (victoryCoroutine != null) StopCoroutine(victoryCoroutine);
            victoryCoroutine = StartCoroutine(PlayVictorySequence());
        }

        public IEnumerator PlayVictorySequence()
        {
            EnsureComponents();

            // 1. Kunci input pergerakan Elian via PlayerController2D.LockMovement(9999f)
            PlayerController2D player = PlayerController2D.Instance ?? Object.FindAnyObjectByType<PlayerController2D>();
            if (player != null)
            {
                player.LockMovement(9999f);
            }

            // 2. Tunggu 1 detik agar suara detak jarum jam terdengar
            yield return new WaitForSeconds(delayBeforeFade);

            // 3. Fade In canvasGroup.alpha secara perlahan (0 ke 1 selama 2 detik)
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < fadeInDuration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                    yield return null;
                }
                canvasGroup.alpha = 1f;

                // 4. Aktifkan raycast & interaksi tombol
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }

            victoryCoroutine = null;
        }

        public void OnRestartClicked()
        {
            // Reset status game managers
            if (LoopBreakerManager.Instance != null)
            {
                LoopBreakerManager.Instance.ResetState();
            }

            if (TimeLoopManager.Instance != null)
            {
                TimeLoopManager.Instance.ResetToDawn();
            }

            if (RealityStabilitySystem.Instance != null)
            {
                RealityStabilitySystem.Instance.RestoreStability(100f);
            }

            PlayerController2D player = PlayerController2D.Instance ?? Object.FindAnyObjectByType<PlayerController2D>();
            if (player != null)
            {
                player.LockMovement(0f);
                player.transform.position = new Vector3(-4.5f, 0f, 0f);
            }

            HideImmediately();

            SceneManager.LoadScene("Kebun");
        }

        public void OnQuitClicked()
        {
            Debug.Log("[VictoryUI] Keluar dari game...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        [ContextMenu("Debug: Test Victory Screen")]
        public void TestVictoryScreen()
        {
            if (Application.isPlaying)
            {
                StartCoroutine(PlayVictorySequence());
            }
            else
            {
                if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.interactable = true;
                }
            }
        }
    }
}
