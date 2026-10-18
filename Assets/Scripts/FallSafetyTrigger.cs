using UnityEngine;

namespace MoonlitHomestead
{
    public class FallSafetyTrigger : MonoBehaviour
    {
        [SerializeField] private Vector3 safeRespawnPosition = new Vector3(-4.5f, 0.5f, 0f);

        public Vector3 SafeRespawnPosition
        {
            get => safeRespawnPosition;
            set => safeRespawnPosition = value;
        }

        public Vector3 RespawnPosition
        {
            get => safeRespawnPosition;
            set => safeRespawnPosition = value;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
            {
                Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                }
                other.transform.position = safeRespawnPosition;
                Debug.Log("[FallSafety] Player dikembalikan ke posisi aman.");
            }
        }
    }
}
