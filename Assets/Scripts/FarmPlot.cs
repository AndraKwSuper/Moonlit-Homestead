using UnityEngine;

namespace MoonlitHomestead
{
    public enum PlotState
    {
        Untilled,
        Tilled,
        Planted,
        Mutated,   // Residu anomali dari loop sebelumnya
        Resonated  // Disiram air tengah malam, siap dipanen kristalnya
    }

    [RequireComponent(typeof(Collider2D))]
    public class FarmPlot : MonoBehaviour
    {
        [Header("Plot State")]
        [SerializeField] private PlotState currentState = PlotState.Untilled;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Visuals - Sprites (Prioritas)")]
        [SerializeField] private Sprite untilledSprite;
        [SerializeField] private Sprite tilledSprite;
        [SerializeField] private Sprite plantedSprite;
        [SerializeField] private Sprite mutatedSprite;
        [SerializeField] private Sprite resonatedSprite;

        [Header("Visuals - Prototype Color Fallback")]
        [SerializeField] private Color untilledColor = new Color(0.5f, 0.4f, 0.3f);
        [SerializeField] private Color tilledColor = new Color(0.35f, 0.25f, 0.15f);
        [SerializeField] private Color plantedColor = new Color(0.2f, 0.6f, 0.3f);
        [SerializeField] private Color mutatedColor = new Color(0.5f, 0.1f, 0.6f); // Ungu eerie
        [SerializeField] private Color resonatedColor = new Color(0.2f, 0.8f, 0.9f); // Cyan terang kosmik

        public PlotState CurrentState => currentState;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            UpdateVisuals();
        }

        private void OnEnable()
        {
            if (TimeLoopManager.Instance != null)
            {
                TimeLoopManager.Instance.OnLoopReset -= HandleLoopReset;
                TimeLoopManager.Instance.OnLoopReset += HandleLoopReset;
            }
        }

        private void Start()
        {
            if (TimeLoopManager.Instance != null)
            {
                TimeLoopManager.Instance.OnLoopReset -= HandleLoopReset;
                TimeLoopManager.Instance.OnLoopReset += HandleLoopReset;
            }
        }

        private void OnDisable()
        {
            if (TimeLoopManager.Instance != null)
            {
                TimeLoopManager.Instance.OnLoopReset -= HandleLoopReset;
            }
        }

        public void Interact(ItemType selectedItem)
        {
            switch (currentState)
            {
                case PlotState.Untilled:
                    if (selectedItem == ItemType.Hoe)
                    {
                        currentState = PlotState.Tilled;
                        UpdateVisuals();
                        Debug.Log("[FarmPlot] Tanah berhasil dicangkul!");
                        if (GameHUD.Instance != null) GameHUD.Instance.ShowMessage("Tanah berhasil dicangkul. Siap ditanami bibit anomali.");
                    }
                    else
                    {
                        if (GameHUD.Instance != null) GameHUD.Instance.ShowMessage("Gunakan Cangkul (Hoe) terlebih dahulu.");
                    }
                    break;

                case PlotState.Tilled:
                    if (selectedItem == ItemType.Seed)
                    {
                        currentState = PlotState.Planted;
                        UpdateVisuals();
                        Debug.Log("[FarmPlot] Anomaly Seed berhasil ditanam!");
                        if (GameHUD.Instance != null) GameHUD.Instance.ShowMessage("Anomaly Seed ditanam. Biarkan menyerap malam hingga reset loop.");
                    }
                    else
                    {
                        if (GameHUD.Instance != null) GameHUD.Instance.ShowMessage("Gunakan Bibit Anomali (Seed) untuk menanam.");
                    }
                    break;

                case PlotState.Planted:
                    if (selectedItem == ItemType.WateringCan)
                    {
                        if (GameHUD.Instance != null) GameHUD.Instance.ShowMessage("Tanaman disiram. Akarnya bergetar halus menyerap radiasi bulan.");
                    }
                    else
                    {
                        if (GameHUD.Instance != null) GameHUD.Instance.ShowMessage("Tanaman sedang berakar... Tunggu siklus loop berakhir.");
                    }
                    break;

                case PlotState.Mutated:
                    if (selectedItem == ItemType.WateringCan)
                    {
                        // Siram tanaman mutasi agar mencapai resonansi kosmik
                        currentState = PlotState.Resonated;
                        UpdateVisuals();
                        if (GameHUD.Instance != null) GameHUD.Instance.ShowMessage("Bunga mutasi bermekaran memancarkan kristal! Siap dipanen dengan cangkul.");
                    }
                    else
                    {
                        // Jika didekati tanpa disiram, menguras kewarasan
                        if (RealityStabilitySystem.Instance != null)
                        {
                            RealityStabilitySystem.Instance.ModifyStability(-10f);
                        }
                        if (GameHUD.Instance != null) GameHUD.Instance.ShowMessage("Tanaman mutasi membisikkan anomali! Gunakan Penyiram untuk menstabilkannya.");
                    }
                    break;

                case PlotState.Resonated:
                    if (selectedItem == ItemType.Hoe || selectedItem == ItemType.None)
                    {
                        // Panen kristal
                        currentState = PlotState.Untilled;
                        UpdateVisuals();
                        if (LoopBreakerManager.Instance != null)
                        {
                            LoopBreakerManager.Instance.ObtainCrystal();
                        }
                    }
                    else
                    {
                        if (GameHUD.Instance != null) GameHUD.Instance.ShowMessage("Gunakan Cangkul (Hoe) atau tangan kosong untuk memanen kristal resonansi ini!");
                    }
                    break;
            }
        }

        public void Interact(string selectedItem)
        {
            if (System.Enum.TryParse<ItemType>(selectedItem, true, out var parsed))
            {
                Interact(parsed);
            }
            else
            {
                Interact(ItemType.None);
            }
        }

        private void HandleLoopReset(int loopIndex)
        {
            // The World Remembers: Tanaman yang ditanam di loop lalu bermutasi jadi anomali
            if (currentState == PlotState.Planted)
            {
                currentState = PlotState.Mutated;
            }
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (spriteRenderer == null) return;

            Sprite targetSprite = null;
            switch (currentState)
            {
                case PlotState.Untilled: targetSprite = untilledSprite; break;
                case PlotState.Tilled: targetSprite = tilledSprite; break;
                case PlotState.Planted: targetSprite = plantedSprite; break;
                case PlotState.Mutated: targetSprite = mutatedSprite; break;
                case PlotState.Resonated: targetSprite = resonatedSprite; break;
            }

            if (targetSprite != null)
            {
                spriteRenderer.sprite = targetSprite;
                spriteRenderer.color = Color.white; // Jangan tint sprite asli
            }
            else
            {
                switch (currentState)
                {
                    case PlotState.Untilled: spriteRenderer.color = untilledColor; break;
                    case PlotState.Tilled: spriteRenderer.color = tilledColor; break;
                    case PlotState.Planted: spriteRenderer.color = plantedColor; break;
                    case PlotState.Mutated: spriteRenderer.color = mutatedColor; break;
                    case PlotState.Resonated: spriteRenderer.color = resonatedColor; break;
                }
            }
        }
    }
}
