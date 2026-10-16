using UnityEngine;

namespace MoonlitHomestead
{
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Target & Offset")]
        [Tooltip("Transform target yang diikuti kamera. Otomatis mencari Player jika kosong.")]
        [SerializeField] private Transform target;
        [Tooltip("Offset posisi kamera relatif terhadap target.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 0.8f, -10f);
        [Tooltip("Kecepatan lerp penghalusan pergerakan kamera.")]
        [SerializeField] private float smoothSpeed = 5f;

        [Header("Batas Gerak Kamera (Clamping)")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private float minX = -10f;
        [SerializeField] private float maxX = 10f;

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

        public float SmoothSpeed
        {
            get => smoothSpeed;
            set => smoothSpeed = value;
        }

        public float SmoothTime
        {
            get => 1f / Mathf.Max(0.01f, smoothSpeed);
            set => smoothSpeed = 1f / Mathf.Max(0.01f, value);
        }

        public bool UseBounds
        {
            get => useBounds;
            set => useBounds = value;
        }

        public float MinX
        {
            get => minX;
            set => minX = value;
        }

        public float MaxX
        {
            get => maxX;
            set => maxX = value;
        }

        private void Awake()
        {
            if (target == null)
            {
                FindPlayerTarget();
            }
        }

        private void Start()
        {
            if (target == null)
            {
                FindPlayerTarget();
            }

            if (target != null)
            {
                Vector3 targetPos = target.position + offset;
                targetPos.z = offset.z;
                if (useBounds)
                {
                    targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
                }
                transform.position = targetPos;
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
            targetPos.z = offset.z;

            if (useBounds)
            {
                targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            }

            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
        }

        private void FindPlayerTarget()
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO == null) playerGO = GameObject.Find("Player");
            if (playerGO != null)
            {
                target = playerGO.transform;
            }
        }
    }
}
