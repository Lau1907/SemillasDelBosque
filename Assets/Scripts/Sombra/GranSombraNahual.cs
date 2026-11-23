using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SemillasDelBosque.Audio;
using SemillasDelBosque.Sistema;

namespace SemillasDelBosque.Sombra
{
    /// <summary>
    /// GranSombraNahual: Mini-jefe del Nivel 4 ("Cima de los Recuerdos").
    /// Patrulla y persigue al jugador dentro de la arena del Castillo de Chapultepec.
    /// Se derrota saltando sobre su cabeza 3 veces; al caer, desbloquea la meta final.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class GranSombraNahual : MonoBehaviour
    {
        [Header("Estadísticas del Mini-Jefe")]
        [SerializeField] private int vidaMaxima = 5;
        [SerializeField] private float velocidadPatrulla = 2.2f;
        [SerializeField] private float velocidadPersecucion = 2.7f;
        [SerializeField] private float velocidadFuria = 3.5f;
        [SerializeField] private float rangoDeteccion = 9f;
        [SerializeField] private float fuerzaReboteJugador = 11.5f;
        [SerializeField] private float tiempoInvulnerabilidad = 0.85f;

        [Header("Límites de Arena")]
        [SerializeField] private float xMinimo = 96f;
        [SerializeField] private float xMaximo = 115f;

        [Header("Referencias de Hijos y Visuales")]
        [SerializeField] private Collider2D colliderCuerpoDanio;
        [SerializeField] private Collider2D colliderCabezaDebil;
        [SerializeField] private Sprite spritePatrulla;
        [SerializeField] private Sprite spritePersecucion;
        [SerializeField] private Sprite spriteAtaque;
        [SerializeField] private AudioClip clipAlerta;
        [SerializeField] private AudioClip clipDano;

        [Header("UI de Jefe (Opcional)")]
        [SerializeField] private Slider barraVida;
        [SerializeField] private TextMeshProUGUI textoNombreJefe;

        private int vidaActual;
        private bool estaDerrotada;
        private bool invulnerable;
        private bool moviendoDerecha = false;
        private bool enPersecucion = false;

        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private Rigidbody2D rb;
        private AudioSource audioSource;
        private SombraDanioContacto danioContacto;
        private Transform jugadorTransform;
        private Color colorBase = Color.white;

        private static readonly int HashPersecucion = Animator.StringToHash("Persecucion");
        private static readonly int HashAtaque = Animator.StringToHash("Ataque");

        public bool EstaDerrotada => estaDerrotada;
        public int VidaActual => vidaActual;
        public int VidaMaxima => vidaMaxima;

        public Collider2D ColliderCuerpoDanio { get => colliderCuerpoDanio; set => colliderCuerpoDanio = value; }
        public Collider2D ColliderCabezaDebil { get => colliderCabezaDebil; set => colliderCabezaDebil = value; }
        public Sprite SpritePatrulla { get => spritePatrulla; set => spritePatrulla = value; }
        public Sprite SpritePersecucion { get => spritePersecucion; set => spritePersecucion = value; }
        public Sprite SpriteAtaque { get => spriteAtaque; set => spriteAtaque = value; }
        public AudioClip ClipAlerta { get => clipAlerta; set => clipAlerta = value; }
        public AudioClip ClipDano { get => clipDano; set => clipDano = value; }
        public Slider BarraVida { get => barraVida; set => barraVida = value; }
        public TextMeshProUGUI TextoNombreJefe { get => textoNombreJefe; set => textoNombreJefe = value; }

        public event Action AlSerDerrotada;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;

            if (spriteRenderer != null)
            {
                colorBase = spriteRenderer.color;
                if (spritePatrulla == null) spritePatrulla = spriteRenderer.sprite;
            }

            vidaActual = vidaMaxima;
            ConfigurarDanioContacto();
            ActualizarUI();
        }

