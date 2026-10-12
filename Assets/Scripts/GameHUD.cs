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
        }

        private void Start()
        {
            if (TimeLoopManager.Instance != null)
            {
                TimeLoopManager.Instance.OnTimeUpdated += HandleTimeUpdated;
                TimeLoopManager.Instance.OnLoopReset += HandleLoopReset;
                HandleTimeUpdated(TimeLoopManager.Instance.CurrentHour, TimeLoopManager.Instance.CurrentMinute);
                HandleLoopReset(TimeLoopManager.Instance.CurrentLoopIndex);
            }

            if (RealityStabilitySystem.Instance != null)
            {
                RealityStabilitySystem.Instance.OnStabilityChanged += HandleStabilityChanged;
                HandleStabilityChanged(RealityStabilitySystem.Instance.CurrentStability, RealityStabilitySystem.Instance.MaxStability);
            }

            if (InventoryHotbar.Instance != null)
            {
                InventoryHotbar.Instance.OnItemSelected += HandleItemSelected;
                HandleItemSelected(InventoryHotbar.Instance.SelectedIndex, InventoryHotbar.Instance.CurrentItem);
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

        private void HandleTimeUpdated(float hour, float minute)
        {
            if (clockText != null)
            {
                string displayHour = (hour >= 24) ? "00" : hour.ToString("00");
                clockText.text = $"{displayHour}:{minute:00}";
            }
        }

        private void HandleLoopReset(int loopIndex)
        {
            if (loopText != null)
            {
                loopText.text = $"Loop {loopIndex:00}";
            }

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
