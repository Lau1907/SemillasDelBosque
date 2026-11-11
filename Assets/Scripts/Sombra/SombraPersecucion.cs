using UnityEngine;
using SemillasDelBosque.Nivel3;
using SemillasDelBosque.Audio;

namespace SemillasDelBosque.Sombra
{
    /// <summary>
    /// SombraPersecucion: Controla los dos estados de la Sombra Espinosa (Patrulla y Persecución).
    /// 
    /// - En Patrulla: Reutiliza el componente PatrullaSombra existente para moverse entre límites.
    /// - En Persecución: Se activa cuando Luna se acerca a menos de distanciaDeteccion (6u).
    ///   Persigue a Luna horizontalmente (solo en X) sin rebasar los límites xMinimo y xMaximo.
    /// - Histéresis: Solo regresa a patrulla cuando Luna se aleja más de distanciaPerdida (9u).
    /// - Sonido y Animación: Reproduce la alerta 'sombra' al detectar y conmuta a la animación
    ///   o al sprite con ojos rojos de persecución.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SombraPersecucion : MonoBehaviour
    {
        public enum EstadoSombra
        {
            Patrulla,
            Persecucion
        }

        [Header("Distancias de Detección (Histéresis)")]
        [Tooltip("Distancia hacia Luna para iniciar la persecución.")]
        [SerializeField] private float distanciaDeteccion = 6f;

        [Tooltip("Distancia hacia Luna para perderla de vista y volver a patrullar.")]
        [SerializeField] private float distanciaPerdida = 9f;

        [Header("Velocidades")]
        [Tooltip("Velocidad de patrulla habitual (sincronizada con PatrullaSombra).")]
        [SerializeField] private float velocidadPatrulla = 1.8f;

        [Tooltip("Velocidad de desplazamiento horizontal durante la persecución.")]
        [SerializeField] private float velocidadPersecucion = 3.2f;

        [Header("Efectos de Audio")]
        [Tooltip("AudioClip 'sombra' que suena como alerta al iniciar la persecución.")]
        [SerializeField] private AudioClip clipAlerta;

        [Range(0f, 1f)]
        [Tooltip("Volumen para el sonido de alerta.")]
        [SerializeField] private float volumenAlerta = 1.0f;

        [Tooltip("AudioSource local de la sombra para reproducir el efecto de alerta.")]
        [SerializeField] private AudioSource audioSource;

        [Header("Gráficos y Sprite")]
        [Tooltip("Sprite de patrulla con ojos amarillos (SombraEspinosa_spritesheet_0).")]
        [SerializeField] private Sprite spritePatrulla;

        [Tooltip("Sprite de persecución con ojos rojos (SombraEspinosa_spritesheet_1).")]
        [SerializeField] private Sprite spritePersecucion;

        [Tooltip("Aplica un tinte rojizo al sprite si no se encuentra un Animator con parámetros.")]
        [SerializeField] private bool tintarAlertaSiNoHayAnimacion = true;

        [SerializeField] private Color colorAlerta = new Color(1f, 0.45f, 0.45f, 1f);

        // Componentes internos
        private PatrullaSombra patrullaSombra;
        private SpriteRenderer spriteRenderer;
        private Animator animator;

        // Referencia al transform del jugador
        private Transform jugadorTransform;

        // Estado actual de la máquina de estados
        private EstadoSombra estadoActual = EstadoSombra.Patrulla;
        private bool estaPersiguiendo = false;
        private Color colorOriginal = Color.white;

        // Parámetro de animación
        private bool tieneParametroPersecucion = false;
        private int idParametroPersecucion;

        // Límites horizontales obtenidos de PatrullaSombra
        private float limiteMinX;
        private float limiteMaxX;

        // Propiedades públicas para el editor y consultas
        public EstadoSombra EstadoActual => estadoActual;
        public float DistanciaDeteccion { get => distanciaDeteccion; set => distanciaDeteccion = value; }
        public float DistanciaPerdida { get => distanciaPerdida; set => distanciaPerdida = value; }
        public float VelocidadPatrulla { get => velocidadPatrulla; set => velocidadPatrulla = value; }
        public float VelocidadPersecucion { get => velocidadPersecucion; set => velocidadPersecucion = value; }
        public AudioClip ClipAlerta { get => clipAlerta; set => clipAlerta = value; }
        public Sprite SpritePatrulla { get => spritePatrulla; set => spritePatrulla = value; }
        public Sprite SpritePersecucion { get => spritePersecucion; set => spritePersecucion = value; }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            patrullaSombra = GetComponent<PatrullaSombra>();

            // Configurar AudioSource para reproducir efectos
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            audioSource.playOnAwake = false;
            audioSource.loop = false;

            // Guardar color y sprite original
            if (spriteRenderer != null)
            {
                colorOriginal = spriteRenderer.color;
                if (spritePatrulla == null)
                {
                    spritePatrulla = spriteRenderer.sprite;
                }
            }

            // Comprobar si hay un Animator con el parámetro bool "Persecucion"
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                foreach (var parametro in animator.parameters)
                {
                    if (parametro.type == AnimatorControllerParameterType.Bool && parametro.name == "Persecucion")
                    {
                        tieneParametroPersecucion = true;
                        idParametroPersecucion = Animator.StringToHash("Persecucion");
                        break;
                    }
                }
            }

