using UnityEngine;

namespace SemillasDelBosque.Nivel3
{
    /// <summary>
    /// Controla el patrullaje horizontal de la Sombra Espinosa entre dos posiciones en el eje X.
    /// Invierte automáticamente la orientación del sprite según la dirección en que camina.
    /// </summary>
    public class PatrullaSombra : MonoBehaviour
    {
        [Header("Límites de Patrullaje")]
        [Tooltip("Límite horizontal izquierdo (coordenada X mínima).")]
        [SerializeField] private float xMinimo;

        [Tooltip("Límite horizontal derecho (coordenada X máxima).")]
        [SerializeField] private float xMaximo;

        [Header("Velocidad y Gráficos")]
        [Tooltip("Velocidad de avance en unidades por segundo.")]
        [SerializeField] private float velocidad = 1.8f;

        [Tooltip("Voltea el SpriteRenderer horizontalmente según la dirección.")]
        [SerializeField] private bool voltearSprite = true;

        private SpriteRenderer spriteRenderer;
        private bool moviendoHaciaDerecha = true;

        public float XMinimo { get => xMinimo; set => xMinimo = value; }
        public float XMaximo { get => xMaximo; set => xMaximo = value; }
        public float Velocidad { get => velocidad; set => velocidad = value; }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Si los límites están en cero, calculamos un rango predeterminado de 2.5 unidades a cada lado
            if (Mathf.Approximately(xMinimo, 0f) && Mathf.Approximately(xMaximo, 0f))
            {
                xMinimo = transform.position.x - 2.5f;
                xMaximo = transform.position.x + 2.5f;
            }
        }

        private void OnEnable()
        {
            ActualizarOrientacion();
        }

        private void Update()
        {
            float paso = velocidad * Time.deltaTime;

            if (moviendoHaciaDerecha)
            {
                transform.position = new Vector3(transform.position.x + paso, transform.position.y, transform.position.z);
                if (transform.position.x >= xMaximo)
                {
                    transform.position = new Vector3(xMaximo, transform.position.y, transform.position.z);
                    moviendoHaciaDerecha = false;
                    ActualizarOrientacion();
                }
            }
            else
            {
                transform.position = new Vector3(transform.position.x - paso, transform.position.y, transform.position.z);
                if (transform.position.x <= xMinimo)
                {
                    transform.position = new Vector3(xMinimo, transform.position.y, transform.position.z);
                    moviendoHaciaDerecha = true;
                    ActualizarOrientacion();
                }
            }
        }

        public void ConfigurarRango(float minX, float maxX, float vel)
        {
            xMinimo = minX;
            xMaximo = maxX;
            velocidad = vel;
        }

        private void ActualizarOrientacion()
        {
            if (voltearSprite && spriteRenderer != null)
            {
                // El spritesheet de la Sombra mira hacia la derecha por defecto
                spriteRenderer.flipX = !moviendoHaciaDerecha;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 posMin = new Vector3(xMinimo, transform.position.y, transform.position.z);
            Vector3 posMax = new Vector3(xMaximo, transform.position.y, transform.position.z);
            Gizmos.DrawLine(posMin, posMax);
            Gizmos.DrawWireSphere(posMin, 0.2f);
            Gizmos.DrawWireSphere(posMax, 0.2f);
        }
    }
}
