using System;
using UnityEngine;

namespace MoonlitHomestead
{
    public class RealityStabilitySystem : MonoBehaviour
    {
        public static RealityStabilitySystem Instance { get; private set; }

        [Header("Stability Settings")]
        [SerializeField] private float maxStability = 100f;
        [SerializeField] private float passiveDecayPerSecond = 0.5f;

        public event Action<float, float> OnStabilityChanged; // current, max
        public event Action OnSanityDepleted;

        public float CurrentStability { get; private set; }
        public float MaxStability => maxStability;
        public float StabilityRatio => Mathf.Clamp01(CurrentStability / maxStability);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            CurrentStability = maxStability;
        }

        private void Start()
        {
            if (TimeLoopManager.Instance != null)
            {
                TimeLoopManager.Instance.OnLoopReset += HandleLoopReset;
            }
            OnStabilityChanged?.Invoke(CurrentStability, maxStability);
        }

        private void OnDestroy()
        {
            if (TimeLoopManager.Instance != null)
            {
                TimeLoopManager.Instance.OnLoopReset -= HandleLoopReset;
            }
        }

        private void Update()
        {
            if (CurrentStability > 0f)
            {
                ModifyStability(-passiveDecayPerSecond * Time.deltaTime);
            }
        }

        public void ModifyStability(float amount)
        {
            CurrentStability = Mathf.Clamp(CurrentStability + amount, 0f, maxStability);
            OnStabilityChanged?.Invoke(CurrentStability, maxStability);

            if (CurrentStability <= 0f)
            {
                OnSanityDepleted?.Invoke();
                // Jika stabilitas habis, paksa trigger time loop lebih awal
                if (TimeLoopManager.Instance != null)
                {
                    TimeLoopManager.Instance.TriggerTimeLoop();
                }
            }
        }

        private void HandleLoopReset(int loopIndex)
        {
            // Reset stabilitas tapi bisa dikurangi sedikit tiap loop untuk efek progressive horror
            float penalty = Mathf.Min(loopIndex * 5f, 30f);
            CurrentStability = Mathf.Max(maxStability - penalty, 20f);
            OnStabilityChanged?.Invoke(CurrentStability, maxStability);
        }
    }
}
