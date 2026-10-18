#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MoonlitHomestead.Editor
{
    public static class WorldBordersSafeSetup
    {
        [InitializeOnLoadMethod]
        private static void AutoRunOnceOnCompile()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool("WorldBordersSafeSetupRun", false))
                {
                    SessionState.SetBool("WorldBordersSafeSetupRun", true);
                    SetupWorldBordersAndCameraClamping();
                }
            };
        }

        [MenuItem("Moonlit Homestead/Setup World Borders & Camera Clamping (Safe)")]
        public static void SetupWorldBordersAndCameraClamping()
        {
            // 1. Parent World_Borders
            GameObject bordersParent = GameObject.Find("World_Borders");
            if (bordersParent == null)
            {
                bordersParent = new GameObject("World_Borders");
                Undo.RegisterCreatedObjectUndo(bordersParent, "Create World_Borders Parent");
            }
            bordersParent.transform.position = Vector3.zero;

            // 2. Child Colliders
            SetupBorderCollider(bordersParent.transform, "Border_Left", new Vector3(-9f, 2.5f, 0f), new Vector2(1f, 10f), false, false);
            SetupBorderCollider(bordersParent.transform, "Border_Right", new Vector3(9f, 2.5f, 0f), new Vector2(1f, 10f), false, false);
            SetupBorderCollider(bordersParent.transform, "Bottom_Fall_Trigger", new Vector3(0f, -6f, 0f), new Vector2(40f, 2f), true, true);

            // 3. Konfigurasi Kamera (Camera.main)
            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = Object.FindAnyObjectByType<Camera>();
            }

            if (cam != null)
            {
                Undo.RecordObject(cam, "Setup Camera Clamping");
                CameraFollow2D follow = cam.GetComponent<CameraFollow2D>();
                if (follow == null)
                {
                    follow = Undo.AddComponent<CameraFollow2D>(cam.gameObject);
                }

                Undo.RecordObject(follow, "Setup CameraFollow2D Boundaries");
                follow.Offset = new Vector3(0f, 0.7f, -10f);
                follow.SmoothTime = 0.15f;
                follow.WorldMinX = -9f;
                follow.WorldMaxX = 9f;
                follow.WorldMinY = -2.2f;
                follow.WorldMaxY = 4.5f;
                follow.ClampCamera = true;

                GameObject playerGO = GameObject.Find("Player");
                if (playerGO != null)
                {
                    follow.Target = playerGO.transform;
                }

                EditorUtility.SetDirty(follow);
                EditorUtility.SetDirty(cam);
                Debug.Log("[WorldBordersSafeSetup] CameraFollow2D boundaries & clamping berhasil di-set.");
            }
            else
            {
                Debug.LogWarning("[WorldBordersSafeSetup] Main Camera tidak ditemukan di scene.");
            }

            // 4. Simpan Scene
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Debug.Log("<color=#70FFE5>[WorldBordersSafeSetup] World Borders, Fall Protection, & Anti-Tembus Camera Clamping berhasil disiapkan!</color>");
        }

        private static void SetupBorderCollider(Transform parent, string childName, Vector3 localPos, Vector2 size, bool isTrigger, bool addFallSafety)
        {
            Transform child = parent.Find(childName);
            GameObject go;
            if (child == null)
            {
                go = new GameObject(childName);
                Undo.RegisterCreatedObjectUndo(go, "Create " + childName);
                go.transform.SetParent(parent);
            }
            else
            {
                go = child.gameObject;
            }

            Undo.RecordObject(go.transform, "Set " + childName + " Transform");
            go.transform.localPosition = localPos;

            BoxCollider2D col = go.GetComponent<BoxCollider2D>();
            if (col == null)
            {
                col = Undo.AddComponent<BoxCollider2D>(go);
            }

            Undo.RecordObject(col, "Set " + childName + " Collider");
            col.size = size;
            col.isTrigger = isTrigger;
            EditorUtility.SetDirty(col);

            if (addFallSafety)
            {
                FallSafetyTrigger safety = go.GetComponent<FallSafetyTrigger>();
                if (safety == null)
                {
                    safety = Undo.AddComponent<FallSafetyTrigger>(go);
                }

                Undo.RecordObject(safety, "Set FallSafetyTrigger");
                safety.SafeRespawnPosition = new Vector3(-4.5f, 0.5f, 0f);
                EditorUtility.SetDirty(safety);
            }

            EditorUtility.SetDirty(go);
        }
    }
}
#endif
