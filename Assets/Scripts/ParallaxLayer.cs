using UnityEngine;

namespace MoonlitHomestead
{
    public class ParallaxLayer : MonoBehaviour
    {
        [Header("Camera Reference (Optional)")]
        [Tooltip("Kamera target yang menjadi acuan pergerakan parallax. Jika kosong, akan otomatis mencari Camera.main.")]
        [SerializeField] private Camera targetCamera;

        [Header("Parallax Settings")]
        [Tooltip("Kecepatan gerak horizontal relatif terhadap pergerakan kamera.")]
        [SerializeField] private float parallaxEffectX = 0.3f;
        [Tooltip("Kecepatan gerak vertikal relatif terhadap pergerakan kamera.")]
        [SerializeField] private float parallaxEffectY = 0.1f;

        private Vector3 startPosition;
        private Vector3 startCameraPosition;
        private bool isInitialized = false;

        public float ParallaxEffectX
        {
            get => parallaxEffectX;
            set => parallaxEffectX = value;
        }

        public float ParallaxEffectY
        {
            get => parallaxEffectY;
            set => parallaxEffectY = value;
        }

        public Camera TargetCamera
        {
            get => targetCamera;
            set => targetCamera = value;
        }

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            startPosition = transform.position;

            if (targetCamera != null)
            {
                startCameraPosition = targetCamera.transform.position;
                isInitialized = true;
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null) return;
                startCameraPosition = targetCamera.transform.position;
            }

            Vector3 camDelta = targetCamera.transform.position - startCameraPosition;
            transform.position = startPosition + new Vector3(camDelta.x * parallaxEffectX, camDelta.y * parallaxEffectY, 0f);
        }
    }
}
