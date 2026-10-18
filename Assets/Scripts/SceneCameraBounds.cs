using UnityEngine;

namespace MoonlitHomestead
{
    public class SceneCameraBounds : MonoBehaviour
    {
        [Header("Camera Clamping Bounds for this Scene")]
        [Tooltip("Aktifkan jika ingin SceneCameraBounds menimpa nilai CameraFollow2D di scene ini.")]
        [SerializeField] private bool overrideCameraBounds = false;
        [SerializeField] private float minX = -6.15f;
        [SerializeField] private float maxX = 6.15f;
        [SerializeField] private float minY = -2.2f;
        [SerializeField] private float maxY = 4.5f;
        [SerializeField] private bool clampCamera = true;

        private void Start()
        {
            if (overrideCameraBounds)
            {
                ApplyBounds();
            }
        }

        private void OnEnable()
        {
            if (overrideCameraBounds)
            {
                ApplyBounds();
            }
        }

        public void ApplyBounds()
        {
            CameraFollow2D cam = Object.FindAnyObjectByType<CameraFollow2D>();
            if (cam != null)
            {
                cam.WorldMinX = minX;
                cam.WorldMaxX = maxX;
                cam.WorldMinY = minY;
                cam.WorldMaxY = maxY;
                cam.ClampCamera = clampCamera;
            }
        }
    }
}
