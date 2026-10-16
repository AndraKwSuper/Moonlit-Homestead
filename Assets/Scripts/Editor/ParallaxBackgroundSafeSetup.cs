#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MoonlitHomestead.Editor
{
    public static class ParallaxBackgroundSafeSetup
    {
        private const string FarMoonPath = "Assets/Sprites/Environment/bg_far_moon.png";
        private const string MidTreelinePath = "Assets/Sprites/Environment/bg_mid_treeline.png";

        [InitializeOnLoadMethod]
        private static void AutoRunOnceOnCompile()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool("ParallaxSetupRun", false))
                {
                    SessionState.SetBool("ParallaxSetupRun", true);
                    if (GameObject.Find("Parallax_Background") == null)
                    {
                        AddParallaxBackgroundSafe();
                    }
                }
            };
        }

        [MenuItem("Moonlit Homestead/Add Parallax Background (Safe)")]
        public static void AddParallaxBackgroundSafe()
        {
            // 1. Pastikan Sorting Layer "Background" terdaftar
            EnsureSortingLayer("Background");

            // 2. Load Sprite Aset Environment
            Sprite farMoonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FarMoonPath);
            Sprite midTreelineSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MidTreelinePath);

            if (farMoonSprite == null)
            {
                Debug.LogError($"[ParallaxSafeSetup] Sprite tidak ditemukan di path: {FarMoonPath}");
                return;
            }

            if (midTreelineSprite == null)
            {
                Debug.LogError($"[ParallaxSafeSetup] Sprite tidak ditemukan di path: {MidTreelinePath}");
                return;
            }

            // 3. Setup Parent Root: Parallax_Background (Z = 5)
            // Kamera berada di Y = 1.2, Z = -10
            Vector3 cameraCenter = new Vector3(0f, 1.2f, 0f);
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraCenter = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, 0f);
            }

            GameObject root = GameObject.Find("Parallax_Background");
            if (root == null)
            {
                root = new GameObject("Parallax_Background");
                Undo.RegisterCreatedObjectUndo(root, "Create Parallax_Background");
            }

            root.transform.position = new Vector3(cameraCenter.x, cameraCenter.y, 5f);
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            // 4. Setup Far_Moon_Layer (Z = 10, Sorting Layer: "Background" / Order: -10)
            GameObject farLayer = GetOrCreateChild(root, "Far_Moon_Layer");
            farLayer.transform.position = new Vector3(cameraCenter.x, cameraCenter.y, 10f); // World Z = 10
            farLayer.transform.rotation = Quaternion.identity;
            farLayer.transform.localScale = new Vector3(1.8f, 1.8f, 1f); // Menutupi viewport 16:9 secara penuh

            SpriteRenderer farSR = GetOrAdd<SpriteRenderer>(farLayer);
            farSR.sprite = farMoonSprite;
            farSR.sortingLayerName = "Background";
            farSR.sortingOrder = -10;
            farSR.color = Color.white;

            ParallaxLayer farParallax = GetOrAdd<ParallaxLayer>(farLayer);
            farParallax.ParallaxEffectX = 0.05f; // Hampir diam di langit
            farParallax.ParallaxEffectY = 0.02f;
            farParallax.Initialize();

            // 5. Setup Mid_Treeline_Layer (Z = 8, Sorting Layer: "Background" / Order: -5)
            GameObject midLayer = GetOrCreateChild(root, "Mid_Treeline_Layer");
            midLayer.transform.position = new Vector3(cameraCenter.x, cameraCenter.y, 8f); // World Z = 8
            midLayer.transform.rotation = Quaternion.identity;
            midLayer.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

            SpriteRenderer midSR = GetOrAdd<SpriteRenderer>(midLayer);
            midSR.sprite = midTreelineSprite;
            midSR.sortingLayerName = "Background";
            midSR.sortingOrder = -5;
            midSR.color = Color.white;

            ParallaxLayer midParallax = GetOrAdd<ParallaxLayer>(midLayer);
            midParallax.ParallaxEffectX = 0.25f; // Bergerak lambat mengikuti pergerakan kamera
            midParallax.ParallaxEffectY = 0.1f;
            midParallax.Initialize();

            // 6. Tandai dan simpan scene
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Debug.Log("<color=#70FFE5>[ParallaxSafeSetup] Multi-Layer Parallax Background ('Parallax_Background') berhasil dipasang secara non-destructive!</color>");
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

        private static void EnsureSortingLayer(string layerName)
        {
            UnityEngine.Object[] tagManagers = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (tagManagers == null || tagManagers.Length == 0) return;

            SerializedObject tagManager = new SerializedObject(tagManagers[0]);
            SerializedProperty sortingLayers = tagManager.FindProperty("m_SortingLayers");
            if (sortingLayers == null) return;

            for (int i = 0; i < sortingLayers.arraySize; i++)
            {
                SerializedProperty entry = sortingLayers.GetArrayElementAtIndex(i);
                SerializedProperty nameProp = entry.FindPropertyRelative("name");
                if (nameProp != null && nameProp.stringValue == layerName)
                {
                    return; // Sudah ada
                }
            }

            // Insert di indeks 0 agar dirender di belakang Default layer
            sortingLayers.InsertArrayElementAtIndex(0);
            SerializedProperty newLayer = sortingLayers.GetArrayElementAtIndex(0);
            newLayer.FindPropertyRelative("name").stringValue = layerName;
            newLayer.FindPropertyRelative("uniqueID").intValue = 31254124;
            newLayer.FindPropertyRelative("locked").boolValue = false;
            tagManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
