#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MoonlitHomestead.Editor
{
    public static class PlayerFocusCameraSetup
    {
        [InitializeOnLoadMethod]
        private static void AutoRunOnceOnCompile()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool("PlayerFocusCameraSetupRun", false))
                {
                    SessionState.SetBool("PlayerFocusCameraSetupRun", true);
                    SetupCameraFocusAndPlayerLayering();
                }
            };
        }

        [MenuItem("Moonlit Homestead/Setup Camera Focus & Player Layering (Safe)")]
        public static void SetupCameraFocusAndPlayerLayering()
        {
            // 1. Setting Main Camera (Zoom-In & Smooth CameraFollow2D)
            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = Object.FindAnyObjectByType<Camera>();
            }

            if (cam == null)
            {
                Debug.LogError("[PlayerFocusCameraSetup] Main Camera tidak ditemukan di scene.");
                return;
            }

            Undo.RecordObject(cam, "Setup Camera Focus");
            cam.orthographic = true;
            cam.orthographicSize = 3.2f; // Zoom-in dari 4.5 ke 3.2 agar karakter Elian menjadi fokus utama

            CameraFollow2D follow = cam.GetComponent<CameraFollow2D>();
            if (follow == null)
            {
                follow = Undo.AddComponent<CameraFollow2D>(cam.gameObject);
            }

            GameObject playerGO = GameObject.Find("Player");
            if (playerGO != null)
            {
                follow.Target = playerGO.transform;
            }

            follow.Offset = new Vector3(0f, 0.7f, -10f); // Komposisi seimbang sedikit di atas kaki Elian
            follow.SmoothTime = 0.15f;
            follow.WorldMinX = -13.5f;
            follow.WorldMaxX = 13.5f;
            follow.WorldMinY = -2.2f;
            follow.WorldMaxY = 4.5f;
            follow.ClampCamera = true;
            EditorUtility.SetDirty(follow);

            // 2. Setting Layering Player (Selalu Terlihat di Depan Ground & Well)
            if (playerGO != null)
            {
                SpriteRenderer playerSR = playerGO.GetComponentInChildren<SpriteRenderer>();
                if (playerSR != null)
                {
                    Undo.RecordObject(playerSR, "Set Player Layering");
                    playerSR.sortingLayerName = "Default";
                    playerSR.sortingOrder = 5; // Lebih tinggi dari Ground (order 0) dan Ancient_Well (order 1)
                    EditorUtility.SetDirty(playerSR);
                    Debug.Log("[PlayerFocusCameraSetup] Player SpriteRenderer sortingOrder di-set ke 5 (selalu di lapisan depan).");
                }
            }
            else
            {
                Debug.LogWarning("[PlayerFocusCameraSetup] GameObject 'Player' tidak ditemukan di scene.");
            }

            // 3. Simpan perubahan scene
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Debug.Log("<color=#70FFE5>[PlayerFocusCameraSetup] Kamera Fokus (Zoom 3.2, Smooth Follow) & Player Layering (Order 5) berhasil disinkronkan!</color>");
        }
    }
}
#endif
