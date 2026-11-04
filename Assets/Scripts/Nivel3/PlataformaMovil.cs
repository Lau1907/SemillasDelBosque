using UnityEngine;

namespace SemillasDelBosque.Nivel3
{
    /// <summary>
    /// Mueve una plataforma de manera continua entre dos puntos del plano 2D.
    /// Utiliza un Rigidbody2D cinemático mediante MovePosition en FixedUpdate
    /// para asegurar una sincronización física correcta con el jugador.
    /// Soporta traslación horizontal, vertical o diagonal, con pausas configurables.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlataformaMovil : MonoBehaviour
    {
        [Header("Puntos de Recorrido")]
        [Tooltip("Posición inicial o primer extremo del recorrido.")]
        [SerializeField] private Vector2 puntoA;

        [Tooltip("Posición final o segundo extremo del recorrido.")]
        [SerializeField] private Vector2 puntoB;

        [Tooltip("Si está activo, inicializa el punto A con la posición que tiene el objeto en la escena.")]
        [SerializeField] private bool usarPosicionInicialComoA = true;

        [Header("Movimiento y Espera")]
        [Tooltip("Velocidad de movimiento en unidades por segundo.")]
        [SerializeField] private float velocidad = 2.0f;

        [Tooltip("Tiempo de espera (en segundos) en cada extremo antes de retornar.")]
        [SerializeField] private float tiempoPausa = 0.5f;

        // Variables de control interno
        private Rigidbody2D rb;
        private Vector2 destinoActual;
        private bool yendoHaciaB = true;
        private bool enPausa = false;
        private float temporizadorPausa = 0f;

        // Propiedades públicas para acceso o ajuste desde el Inspector/Editor
        public Vector2 PuntoA { get => puntoA; set => puntoA = value; }
        public Vector2 PuntoB { get => puntoB; set => puntoB = value; }
        public float Velocidad { get => velocidad; set => velocidad = value; }
        public float TiempoPausa { get => tiempoPausa; set => tiempoPausa = value; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (usarPosicionInicialComoA)
            {
                puntoA = transform.position;
            }

            destinoActual = puntoB;
        }

        private void FixedUpdate()
        {
            // Si la plataforma está pausada en un extremo, descontamos el temporizador
            if (enPausa)
            {
                temporizadorPausa -= Time.fixedDeltaTime;
                if (temporizadorPausa <= 0f)
                {
                    enPausa = false;
                    yendoHaciaB = !yendoHaciaB;
                    destinoActual = yendoHaciaB ? puntoB : puntoA;
                }
                return;
            }

            // Movemos la plataforma con física cinemática hacia el objetivo actual
            Vector2 nuevaPos = Vector2.MoveTowards(rb.position, destinoActual, velocidad * Time.fixedDeltaTime);
            rb.MovePosition(nuevaPos);

            // Verificamos si alcanzamos el punto destino
            if (Vector2.Distance(nuevaPos, destinoActual) < 0.01f)
            {
                rb.position = destinoActual;
                enPausa = true;
                temporizadorPausa = tiempoPausa;
            }
        }

        /// <summary>
        /// Configura los puntos y velocidades desde scripts constructores del editor.
        /// </summary>
        public void ConfigurarPuntos(Vector2 inicio, Vector2 fin, float vel, float pausa)
        {
            puntoA = inicio;
            puntoB = fin;
            velocidad = vel;
            tiempoPausa = pausa;
            usarPosicionInicialComoA = false;
        }

        private void OnDrawGizmosSelected()
        {
            // Visualización amigable de la trayectoria en la ventana de Scene
            Vector3 posA = Application.isPlaying ? (Vector3)puntoA : (usarPosicionInicialComoA ? transform.position : (Vector3)puntoA);
            Vector3 posB = (Vector3)puntoB;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(posA, posB);
            Gizmos.DrawWireSphere(posA, 0.25f);
            Gizmos.DrawWireSphere(posB, 0.25f);
        }
    }
}
