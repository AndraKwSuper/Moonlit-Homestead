using UnityEngine;

namespace MoonlitHomestead
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Target & Offset")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0.7f, -10f);
        [SerializeField] private float smoothTime = 0.15f;

        [Header("World Boundaries (Batas Absolut Dunia)")]
        [Tooltip("Batas paling kiri dunia game (misal: tebing kiri di X = -13.5).")]
        [SerializeField] private float worldMinX = -13.5f;
        [Tooltip("Batas paling kanan dunia game (misal: tebing kanan di X = 13.5).")]
        [SerializeField] private float worldMaxX = 13.5f;
        [Tooltip("Batas paling bawah (mencegah kamera melihat ke bawah tanah/void).")]
        [SerializeField] private float worldMinY = -2.2f;
        [Tooltip("Batas atas langit.")]
        [SerializeField] private float worldMaxY = 4.5f;
        [SerializeField] private bool clampCamera = true;

        private Camera cam;
        private Vector3 currentVelocity = Vector3.zero;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public Vector3 Offset
        {
            get => offset;
            set => offset = value;
        }

        public float SmoothTime
        {
            get => smoothTime;
            set => smoothTime = value;
        }

        public float SmoothSpeed
        {
            get => 1f / Mathf.Max(0.001f, smoothTime);
            set => smoothTime = 1f / Mathf.Max(0.001f, value);
        }

        public float WorldMinX
        {
            get => worldMinX;
            set => worldMinX = value;
        }

        public float WorldMaxX
        {
            get => worldMaxX;
            set => worldMaxX = value;
        }

        public float WorldMinY
        {
            get => worldMinY;
            set => worldMinY = value;
        }

        public float WorldMaxY
        {
            get => worldMaxY;
            set => worldMaxY = value;
        }

        public bool ClampCamera
        {
            get => clampCamera;
            set => clampCamera = value;
        }

        // Backward compatibility
        public bool UseBounds
        {
            get => clampCamera;
            set => clampCamera = value;
        }

        public float MinX
        {
            get => worldMinX;
            set => worldMinX = value;
        }

        public float MaxX
        {
            get => worldMaxX;
            set => worldMaxX = value;
        }

        public static CameraFollow2D Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);

            cam = GetComponent<Camera>();
            if (target == null) FindPlayerTarget();
        }

        private void Start()
        {
            if (cam == null) cam = GetComponent<Camera>();
            if (target == null) FindPlayerTarget();
            if (target != null)
            {
                Vector3 startPos = CalculateClampedPosition(target.position + offset);
                transform.position = startPos;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindPlayerTarget();
                if (target == null) return;
            }

            Vector3 targetPos = target.position + offset;
            Vector3 clampedTargetPos = CalculateClampedPosition(targetPos);

            transform.position = Vector3.SmoothDamp(transform.position, clampedTargetPos, ref currentVelocity, smoothTime);
        }

        private Vector3 CalculateClampedPosition(Vector3 desiredPos)
        {
            desiredPos.z = offset.z;
            if (!clampCamera || cam == null) return desiredPos;

            float camHalfHeight = cam.orthographicSize;
            float camHalfWidth = camHalfHeight * cam.aspect;

            float minAllowedX = worldMinX + camHalfWidth;
            float maxAllowedX = worldMaxX - camHalfWidth;

            if (minAllowedX < maxAllowedX)
            {
                desiredPos.x = Mathf.Clamp(desiredPos.x, minAllowedX, maxAllowedX);
            }
            else
            {
                desiredPos.x = (worldMinX + worldMaxX) * 0.5f;
            }

            float minAllowedY = worldMinY + camHalfHeight;
            float maxAllowedY = worldMaxY - camHalfHeight;

            if (minAllowedY < maxAllowedY)
            {
                desiredPos.y = Mathf.Clamp(desiredPos.y, minAllowedY, maxAllowedY);
            }
            else
            {
                desiredPos.y = (worldMinY + worldMaxY) * 0.5f;
            }

            return desiredPos;
        }

        private void FindPlayerTarget()
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO == null) playerGO = GameObject.Find("Player");
            if (playerGO != null) target = playerGO.transform;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector3 center = new Vector3((worldMinX + worldMaxX) * 0.5f, (worldMinY + worldMaxY) * 0.5f, 0f);
            Vector3 size = new Vector3(worldMaxX - worldMinX, worldMaxY - worldMinY, 1f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
