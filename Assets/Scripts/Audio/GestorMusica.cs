using System.Collections;
using UnityEngine;

namespace SemillasDelBosque.Audio
{
    /// <summary>
    /// GestorMusica: Controla el AudioSource del objeto "Musica" en la escena.
    /// Administra la música de fondo del nivel y realiza transiciones suaves (crossfade)
    /// entre la exploración normal, la persecución de sombras y la victoria cuando
    /// la zona ha sido restaurada.
    /// 
    /// Implementa un patrón Singleton simple para que cualquier script (como SombraPersecucion)
    /// pueda acceder a él fácilmente mediante GestorMusica.Instancia.
    /// </summary>
    public class GestorMusica : MonoBehaviour
    {
        // Singleton accesible globalmente
        public static GestorMusica Instancia { get; private set; }

        [Header("Componente de Audio")]
        [Tooltip("AudioSource que reproducirá la música ambiental.")]
        [SerializeField] private AudioSource audioSource;

        [Header("Pistas de Música")]
        [Tooltip("Música de exploración que suena al recorrer el nivel con calma.")]
        [SerializeField] private AudioClip musicaExploracion;

        [Tooltip("Música tensa que suena cuando una o más sombras persiguen a Luna.")]
        [SerializeField] private AudioClip musicaPersecucion;

        [Tooltip("Música triunfal que suena cuando se restaura la zona.")]
        [SerializeField] private AudioClip musicaZonaRestaurada;

        [Header("Parámetros de Transición")]
        [Tooltip("Duración en segundos del fundido suave al cambiar de pista.")]
        [SerializeField] private float duracionFundido = 0.6f;

        [Tooltip("Volumen objetivo de la música de fondo.")]
        [Range(0f, 1f)]
        [SerializeField] private float volumenMaximo = 0.4f;

        // Contador de cuántas sombras están persiguiendo a Luna en este momento
        private int sombrasPersiguiendo = 0;

        // Bandera para proteger la música victoriosa de zona restaurada
        private bool zonaRestaurada = false;

        // Referencia a la corrutina de fundido activa para poder cancelarla si cambia rápido de estado
        private Coroutine corrutinaFundido;

        // Propiedades públicas de consulta
        public int SombrasPersiguiendo => sombrasPersiguiendo;
        public bool EstaEnZonaRestaurada => zonaRestaurada;

        public AudioClip MusicaExploracion { get => musicaExploracion; set => musicaExploracion = value; }
        public AudioClip MusicaPersecucion { get => musicaPersecucion; set => musicaPersecucion = value; }
        public AudioClip MusicaZonaRestaurada { get => musicaZonaRestaurada; set => musicaZonaRestaurada = value; }

        private void Awake()
        {
            // Inicialización del Singleton
            if (Instancia != null && Instancia != this)
            {
                Destroy(gameObject);
                return;
            }
            Instancia = this;

            // Asegurar que contamos con el AudioSource
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }

        private void Start()
        {
            // Al arrancar la escena, reproducir música de exploración si no hay otra sonando
            if (musicaExploracion != null && !audioSource.isPlaying)
            {
                audioSource.clip = musicaExploracion;
                audioSource.volume = volumenMaximo;
                audioSource.Play();
            }
        }

        /// <summary>
        /// Se llama cuando una sombra comienza a perseguir a Luna.
        /// Cuando el contador pasa de 0 a 1, transiciona hacia musica_persecucion.
        /// </summary>
        public void EntrarPersecucion()
        {
            sombrasPersiguiendo++;

            // Si es la primera sombra que inicia persecución y la zona aún no está restaurada
            if (sombrasPersiguiendo == 1 && !zonaRestaurada)
            {
                TransicionarA(musicaPersecucion);
            }
        }

        /// <summary>
        /// Se llama cuando una sombra pierde a Luna o es desactivada.
        /// Cuando el contador regresa a 0, vuelve suavemente a musica_exploracion.
        /// </summary>
        public void SalirPersecucion()
        {
            sombrasPersiguiendo = Mathf.Max(0, sombrasPersiguiendo - 1);

            // Si ya no queda ninguna sombra en persecución y la zona no está restaurada
            if (sombrasPersiguiendo == 0 && !zonaRestaurada)
            {
                TransicionarA(musicaExploracion);
            }
        }

        /// <summary>
        /// Se llama al restaurar la zona / completar el nivel.
        /// Cambia permanentemente a la música de zona restaurada.
        /// </summary>
        public void ZonaRestaurada()
        {
            zonaRestaurada = true;
            TransicionarA(musicaZonaRestaurada);
        }

        /// <summary>
        /// Inicia una transición con fundido (crossfade) hacia una nueva pista.
        /// </summary>
        public void TransicionarA(AudioClip nuevoClip)
        {
            if (nuevoClip == null) return;

            // Si la pista ya está sonando, evitamos reiniciar la canción
            if (audioSource.clip == nuevoClip && audioSource.isPlaying) return;

            // Si hay un fundido previo en curso, lo detenemos para no solapar corrutinas
            if (corrutinaFundido != null)
            {
                StopCoroutine(corrutinaFundido);
            }

            if (gameObject.activeInHierarchy)
            {
                corrutinaFundido = StartCoroutine(CorrutinaFundido(nuevoClip));
            }
            else
            {
                // Fallback directo si el objeto estuviera inactivo
                audioSource.clip = nuevoClip;
                audioSource.volume = volumenMaximo;
                audioSource.Play();
            }
        }

        /// <summary>
        /// Corrutina que baja el volumen suavemente, cambia el clip y vuelve a subir el volumen.
        /// </summary>
        private IEnumerator CorrutinaFundido(AudioClip nuevoClip)
        {
            float mitadTiempo = Mathf.Max(0.01f, duracionFundido * 0.5f);
            float volumenInicial = audioSource.volume;

            // 1. Fade Out (Bajar volumen actual a cero)
            float t = 0f;
            while (t < mitadTiempo)
            {
                t += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(volumenInicial, 0f, t / mitadTiempo);
                yield return null;
            }
            audioSource.volume = 0f;

            // 2. Cambiar de canción
            audioSource.clip = nuevoClip;
            audioSource.Play();

            // 3. Fade In (Subir volumen desde cero hasta el volumen máximo configurado)
            t = 0f;
            while (t < mitadTiempo)
            {
                t += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(0f, volumenMaximo, t / mitadTiempo);
                yield return null;
            }
            audioSource.volume = volumenMaximo;

            corrutinaFundido = null;
        }

        private void OnDestroy()
        {
            if (Instancia == this)
            {
                Instancia = null;
            }
        }
    }
}

