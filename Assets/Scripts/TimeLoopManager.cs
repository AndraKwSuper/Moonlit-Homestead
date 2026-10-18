using System;
using UnityEngine;

namespace MoonlitHomestead
{
    public class TimeLoopManager : MonoBehaviour
    {
        public static TimeLoopManager Instance { get; private set; }

        [Header("Clock Settings")]
        [Tooltip("Durasi 1 siklus hari dalam detik nyata")]
        [SerializeField] private float dayDurationSeconds = 60f;
        [SerializeField] private float startHour = 18f; // Mulai jam 18:00 (Senja/Malam)
        [SerializeField] private float endHour = 24f;   // Jam 24:00 (Tengah malam = loop trigger)

        [Header("Loop Tracking")]
        [SerializeField] private int currentLoop = 0;

        [Header("Debug Settings")]
        [Tooltip("Aktifkan tombol shortcut keyboard untuk skip ke loop berikutnya")]
        [SerializeField] private bool enableDebugSkip = true;
        [Tooltip("Tombol shortcut keyboard untuk skip langsung ke loop berikutnya")]
        [SerializeField] private KeyCode debugSkipLoopKey = KeyCode.L;

        public event Action<int> OnLoopReset;
        public event Action<float, float> OnTimeUpdated; // CurrentHour, Minute

        public float CurrentTimeRatio { get; private set; }
        public float CurrentHour { get; private set; }
        public float CurrentMinute { get; private set; }
        public int CurrentLoopIndex => currentLoop;
        public bool IsLoopFrozen => isLoopFrozen;

        private float elapsedTime = 0f;
        private bool isRewinding = false;
        private bool isLoopFrozen = false; // Saat loop berhasil dipecahkan

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

        private void Start()
        {
            ResetDayTimer();

            // Safe subscription check
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

        public void FreezeLoop()
        {
            isLoopFrozen = true;
            CurrentHour = 0f;
            CurrentMinute = 1f;
            OnTimeUpdated?.Invoke(CurrentHour, CurrentMinute);
        }

        private void HandleLoopBroken()
        {
            FreezeLoop();
        }

        private void Update()
        {
            if (enableDebugSkip && Input.GetKeyDown(debugSkipLoopKey))
            {
                SkipToNextLoop();
                return;
            }

            if (isRewinding || isLoopFrozen) return;

            elapsedTime += Time.deltaTime;
            CurrentTimeRatio = Mathf.Clamp01(elapsedTime / dayDurationSeconds);

            float totalHoursSpan = endHour - startHour;
            float rawHour = startHour + (CurrentTimeRatio * totalHoursSpan);
            CurrentHour = Mathf.Floor(rawHour);
            CurrentMinute = Mathf.Floor((rawHour - CurrentHour) * 60f);

            OnTimeUpdated?.Invoke(CurrentHour, CurrentMinute);

            if (elapsedTime >= dayDurationSeconds)
            {
                TriggerTimeLoop();
            }
        }

        [ContextMenu("Debug: Skip to Next Loop")]
        public void SkipToNextLoop()
        {
            if (isRewinding) return;
            isLoopFrozen = false;
            Debug.Log($"[TimeLoopManager] Debug Skip dipicu! Memaksa lompat ke Loop #{currentLoop + 1}...");
            TriggerTimeLoop();
        }

        public void TriggerTimeLoop()
        {
            if (isRewinding || isLoopFrozen) return;
            StartCoroutine(TimeLoopSequence());
        }

        private System.Collections.IEnumerator TimeLoopSequence()
        {
            isRewinding = true;
            currentLoop++;
            Debug.Log($"[TimeLoopManager] 00:00 Reached! Reality Collapsing. Starting Loop {currentLoop}...");

            // Pemicu transisi visual layar (fade to black dimulai)
            OnLoopReset?.Invoke(currentLoop);

            // Beri jeda 0.75 detik agar layar telah menjadi gelap pekat (fade-out selesai)
            // sebelum reset jarum jam dan waktu kembali ke senja (18:00)
            yield return new WaitForSeconds(0.75f);

            // Reset timer jam setelah layar benar-benar hitam pekat agar pemain tidak melihat jarum jam melompat tiba-tiba
            ResetDayTimer();

            // Tahan rewind selama sisa transisi gelap dan fade-in (~2.5 detik)
            yield return new WaitForSeconds(2.5f);

            isRewinding = false;
        }

        private void ResetDayTimer()
        {
            elapsedTime = 0f;
            CurrentTimeRatio = 0f;
            CurrentHour = startHour;
            CurrentMinute = 0f;
            OnTimeUpdated?.Invoke(CurrentHour, CurrentMinute);
        }

        public void ResetToDawn()
        {
            isLoopFrozen = false;
            isRewinding = false;
            currentLoop = 1;
            ResetDayTimer();
        }
    }
}