        private void ConfigurarDanioContacto()
        {
            if (colliderCuerpoDanio == null) return;
            colliderCuerpoDanio.gameObject.tag = "Untagged";
            danioContacto = colliderCuerpoDanio.GetComponent<SombraDanioContacto>();
            if (danioContacto == null)
            {
                danioContacto = colliderCuerpoDanio.gameObject.AddComponent<SombraDanioContacto>();
            }
            danioContacto.Configurar(1, 10.5f, 7.0f);
        }

        private void Start()
        {
            BuscarJugador();
            ActualizarUI();
        }

        private void Update()
        {
            if (estaDerrotada) return;

            if (jugadorTransform == null)
            {
                BuscarJugador();
            }

            bool detectar = false;
            if (jugadorTransform != null)
            {
                float dist = Vector2.Distance(transform.position, jugadorTransform.position);
                float difY = Mathf.Abs(jugadorTransform.position.y - transform.position.y);
                // Si el jugador está en las plataformas superiores para saltar encima, el jefe sigue fluyendo por la arena
                detectar = dist <= rangoDeteccion &&
                           difY <= 2.2f &&
                           jugadorTransform.position.x >= xMinimo - 3f &&
                           jugadorTransform.position.x <= xMaximo + 3f;
            }

            if (detectar && !enPersecucion)
            {
                IniciarPersecucion();
            }
            else if (!detectar && enPersecucion)
            {
                DetenerPersecucion();
            }

            MoverJefe();
        }

        public void ConfigurarArena(float minX, float maxX)
        {
            xMinimo = minX;
            xMaximo = maxX;
        }

        private void BuscarJugador()
        {
            GameObject go = UtilidadesJugador.BuscarJugador();
            if (go != null)
            {
                jugadorTransform = go.transform;
            }
        }

        private void IniciarPersecucion()
        {
            enPersecucion = true;
            if (clipAlerta != null && audioSource != null)
            {
                audioSource.PlayOneShot(clipAlerta);
            }
            if (GestorMusica.Instancia != null)
            {
                GestorMusica.Instancia.EntrarPersecucion();
            }
            ActualizarSpriteEstado();
        }

        private void DetenerPersecucion()
        {
            enPersecucion = false;
            if (GestorMusica.Instancia != null)
            {
                GestorMusica.Instancia.SalirPersecucion();
            }
            ActualizarSpriteEstado();
        }

        private void MoverJefe()
        {
            float velocidadActual = enPersecucion
                ? (vidaActual <= 1 ? velocidadFuria : (vidaActual <= 3 ? velocidadPersecucion + 0.4f : velocidadPersecucion))
                : velocidadPatrulla;

            if (enPersecucion && jugadorTransform != null)
            {
                float diffX = jugadorTransform.position.x - transform.position.x;
                if (Mathf.Abs(diffX) > 0.15f)
                {
                    moviendoDerecha = diffX > 0f;
                }
            }

            float dir = moviendoDerecha ? 1f : -1f;
            float nuevaX = transform.position.x + dir * velocidadActual * Time.deltaTime;

            if (nuevaX >= xMaximo)
            {
                nuevaX = xMaximo;
                moviendoDerecha = false;
            }
            else if (nuevaX <= xMinimo)
            {
                nuevaX = xMinimo;
                moviendoDerecha = true;
            }

            transform.position = new Vector3(nuevaX, transform.position.y, transform.position.z);

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !moviendoDerecha;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (estaDerrotada || invulnerable) return;
            if (!UtilidadesJugador.EsJugador(other)) return;

            // Verificar si el jugador cayó o saltó por encima del punto débil del mini-jefe
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

            float umbralCabeza = colliderCabezaDebil != null
                ? colliderCabezaDebil.bounds.min.y - 0.15f
                : transform.position.y + 0.35f;

            bool impactoDesdeArriba = other.transform.position.y >= umbralCabeza && vy <= 0.6f;

            if (impactoDesdeArriba)
            {
                if (rbJugador != null)
                {
#if UNITY_6000_0_OR_NEWER
                    rbJugador.linearVelocity = new Vector2(rbJugador.linearVelocity.x, fuerzaReboteJugador);
#else
                    rbJugador.velocity = new Vector2(rbJugador.velocity.x, fuerzaReboteJugador);
#endif
                }
                RecibirGolpe(1);
            }
        }

