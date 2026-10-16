using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoonlitHomestead
{
    public class GameHUD : MonoBehaviour
    {
        [Header("Clock & Loop UI")]
        [SerializeField] private Text clockText;
        [SerializeField] private Text loopText;
        [SerializeField] private RectTransform hourHandTransform;
        [SerializeField] private RectTransform minuteHandTransform;

        [Header("Stability Bar UI")]
        [SerializeField] private Slider stabilitySlider;
        [SerializeField] private Image stabilityFill;

        [Header("Dialogue / Notification UI")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Text dialogueText;

        [Header("Item Feedback UI (Optional)")]
        [SerializeField] private Text itemText;

        public static GameHUD Instance { get; private set; }

        private Coroutine hideMessageCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureReferences();
        }

        private void OnEnable()
        {
            EnsureReferences();
            BindEvents();
        }

        private void Start()
        {
            EnsureReferences();
            BindEvents();

            if (TimeLoopManager.Instance != null)
            {
                HandleTimeUpdated(TimeLoopManager.Instance.CurrentHour, TimeLoopManager.Instance.CurrentMinute);
                UpdateLoopDisplay(TimeLoopManager.Instance.CurrentLoopIndex);
            }

            if (RealityStabilitySystem.Instance != null)
            {
                HandleStabilityChanged(RealityStabilitySystem.Instance.CurrentStability, RealityStabilitySystem.Instance.MaxStability);
            }

            if (InventoryHotbar.Instance != null)
            {
                HandleItemSelected(InventoryHotbar.Instance.SelectedIndex, InventoryHotbar.Instance.CurrentItem);
            }
        }

        private void Update()
        {
            // Continuous update fallback to guarantee clock hands always rotate live
            if (TimeLoopManager.Instance != null)
            {
                HandleTimeUpdated(TimeLoopManager.Instance.CurrentHour, TimeLoopManager.Instance.CurrentMinute);
            }
        }

        private void EnsureReferences()
        {
            if (clockText == null)
            {
                var go = GameObject.Find("ClockText");
                if (go != null) clockText = go.GetComponent<Text>();
            }
            if (loopText == null)
            {
                var go = GameObject.Find("LoopText");
                if (go != null) loopText = go.GetComponent<Text>();
            }
            if (hourHandTransform == null)
            {
                var go = GameObject.Find("HourHand");
                if (go != null) hourHandTransform = go.GetComponent<RectTransform>();
            }
            if (minuteHandTransform == null)
            {
                var go = GameObject.Find("MinuteHand");
                if (go != null) minuteHandTransform = go.GetComponent<RectTransform>();
            }
            if (stabilitySlider == null)
            {
                var go = GameObject.Find("StabilitySlider");
                if (go != null) stabilitySlider = go.GetComponent<Slider>();
            }
            if (stabilityFill == null && stabilitySlider != null && stabilitySlider.fillRect != null)
            {
                stabilityFill = stabilitySlider.fillRect.GetComponent<Image>();
            }
            if (dialoguePanel == null)
            {
                dialoguePanel = GameObject.Find("DialoguePanel");
            }
            if (dialogueText == null)
            {
                var go = GameObject.Find("DialogueText");
                if (go != null) dialogueText = go.GetComponent<Text>();
            }
            if (itemText == null)
            {
                var go = GameObject.Find("HotbarText");
                if (go != null) itemText = go.GetComponent<Text>();
            }
        }

        private void BindEvents()
        {
            if (TimeLoopManager.Instance != null)
            {
                TimeLoopManager.Instance.OnTimeUpdated -= HandleTimeUpdated;
                TimeLoopManager.Instance.OnTimeUpdated += HandleTimeUpdated;
                TimeLoopManager.Instance.OnLoopReset -= HandleLoopReset;
                TimeLoopManager.Instance.OnLoopReset += HandleLoopReset;
            }

            if (RealityStabilitySystem.Instance != null)
            {
                RealityStabilitySystem.Instance.OnStabilityChanged -= HandleStabilityChanged;
                RealityStabilitySystem.Instance.OnStabilityChanged += HandleStabilityChanged;
            }

            if (InventoryHotbar.Instance != null)
            {
                InventoryHotbar.Instance.OnItemSelected -= HandleItemSelected;
                InventoryHotbar.Instance.OnItemSelected += HandleItemSelected;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (TimeLoopManager.Instance != null)
            {
                TimeLoopManager.Instance.OnTimeUpdated -= HandleTimeUpdated;
                TimeLoopManager.Instance.OnLoopReset -= HandleLoopReset;
            }

            if (RealityStabilitySystem.Instance != null)
            {
                RealityStabilitySystem.Instance.OnStabilityChanged -= HandleStabilityChanged;
            }

            if (InventoryHotbar.Instance != null)
            {
                InventoryHotbar.Instance.OnItemSelected -= HandleItemSelected;
            }
        }

        private void HandleItemSelected(int slotIndex, InventoryItem item)
        {
            if (itemText != null)
            {
                itemText.text = item != null ? $"[Slot {slotIndex + 1}] {item.displayName}" : $"[Slot {slotIndex + 1}] Kosong";
            }
        }

        public void HandleTimeUpdated(float hour, float minute)
        {
            if (clockText != null)
            {
                string displayHour = (hour >= 24) ? "00" : hour.ToString("00");
                clockText.text = $"{displayHour}:{minute:00}";
            }

            // Rotasi jarum jam & menit analog
            if (hourHandTransform != null)
            {
                // 1 putaran 12 jam = 360 derajat (30 derajat per jam)
                float hourDeg = (hour % 12f + minute / 60f) * 30f;
                hourHandTransform.localEulerAngles = new Vector3(0f, 0f, -hourDeg);
            }

            if (minuteHandTransform != null)
            {
                // 1 putaran 60 menit = 360 derajat (6 derajat per menit)
                float minuteDeg = minute * 6f;
                minuteHandTransform.localEulerAngles = new Vector3(0f, 0f, -minuteDeg);
            }
        }

        public void UpdateLoopDisplay(int loopIndex)
        {
            if (loopText != null)
            {
                loopText.text = $"Loop {loopIndex:D2}";
            }
        }

        private void HandleLoopReset(int loopIndex)
        {
            UpdateLoopDisplay(loopIndex);
            ShowMessage($"Reality rewinds... The soil remembers loop {loopIndex}.");
        }

        private void HandleStabilityChanged(float current, float max)
        {
            if (stabilitySlider != null)
            {
                stabilitySlider.maxValue = max;
                stabilitySlider.value = current;
            }

            if (stabilityFill != null)
            {
                float ratio = (max > 0f) ? current / max : 0f;
                stabilityFill.color = Color.Lerp(new Color(0.7f, 0.1f, 0.1f), new Color(0.2f, 0.8f, 0.6f), ratio);
            }
        }

        public void ShowMessage(string message)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            if (dialogueText != null) dialogueText.text = message;

            if (hideMessageCoroutine != null) StopCoroutine(hideMessageCoroutine);
            hideMessageCoroutine = StartCoroutine(HideMessageRoutine(4f));
        }

        private IEnumerator HideMessageRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }
    }
}
