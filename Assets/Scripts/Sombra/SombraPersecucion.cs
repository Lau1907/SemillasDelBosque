using System.Collections;
using UnityEngine;
using SemillasDelBosque.Nivel3;
using SemillasDelBosque.Audio;
using SemillasDelBosque.Sistema;

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
        [Tooltip("Distancia hacia Nahui/Luna para iniciar la persecución.")]
        [SerializeField] private float distanciaDeteccion = 4.8f;

        [Tooltip("Distancia hacia Nahui/Luna para perderla de vista y volver a patrullar.")]
        [SerializeField] private float distanciaPerdida = 7.0f;

        [Tooltip("Diferencia máxima en Y para detectar al jugador; si está en una plataforma elevada, la sombra sigue su ruta.")]
        [SerializeField] private float alturaMaximaDeteccion = 2.0f;

        [Header("Flujo de Camino y Enfriamiento")]
        [Tooltip("Si está desactivado, la sombra mantiene su recorrido de patrulla fluido sin abalanzarse sobre el jugador.")]
        [SerializeField] private bool perseguirAlJugador = false;

        [Tooltip("Duración máxima de una embestida de persecución antes de retomar el recorrido normal.")]
        [SerializeField] private float tiempoMaximoPersecucion = 2.6f;

        [Tooltip("Tiempo de enfriamiento tras perseguir durante el cual la sombra fluye en su patrulla.")]
        [SerializeField] private float tiempoEnfriamiento = 2.8f;

        [Header("Velocidades")]
        [Tooltip("Velocidad de patrulla habitual (sincronizada con PatrullaSombra).")]
        [SerializeField] private float velocidadPatrulla = 1.8f;

        [Tooltip("Velocidad de desplazamiento horizontal durante la persecución.")]
        [SerializeField] private float velocidadPersecucion = 3.2f;

        [Header("Combate y Golpes")]
        [Tooltip("Golpes sobre la cabeza necesarios para derrotar a esta sombra.")]
        [SerializeField] private int vidaMaxima = 2;

        [SerializeField] private float fuerzaReboteJugador = 10.5f;
        [SerializeField] private AudioClip clipDano;

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

        // Combate
        private int vidaActual;
        private bool estaDerrotada = false;
        private bool invulnerableGolpe = false;
        private SombraDanioContacto contactoDanio;
        private Collider2D colliderCabeza;

        // Estado actual de la máquina de estados
        private EstadoSombra estadoActual = EstadoSombra.Patrulla;
        private bool estaPersiguiendo = false;
        private float tiempoEnPersecucion = 0f;
        private float tiempoEnBorde = 0f;
        private float enfriamientoRestante = 0f;
        private Vector3 ultimaPosJugador;
        private bool tieneUltimaPosJugador = false;
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
        public AudioClip ClipDano { get => clipDano; set => clipDano = value; }
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

            // Configurar hitboxes de combate (stomp arriba + medio corazón con empuje en el cuerpo)
            ConfigurarCombateYHitboxes();

            // Inicializar y sincronizar límites desde PatrullaSombra
            ActualizarLimitesDesdePatrulla();
            vidaActual = vidaMaxima;
        }

        private void ConfigurarCombateYHitboxes()
        {
            // Para recibir triggers de los hijos y rebotar a Nahui sin teletransporte instantáneo de LunaGraph
            Rigidbody2D rbLocal = GetComponent<Rigidbody2D>();
            if (rbLocal == null) rbLocal = gameObject.AddComponent<Rigidbody2D>();
            rbLocal.bodyType = RigidbodyType2D.Kinematic;
            rbLocal.useFullKinematicContacts = true;

            gameObject.tag = "Untagged";

            BoxCollider2D colPadre = GetComponent<BoxCollider2D>();
            if (colPadre != null) colPadre.enabled = false;

            // Hijo 1: CuerpoDanio (medio corazón + empuje)
            Transform tCuerpo = transform.Find("CuerpoDanio");
            GameObject goCuerpo;
            if (tCuerpo == null)
            {
                goCuerpo = new GameObject("CuerpoDanio");
                goCuerpo.transform.SetParent(transform, false);
                goCuerpo.transform.localPosition = new Vector3(0f, -0.10f, 0f);
                BoxCollider2D colCuerpo = goCuerpo.AddComponent<BoxCollider2D>();
                colCuerpo.isTrigger = true;
                colCuerpo.size = new Vector2(0.76f, 0.52f);
            }
            else
            {
                goCuerpo = tCuerpo.gameObject;
            }
            goCuerpo.tag = "Untagged";
            contactoDanio = goCuerpo.GetComponent<SombraDanioContacto>();
            if (contactoDanio == null) contactoDanio = goCuerpo.AddComponent<SombraDanioContacto>();
            contactoDanio.Configurar(1, 8.2f, 6.2f);

            // Hijo 2: CabezaPuntoDebil (saltar encima para golpear)
            Transform tCabeza = transform.Find("CabezaPuntoDebil");
            GameObject goCabeza;
            if (tCabeza == null)
            {
                goCabeza = new GameObject("CabezaPuntoDebil");
                goCabeza.transform.SetParent(transform, false);
                goCabeza.transform.localPosition = new Vector3(0f, 0.34f, 0f);
                BoxCollider2D colCab = goCabeza.AddComponent<BoxCollider2D>();
                colCab.isTrigger = true;
                colCab.size = new Vector2(0.95f, 0.38f);
                colliderCabeza = colCab;
            }
            else
            {
                goCabeza = tCabeza.gameObject;
                colliderCabeza = goCabeza.GetComponent<Collider2D>();
            }
            goCabeza.tag = "Untagged";
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
            if (estaDerrotada) return;

            // Si no tenemos al jugador (o reapareció), intentar localizarlo
            if (jugadorTransform == null)
            {
                BuscarJugador();
                if (jugadorTransform == null) return;
                ultimaPosJugador = jugadorTransform.position;
                tieneUltimaPosJugador = true;
            }

            if (enfriamientoRestante > 0f)
            {
                enfriamientoRestante -= Time.deltaTime;
            }

            // Si el jugador fue teletransportado (checkpoint o daño), retomar patrulla de inmediato
            if (tieneUltimaPosJugador && Vector2.Distance(jugadorTransform.position, ultimaPosJugador) > 3.5f)
            {
                ultimaPosJugador = jugadorTransform.position;
                if (estadoActual == EstadoSombra.Persecucion)
                {
                    CambiarEstado(EstadoSombra.Patrulla);
                    enfriamientoRestante = tiempoEnfriamiento;
                    return;
                }
            }
            ultimaPosJugador = jugadorTransform.position;
            tieneUltimaPosJugador = true;

            // Medir distancia en 2D y diferencia vertical
            float distancia = Vector2.Distance(transform.position, jugadorTransform.position);
            float difY = Mathf.Abs(jugadorTransform.position.y - transform.position.y);

            // Ejecutar lógica según el estado activo
            if (estadoActual == EstadoSombra.Patrulla)
            {
                ActualizarPatrulla(distancia, difY);
            }
            else if (estadoActual == EstadoSombra.Persecucion)
            {
                ActualizarPersecucion(distancia, difY);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (estaDerrotada || invulnerableGolpe) return;
            if (!UtilidadesJugador.EsJugador(other)) return;

            Rigidbody2D rbJugador = other.attachedRigidbody != null
                ? other.attachedRigidbody
                : other.GetComponent<Rigidbody2D>();

            float vy = 0f;
            if (rbJugador != null)
            {
#if UNITY_6000_0_OR_NEWER
                vy = rbJugador.linearVelocity.y;
#else
                vy = rbJugador.velocity.y;
#endif
            }

            float umbralCabeza = colliderCabeza != null
                ? colliderCabeza.bounds.min.y - 0.12f
                : transform.position.y + 0.12f;

            if (other.transform.position.y >= umbralCabeza && vy <= 0.8f)
            {
                if (rbJugador != null)
                {
#if UNITY_6000_0_OR_NEWER
                    rbJugador.linearVelocity = new Vector2(rbJugador.linearVelocity.x, fuerzaReboteJugador);
#else
                    rbJugador.velocity = new Vector2(rbJugador.velocity.x, fuerzaReboteJugador);
#endif
                }

                if (contactoDanio != null)
                {
                    contactoDanio.PausarDanio(0.55f);
                }

                RecibirGolpeEnCabeza();
            }
        }

        private void RecibirGolpeEnCabeza()
        {
            vidaActual = Mathf.Max(0, vidaActual - 1);

            if (audioSource != null)
            {
                AudioClip sonido = clipDano != null ? clipDano : clipAlerta;
                if (sonido != null) audioSource.PlayOneShot(sonido, volumenAlerta);
            }

            if (vidaActual <= 0)
            {
                StartCoroutine(CorrutinaDerrotaSombra());
            }
            else
            {
                StartCoroutine(CorrutinaParpadeoGolpe());
            }
        }

        private IEnumerator CorrutinaParpadeoGolpe()
        {
            invulnerableGolpe = true;
            if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.35f, 0.35f, 0.9f);
            yield return new WaitForSeconds(0.18f);
            if (spriteRenderer != null && !estaDerrotada) spriteRenderer.color = colorOriginal;
            yield return new WaitForSeconds(0.22f);
            invulnerableGolpe = false;
        }

        private IEnumerator CorrutinaDerrotaSombra()
        {
            estaDerrotada = true;
            invulnerableGolpe = true;

            if (patrullaSombra != null) patrullaSombra.enabled = false;
            if (contactoDanio != null) contactoDanio.gameObject.SetActive(false);
            if (colliderCabeza != null) colliderCabeza.enabled = false;

            if (estaPersiguiendo)
            {
                estaPersiguiendo = false;
                GestorMusica gestor = ObtenerGestorMusica();
                if (gestor != null) gestor.SalirPersecucion();
            }

            float t = 0f;
            float duracion = 0.35f;
            Vector3 escalaIni = transform.localScale;
            while (t < duracion)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duracion);
                transform.localScale = Vector3.Lerp(escalaIni, Vector3.zero, p);
                yield return null;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Comprueba si el jugador está al mismo nivel y dentro del radio (sin enfriamiento activo).
        /// </summary>
        private void ActualizarPatrulla(float distancia, float difY)
        {
            if (enfriamientoRestante <= 0f && difY <= alturaMaximaDeteccion && distancia < distanciaDeteccion)
            {
                CambiarEstado(EstadoSombra.Persecucion);
            }
        }

        /// <summary>
        /// En modo flujo de camino (perseguirAlJugador == false), mantiene PatrullaSombra activa mientras muestra alerta.
        /// En modo persecución activa, realiza una embestida acotada y luego retoma su patrulla.
        /// </summary>
        private void ActualizarPersecucion(float distancia, float difY)
        {
            if (distancia > distanciaPerdida || difY > alturaMaximaDeteccion + 0.6f)
            {
                CambiarEstado(EstadoSombra.Patrulla);
                enfriamientoRestante = tiempoEnfriamiento;
                return;
            }

            // Modo flujo de camino: alerta visual y musical sin abalanzarse ni bloquear al jugador
            if (!perseguirAlJugador)
            {
                return;
            }

            tiempoEnPersecucion += Time.deltaTime;
            if (tiempoEnPersecucion >= tiempoMaximoPersecucion)
            {
                CambiarEstado(EstadoSombra.Patrulla);
                enfriamientoRestante = tiempoEnfriamiento;
                return;
            }

            float diferenciaX = jugadorTransform.position.x - transform.position.x;

            if (Mathf.Abs(diferenciaX) > 0.1f)
            {
                float direccionX = Mathf.Sign(diferenciaX);
                float paso = direccionX * velocidadPersecucion * Time.deltaTime;

                ActualizarLimitesDesdePatrulla();
                float prevX = transform.position.x;
                float nuevaX = Mathf.Clamp(prevX + paso, limiteMinX, limiteMaxX);
                transform.position = new Vector3(nuevaX, transform.position.y, transform.position.z);

                if (spriteRenderer != null)
                {
                    // El spritesheet mira hacia la derecha por defecto
                    spriteRenderer.flipX = direccionX < 0f;
                }

                // Si lleva tiempo atascada contra el límite de su plataforma, retoma la patrulla hacia el otro lado
                if (Mathf.Abs(nuevaX - prevX) < 0.001f)
                {
                    tiempoEnBorde += Time.deltaTime;
                    if (tiempoEnBorde >= 0.65f)
                    {
                        CambiarEstado(EstadoSombra.Patrulla);
                        enfriamientoRestante = tiempoEnfriamiento;
                    }
                }
                else
                {
                    tiempoEnBorde = 0f;
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
                    tiempoEnPersecucion = 0f;
                    tiempoEnBorde = 0f;

                    // 1. Solo pausar la patrulla si está habilitada la persecución directa sobre el jugador
                    if (patrullaSombra != null && perseguirAlJugador)
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
        /// Busca al jugador soportando Tags "Jugador", "Luna", "Nahui" y nombres "Luna"/"Nahui".
        /// </summary>
        private Transform BuscarJugador()
        {
            GameObject encontrado = UtilidadesJugador.BuscarJugador();
            if (encontrado != null)
            {
                jugadorTransform = encontrado.transform;
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
