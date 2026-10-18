using System;
using UnityEngine;

namespace MoonlitHomestead
{
    public class LoopBreakerManager : MonoBehaviour
    {
        public static LoopBreakerManager Instance { get; private set; }

        [Header("Quest State")]
        [SerializeField] private bool hasCrystal = false;
        [SerializeField] private bool isLoopBroken = false;

        public bool HasCrystal => hasCrystal;
        public bool IsLoopBroken => isLoopBroken;

        public event Action OnCrystalHarvested;
        public event Action OnLoopBroken;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }

        public void ResetState()
        {
            hasCrystal = false;
            isLoopBroken = false;
        }

        public void ObtainCrystal()
        {
            if (hasCrystal) return;
            hasCrystal = true;
            Debug.Log("[LoopBreakerManager] Menemukan Lunar Resonance Crystal dari tanaman anomali!");
            OnCrystalHarvested?.Invoke();

            if (GameHUD.Instance != null)
            {
                GameHUD.Instance.ShowMessage("Kamu memanen kristal resonansi anomali! Gunakan pada jam saku di ambang tengah malam (23:55+).");
            }
        }

        public bool TryBreakLoop(float currentHour, float currentMinute)
        {
            if (isLoopBroken) return false;

            if (!hasCrystal)
            {
                if (GameHUD.Instance != null)
                {
                    GameHUD.Instance.ShowMessage("Jarum jam saku macet membeku di 00:00... Kamu butuh kristal resonansi.");
                }
                return false;
            }

            // Syarat waktu: Cukup masuk jam 23:00 ke atas (window 1 jam game yang nyaman)
            bool isNearMidnight = (currentHour >= 23);

            if (isNearMidnight)
            {
                BreakLoopSuccess();
                return true;
            }
            else
            {
                if (GameHUD.Instance != null)
                {
                    GameHUD.Instance.ShowMessage($"Jarum belum merespons ({currentHour:00}:{currentMinute:00}). Tunggu hingga waktu mendekati 00:00!");
                }
                return false;
            }
        }

        private void BreakLoopSuccess()
        {
            isLoopBroken = true;
            Debug.Log("[LoopBreakerManager] LOOP TERPECAHKAN! Jarum jam bergerak melewati 00:00!");
            OnLoopBroken?.Invoke();

            if (GameHUD.Instance != null)
            {
                GameHUD.Instance.ShowMessage("KLIK! Jam saku berdetik ke 00:01... Kabut malam memudar. FAJAR PERTAMA TIBA!");
            }
        }
    }
}