            // Inicializar y sincronizar límites desde PatrullaSombra
            ActualizarLimitesDesdePatrulla();
        }

        private void Start()
        {
            BuscarJugador();

            // Sincronizar velocidad de patrulla inicial
            if (patrullaSombra != null)
            {
                patrullaSombra.Velocidad = velocidadPatrulla;
            }

            // Arrancar en estado de patrulla
            CambiarEstado(EstadoSombra.Patrulla);
        }

        private void Update()
        {
            // Si no tenemos al jugador (o reapareció), intentar localizarlo
            if (jugadorTransform == null)
            {
                BuscarJugador();
                if (jugadorTransform == null) return;
            }

            // Medir distancia en 2D hacia Luna
            float distancia = Vector2.Distance(transform.position, jugadorTransform.position);

            // Ejecutar lógica según el estado activo
            if (estadoActual == EstadoSombra.Patrulla)
            {
                ActualizarPatrulla(distancia);
            }
            else if (estadoActual == EstadoSombra.Persecucion)
            {
                ActualizarPersecucion(distancia);
            }
        }

        /// <summary>
        /// Comprueba si Luna ha entrado en el radio de detección para comenzar a perseguir.
        /// </summary>
        private void ActualizarPatrulla(float distancia)
        {
            if (distancia < distanciaDeteccion)
            {
                CambiarEstado(EstadoSombra.Persecucion);
            }
        }

        /// <summary>
        /// Mueve a la sombra hacia Luna solo en X dentro de sus límites y verifica la histéresis.
        /// </summary>
        private void ActualizarPersecucion(float distancia)
        {
            // Histéresis: Si Luna se aleja más de distanciaPerdida, volver a patrulla
            if (distancia > distanciaPerdida)
            {
                CambiarEstado(EstadoSombra.Patrulla);
                return;
            }

            // Calcular diferencia en el eje X
            float diferenciaX = jugadorTransform.position.x - transform.position.x;

            // Zona muerta pequeña para evitar vibración cuando Luna está alineada verticalmente
            if (Mathf.Abs(diferenciaX) > 0.05f)
            {
                float direccionX = Mathf.Sign(diferenciaX);
                float paso = direccionX * velocidadPersecucion * Time.deltaTime;

                ActualizarLimitesDesdePatrulla();
                float nuevaX = Mathf.Clamp(transform.position.x + paso, limiteMinX, limiteMaxX);

                transform.position = new Vector3(nuevaX, transform.position.y, transform.position.z);

                // Voltear sprite: flipX = true mirando a la derecha (idéntico a PatrullaSombra)
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = direccionX > 0;
                }
            }
        }

        /// <summary>
        /// Realiza la transición limpia entre los estados Patrulla y Persecución.
        /// </summary>
        public void CambiarEstado(EstadoSombra nuevoEstado)
        {
            estadoActual = nuevoEstado;

            if (nuevoEstado == EstadoSombra.Persecucion)
            {
                if (!estaPersiguiendo)
                {
                    estaPersiguiendo = true;

                    // 1. Pausar el script de patrulla para que no interfiera con el movimiento
                    if (patrullaSombra != null)
                    {
                        patrullaSombra.enabled = false;
                    }

                    // 2. Reproducir efecto sonoro de alerta una sola vez
                    if (clipAlerta != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(clipAlerta, volumenAlerta);
                    }

                    // 3. Notificar al Gestor de Música que una sombra comenzó a perseguir
                    GestorMusica gestor = ObtenerGestorMusica();
                    if (gestor != null)
                    {
                        gestor.EntrarPersecucion();
                    }

                    // 4. Cambiar gráficos a persecución (ojos rojos)
                    AplicarGraficosPersecucion(true);
                }
            }
            else // EstadoSombra.Patrulla
            {
                if (estaPersiguiendo)
                {
                    estaPersiguiendo = false;

                    // 1. Notificar al Gestor de Música que esta sombra abandonó la persecución
                    GestorMusica gestor = ObtenerGestorMusica();
                    if (gestor != null)
                    {
                        gestor.SalirPersecucion();
                    }
                }

                // 2. Reanudar el script de patrulla existente
                if (patrullaSombra != null)
                {
                    patrullaSombra.Velocidad = velocidadPatrulla;
                    patrullaSombra.enabled = true;
                }

                // 3. Restaurar gráficos a patrulla (ojos amarillos)
                AplicarGraficosPersecucion(false);
            }
        }

        /// <summary>
        /// Cambia entre la apariencia visual de patrulla y persecución.
        /// Soporta Animator si existe el parámetro, o alterna sprites y tinte directamente.
        /// </summary>
        private void AplicarGraficosPersecucion(bool enPersecucion)
        {
            // Opción 1: Controlador de Animator con parámetro booleano "Persecucion"
            if (tieneParametroPersecucion && animator != null)
            {
                animator.SetBool(idParametroPersecucion, enPersecucion);
            }
            else
            {
                // Opción 2: Sin Animator activo, cambiar de sprite directamente
                if (spriteRenderer != null)
                {
                    if (enPersecucion && spritePersecucion != null)
                    {
                        spriteRenderer.sprite = spritePersecucion;
                    }
                    else if (!enPersecucion && spritePatrulla != null)
                    {
                        spriteRenderer.sprite = spritePatrulla;
                    }

                    // Opción 3: Aplicar tinte complementario para retroalimentación visual clara
                    if (tintarAlertaSiNoHayAnimacion)
                    {
                        spriteRenderer.color = enPersecucion ? colorAlerta : colorOriginal;
                    }
                }
            }
        }

        /// <summary>
        /// Lee los límites xMinimo y xMaximo definidos en PatrullaSombra.
        /// </summary>
        private void ActualizarLimitesDesdePatrulla()
        {
            if (patrullaSombra != null)
            {
                limiteMinX = patrullaSombra.XMinimo;
                limiteMaxX = patrullaSombra.XMaximo;
            }
            else
            {
                if (Mathf.Approximately(limiteMinX, 0f) && Mathf.Approximately(limiteMaxX, 0f))
                {
                    limiteMinX = transform.position.x - 2.5f;
                    limiteMaxX = transform.position.x + 2.5f;
                }
            }
        }

        /// <summary>
        /// Busca al jugador soportando Tag "Luna" (Nivel 1), Tag "Jugador" (Nivel 3) o Nombre "Luna".
        /// </summary>
        private Transform BuscarJugador()
        {
            // 1. Tag "Luna"
            try
            {
                GameObject objLuna = GameObject.FindWithTag("Luna");
                if (objLuna != null)
                {
                    jugadorTransform = objLuna.transform;
                    return jugadorTransform;
                }
            }
            catch {}

            // 2. Tag "Jugador"
            try
            {
                GameObject objJugador = GameObject.FindWithTag("Jugador");
                if (objJugador != null)
                {
                    jugadorTransform = objJugador.transform;
                    return jugadorTransform;
                }
            }
            catch {}

            // 3. Nombre del objeto "Luna"
            GameObject objPorNombre = GameObject.Find("Luna");
            if (objPorNombre != null)
            {
                jugadorTransform = objPorNombre.transform;
                return jugadorTransform;
            }

            return null;
        }

        private GestorMusica ObtenerGestorMusica()
        {
            if (GestorMusica.Instancia != null)
            {
                return GestorMusica.Instancia;
            }
#if UNITY_2023_1_OR_NEWER
            return FindAnyObjectByType<GestorMusica>();
#else
            return FindObjectOfType<GestorMusica>();
#endif
        }

        private void OnDisable()
        {
            // Si la sombra se desactiva (por ejemplo al morir o recargar checkpoint),
            // aseguramos que el contador de música no quede bloqueado en persecución
            if (estaPersiguiendo)
            {
                estaPersiguiendo = false;
                GestorMusica gestor = ObtenerGestorMusica();
                if (gestor != null)
                {
                    gestor.SalirPersecucion();
                }
            }
        }

        private void OnDestroy()
        {
            if (estaPersiguiendo)
            {
                estaPersiguiendo = false;
                GestorMusica gestor = ObtenerGestorMusica();
                if (gestor != null)
                {
                    gestor.SalirPersecucion();
                }
            }
        }

        /// <summary>
        /// Dibuja los radios de detección (amarillo) y pérdida (rojo),
        /// además del rango horizontal de patrulla (cian).
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // Radio de Detección (6 unidades por defecto)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, distanciaDeteccion);

            // Radio de Pérdida por Histéresis (9 unidades por defecto)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, distanciaPerdida);

            // Límites horizontales de patrulla y persecución
            ActualizarLimitesDesdePatrulla();
            Gizmos.color = Color.cyan;
            Vector3 posMin = new Vector3(limiteMinX, transform.position.y, transform.position.z);
            Vector3 posMax = new Vector3(limiteMaxX, transform.position.y, transform.position.z);
            Gizmos.DrawLine(posMin, posMax);
            Gizmos.DrawWireCube(posMin, new Vector3(0.1f, 0.5f, 0.1f));
            Gizmos.DrawWireCube(posMax, new Vector3(0.1f, 0.5f, 0.1f));
        }
    }
}
