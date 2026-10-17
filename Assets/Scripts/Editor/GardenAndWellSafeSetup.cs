#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MoonlitHomestead.Editor
{
    public static class GardenAndWellSafeSetup
    {
        private const string CliffLeftSpritePath = "Assets/Sprites/Environment/Tiles/tile_cliff_edge_left.png";
        private const string GrassSpritePath = "Assets/Sprites/Environment/Tiles/tile_grass_surface.png";
        private const string CliffRightSpritePath = "Assets/Sprites/Environment/Tiles/tile_cliff_edge_right.png";
        private const string UndergroundDirtSpritePath = "Assets/Sprites/Environment/Tiles/tile_underground_dirt.png";
        private const string TilledSoilSpritePath = "Assets/Sprites/Environment/Tiles/tile_tilled_soil.png";
        private const string AncientWellSpritePath = "Assets/Sprites/Environment/Props/prop_ancient_well.png";

        private const string TileAssetFolder = "Assets/Sprites/Environment/Tiles/";

        [InitializeOnLoadMethod]
        private static void AutoRunOnceOnCompile()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool("GardenWellSetupV3Run", false))
                {
                    SessionState.SetBool("GardenWellSetupV3Run", true);
                    SetupGardenAndWellSafe();
                }
            };
        }

        [MenuItem("Moonlit Homestead/Setup Garden & Well (Safe)")]
        public static void SetupGardenAndWellSafe()
        {
            // 1. Load Sprite Aset (256x256 px, PPU 256 -> 1.0 unit per tile)
            Sprite cliffLeftSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CliffLeftSpritePath);
            Sprite grassSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GrassSpritePath);
            Sprite cliffRightSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CliffRightSpritePath);
            Sprite undergroundDirtSprite = AssetDatabase.LoadAssetAtPath<Sprite>(UndergroundDirtSpritePath);
            Sprite tilledSoilSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TilledSoilSpritePath);
            Sprite ancientWellSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AncientWellSpritePath);

            if (grassSprite == null || ancientWellSprite == null)
            {
                Debug.LogError("[GardenWellSafeSetup] Sprite tile atau sumur tidak ditemukan di Assets/Sprites/Environment/");
                return;
            }

            // 2. Dapatkan atau Buat Tile Asset (ScriptableObject)
            Tile cliffLeftTile = GetOrCreateTile(TileAssetFolder + "Tile_Cliff_Edge_Left.asset", cliffLeftSprite);
            Tile grassTile = GetOrCreateTile(TileAssetFolder + "Tile_Grass_Surface.asset", grassSprite);
            Tile cliffRightTile = GetOrCreateTile(TileAssetFolder + "Tile_Cliff_Edge_Right.asset", cliffRightSprite);
            Tile undergroundDirtTile = GetOrCreateTile(TileAssetFolder + "Tile_Underground_Dirt.asset", undergroundDirtSprite);
            if (tilledSoilSprite != null)
            {
                GetOrCreateTile(TileAssetFolder + "Tile_Tilled_Soil.asset", tilledSoilSprite);
            }

            // 3. Buat Parent Environment_World
            GameObject envWorld = GameObject.Find("Environment_World");
            if (envWorld == null)
            {
                envWorld = new GameObject("Environment_World");
                Undo.RegisterCreatedObjectUndo(envWorld, "Create Environment_World");
            }
            envWorld.transform.position = Vector3.zero;

            // 4. Konfigurasi Grid (cellSize = 1.0 x 1.0 x 0, cellGap = 0)
            GameObject gridGO = GetOrCreateChild(envWorld, "Grid");
            gridGO.transform.position = Vector3.zero;
            gridGO.transform.rotation = Quaternion.identity;
            gridGO.transform.localScale = Vector3.one;

            Grid grid = GetOrAdd<Grid>(gridGO);
            grid.cellSize = new Vector3(1.0f, 1.0f, 0f);
            grid.cellGap = Vector3.zero;

            // 5. Konfigurasi Ground_Tilemap
            GameObject tilemapGO = GetOrCreateChild(gridGO, "Ground_Tilemap");
            tilemapGO.transform.localPosition = Vector3.zero;
            tilemapGO.transform.localRotation = Quaternion.identity;
            tilemapGO.transform.localScale = Vector3.one;

            Tilemap tilemap = GetOrAdd<Tilemap>(tilemapGO);
            TilemapRenderer tmRenderer = GetOrAdd<TilemapRenderer>(tilemapGO);
            tmRenderer.sortingLayerName = "Default";
            tmRenderer.sortingOrder = 0;

            // Hapus TilemapCollider2D jika ada (Mencegah karakter tersangkut / ghost vertices)
            TilemapCollider2D oldTilemapCol = tilemapGO.GetComponent<TilemapCollider2D>();
            if (oldTilemapCol != null)
            {
                Object.DestroyImmediate(oldTilemapCol);
                Debug.Log("[GardenWellSafeSetup] TilemapCollider2D dihapus untuk mencegah bug ghost vertices.");
            }

            // Pasang SATU BoxCollider2D satu bentang horizontal rata
            BoxCollider2D groundCol = GetOrAdd<BoxCollider2D>(tilemapGO);
            groundCol.isTrigger = false;
            groundCol.size = new Vector2(28f, 1.0f);
            groundCol.offset = new Vector2(0f, -1.0f); // Permukaan atas collider berada tepat di bawah telapak kaki Elian tanpa celah vertikal

            Tile tilledSoilTile = (tilledSoilSprite != null) ? GetOrCreateTile(TileAssetFolder + "Tile_Tilled_Soil.asset", tilledSoilSprite) : null;

            // 6. Hapus dan susun ulang tile sepanjang X dari -13 sampai +13
            tilemap.ClearAllTiles();
            for (int x = -13; x <= 13; x++)
            {
                // Permukaan rumput alami (y = -1)
                Tile surfaceTile = grassTile;
                if (x == -13) surfaceTile = cliffLeftTile;
                else if (x == 13) surfaceTile = cliffRightTile;

                tilemap.SetTile(new Vector3Int(x, -1, 0), surfaceTile);

                // Lapisan tanah bawah (y = -2 dan y = -3) secara penuh dari X = -13 s/d 13
                if (undergroundDirtTile != null)
                {
                    tilemap.SetTile(new Vector3Int(x, -2, 0), undergroundDirtTile);
                    tilemap.SetTile(new Vector3Int(x, -3, 0), undergroundDirtTile);
                }
            }

            // 7. Pembersihan Objek Lama
            GameObject oldGround = GameObject.Find("Ground");
            if (oldGround != null)
            {
                oldGround.SetActive(false);
                Debug.Log("[GardenWellSafeSetup] Lantai prototipe lama ('Ground') dinonaktifkan (SetActive(false)).");
            }

            // 8. Pasang Sumur Kuno (Ancient_Well) menempel di lantai tanah (X = 3.5, Y = -0.5)
            GameObject wellGO = GameObject.Find("Ancient_Well");
            if (wellGO == null)
            {
                wellGO = GetOrCreateChild(envWorld, "Ancient_Well");
            }

            wellGO.transform.position = new Vector3(3.5f, -0.5f, 0f);
            wellGO.transform.rotation = Quaternion.identity;
            wellGO.transform.localScale = Vector3.one;

            SpriteRenderer wellSR = GetOrAdd<SpriteRenderer>(wellGO);
            wellSR.sprite = ancientWellSprite;
            wellSR.sortingLayerName = "Default";
            wellSR.sortingOrder = 1;
            wellSR.color = Color.white;

            BoxCollider2D wellCol = GetOrAdd<BoxCollider2D>(wellGO);
            wellCol.isTrigger = true;
            wellCol.size = new Vector2(2f, 2f);
            wellCol.offset = new Vector2(0f, 1f);

            GetOrAdd<AncientWell>(wellGO);

            // 9. Simpan scene
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Debug.Log("<color=#70FFE5>[GardenWellSafeSetup] Setup Tilemap Kebun (PPU 256, CellSize 1x1, BoxCollider2D 28x1) & Sumur Kuno berhasil disinkronkan!</color>");
        }

        private static Tile GetOrCreateTile(string assetPath, Sprite sprite)
        {
            if (sprite == null) return null;

            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(assetPath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                AssetDatabase.CreateAsset(tile, assetPath);
            }
            else
            {
                tile.sprite = sprite;
                EditorUtility.SetDirty(tile);
            }
            return tile;
        }

        private static GameObject GetOrCreateChild(GameObject parent, string childName)
        {
            Transform existing = parent.transform.Find(childName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject newChild = new GameObject(childName);
            Undo.RegisterCreatedObjectUndo(newChild, $"Create {childName}");
            newChild.transform.SetParent(parent.transform);
            return newChild;
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (comp == null)
            {
                comp = go.AddComponent<T>();
                Undo.RegisterCreatedObjectUndo(comp, $"Add {typeof(T).Name}");
            }
            return comp;
        }
    }
}
#endif
