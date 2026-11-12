using System.Collections;
using UnityEngine;
using SemillasDelBosque.Audio;
using SemillasDelBosque.Sistema;

namespace SemillasDelBosque.Jugador
{
    /// <summary>
    /// SistemaVidaJugador: Administra la vida de Nahui/Luna en medios corazones (6 = 3 corazones),
    /// aplica empuje físico (knockback) al chocar con sombras/jefe, invulnerabilidad temporal con parpadeo
    /// y sincronización con la variable 'vidas' y Game Over de Visual Scripting.
    /// </summary>
    [DefaultExecutionOrder(50)]
    [RequireComponent(typeof(Rigidbody2D))]
    public class SistemaVidaJugador : MonoBehaviour
    {
        [Header("Salud en Medios Corazones")]
        [SerializeField] private int mediosCorazonesMax = 6;
        [SerializeField] private float tiempoInvulnerabilidad = 1.15f;
        [SerializeField] private float duracionEmpuje = 0.24f;

        [Header("Audio")]
        [SerializeField] private AudioClip clipDano;

        private int mediosCorazonesActuales;
        private bool invulnerable = false;
        private bool derrotado = false;

        private float tiempoEmpujeRestante = 0f;
        private float velocidadEmpujeX = 0f;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;
        private GameObject panelGameOverCache;

        public int MediosCorazones => mediosCorazonesActuales;
        public int MediosCorazonesMax => mediosCorazonesMax;
        public bool EstaInvulnerable => invulnerable;
        public AudioClip ClipDano { get => clipDano; set => clipDano = value; }

        public static SistemaVidaJugador ObtenerOAgregar(GameObject jugador)
        {
            if (jugador == null) return null;
            SistemaVidaJugador s = jugador.GetComponent<SistemaVidaJugador>();
            if (s == null) s = jugador.AddComponent<SistemaVidaJugador>();
            return s;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;

            mediosCorazonesActuales = mediosCorazonesMax;
            panelGameOverCache = BuscarObjetoInactivo("PanelGameOver");
        }

        private void Start()
        {
            int vidasVS = UtilidadesJugador.LeerVariableEscena("vidas", 3);
            mediosCorazonesActuales = Mathf.Clamp(vidasVS * 2, 0, mediosCorazonesMax);
            SincronizarVariableVidas();
        }

        private void Update()
        {
            if (derrotado) return;

            // Si Visual Scripting bajó 'vidas' por otro medio (ej. AguaOscura o Trampa), sincronizar hacia abajo
            int vidasVS = UtilidadesJugador.LeerVariableEscena("vidas", 3);
            int techoMedios = vidasVS * 2;
            if (techoMedios < mediosCorazonesActuales - 1)
            {
                mediosCorazonesActuales = Mathf.Max(0, techoMedios);
            }
        }

        private void LateUpdate()
        {
            AplicarEmpujeSiActivo(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            AplicarEmpujeSiActivo(0f);
        }

        private void AplicarEmpujeSiActivo(float dt)
        {
            if (tiempoEmpujeRestante <= 0f || rb == null) return;

            if (dt > 0f)
            {
                tiempoEmpujeRestante -= dt;
            }

            float factor = Mathf.Clamp01(tiempoEmpujeRestante / Mathf.Max(0.01f, duracionEmpuje));
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = new Vector2(velocidadEmpujeX * (0.4f + 0.6f * factor), rb.linearVelocity.y);
#else
            rb.velocity = new Vector2(velocidadEmpujeX * (0.4f + 0.6f * factor), rb.velocity.y);
#endif
        }

        /// <summary>
        /// Resta medios corazones (1 = medio corazón), empuja al jugador lejos del enemigo
        /// y activa invulnerabilidad con parpadeo.
        /// </summary>
        public void RecibirDanioMedioCorazon(int cantidadMedios, Vector2 origenEnemigo, float fuerzaEmpujeX = 8.5f, float fuerzaEmpujeY = 6.5f)
        {
            if (invulnerable || derrotado) return;

            mediosCorazonesActuales = Mathf.Max(0, mediosCorazonesActuales - Mathf.Max(1, cantidadMedios));
            SincronizarVariableVidas();

            // Reproducir sonido de daño
            if (clipDano != null && audioSource != null)
            {
                float vol = GestorJuego.Instancia != null ? GestorJuego.Instancia.VolumenSFX : 1f;
                audioSource.PlayOneShot(clipDano, vol);
            }

            // Calcular dirección de empuje (alejar de la sombra)
            float dirX = (transform.position.x >= origenEnemigo.x) ? 1f : -1f;
            velocidadEmpujeX = dirX * fuerzaEmpujeX;
            tiempoEmpujeRestante = duracionEmpuje;

            if (rb != null)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = new Vector2(velocidadEmpujeX, fuerzaEmpujeY);
#else
                rb.velocity = new Vector2(velocidadEmpujeX, fuerzaEmpujeY);
#endif
            }

            if (mediosCorazonesActuales <= 0)
            {
                ActivarDerrota();
            }
            else
            {
                StartCoroutine(CorrutinaInvulnerabilidad());
            }
        }

        private void SincronizarVariableVidas()
        {
            int corazonesRedondeados = Mathf.CeilToInt(mediosCorazonesActuales / 2f);
            UtilidadesJugador.EscribirVariableEscena("vidas", corazonesRedondeados);
        }

        private void ActivarDerrota()
        {
            derrotado = true;
            invulnerable = true;
            UtilidadesJugador.EscribirVariableEscena("vidas", 0);

            if (panelGameOverCache == null)
            {
                panelGameOverCache = BuscarObjetoInactivo("PanelGameOver");
            }
            if (panelGameOverCache != null)
            {
                panelGameOverCache.SetActive(true);
            }

            Time.timeScale = 0f;
        }

        private IEnumerator CorrutinaInvulnerabilidad()
        {
            invulnerable = true;
            Color original = spriteRenderer != null ? spriteRenderer.color : Color.white;
            float t = 0f;

            while (t < tiempoInvulnerabilidad)
            {
                t += 0.1f;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = (spriteRenderer.color.a > 0.6f)
                        ? new Color(1f, 0.55f, 0.55f, 0.35f)
                        : original;
                }
                yield return new WaitForSeconds(0.1f);
            }

            if (spriteRenderer != null) spriteRenderer.color = original;
            invulnerable = false;
        }

        private static GameObject BuscarObjetoInactivo(string nombre)
        {
            Transform[] todos = FindObjectsByType<Transform>(FindObjectsInactive.Include);
            for (int i = 0; i < todos.Length; i++)
            {
                if (todos[i].name == nombre) return todos[i].gameObject;
            }
            return null;
        }
    }
}

