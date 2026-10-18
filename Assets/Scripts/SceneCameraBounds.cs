using UnityEngine;

namespace MoonlitHomestead
{
    public class SceneCameraBounds : MonoBehaviour
    {
        [Header("Camera Clamping Bounds for this Scene")]
        [SerializeField] private float minX = -10.5f;
        [SerializeField] private float maxX = 10.5f;
        [SerializeField] private float minY = -2.2f;
        [SerializeField] private float maxY = 4.5f;
        [SerializeField] private bool clampCamera = true;

        private void Start()
        {
            ApplyBounds();
        }

        private void OnEnable()
        {
            ApplyBounds();
        }

        public void ApplyBounds()
        {
            CameraFollow2D cam = CameraFollow2D.Instance ?? Object.FindAnyObjectByType<CameraFollow2D>();
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
