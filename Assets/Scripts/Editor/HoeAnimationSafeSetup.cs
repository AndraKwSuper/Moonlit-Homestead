#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MoonlitHomestead.Editor
{
    public static class HoeAnimationSafeSetup
    {
        private const string ControllerPath = "Assets/ANim/Player.controller";
        private const string HoeingAnimPath = "Assets/ANim/Hoeing.anim";
        private const string PlantingAnimPath = "Assets/ANim/Planting.anim";
        private const string WateringAnimPath = "Assets/ANim/Watering.anim";
        private const string HarvestingAnimPath = "Assets/ANim/Harvesting.anim";

        [MenuItem("Moonlit Homestead/Setup Hoe Animation (Safe)")]
        public static void SetupHoeAnimationSafe()
        {
            // 0. Standardisasi Importer Sprite Aksi mencangkul agar identik 1:1 dengan Elian_Idle.png
            string idlePath = "Assets/Sprites/Characters/Elian_Idle.png";
            TextureImporter idleImporter = AssetImporter.GetAtPath(idlePath) as TextureImporter;

            for (int i = 1; i <= 5; i++)
            {
                string path = $"Assets/Sprites/Characters/Actions/Hoe/Elian_Hoe_{i}.png";
                TextureImporter hoeImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (hoeImporter != null && idleImporter != null)
                {
                    bool needReimport = false;

                    TextureImporterSettings idleSettings = new TextureImporterSettings();
                    idleImporter.ReadTextureSettings(idleSettings);

                    TextureImporterSettings hoeSettings = new TextureImporterSettings();
                    hoeImporter.ReadTextureSettings(hoeSettings);

                    if (hoeImporter.textureType != TextureImporterType.Sprite)
                    {
                        hoeImporter.textureType = TextureImporterType.Sprite;
                        needReimport = true;
                    }

                    if (hoeImporter.spriteImportMode != SpriteImportMode.Single)
                    {
                        hoeImporter.spriteImportMode = SpriteImportMode.Single;
                        needReimport = true;
                    }

                    if (Mathf.Abs(hoeImporter.spritePixelsPerUnit - idleImporter.spritePixelsPerUnit) > 0.01f)
                    {
                        hoeImporter.spritePixelsPerUnit = idleImporter.spritePixelsPerUnit; // 280
                        needReimport = true;
                    }

                    if (hoeSettings.spriteAlignment != idleSettings.spriteAlignment || hoeSettings.spritePivot != idleSettings.spritePivot)
                    {
                        hoeSettings.spriteAlignment = idleSettings.spriteAlignment; // 7 (BottomCenter)
                        hoeSettings.spritePivot = idleSettings.spritePivot; // {x: 0.5, y: 0.0625}
                        hoeImporter.SetTextureSettings(hoeSettings);
                        needReimport = true;
                    }

                    if (hoeImporter.filterMode != idleImporter.filterMode)
                    {
                        hoeImporter.filterMode = idleImporter.filterMode;
                        needReimport = true;
                    }

                    if (needReimport)
                    {
                        hoeImporter.SaveAndReimport();
                    }
                }
            }

            // 1. Verifikasi / Konfigurasi Animation Clip Hoeing.anim
            AnimationClip hoeingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(HoeingAnimPath);
            if (hoeingClip == null)
            {
                hoeingClip = new AnimationClip { name = "Hoeing" };
                AssetDatabase.CreateAsset(hoeingClip, HoeingAnimPath);
            }

            // Load 5 sprite frame
            Sprite[] frames = new Sprite[5];
            for (int i = 1; i <= 5; i++)
            {
                string path = $"Assets/Sprites/Characters/Actions/Hoe/Elian_Hoe_{i}.png";
                frames[i - 1] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (frames[i - 1] == null)
                {
                    Debug.LogWarning($"[HoeAnimationSafeSetup] Sprite tidak ditemukan di path: {path}");
                }
            }

            // Setup curve binding untuk SpriteRenderer (m_Sprite)
            EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[5];
            for (int i = 0; i < 5; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * 0.1f, // 10 fps (0.0s, 0.1s, 0.2s, 0.3s, 0.4s)
                    value = frames[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(hoeingClip, binding, keyframes);
            hoeingClip.frameRate = 10;

            // Nonaktifkan Loop Time (One-shot action)
            var settings = AnimationUtility.GetAnimationClipSettings(hoeingClip);
            settings.loopTime = false;
            settings.stopTime = 0.5f;
            AnimationUtility.SetAnimationClipSettings(hoeingClip, settings);
            EditorUtility.SetDirty(hoeingClip);

            // 2. Konfigurasi Animator Controller Player.controller
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError($"[HoeAnimationSafeSetup] AnimatorController tidak ditemukan di {ControllerPath}");
                return;
            }

            // Tambahkan Trigger Parameter: TriggerHoe jika belum ada
            bool hasTriggerHoe = false;
            foreach (var param in controller.parameters)
            {
                if (param.name == "TriggerHoe" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasTriggerHoe = true;
                    break;
                }
            }

            if (!hasTriggerHoe)
            {
                controller.AddParameter("TriggerHoe", AnimatorControllerParameterType.Trigger);
            }

            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

            // Cari atau buat state Hoeing
            AnimatorState hoeingState = null;
            AnimatorState idleState = null;

            foreach (var childState in rootStateMachine.states)
            {
                if (childState.state.name == "Hoeing")
                {
                    hoeingState = childState.state;
                }
                else if (childState.state.name == "Idle")
                {
                    idleState = childState.state;
                }
            }

            if (hoeingState == null)
            {
                hoeingState = rootStateMachine.AddState("Hoeing", new Vector3(265, 175, 0));
            }

            hoeingState.motion = hoeingClip;

            // Transisi Any State -> Hoeing
            bool hasAnyStateToHoeing = false;
            foreach (var transition in rootStateMachine.anyStateTransitions)
            {
                if (transition.destinationState == hoeingState)
                {
                    hasAnyStateToHoeing = true;
                    transition.hasExitTime = false;
                    transition.duration = 0.05f;
                    transition.canTransitionToSelf = false;
                    break;
                }
            }

            if (!hasAnyStateToHoeing)
            {
                var anyTrans = rootStateMachine.AddAnyStateTransition(hoeingState);
                anyTrans.hasExitTime = false;
                anyTrans.duration = 0.05f;
                anyTrans.canTransitionToSelf = false;
                anyTrans.AddCondition(AnimatorConditionMode.If, 0, "TriggerHoe");
            }

            // Transisi Hoeing -> Idle
            if (idleState != null)
            {
                bool hasHoeingToIdle = false;
                foreach (var transition in hoeingState.transitions)
                {
                    if (transition.destinationState == idleState)
                    {
                        hasHoeingToIdle = true;
                        transition.hasExitTime = true;
                        transition.exitTime = 1f;
                        transition.duration = 0.1f;
                        break;
                    }
                }

                if (!hasHoeingToIdle)
                {
                    var exitTrans = hoeingState.AddTransition(idleState);
                    exitTrans.hasExitTime = true;
                    exitTrans.exitTime = 1f;
                    exitTrans.duration = 0.1f;
                }
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log("[HoeAnimationSafeSetup] Setup animasi aksi mencangkul (Hoeing) berhasil disinkronkan secara aman.");
        }

        [MenuItem("Moonlit Homestead/Setup Plant Animation (Safe)")]
        public static void SetupPlantAnimationSafe()
        {
            // 0. Standardisasi Importer Sprite Aksi menanam agar identik 1:1 dengan Elian_Idle.png
            string idlePath = "Assets/Sprites/Characters/Elian_Idle.png";
            TextureImporter idleImporter = AssetImporter.GetAtPath(idlePath) as TextureImporter;

            for (int i = 1; i <= 4; i++)
            {
                string path = $"Assets/Sprites/Characters/Actions/Plant/Elian_Plant_{i}.png";
                TextureImporter plantImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (plantImporter != null && idleImporter != null)
                {
                    bool needReimport = false;

                    TextureImporterSettings idleSettings = new TextureImporterSettings();
                    idleImporter.ReadTextureSettings(idleSettings);

                    TextureImporterSettings plantSettings = new TextureImporterSettings();
                    plantImporter.ReadTextureSettings(plantSettings);

                    if (plantImporter.textureType != TextureImporterType.Sprite)
                    {
                        plantImporter.textureType = TextureImporterType.Sprite;
                        needReimport = true;
                    }

                    if (plantImporter.spriteImportMode != SpriteImportMode.Single)
                    {
                        plantImporter.spriteImportMode = SpriteImportMode.Single;
                        needReimport = true;
                    }

                    if (Mathf.Abs(plantImporter.spritePixelsPerUnit - idleImporter.spritePixelsPerUnit) > 0.01f)
                    {
                        plantImporter.spritePixelsPerUnit = idleImporter.spritePixelsPerUnit; // 280
                        needReimport = true;
                    }

                    if (plantSettings.spriteAlignment != idleSettings.spriteAlignment || plantSettings.spritePivot != idleSettings.spritePivot)
                    {
                        plantSettings.spriteAlignment = idleSettings.spriteAlignment; // 7 (BottomCenter)
                        plantSettings.spritePivot = idleSettings.spritePivot; // {x: 0.5, y: 0.0625}
                        plantImporter.SetTextureSettings(plantSettings);
                        needReimport = true;
                    }

                    if (plantImporter.filterMode != idleImporter.filterMode)
                    {
                        plantImporter.filterMode = idleImporter.filterMode;
                        needReimport = true;
                    }

                    if (needReimport)
                    {
                        plantImporter.SaveAndReimport();
                    }
                }
            }

            // 1. Verifikasi / Konfigurasi Animation Clip Planting.anim
            AnimationClip plantingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(PlantingAnimPath);
            if (plantingClip == null)
            {
                plantingClip = new AnimationClip { name = "Planting" };
                AssetDatabase.CreateAsset(plantingClip, PlantingAnimPath);
            }

            Sprite[] frames = new Sprite[4];
            for (int i = 1; i <= 4; i++)
            {
                string path = $"Assets/Sprites/Characters/Actions/Plant/Elian_Plant_{i}.png";
                frames[i - 1] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (frames[i - 1] == null)
                {
                    Debug.LogWarning($"[PlantAnimationSafeSetup] Sprite tidak ditemukan di path: {path}");
                }
            }

            EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[4];
            for (int i = 0; i < 4; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * 0.125f, // 8 fps (0.0s, 0.125s, 0.25s, 0.375s)
                    value = frames[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(plantingClip, binding, keyframes);
            plantingClip.frameRate = 8;

            var settings = AnimationUtility.GetAnimationClipSettings(plantingClip);
            settings.loopTime = false;
            settings.stopTime = 0.5f;
            AnimationUtility.SetAnimationClipSettings(plantingClip, settings);
            EditorUtility.SetDirty(plantingClip);

            // 2. Konfigurasi Animator Controller Player.controller
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError($"[PlantAnimationSafeSetup] AnimatorController tidak ditemukan di {ControllerPath}");
                return;
            }

            bool hasTriggerPlant = false;
            foreach (var param in controller.parameters)
            {
                if (param.name == "TriggerPlant" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasTriggerPlant = true;
                    break;
                }
            }

            if (!hasTriggerPlant)
            {
                controller.AddParameter("TriggerPlant", AnimatorControllerParameterType.Trigger);
            }

            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

            AnimatorState plantingState = null;
            AnimatorState idleState = null;

            foreach (var childState in rootStateMachine.states)
            {
                if (childState.state.name == "Planting")
                {
                    plantingState = childState.state;
                }
                else if (childState.state.name == "Idle")
                {
                    idleState = childState.state;
                }
            }

            if (plantingState == null)
            {
                plantingState = rootStateMachine.AddState("Planting", new Vector3(265, 240, 0));
            }

            plantingState.motion = plantingClip;

            // Transisi Any State -> Planting
            bool hasAnyStateToPlanting = false;
            foreach (var transition in rootStateMachine.anyStateTransitions)
            {
                if (transition.destinationState == plantingState)
                {
                    hasAnyStateToPlanting = true;
                    transition.hasExitTime = false;
                    transition.duration = 0.05f;
                    transition.canTransitionToSelf = false;
                    break;
                }
            }

            if (!hasAnyStateToPlanting)
            {
                var anyTrans = rootStateMachine.AddAnyStateTransition(plantingState);
                anyTrans.hasExitTime = false;
                anyTrans.duration = 0.05f;
                anyTrans.canTransitionToSelf = false;
                anyTrans.AddCondition(AnimatorConditionMode.If, 0, "TriggerPlant");
            }

            // Transisi Planting -> Idle
            if (idleState != null)
            {
                bool hasPlantingToIdle = false;
                foreach (var transition in plantingState.transitions)
                {
                    if (transition.destinationState == idleState)
                    {
                        hasPlantingToIdle = true;
                        transition.hasExitTime = true;
                        transition.exitTime = 1f;
                        transition.duration = 0.1f;
                        break;
                    }
                }

                if (!hasPlantingToIdle)
                {
                    var exitTrans = plantingState.AddTransition(idleState);
                    exitTrans.hasExitTime = true;
                    exitTrans.exitTime = 1f;
                    exitTrans.duration = 0.1f;
                }
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log("[PlantAnimationSafeSetup] Setup animasi aksi menanam (Planting) berhasil disinkronkan secara aman.");
        }

        [MenuItem("Moonlit Homestead/Setup Water Animation (Safe)")]
        public static void SetupWaterAnimationSafe()
        {
            // 0. Standardisasi Importer Sprite Aksi menyiram agar identik 1:1 dengan Elian_Idle.png
            string idlePath = "Assets/Sprites/Characters/Elian_Idle.png";
            TextureImporter idleImporter = AssetImporter.GetAtPath(idlePath) as TextureImporter;

            for (int i = 1; i <= 4; i++)
            {
                string path = $"Assets/Sprites/Characters/Actions/Water/Elian_Water_{i}.png";
                TextureImporter waterImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (waterImporter != null && idleImporter != null)
                {
                    bool needReimport = false;

                    TextureImporterSettings idleSettings = new TextureImporterSettings();
                    idleImporter.ReadTextureSettings(idleSettings);

                    TextureImporterSettings waterSettings = new TextureImporterSettings();
                    waterImporter.ReadTextureSettings(waterSettings);

                    if (waterImporter.textureType != TextureImporterType.Sprite)
                    {
                        waterImporter.textureType = TextureImporterType.Sprite;
                        needReimport = true;
                    }

                    if (waterImporter.spriteImportMode != SpriteImportMode.Single)
                    {
                        waterImporter.spriteImportMode = SpriteImportMode.Single;
                        needReimport = true;
                    }

                    if (Mathf.Abs(waterImporter.spritePixelsPerUnit - idleImporter.spritePixelsPerUnit) > 0.01f)
                    {
                        waterImporter.spritePixelsPerUnit = idleImporter.spritePixelsPerUnit; // 280
                        needReimport = true;
                    }

                    if (waterSettings.spriteAlignment != idleSettings.spriteAlignment || waterSettings.spritePivot != idleSettings.spritePivot)
                    {
                        waterSettings.spriteAlignment = idleSettings.spriteAlignment; // 7 (BottomCenter)
                        waterSettings.spritePivot = idleSettings.spritePivot; // {x: 0.5, y: 0.0625}
                        waterImporter.SetTextureSettings(waterSettings);
                        needReimport = true;
                    }

                    if (waterImporter.filterMode != idleImporter.filterMode)
                    {
                        waterImporter.filterMode = idleImporter.filterMode;
                        needReimport = true;
                    }

                    if (needReimport)
                    {
                        waterImporter.SaveAndReimport();
                    }
                }
            }

            // 1. Verifikasi / Konfigurasi Animation Clip Watering.anim
            AnimationClip wateringClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(WateringAnimPath);
            if (wateringClip == null)
            {
                wateringClip = new AnimationClip { name = "Watering" };
                AssetDatabase.CreateAsset(wateringClip, WateringAnimPath);
            }

            Sprite[] frames = new Sprite[4];
            for (int i = 1; i <= 4; i++)
            {
                string path = $"Assets/Sprites/Characters/Actions/Water/Elian_Water_{i}.png";
                frames[i - 1] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (frames[i - 1] == null)
                {
                    Debug.LogWarning($"[WaterAnimationSafeSetup] Sprite tidak ditemukan di path: {path}");
                }
            }

            EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[4];
            for (int i = 0; i < 4; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * 0.15f, // ~0.65s total duration
                    value = frames[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(wateringClip, binding, keyframes);
            wateringClip.frameRate = 10;

            var settings = AnimationUtility.GetAnimationClipSettings(wateringClip);
            settings.loopTime = false;
            settings.stopTime = 0.65f;
            AnimationUtility.SetAnimationClipSettings(wateringClip, settings);
            EditorUtility.SetDirty(wateringClip);

            // 2. Konfigurasi Animator Controller Player.controller
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError($"[WaterAnimationSafeSetup] AnimatorController tidak ditemukan di {ControllerPath}");
                return;
            }

            bool hasTriggerWater = false;
            foreach (var param in controller.parameters)
            {
                if (param.name == "TriggerWater" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasTriggerWater = true;
                    break;
                }
            }

            if (!hasTriggerWater)
            {
                controller.AddParameter("TriggerWater", AnimatorControllerParameterType.Trigger);
            }

            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

            AnimatorState wateringState = null;
            AnimatorState idleState = null;

            foreach (var childState in rootStateMachine.states)
            {
                if (childState.state.name == "Watering")
                {
                    wateringState = childState.state;
                }
                else if (childState.state.name == "Idle")
                {
                    idleState = childState.state;
                }
            }

            if (wateringState == null)
            {
                wateringState = rootStateMachine.AddState("Watering", new Vector3(265, 305, 0));
            }

            wateringState.motion = wateringClip;

            // Transisi Any State -> Watering
            bool hasAnyStateToWatering = false;
            foreach (var transition in rootStateMachine.anyStateTransitions)
            {
                if (transition.destinationState == wateringState)
                {
                    hasAnyStateToWatering = true;
                    transition.hasExitTime = false;
                    transition.duration = 0.05f;
                    transition.canTransitionToSelf = false;
                    break;
                }
            }

            if (!hasAnyStateToWatering)
            {
                var anyTrans = rootStateMachine.AddAnyStateTransition(wateringState);
                anyTrans.hasExitTime = false;
                anyTrans.duration = 0.05f;
                anyTrans.canTransitionToSelf = false;
                anyTrans.AddCondition(AnimatorConditionMode.If, 0, "TriggerWater");
            }

            // Transisi Watering -> Idle
            if (idleState != null)
            {
                bool hasWateringToIdle = false;
                foreach (var transition in wateringState.transitions)
                {
                    if (transition.destinationState == idleState)
                    {
                        hasWateringToIdle = true;
                        transition.hasExitTime = true;
                        transition.exitTime = 1f;
                        transition.duration = 0.1f;
                        break;
                    }
                }

                if (!hasWateringToIdle)
                {
                    var exitTrans = wateringState.AddTransition(idleState);
                    exitTrans.hasExitTime = true;
                    exitTrans.exitTime = 1f;
                    exitTrans.duration = 0.1f;
                }
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log("[WaterAnimationSafeSetup] Setup animasi aksi menyiram (Watering) berhasil disinkronkan secara aman.");
        }

        [MenuItem("Moonlit Homestead/Setup Harvest Animation (Safe)")]
        public static void SetupHarvestAnimationSafe()
        {
            // 0. Standardisasi Importer Sprite Aksi memetik/panen agar identik 1:1 dengan Elian_Idle.png
            string idlePath = "Assets/Sprites/Characters/Elian_Idle.png";
            TextureImporter idleImporter = AssetImporter.GetAtPath(idlePath) as TextureImporter;

            for (int i = 1; i <= 4; i++)
            {
                string path = $"Assets/Sprites/Characters/Actions/Harvest/Elian_Harvest_{i}.png";
                TextureImporter harvestImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (harvestImporter != null && idleImporter != null)
                {
                    bool needReimport = false;

                    TextureImporterSettings idleSettings = new TextureImporterSettings();
                    idleImporter.ReadTextureSettings(idleSettings);

                    TextureImporterSettings harvestSettings = new TextureImporterSettings();
                    harvestImporter.ReadTextureSettings(harvestSettings);

                    if (harvestImporter.textureType != TextureImporterType.Sprite)
                    {
                        harvestImporter.textureType = TextureImporterType.Sprite;
                        needReimport = true;
                    }

                    if (harvestImporter.spriteImportMode != SpriteImportMode.Single)
                    {
                        harvestImporter.spriteImportMode = SpriteImportMode.Single;
                        needReimport = true;
                    }

                    if (Mathf.Abs(harvestImporter.spritePixelsPerUnit - idleImporter.spritePixelsPerUnit) > 0.01f)
                    {
                        harvestImporter.spritePixelsPerUnit = idleImporter.spritePixelsPerUnit; // 280
                        needReimport = true;
                    }

                    if (harvestSettings.spriteAlignment != idleSettings.spriteAlignment || harvestSettings.spritePivot != idleSettings.spritePivot)
                    {
                        harvestSettings.spriteAlignment = idleSettings.spriteAlignment; // 7 (BottomCenter)
                        harvestSettings.spritePivot = idleSettings.spritePivot; // {x: 0.5, y: 0.0625}
                        harvestImporter.SetTextureSettings(harvestSettings);
                        needReimport = true;
                    }

                    if (harvestImporter.filterMode != idleImporter.filterMode)
                    {
                        harvestImporter.filterMode = idleImporter.filterMode;
                        needReimport = true;
                    }

                    if (needReimport)
                    {
                        harvestImporter.SaveAndReimport();
                    }
                }
            }

            // 1. Verifikasi / Konfigurasi Animation Clip Harvesting.anim
            AnimationClip harvestClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(HarvestingAnimPath);
            if (harvestClip == null)
            {
                harvestClip = new AnimationClip { name = "Harvesting" };
                AssetDatabase.CreateAsset(harvestClip, HarvestingAnimPath);
            }

            Sprite[] frames = new Sprite[4];
            for (int i = 1; i <= 4; i++)
            {
                string path = $"Assets/Sprites/Characters/Actions/Harvest/Elian_Harvest_{i}.png";
                frames[i - 1] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (frames[i - 1] == null)
                {
                    Debug.LogWarning($"[HarvestAnimationSafeSetup] Sprite tidak ditemukan di path: {path}");
                }
            }

            EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[4];
            for (int i = 0; i < 4; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * 0.15f, // ~0.65s total duration
                    value = frames[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(harvestClip, binding, keyframes);
            harvestClip.frameRate = 10;

            var settings = AnimationUtility.GetAnimationClipSettings(harvestClip);
            settings.loopTime = false;
            settings.stopTime = 0.65f;
            AnimationUtility.SetAnimationClipSettings(harvestClip, settings);
            EditorUtility.SetDirty(harvestClip);

            // 2. Konfigurasi Animator Controller Player.controller
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError($"[HarvestAnimationSafeSetup] AnimatorController tidak ditemukan di {ControllerPath}");
                return;
            }

            bool hasTriggerHarvest = false;
            foreach (var param in controller.parameters)
            {
                if (param.name == "TriggerHarvest" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasTriggerHarvest = true;
                    break;
                }
            }

            if (!hasTriggerHarvest)
            {
                controller.AddParameter("TriggerHarvest", AnimatorControllerParameterType.Trigger);
            }

            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

            AnimatorState harvestState = null;
            AnimatorState idleState = null;

            foreach (var childState in rootStateMachine.states)
            {
                if (childState.state.name == "Harvesting")
                {
                    harvestState = childState.state;
                }
                else if (childState.state.name == "Idle")
                {
                    idleState = childState.state;
                }
            }

            if (harvestState == null)
            {
                harvestState = rootStateMachine.AddState("Harvesting", new Vector3(265, 370, 0));
            }

            harvestState.motion = harvestClip;

            // Transisi Any State -> Harvesting
            bool hasAnyStateToHarvest = false;
            foreach (var transition in rootStateMachine.anyStateTransitions)
            {
                if (transition.destinationState == harvestState)
                {
                    hasAnyStateToHarvest = true;
                    transition.hasExitTime = false;
                    transition.duration = 0.05f;
                    transition.canTransitionToSelf = false;
                    break;
                }
            }

            if (!hasAnyStateToHarvest)
            {
                var anyTrans = rootStateMachine.AddAnyStateTransition(harvestState);
                anyTrans.hasExitTime = false;
                anyTrans.duration = 0.05f;
                anyTrans.canTransitionToSelf = false;
                anyTrans.AddCondition(AnimatorConditionMode.If, 0, "TriggerHarvest");
            }

            // Transisi Harvesting -> Idle
            if (idleState != null)
            {
                bool hasHarvestToIdle = false;
                foreach (var transition in harvestState.transitions)
                {
                    if (transition.destinationState == idleState)
                    {
                        hasHarvestToIdle = true;
                        transition.hasExitTime = true;
                        transition.exitTime = 1f;
                        transition.duration = 0.1f;
                        break;
                    }
                }

                if (!hasHarvestToIdle)
                {
                    var exitTrans = harvestState.AddTransition(idleState);
                    exitTrans.hasExitTime = true;
                    exitTrans.exitTime = 1f;
                    exitTrans.duration = 0.1f;
                }
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log("[HarvestAnimationSafeSetup] Setup animasi aksi memetik/panen (Harvesting) berhasil disinkronkan secara aman.");
        }

        [MenuItem("Moonlit Homestead/Debug/Skip to Next Loop (L)")]
        public static void DebugSkipToNextLoopMenu()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[Debug] Fitur Skip Loop hanya dapat digunakan saat Play Mode sedang berjalan.");
                return;
            }

            if (TimeLoopManager.Instance != null)
            {
                TimeLoopManager.Instance.SkipToNextLoop();
            }
            else
            {
                Debug.LogWarning("[Debug] TimeLoopManager.Instance tidak ditemukan di scene.");
            }
        }
    }
}
#endif
