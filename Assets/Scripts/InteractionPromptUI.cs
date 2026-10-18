using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace MoonlitHomestead
{
    public class InteractionPromptUI : MonoBehaviour
    {
        public static InteractionPromptUI Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image plaqueBackground;
        [SerializeField] private Image keyBadgeImage;
        [SerializeField] private TMP_Text keyText;
        [SerializeField] private Image actionIconImage;
        [SerializeField] private TMP_Text actionText;
        [SerializeField] private Image pointerArrowImage;

        [Header("Motion & Animation")]
        [SerializeField] private float fadeSpeed = 10f;
        [SerializeField] private float bobSpeed = 3f;
        [SerializeField] private float bobHeight = 0.08f;

        private Vector3 baseWorldPosition;
        private float targetAlpha = 0f;
        private bool isShowing = false;
        private object currentOwner = null;

        public bool IsShowing => isShowing;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);

            if (canvas == null) canvas = GetComponent<Canvas>();
            if (canvasGroup == null) canvasGroup = GetComponentInChildren<CanvasGroup>();

            EnsureCameraAssigned();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            EnsureCameraAssigned();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureCameraAssigned();
            Hide(null, true);
        }

        public void EnsureCameraAssigned()
        {
            if (canvas == null) canvas = GetComponent<Canvas>();
            if (canvas != null && (canvas.worldCamera == null || !canvas.worldCamera.gameObject.activeInHierarchy))
            {
                Camera cam = Camera.main;
                if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
                if (cam != null)
                {
                    canvas.worldCamera = cam;
                }
            }
        }

        private void Update()
        {
            // Pastikan camera selalu valid
            if (canvas != null && (canvas.worldCamera == null || !canvas.worldCamera.gameObject.activeInHierarchy))
            {
                EnsureCameraAssigned();
            }

            // Smooth Fade
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
            }

            // Gentle Bobbing & Position Following
            if (targetAlpha > 0f || (canvasGroup != null && canvasGroup.alpha > 0.01f))
            {
                float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                transform.position = baseWorldPosition + new Vector3(0f, bobOffset, 0f);
            }
        }

        public void Show(Vector3 worldPos, Sprite actionIcon, string actionName, string keyName = "E", object requester = null)
        {
            EnsureCameraAssigned();

            currentOwner = requester;

            if (!isShowing && (canvasGroup == null || canvasGroup.alpha < 0.05f))
            {
                transform.position = worldPos;
            }

            baseWorldPosition = worldPos;
            targetAlpha = 1f;
            isShowing = true;

            if (keyText != null)
            {
                keyText.text = string.IsNullOrEmpty(keyName) ? "E" : keyName;
            }

            if (actionText != null)
            {
                actionText.text = actionName ?? string.Empty;
            }

            if (actionIconImage != null)
            {
                if (actionIcon != null)
                {
                    actionIconImage.sprite = actionIcon;
                    actionIconImage.gameObject.SetActive(true);
                }
                else
                {
                    actionIconImage.gameObject.SetActive(false);
                }
            }
        }

        public void Hide(object requester = null, bool force = false)
        {
            if (!force && currentOwner != null && requester != null && currentOwner != requester)
            {
                // Ditolak: Jangan sembunyikan jika requester lain sedang memegang kepemilikan prompt
                return;
            }

            targetAlpha = 0f;
            isShowing = false;
            currentOwner = null;
        }
    }
}
