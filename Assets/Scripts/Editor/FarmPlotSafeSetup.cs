#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MoonlitHomestead.Editor
{
    public static class FarmPlotSafeSetup
    {
        private const string GothicPlanterSpritePath = "Assets/Sprites/Environment/Props/prop_gothic_planter.png";
        private const string GrassSpritePath = "Assets/Sprites/Environment/Tiles/tile_grass_surface.png";
        private const string GrassTileAssetPath = "Assets/Sprites/Environment/Tiles/Tile_Grass_Surface.asset";
        private const string PlantedSpritePath = "Assets/Sprites/Environment/Crops/crop_stage1_sprout.png";
        private const string MutatedSpritePath = "Assets/Sprites/Environment/Crops/crop_stage2_mutated.png";
        private const string ResonatedSpritePath = "Assets/Sprites/Environment/Crops/crop_stage3_resonated.png";

        [InitializeOnLoadMethod]
        private static void AutoRunOnceOnCompile()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool("FarmPlotsGothicV2Run", false))
                {
                    SessionState.SetBool("FarmPlotsGothicV2Run", true);
                    SetupFarmPlotsSafe();
                }
            };
        }

        [MenuItem("Moonlit Homestead/Setup Farm Plots (Safe)")]
        public static void SetupFarmPlotsSafe()
        {
            // 1. Muat Aset Sprite
            Sprite planterSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GothicPlanterSpritePath);
            Sprite grassSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GrassSpritePath);
            Sprite plantedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlantedSpritePath);
            Sprite mutatedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MutatedSpritePath);
            Sprite resonatedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ResonatedSpritePath);

            Tile grassTile = AssetDatabase.LoadAssetAtPath<Tile>(GrassTileAssetPath);
            if (grassTile == null && grassSprite != null)
            {
                grassTile = ScriptableObject.CreateInstance<Tile>();
                grassTile.sprite = grassSprite;
                AssetDatabase.CreateAsset(grassTile, GrassTileAssetPath);
                AssetDatabase.SaveAssets();
            }

            if (planterSprite == null) Debug.LogWarning("[FarmPlotSafeSetup] Gothic Planter sprite tidak ditemukan di " + GothicPlanterSpritePath);
            if (plantedSprite == null) Debug.LogWarning("[FarmPlotSafeSetup] Planted sprite tidak ditemukan di " + PlantedSpritePath);
            if (mutatedSprite == null) Debug.LogWarning("[FarmPlotSafeSetup] Mutated sprite tidak ditemukan di " + MutatedSpritePath);
            if (resonatedSprite == null) Debug.LogWarning("[FarmPlotSafeSetup] Resonated sprite tidak ditemukan di " + ResonatedSpritePath);

            // 2. Kembalikan Seluruh Permukaan Lantai Ground_Tilemap ke Rumput Alami (tile_grass_surface)
            GameObject tilemapGO = GameObject.Find("Ground_Tilemap");
            if (tilemapGO != null)
            {
                Tilemap tilemap = tilemapGO.GetComponent<Tilemap>();
                if (tilemap != null && grassTile != null)
                {
                    Undo.RecordObject(tilemap, "Restore Pure Grass Surface on Tilemap");
                    for (int x = -12; x <= 12; x++)
                    {
                        tilemap.SetTile(new Vector3Int(x, -1, 0), grassTile);
                    }
                    EditorUtility.SetDirty(tilemap);
                    Debug.Log("[FarmPlotSafeSetup] Permukaan Ground_Tilemap (y = -1) 100% dipulihkan ke rumput mulus alami!");
                }
            }

            // 3. Buat / Dapatkan Parent FarmPlots di Scene
            GameObject plotsParent = null;
            var rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var go in rootObjects)
            {
                if (go.name == "FarmPlots")
                {
                    plotsParent = go;
                    break;
                }
            }

            if (plotsParent == null)
            {
                plotsParent = GameObject.Find("FarmPlots");
            }

            if (plotsParent == null)
            {
                plotsParent = new GameObject("FarmPlots");
                Undo.RegisterCreatedObjectUndo(plotsParent, "Create FarmPlots");
            }

            Undo.RecordObject(plotsParent, "Setup FarmPlots Parent");
            plotsParent.SetActive(true);
            plotsParent.transform.position = Vector3.zero;
            plotsParent.transform.rotation = Quaternion.identity;
            plotsParent.transform.localScale = Vector3.one;

            // 4. Konfigurasi 3 Petak Gotik (Gothic Planter Pots)
            Vector3[] plotPositions = new Vector3[]
            {
                new Vector3(-4.5f, 0.0f, 0f),
                new Vector3(-3.0f, 0.0f, 0f),
                new Vector3(-1.5f, 0.0f, 0f)
            };

            for (int i = 0; i < plotPositions.Length; i++)
            {
                string plotName = "FarmPlot_" + (i + 1);
                Transform childTransform = plotsParent.transform.Find(plotName);
                GameObject plotGO;

                if (childTransform == null)
                {
                    GameObject existing = GameObject.Find(plotName);
                    if (existing != null)
                    {
                        plotGO = existing;
                        Undo.SetTransformParent(plotGO.transform, plotsParent.transform, "Parent " + plotName);
                    }
                    else
                    {
                        plotGO = new GameObject(plotName);
                        Undo.RegisterCreatedObjectUndo(plotGO, "Create " + plotName);
                        plotGO.transform.SetParent(plotsParent.transform);
                    }
                }
                else
                {
                    plotGO = childTransform.gameObject;
                }

                Undo.RecordObject(plotGO, "Configure " + plotName);
                plotGO.SetActive(true);
                plotGO.transform.localPosition = plotPositions[i];
                plotGO.transform.localRotation = Quaternion.identity;
                plotGO.transform.localScale = Vector3.one;

                // Hapus SpriteRenderer langsung di root agar tidak dobel/konflik dengan child Pot_Base & Crop_Plant
                SpriteRenderer rootSR = plotGO.GetComponent<SpriteRenderer>();
                if (rootSR != null)
                {
                    Undo.DestroyObjectImmediate(rootSR);
                }

                // BoxCollider2D: isTrigger = true, Size (1.2, 1.8), Offset (-0.218, 0.9)
                // Offset x = -0.218f karena artwork pot di prop_gothic_planter.png berada di area x=[0..459] (center -0.22f)
                BoxCollider2D col = plotGO.GetComponent<BoxCollider2D>();
                if (col == null)
                {
                    col = Undo.AddComponent<BoxCollider2D>(plotGO);
                }
                Undo.RecordObject(col, "Configure " + plotName + " Collider");
                col.isTrigger = true;
                col.size = new Vector2(1.2f, 1.8f);
                col.offset = new Vector2(-0.218f, 0.9f);

                // Child 1: Pot_Base (Menampilkan pot gotik)
                GameObject potBaseGO = GetOrCreateChild(plotGO, "Pot_Base");
                potBaseGO.transform.localPosition = Vector3.zero;
                potBaseGO.transform.localRotation = Quaternion.identity;
                potBaseGO.transform.localScale = new Vector3(0.45f, 0.45f, 1f); // Setinggi pinggang Elian (~0.75m - 0.8m)

                SpriteRenderer potSR = potBaseGO.GetComponent<SpriteRenderer>();
                if (potSR == null) potSR = Undo.AddComponent<SpriteRenderer>(potBaseGO);
                Undo.RecordObject(potSR, "Configure Pot_Base SpriteRenderer");
                potSR.sprite = planterSprite;
                potSR.sortingLayerName = "Default";
                potSR.sortingOrder = 2; // Di depan rumput lantai
                potSR.color = Color.white;
                EditorUtility.SetDirty(potSR);

                // Child 2: Crop_Plant (Menampilkan tunas/mutasi/kristal keluar dari bibir pot)
                // Disinkronkan persis dengan fitting FarmPlot_1: localPosition (-0.218f, 0.566f, 0f), localScale (0.355f, 0.355f, 1f)
                GameObject cropPlantGO = GetOrCreateChild(plotGO, "Crop_Plant");
                cropPlantGO.transform.localPosition = new Vector3(-0.218f, 0.566f, 0f); // Tepat senter di bibir pot
                cropPlantGO.transform.localRotation = Quaternion.identity;
                cropPlantGO.transform.localScale = new Vector3(0.355f, 0.355f, 1f); // Presisi selebar diameter bibir pot

                SpriteRenderer cropSR = cropPlantGO.GetComponent<SpriteRenderer>();
                if (cropSR == null) cropSR = Undo.AddComponent<SpriteRenderer>(cropPlantGO);
                Undo.RecordObject(cropSR, "Configure Crop_Plant SpriteRenderer");
                cropSR.sortingLayerName = "Default";
                cropSR.sortingOrder = 3; // Di depan pot
                cropSR.sprite = null;
                cropSR.color = new Color(1f, 1f, 1f, 0f); // Bersih saat belum ditanam
                EditorUtility.SetDirty(cropSR);

                // Komponen FarmPlot pada Root
                FarmPlot farmPlot = plotGO.GetComponent<FarmPlot>();
                if (farmPlot == null)
                {
                    farmPlot = Undo.AddComponent<FarmPlot>(plotGO);
                }
                Undo.RecordObject(farmPlot, "Configure " + plotName + " FarmPlot");
                farmPlot.currentState = PlotState.Tilled; // Pot sudah siap tanam bibit
                farmPlot.SetSprites(null, null, plantedSprite, mutatedSprite, resonatedSprite);

                SerializedObject so = new SerializedObject(farmPlot);
                var propState = so.FindProperty("currentState");
                var propUntilled = so.FindProperty("untilledSprite");
                var propTilled = so.FindProperty("tilledSprite");
                var propPlanted = so.FindProperty("plantedSprite");
                var propMutated = so.FindProperty("mutatedSprite");
                var propResonated = so.FindProperty("resonatedSprite");
                var propSR = so.FindProperty("spriteRenderer");

                if (propState != null) propState.enumValueIndex = (int)PlotState.Tilled;
                if (propUntilled != null) propUntilled.objectReferenceValue = null;
                if (propTilled != null) propTilled.objectReferenceValue = null;
                if (propPlanted != null) propPlanted.objectReferenceValue = plantedSprite;
                if (propMutated != null) propMutated.objectReferenceValue = mutatedSprite;
                if (propResonated != null) propResonated.objectReferenceValue = resonatedSprite;
                if (propSR != null) propSR.objectReferenceValue = cropSR;
                so.ApplyModifiedProperties();

                farmPlot.UpdateVisuals();

                EditorUtility.SetDirty(farmPlot);
                EditorUtility.SetDirty(col);
                EditorUtility.SetDirty(plotGO);
            }

            // Jika ada FarmPlot_4 lama, nonaktifkan agar hierarki rapi 3 pot
            Transform p4 = plotsParent.transform.Find("FarmPlot_4");
            if (p4 != null)
            {
                Undo.RecordObject(p4.gameObject, "Deactivate FarmPlot_4");
                p4.gameObject.SetActive(false);
            }

            EditorUtility.SetDirty(plotsParent);

            // 5. Sinkronkan PlayerInteraction di Scene
            GameObject playerGO = GameObject.Find("Player");
            if (playerGO != null)
            {
                PlayerInteraction pi = playerGO.GetComponent<PlayerInteraction>();
                if (pi != null)
                {
                    Undo.RecordObject(pi, "Update PlayerInteraction Detection");
                    pi.InteractRange = 1.2f;
                    pi.GroundOffset = new Vector2(0.4f, -0.2f);

                    SerializedObject soPlayer = new SerializedObject(pi);
                    var propRange = soPlayer.FindProperty("interactRange");
                    var propOffset = soPlayer.FindProperty("groundOffset");
                    if (propRange != null) propRange.floatValue = 1.2f;
                    if (propOffset != null) propOffset.vector2Value = new Vector2(0.4f, -0.2f);
                    soPlayer.ApplyModifiedProperties();
                    EditorUtility.SetDirty(pi);
                    Debug.Log("[FarmPlotSafeSetup] PlayerInteraction detection settings disinkronkan: range=1.2f, offset=(0.4f, -0.2f).");
                }
            }

            // 6. Simpan Perubahan Scene
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Debug.Log("<color=#70FFE5>[FarmPlotSafeSetup] Gothic Planter Pot System & Rumput Murni Tilemap berhasil disiapkan!</color>");
        }

        private static GameObject GetOrCreateChild(GameObject parent, string childName)
        {
            Transform child = parent.transform.Find(childName);
            if (child == null)
            {
                GameObject newChild = new GameObject(childName);
                Undo.RegisterCreatedObjectUndo(newChild, "Create " + childName);
                newChild.transform.SetParent(parent.transform);
                return newChild;
            }
            return child.gameObject;
        }
    }
}
#endif
