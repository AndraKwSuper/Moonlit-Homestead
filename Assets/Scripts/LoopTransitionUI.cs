using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlitHomestead
{
    public class LoopTransitionUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image overlayImage;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Text loopNumberText;
        [SerializeField] private TMP_Text loreQuoteText;

        [Header("Timing Configuration")]
        [SerializeField] private float fadeOutDuration = 0.7f;
        [SerializeField] private float holdDuration = 1.5f;
        [SerializeField] private float fadeInDuration = 0.8f;
        [SerializeField] private float textFadeInDuration = 0.6f;

        [Header("Respawn Point")]
        [SerializeField] private Vector3 respawnPosition = new Vector3(-4.5f, 0.0f, 0f);

        [Header("Default Lore Quote")]
        [TextArea(2, 4)]
        [SerializeField] private string defaultLoreQuote = "'Waktu berputar kembali ke senja... Namun tanah tetap mengingat.'";

        public static LoopTransitionUI Instance { get; private set; }

        private Coroutine transitionCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ForceCloseOverlay();
        }

        public void ForceCloseOverlay()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            SetTextAlpha(0f);
        }

        public IEnumerator FadeOut(float duration)
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                float elapsed = 0f;
                float startAlpha = canvasGroup.alpha;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / duration);
                    yield return null;
                }
                canvasGroup.alpha = 1f;
            }
        }

        public IEnumerator FadeIn(float duration)
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                float elapsed = 0f;
                float startAlpha = canvasGroup.alpha;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                    yield return null;
                }
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        private void Start()
        {
            BindTimeLoop();
        }

        private void OnEnable()
        {
            BindTimeLoop();
        }

        private void OnDisable()
        {
            UnbindTimeLoop();
        }

        private void OnDestroy()
        {
            UnbindTimeLoop();
        }

        private void BindTimeLoop()
        {
            if (TimeLoopManager.Instance != null)
            {
                TimeLoopManager.Instance.OnLoopReset -= HandleLoopReset;
                TimeLoopManager.Instance.OnLoopReset += HandleLoopReset;
            }
        }

        private void UnbindTimeLoop()
        {
            if (TimeLoopManager.Instance != null)
            {
                TimeLoopManager.Instance.OnLoopReset -= HandleLoopReset;
            }
        }

        private void HandleLoopReset(int loopIndex)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }
            transitionCoroutine = StartCoroutine(PlayLoopTransition(loopIndex));
        }

        public IEnumerator PlayLoopTransition(int loopIndex)
        {
            // 1. Kunci input pergerakan Elian via PlayerController2D.LockMovement(3.5f)
            PlayerController2D player = PlayerController2D.Instance != null ? PlayerController2D.Instance : FindObjectOfType<PlayerController2D>();
            if (player != null)
            {
                player.LockMovement(3.5f);
            }

            // 2. Blokir raycast UI agar pemain tidak bisa mengklik HUD saat layar transisi
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }

            // 3. Update teks
            if (loopNumberText != null)
            {
                loopNumberText.text = $"LOOP {loopIndex:00}";
            }
            if (subtitleText != null && string.IsNullOrEmpty(subtitleText.text))
            {
                subtitleText.text = "REALITAS RUNTUH...";
            }
            if (loreQuoteText != null && string.IsNullOrEmpty(loreQuoteText.text))
            {
                loreQuoteText.text = defaultLoreQuote;
            }

            // Awal transisi: teks dibuat transparan
            SetTextAlpha(0f);

            // 4. Flash & Fade Out (0.7 detik): Lerp canvasGroup.alpha dari 0 ke 1
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeOutDuration);
                }
                yield return null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            // 5. Tahan Gelap & Tampilkan Teks (1.5 detik):
            // - Pindahkan posisi Player ke titik respawn awal (-4.5, 0, 0)
            if (player != null)
            {
                player.transform.position = respawnPosition;
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                }
            }
            else
            {
                GameObject playerGO = GameObject.FindWithTag("Player") ?? GameObject.Find("Player");
                if (playerGO != null)
                {
                    playerGO.transform.position = respawnPosition;
                    Rigidbody2D rb = playerGO.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.velocity = Vector2.zero;
                }
            }

            // - Tampilkan efek teks fade-in lembut
            float textElapsed = 0f;
            while (textElapsed < textFadeInDuration)
            {
                textElapsed += Time.deltaTime;
                SetTextAlpha(Mathf.Clamp01(textElapsed / textFadeInDuration));
                yield return null;
            }
            SetTextAlpha(1f);

            // Sisa durasi tahan gelap
            float remainingHold = Mathf.Max(0f, holdDuration - textFadeInDuration);
            if (remainingHold > 0f)
            {
                yield return new WaitForSeconds(remainingHold);
            }

            // 6. Fade In Kembali ke Game (0.8 detik): Lerp canvasGroup.alpha dari 1 ke 0
            elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeInDuration));
                }
                yield return null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            SetTextAlpha(0f);
            transitionCoroutine = null;
        }

        private void SetTextAlpha(float alpha)
        {
            if (subtitleText != null)
            {
                Color c = subtitleText.color;
                c.a = alpha;
                subtitleText.color = c;
            }
            if (loopNumberText != null)
            {
                Color c = loopNumberText.color;
                c.a = alpha;
                loopNumberText.color = c;
            }
            if (loreQuoteText != null)
            {
                Color c = loreQuoteText.color;
                c.a = alpha;
                loreQuoteText.color = c;
            }
        }

        [ContextMenu("Debug: Test Transition")]
        public void TestTransition()
        {
            if (Application.isPlaying)
            {
                StartCoroutine(PlayLoopTransition(2));
            }
            else
            {
                Debug.LogWarning("[LoopTransitionUI] Test transition hanya bisa dijalankan saat Play Mode.");
            }
        }
    }
}
