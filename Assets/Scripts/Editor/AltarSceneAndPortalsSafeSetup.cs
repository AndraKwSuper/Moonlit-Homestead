#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MoonlitHomestead.Editor
{
    public static class AltarSceneAndPortalsSafeSetup
    {
        private const string KebunScenePath = "Assets/Scenes/Kebun.unity";
        private const string AltarScenePath = "Assets/Scenes/AltarJam.unity";

        private const string AltarSpritePath = "Assets/Sprites/Environment/Props/prop_chronos_altar.png";
        private const string BgFarSpritePath = "Assets/Sprites/Environment/bg_far_moon.png";
        private const string BgMidSpritePath = "Assets/Sprites/Environment/bg_mid_treeline.png";

        private const string GrassTilePath = "Assets/Sprites/Environment/Tiles/Tile_Grass_Surface.asset";
        private const string CliffLeftTilePath = "Assets/Sprites/Environment/Tiles/Tile_Cliff_Edge_Left.asset";
        private const string CliffRightTilePath = "Assets/Sprites/Environment/Tiles/Tile_Cliff_Edge_Right.asset";
        private const string DirtTilePath = "Assets/Sprites/Environment/Tiles/Tile_Underground_Dirt.asset";

        [InitializeOnLoadMethod]
        private static void AutoRunOnceOnCompile()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool("AltarPortalsSetupV3Run", false))
                {
                    SessionState.SetBool("AltarPortalsSetupV3Run", true);
                    SetupAltarSceneAndPortalsSafe();
                }
            };
        }

        [MenuItem("Moonlit Homestead/Setup Altar Scene and Portals (Safe)")]
        public static void SetupAltarSceneAndPortalsSafe()
        {
            Debug.Log("<color=#70FFE5>[AltarPortalsSetup] Memulai setup Multi-Scene Portals & Scene Altar Jam...</color>");

            // 1. Setup Portal di Scene Kebun
            SetupKebunPortal();

            // 2. Setup Scene AltarJam
            SetupAltarJamScene();

            // 3. Tambahkan ke Build Settings
            UpdateBuildSettings();

            // 4. Pastikan Editor kembali membuka scene Kebun sebagai scene utama
            if (EditorSceneManager.GetActiveScene().path != KebunScenePath)
            {
                EditorSceneManager.OpenScene(KebunScenePath, OpenSceneMode.Single);
            }

            Debug.Log("<color=#70FFE5>[AltarPortalsSetup] Setup Multi-Scene Transition Portal & Scene Altar Jam SUKSES!</color>");
        }

        private static void SetupKebunPortal()
        {
            if (EditorSceneManager.GetActiveScene().path != KebunScenePath)
            {
                EditorSceneManager.OpenScene(KebunScenePath, OpenSceneMode.Single);
            }

            var kebunScene = EditorSceneManager.GetActiveScene();

            // Cari / Buat Portal_To_Altar di X = 13.0, Y = 0.5
            GameObject portalGO = GameObject.Find("Portal_To_Altar");
            if (portalGO == null)
            {
                portalGO = new GameObject("Portal_To_Altar");
                Undo.RegisterCreatedObjectUndo(portalGO, "Create Portal_To_Altar");
            }

            Undo.RecordObject(portalGO, "Configure Portal_To_Altar");
            portalGO.transform.position = new Vector3(13.0f, 0.5f, 0f);
            portalGO.transform.rotation = Quaternion.identity;
            portalGO.transform.localScale = Vector3.one;

            BoxCollider2D portalCol = portalGO.GetComponent<BoxCollider2D>();
            if (portalCol == null)
            {
                portalCol = Undo.AddComponent<BoxCollider2D>(portalGO);
            }
            Undo.RecordObject(portalCol, "Configure Portal_To_Altar Collider");
            portalCol.isTrigger = true;
            portalCol.size = new Vector2(1.5f, 5f);
            portalCol.offset = Vector2.zero;

            ScenePortal portalScript = portalGO.GetComponent<ScenePortal>();
            if (portalScript == null)
            {
                portalScript = Undo.AddComponent<ScenePortal>(portalGO);
            }
            Undo.RecordObject(portalScript, "Configure Portal_To_Altar Script");
            portalScript.TargetSceneName = "AltarJam";
            portalScript.SpawnPositionInTargetScene = new Vector3(-8f, 0f, 0f);
            portalScript.PortalLabel = "Menuju Reruntuhan Altar";
            portalScript.WalkThroughDirection = Vector2.right;

            // Sesuaikan Border_Right di Kebun agar berada di belakang portal (X = 14.2f) sehingga pemain bisa lewat X=13.0
            GameObject borderRight = GameObject.Find("Border_Right");
            if (borderRight != null)
            {
                Undo.RecordObject(borderRight.transform, "Adjust Border_Right Position");
                borderRight.transform.position = new Vector3(14.2f, 2.5f, 0f);
                EditorUtility.SetDirty(borderRight);
            }

            // Sesuaikan CameraFollow2D WorldMaxX ke 13.5 agar kamera bisa mengikuti hingga ke portal
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                CameraFollow2D camFollow = mainCam.GetComponent<CameraFollow2D>();
                if (camFollow != null)
                {
                    Undo.RecordObject(camFollow, "Adjust CameraFollow2D WorldMaxX");
                    camFollow.WorldMaxX = 13.5f;
                    camFollow.WorldMinX = -13.5f;
                    EditorUtility.SetDirty(camFollow);
                }
            }

            EditorUtility.SetDirty(portalCol);
            EditorUtility.SetDirty(portalScript);
            EditorUtility.SetDirty(portalGO);

            EditorSceneManager.MarkSceneDirty(kebunScene);
            EditorSceneManager.SaveScene(kebunScene);
            Debug.Log("[AltarPortalsSetup] Portal_To_Altar berhasil dipasang di Scene Kebun (X = 13.0f).");
        }

        private static void SetupAltarJamScene()
        {
            UnityEngine.SceneManagement.Scene altarScene;
            if (File.Exists(AltarScenePath))
            {
                altarScene = EditorSceneManager.OpenScene(AltarScenePath, OpenSceneMode.Single);
            }
            else
            {
                altarScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }

            // 1. Muat Aset
            Sprite altarSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AltarSpritePath);
            Sprite bgFarSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BgFarSpritePath);
            Sprite bgMidSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BgMidSpritePath);

            Tile grassTile = AssetDatabase.LoadAssetAtPath<Tile>(GrassTilePath);
            Tile cliffLeftTile = AssetDatabase.LoadAssetAtPath<Tile>(CliffLeftTilePath);
            Tile cliffRightTile = AssetDatabase.LoadAssetAtPath<Tile>(CliffRightTilePath);
            Tile dirtTile = AssetDatabase.LoadAssetAtPath<Tile>(DirtTilePath);

            // 2. Root Environment
            GameObject envWorld = GameObject.Find("Environment_World");
            if (envWorld == null) envWorld = new GameObject("Environment_World");
            envWorld.transform.position = Vector3.zero;

            // 3. Parallax Background
            GameObject bgParent = GetOrCreateChild(envWorld, "Background");
            bgParent.transform.localPosition = Vector3.zero;

            GameObject bgFarGO = GetOrCreateChild(bgParent, "Background_Far");
            bgFarGO.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            bgFarGO.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
            SpriteRenderer bgFarSR = GetOrAdd<SpriteRenderer>(bgFarGO);
            bgFarSR.sprite = bgFarSprite;
            bgFarSR.sortingLayerName = "Default";
            bgFarSR.sortingOrder = -10;

            GameObject bgMidGO = GetOrCreateChild(bgParent, "Background_Mid");
            bgMidGO.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            bgMidGO.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
            SpriteRenderer bgMidSR = GetOrAdd<SpriteRenderer>(bgMidGO);
            bgMidSR.sprite = bgMidSprite;
            bgMidSR.sortingLayerName = "Default";
            bgMidSR.sortingOrder = -5;

            // 4. Grid & Ground Tilemap (X = -10 sampai +10)
            GameObject gridGO = GetOrCreateChild(envWorld, "Grid");
            gridGO.transform.localPosition = Vector3.zero;
            Grid grid = GetOrAdd<Grid>(gridGO);
            grid.cellSize = new Vector3(1.0f, 1.0f, 0f);
            grid.cellGap = Vector3.zero;

            GameObject tilemapGO = GetOrCreateChild(gridGO, "Ground_Tilemap");
            tilemapGO.transform.localPosition = Vector3.zero;
            Tilemap tilemap = GetOrAdd<Tilemap>(tilemapGO);
            TilemapRenderer tmRenderer = GetOrAdd<TilemapRenderer>(tilemapGO);
            tmRenderer.sortingLayerName = "Default";
            tmRenderer.sortingOrder = 0;

            // Susun Tile Lantai
            tilemap.ClearAllTiles();
            for (int x = -10; x <= 10; x++)
            {
                // Permukaan rumput / tebing (Y = -1)
                Tile surfaceTile = grassTile;
                if (x == -10 && cliffLeftTile != null) surfaceTile = cliffLeftTile;
                else if (x == 10 && cliffRightTile != null) surfaceTile = cliffRightTile;

                if (surfaceTile != null)
                {
                    tilemap.SetTile(new Vector3Int(x, -1, 0), surfaceTile);
                }

                // Subsurface tanah bawah (Y = -2 s/d -4)
                if (dirtTile != null)
                {
                    tilemap.SetTile(new Vector3Int(x, -2, 0), dirtTile);
                    tilemap.SetTile(new Vector3Int(x, -3, 0), dirtTile);
                    tilemap.SetTile(new Vector3Int(x, -4, 0), dirtTile);
                }
            }

            // Collider Lantai Fisik
            BoxCollider2D groundCol = GetOrAdd<BoxCollider2D>(tilemapGO);
            groundCol.isTrigger = false;
            groundCol.size = new Vector2(22f, 1.0f);
            groundCol.offset = new Vector2(0f, -1.0f); // Top edge collider rata di Y = 0

            // 5. World Borders & Fall Safety
            GameObject bordersParent = GetOrCreateChild(envWorld, "WorldBorders");
            bordersParent.transform.localPosition = Vector3.zero;

            SetupBorder(bordersParent.transform, "Border_Left", new Vector3(-10.8f, 2.5f, 0f), new Vector2(1f, 10f), false, false);
            SetupBorder(bordersParent.transform, "Border_Right", new Vector3(10.8f, 2.5f, 0f), new Vector2(1f, 10f), false, false);
            SetupBorder(bordersParent.transform, "Bottom_Fall_Trigger", new Vector3(0f, -6f, 0f), new Vector2(30f, 2f), true, true);

            // 6. Chronos_Altar di Tengah (X = 0, Y = 0)
            GameObject altarGO = GetOrCreateChild(envWorld, "Chronos_Altar");
            altarGO.transform.localPosition = new Vector3(0f, 0f, 0f);
            altarGO.transform.localScale = Vector3.one;

            SpriteRenderer altarSR = GetOrAdd<SpriteRenderer>(altarGO);
            altarSR.sprite = altarSprite;
            altarSR.sortingLayerName = "Default";
            altarSR.sortingOrder = 2;

            BoxCollider2D altarCol = GetOrAdd<BoxCollider2D>(altarGO);
            altarCol.isTrigger = true;
            altarCol.size = new Vector2(2.5f, 3.0f);
            altarCol.offset = new Vector2(0f, 1.5f);

            ChronosAltar altarScript = GetOrAdd<ChronosAltar>(altarGO);

            // 7. Portal Kembali ke Kebun di Ujung Kiri (X = -9.5, Y = 0.5)
            GameObject returnPortalGO = GetOrCreateChild(envWorld, "Portal_To_Kebun");
            returnPortalGO.transform.localPosition = new Vector3(-9.5f, 0.5f, 0f);
            returnPortalGO.transform.localScale = Vector3.one;

            BoxCollider2D returnPortalCol = GetOrAdd<BoxCollider2D>(returnPortalGO);
            returnPortalCol.isTrigger = true;
            returnPortalCol.size = new Vector2(1.5f, 5f);
            returnPortalCol.offset = Vector2.zero;

            ScenePortal returnPortalScript = GetOrAdd<ScenePortal>(returnPortalGO);
            returnPortalScript.TargetSceneName = "Kebun";
            returnPortalScript.SpawnPositionInTargetScene = new Vector3(12f, 0f, 0f);
            returnPortalScript.PortalLabel = "Kembali ke Kebun";
            returnPortalScript.WalkThroughDirection = Vector2.left;

            // 8. SceneCameraBounds untuk AltarJam
            GameObject boundsGO = GetOrCreateChild(envWorld, "SceneCameraBounds");
            SceneCameraBounds bounds = GetOrAdd<SceneCameraBounds>(boundsGO);
            SerializedObject soBounds = new SerializedObject(bounds);
            var pMinX = soBounds.FindProperty("minX");
            var pMaxX = soBounds.FindProperty("maxX");
            var pMinY = soBounds.FindProperty("minY");
            var pMaxY = soBounds.FindProperty("maxY");
            if (pMinX != null) pMinX.floatValue = -10.0f;
            if (pMaxX != null) pMaxX.floatValue = 10.0f;
            if (pMinY != null) pMinY.floatValue = -1.2f;
            if (pMaxY != null) pMaxY.floatValue = 3.5f;
            soBounds.ApplyModifiedProperties();

            // 9. Main Camera untuk AltarJam (Mencegah layar blank saat scene dibuka / dites standalone)
            GameObject camGO = GameObject.FindWithTag("MainCamera");
            if (camGO == null)
            {
                camGO = GameObject.Find("Main Camera");
            }
            if (camGO == null)
            {
                camGO = new GameObject("Main Camera");
                camGO.tag = "MainCamera";
            }
            camGO.transform.position = new Vector3(0f, 0.7f, -10f);
            Camera cam = GetOrAdd<Camera>(camGO);
            cam.orthographic = true;
            cam.orthographicSize = 3.2f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(11f / 255f, 8f / 255f, 19f / 255f);
            GetOrAdd<AudioListener>(camGO);

            CameraFollow2D camFollow = GetOrAdd<CameraFollow2D>(camGO);
            camFollow.Offset = new Vector3(0f, 0.7f, -10f);
            camFollow.SmoothTime = 0.15f;
            camFollow.WorldMinX = -10.0f;
            camFollow.WorldMaxX = 10.0f;
            camFollow.WorldMinY = -1.2f;
            camFollow.WorldMaxY = 3.5f;
            camFollow.ClampCamera = true;

            // Simpan Scene AltarJam
            EditorSceneManager.MarkSceneDirty(altarScene);
            EditorSceneManager.SaveScene(altarScene, AltarScenePath);
            Debug.Log("[AltarPortalsSetup] Scene AltarJam berhasil dibuat dan disimpan di " + AltarScenePath);
        }

        private static void UpdateBuildSettings()
        {
            var originalScenes = EditorBuildSettings.scenes;
            List<EditorBuildSettingsScene> newScenes = new List<EditorBuildSettingsScene>();
            bool hasKebun = false;
            bool hasAltar = false;

            foreach (var s in originalScenes)
            {
                if (s.path == KebunScenePath) hasKebun = true;
                if (s.path == AltarScenePath) hasAltar = true;
                newScenes.Add(s);
            }

            if (!hasKebun)
            {
                newScenes.Insert(0, new EditorBuildSettingsScene(KebunScenePath, true));
            }
            if (!hasAltar)
            {
                newScenes.Add(new EditorBuildSettingsScene(AltarScenePath, true));
            }

            EditorBuildSettings.scenes = newScenes.ToArray();
            Debug.Log("[AltarPortalsSetup] Build Settings diperbarui: Kebun & AltarJam terdaftar.");
        }

        private static GameObject GetOrCreateChild(GameObject parent, string childName)
        {
            Transform child = parent.transform.Find(childName);
            if (child == null)
            {
                GameObject newChild = new GameObject(childName);
                newChild.transform.SetParent(parent.transform, false);
                return newChild;
            }
            return child.gameObject;
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (comp == null)
            {
                comp = go.AddComponent<T>();
            }
            return comp;
        }

        private static void SetupBorder(Transform parent, string name, Vector3 pos, Vector2 size, bool isTrigger, bool addFallSafety)
        {
            GameObject borderGO = GetOrCreateChild(parent.gameObject, name);
            borderGO.transform.localPosition = pos;
            BoxCollider2D col = GetOrAdd<BoxCollider2D>(borderGO);
            col.isTrigger = isTrigger;
            col.size = size;

            if (addFallSafety)
            {
                FallSafetyTrigger safety = GetOrAdd<FallSafetyTrigger>(borderGO);
                safety.SafeRespawnPosition = new Vector3(-8f, 0.5f, 0f);
            }
        }
    }
}
#endif
