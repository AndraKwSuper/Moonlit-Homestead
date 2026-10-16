using UnityEngine;

namespace MoonlitHomestead
{
    public enum PromptPositionMode
    {
        AbovePlayerHead,
        AboveTarget
    }

    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Detection Settings (Side-Scroller)")]
        [SerializeField] private float interactRange = 0.45f;
        [SerializeField] private Vector2 groundOffset = new Vector2(0.35f, -0.1f);
        [SerializeField] private LayerMask interactableLayer = ~0;

        [Header("Floating Interaction Prompt")]
        [SerializeField] private InteractionPromptUI interactionPrompt;
        [SerializeField] private PromptPositionMode positionMode = PromptPositionMode.AbovePlayerHead;
        [SerializeField] private Vector3 playerHeadOffset = new Vector3(0f, 1.35f, 0f);
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 0.8f, 0f);
        [SerializeField] private Sprite iconTill;
        [SerializeField] private Sprite iconPlant;
        [SerializeField] private Sprite iconWater;
        [SerializeField] private Sprite iconHarvest;
        [SerializeField] private Sprite iconInspect;

        [Header("Action Animations")]
        [SerializeField] private Animator animator;
        [SerializeField] private float hoeActionDuration = 0.5f;
        [SerializeField] private float plantActionDuration = 0.5f;
        [SerializeField] private float waterActionDuration = 0.6f;
        [SerializeField] private float harvestActionDuration = 0.6f;

        private PlayerController2D controller;
        private readonly Collider2D[] hitBuffer = new Collider2D[16];
        private static readonly int TriggerHoeParam = Animator.StringToHash("TriggerHoe");
        private static readonly int TriggerPlantParam = Animator.StringToHash("TriggerPlant");
        private static readonly int TriggerWaterParam = Animator.StringToHash("TriggerWater");
        private static readonly int TriggerHarvestParam = Animator.StringToHash("TriggerHarvest");

        private void Awake()
        {
            controller = GetComponent<PlayerController2D>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
#if UNITY_EDITOR
            LoadActionIconsInEditor();
#endif
        }

        private void Start()
        {
            if (interactionPrompt == null)
            {
                interactionPrompt = InteractionPromptUI.Instance ?? Object.FindAnyObjectByType<InteractionPromptUI>();
            }
        }

        private void Update()
        {
            UpdateInteractionPrompt();

            // Interaksi lingkungan / ladang dengan tombol E
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryInteract();
            }

            // Gunakan item terpilih langsung (misal Pocket Watch atau Journal) dengan tombol F
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (controller != null && controller.IsMovementLocked) return;
                UseActiveItemDirectly();
            }
        }

        private void UseActiveItemDirectly()
        {
            if (InventoryHotbar.Instance == null || InventoryHotbar.Instance.CurrentItem == null) return;

            ItemType itemType = InventoryHotbar.Instance.CurrentItem.Type;

            if (itemType == ItemType.Watch)
            {
                if (TimeLoopManager.Instance != null && LoopBreakerManager.Instance != null)
                {
                    LoopBreakerManager.Instance.TryBreakLoop(
                        TimeLoopManager.Instance.CurrentHour,
                        TimeLoopManager.Instance.CurrentMinute
                    );
                }
            }
            else if (itemType == ItemType.Journal)
            {
                if (GameHUD.Instance != null)
                {
                    GameHUD.Instance.ShowMessage("Catatan Pengamat: 'Tanam bibit -> biarkan bermutasi di loop baru -> siram dengan kaleng seng -> panen kristal -> pasang di jam saku saat 23:55.'");
                }
            }
        }

        private FarmPlot GetBestInteractablePlot(out Vector2 detectionOrigin, out float bestDistance)
        {
            float facingX = 1f;
            if (controller != null && Mathf.Abs(controller.FacingDirection.x) > 0.01f)
            {
                facingX = Mathf.Sign(controller.FacingDirection.x);
            }

            detectionOrigin = (Vector2)transform.position + new Vector2(facingX * groundOffset.x, groundOffset.y);

            int hitCount = Physics2D.OverlapCircleNonAlloc(detectionOrigin, interactRange, hitBuffer, interactableLayer);
            FarmPlot bestPlot = null;
            bestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = hitBuffer[i];
                if (col == null || col.gameObject == gameObject) continue;

                FarmPlot plot = col.GetComponent<FarmPlot>();
                if (plot == null) continue;

                Vector2 closestPoint = col.ClosestPoint(detectionOrigin);

                // Validasi arah hadap: pastikan titik kontak berada di depan hadap player (atau player sedang di atas kolider plot)
                float horizontalRel = (closestPoint.x - transform.position.x) * facingX;
                if (horizontalRel < -0.15f)
                {
                    // Target berada di belakang tubuh player
                    continue;
                }

                float distance = Vector2.Distance(detectionOrigin, closestPoint);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestPlot = plot;
                }
            }

            return bestPlot;
        }

        private FarmPlot GetBestInteractablePlot(out Vector2 detectionOrigin)
        {
            return GetBestInteractablePlot(out detectionOrigin, out _);
        }

        private AncientWell GetBestInteractableWell(out Vector2 detectionOrigin, out float bestDistance)
        {
            float facingX = 1f;
            if (controller != null && Mathf.Abs(controller.FacingDirection.x) > 0.01f)
            {
                facingX = Mathf.Sign(controller.FacingDirection.x);
            }

            detectionOrigin = (Vector2)transform.position + new Vector2(facingX * groundOffset.x, groundOffset.y);

            int hitCount = Physics2D.OverlapCircleNonAlloc(detectionOrigin, interactRange + 0.6f, hitBuffer, interactableLayer);
            AncientWell bestWell = null;
            bestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = hitBuffer[i];
                if (col == null || col.gameObject == gameObject) continue;

                AncientWell well = col.GetComponent<AncientWell>();
                if (well == null) continue;

                Vector2 closestPoint = col.ClosestPoint(detectionOrigin);
                float horizontalRel = (closestPoint.x - transform.position.x) * facingX;
                if (horizontalRel < -0.3f)
                {
                    // Target sumur berada di belakang tubuh player
                    continue;
                }

                float distance = Vector2.Distance(detectionOrigin, closestPoint);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestWell = well;
                }
            }

            return bestWell;
        }

        public void TriggerHoeAction()
        {
            if (animator != null)
            {
                animator.SetTrigger(TriggerHoeParam);
            }

            if (controller != null)
            {
                controller.LockMovement(hoeActionDuration);
            }
        }

        public void TriggerPlantAction()
        {
            if (animator != null)
            {
                animator.SetTrigger(TriggerPlantParam);
            }

            if (controller != null)
            {
                controller.LockMovement(plantActionDuration);
            }
        }

        public void TriggerWaterAction()
        {
            if (animator != null)
            {
                animator.SetTrigger(TriggerWaterParam);
            }

            if (controller != null)
            {
                controller.LockMovement(waterActionDuration);
            }
        }

        public void TriggerHarvestAction()
        {
            if (animator != null)
            {
                animator.SetTrigger(TriggerHarvestParam);
            }

            if (controller != null)
            {
                controller.LockMovement(harvestActionDuration);
            }
        }

        private void TryInteract()
        {
            if (controller != null && controller.IsMovementLocked) return;

            ItemType selectedItem = ItemType.None;
            if (InventoryHotbar.Instance != null && InventoryHotbar.Instance.CurrentItem != null)
            {
                selectedItem = InventoryHotbar.Instance.CurrentItem.Type;
            }

            FarmPlot plot = GetBestInteractablePlot(out _, out float plotDist);
            AncientWell well = GetBestInteractableWell(out _, out float wellDist);

            if (plot != null && (well == null || plotDist <= wellDist))
            {
                if (plot.CurrentState == PlotState.Resonated && (selectedItem == ItemType.Hoe || selectedItem == ItemType.None))
                {
                    TriggerHarvestAction();
                }
                else if (selectedItem == ItemType.Hoe)
                {
                    TriggerHoeAction();
                }
                else if (selectedItem == ItemType.Seed && plot.CurrentState == PlotState.Tilled)
                {
                    TriggerPlantAction();
                }
                else if (selectedItem == ItemType.WateringCan && (plot.CurrentState == PlotState.Planted || plot.CurrentState == PlotState.Mutated))
                {
                    TriggerWaterAction();
                }
                plot.Interact(selectedItem);
            }
            else if (well != null)
            {
                if (selectedItem == ItemType.WateringCan)
                {
                    TriggerWaterAction();
                }
                well.Interact(selectedItem);
            }
            else
            {
                if (selectedItem == ItemType.Hoe)
                {
                    TriggerHoeAction();
                }
                else
                {
                    // Jika tidak ada ladang di depan, dan memegang jam saku atau jurnal, gunakan item tersebut
                    UseActiveItemDirectly();
                }
            }
        }

        private void UpdateInteractionPrompt()
        {
            if (interactionPrompt == null)
            {
                interactionPrompt = InteractionPromptUI.Instance ?? Object.FindAnyObjectByType<InteractionPromptUI>();
                if (interactionPrompt == null) return;
            }

            FarmPlot targetPlot = GetBestInteractablePlot(out _, out float plotDist);
            AncientWell targetWell = GetBestInteractableWell(out _, out float wellDist);

            if (targetPlot != null && (targetWell == null || plotDist <= wellDist))
            {
                (Sprite icon, string actionName) = GetPlotPromptData(targetPlot.CurrentState);
                Vector3 promptPos = (positionMode == PromptPositionMode.AbovePlayerHead)
                    ? transform.position + playerHeadOffset
                    : targetPlot.transform.position + targetOffset;

                interactionPrompt.Show(promptPos, icon, actionName, "E");
            }
            else if (targetWell != null)
            {
                Vector3 promptPos = (positionMode == PromptPositionMode.AbovePlayerHead)
                    ? transform.position + playerHeadOffset
                    : targetWell.transform.position + targetWell.PromptOffset;

                interactionPrompt.Show(promptPos, iconWater, targetWell.ActionName, targetWell.HotkeyText);
            }
            else
            {
                interactionPrompt.Hide();
            }
        }

        private (Sprite icon, string actionName) GetPlotPromptData(PlotState state)
        {
            switch (state)
            {
                case PlotState.Untilled:
                    return (iconTill, "Cangkul");
                case PlotState.Tilled:
                    return (iconPlant, "Tanam");
                case PlotState.Planted:
                    return (iconWater, "Siram");
                case PlotState.Mutated:
                    return (iconWater, "Stabilkan");
                case PlotState.Resonated:
                    return (iconHarvest, "Panen");
                default:
                    return (iconInspect, "Periksa");
            }
        }

#if UNITY_EDITOR
        private void LoadActionIconsInEditor()
        {
            if (iconTill == null) iconTill = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Interaction/icon_action_till.png");
            if (iconPlant == null) iconPlant = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Interaction/icon_action_plant.png");
            if (iconWater == null) iconWater = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Interaction/icon_action_water.png");
            if (iconHarvest == null) iconHarvest = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Interaction/icon_action_harvest.png");
            if (iconInspect == null) iconInspect = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Interaction/icon_action_inspect.png");
        }
#endif

        private void OnDrawGizmosSelected()
        {
            float facingX = 1f;
            if (controller != null && Mathf.Abs(controller.FacingDirection.x) > 0.01f)
            {
                facingX = Mathf.Sign(controller.FacingDirection.x);
            }

            Vector2 origin = (Vector2)transform.position + new Vector2(facingX * groundOffset.x, groundOffset.y);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(origin, interactRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + new Vector2(facingX * 0.35f, 0f));
        }
    }
}