        public void RecibirGolpe(int cantidad)
        {
            if (estaDerrotada || invulnerable) return;

            vidaActual = Mathf.Max(0, vidaActual - cantidad);
            ActualizarUI();

            if (clipDano != null && audioSource != null)
            {
                audioSource.PlayOneShot(clipDano);
            }

            if (vidaActual <= 0)
            {
                StartCoroutine(CorrutinaDerrota());
            }
            else
            {
                StartCoroutine(CorrutinaInvulnerabilidad());
            }
        }

        private IEnumerator CorrutinaInvulnerabilidad()
        {
            invulnerable = true;
            if (colliderCuerpoDanio != null) colliderCuerpoDanio.enabled = false;
            if (danioContacto != null) danioContacto.PausarDanio(tiempoInvulnerabilidad);

            float transcurrido = 0f;
            while (transcurrido < tiempoInvulnerabilidad)
            {
                transcurrido += 0.12f;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = (spriteRenderer.color == colorBase)
                        ? new Color(1f, 0.35f, 0.35f, 0.7f)
                        : colorBase;
                }
                yield return new WaitForSeconds(0.12f);
            }

            if (spriteRenderer != null) spriteRenderer.color = colorBase;
            if (colliderCuerpoDanio != null && !estaDerrotada) colliderCuerpoDanio.enabled = true;
            invulnerable = false;
            ActualizarSpriteEstado();
        }

        private IEnumerator CorrutinaDerrota()
        {
            estaDerrotada = true;
            invulnerable = true;

            if (colliderCuerpoDanio != null) colliderCuerpoDanio.enabled = false;
            if (colliderCabezaDebil != null) colliderCabezaDebil.enabled = false;

            if (enPersecucion)
            {
                enPersecucion = false;
                if (GestorMusica.Instancia != null)
                {
                    GestorMusica.Instancia.SalirPersecucion();
                }
            }

            Debug.Log("[GranSombraNahual] ¡Gran Sombra de Nahual derrotada! El camino hacia la meta está libre.");

            float duracion = 1.0f;
            float t = 0f;
            Vector3 escalaInicial = transform.localScale;
            while (t < duracion)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duracion);
                transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, p);
                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = 1f - p;
                    spriteRenderer.color = c;
                }
                yield return null;
            }

            if (barraVida != null) barraVida.gameObject.SetActive(false);
            if (textoNombreJefe != null) textoNombreJefe.gameObject.SetActive(false);

            AlSerDerrotada?.Invoke();
            gameObject.SetActive(false);
        }

        private void ActualizarSpriteEstado()
        {
            bool enFuria = enPersecucion && vidaActual <= 3;

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetBool(HashPersecucion, enPersecucion);
                animator.SetBool(HashAtaque, enFuria);
            }

            if (spriteRenderer == null) return;

            if (enFuria && spriteAtaque != null)
            {
                spriteRenderer.sprite = spriteAtaque;
            }
            else if (enPersecucion && spritePersecucion != null)
            {
                spriteRenderer.sprite = spritePersecucion;
            }
            else if (spritePatrulla != null)
            {
                spriteRenderer.sprite = spritePatrulla;
            }

            if (vidaActual <= 1)
            {
                spriteRenderer.color = new Color(1f, 0.55f, 0.65f, 1f);
            }
        }

        private void ActualizarUI()
        {
            if (barraVida != null)
            {
                barraVida.maxValue = vidaMaxima;
                barraVida.value = vidaActual;
            }
            if (textoNombreJefe != null)
            {
                textoNombreJefe.text = $"Gran Nahual de la Niebla ({vidaActual}/{vidaMaxima})";
            }
        }

        private void OnDisable()
        {
            if (enPersecucion)
            {
                enPersecucion = false;
                if (GestorMusica.Instancia != null)
                {
                    GestorMusica.Instancia.SalirPersecucion();
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Vector3 a = new Vector3(xMinimo, transform.position.y, transform.position.z);
            Vector3 b = new Vector3(xMaximo, transform.position.y, transform.position.z);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        }
    }
}

