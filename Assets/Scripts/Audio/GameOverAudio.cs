using UnityEngine;
using SemillasDelBosque.Audio;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SemillasDelBosque.Audio
{
    /// <summary>
    /// GameOverAudio: Monitorea la variable de escena 'vidas'.
    /// Cuando vidas llega a 0, reproduce el clip de Game Over una única vez
    /// y pausa la música de fondo de GestorMusica.
    /// </summary>
    public class GameOverAudio : MonoBehaviour
    {
        [Header("Audio")]
        [Tooltip("Clip de sonido de derrota (gameover.wav).")]
        [SerializeField] private AudioClip clipGameOver;

        [Tooltip("AudioSource para reproducir el clip.")]
        [SerializeField] private AudioSource audioSource;

        private bool yaReproducido = false;

        public AudioClip ClipGameOver { get => clipGameOver; set => clipGameOver = value; }

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            audioSource.playOnAwake = false;
        }

        private void OnEnable()
        {
            // Si el objeto o panel se activa y aún hay vidas, reiniciar bandera
            int vidas = ObtenerVidas();
            if (vidas > 0)
            {
                yaReproducido = false;
            }
            else if (vidas <= 0 && !yaReproducido)
            {
                EjecutarGameOverAudio();
            }
        }

        private void Update()
        {
            if (yaReproducido) return;

            int vidas = ObtenerVidas();
            if (vidas <= 0)
            {
                EjecutarGameOverAudio();
            }
        }

        private int ObtenerVidas()
        {
            try
            {
                if (Unity.VisualScripting.Variables.ActiveScene != null)
                {
                    return Unity.VisualScripting.Variables.ActiveScene.Get<int>("vidas");
                }
            }
            catch
            {
                // En modo edición o variables no listas
            }
            return 3;
        }

        private void EjecutarGameOverAudio()
        {
            yaReproducido = true;
            Debug.Log("[GameOverAudio] Luna se ha quedado sin vidas (0). Reproduciendo gameover.wav y pausando música...");

            // 1. Pausar la música de fondo del GestorMusica
            if (GestorMusica.Instancia != null)
            {
                AudioSource musicaSrc = GestorMusica.Instancia.GetComponent<AudioSource>();
                if (musicaSrc != null && musicaSrc.isPlaying)
                {
                    musicaSrc.Pause();
                }
            }

            // 2. Reproducir el clip de Game Over
            if (clipGameOver != null && audioSource != null)
            {
                audioSource.PlayOneShot(clipGameOver);
            }
        }

#if UNITY_EDITOR
        [MenuItem("Semillas del Bosque/Audio/Agregar Audio Game Over")]
        public static void MenuAgregarGameOverAudio()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[GameOverAudio] No se encontró ningún Canvas en la escena.");
                return;
            }

            // Buscar si ya existe en Canvas o en PanelGameOver
            GameOverAudio existente = canvas.GetComponentInChildren<GameOverAudio>(true);
            if (existente != null)
            {
                Debug.LogWarning($"[GameOverAudio] Ya existe el componente GameOverAudio en '{existente.gameObject.name}'. Reconfigurando sin duplicar...");
                ConfigurarClip(existente);
                Selection.activeGameObject = existente.gameObject;
                return;
            }

            Debug.Log("[GameOverAudio] Agregando GameOverAudio al Canvas...");
            Undo.RegisterCompleteObjectUndo(canvas.gameObject, "Agregar GameOverAudio");

            GameOverAudio audio = Undo.AddComponent<GameOverAudio>(canvas.gameObject);
            ConfigurarClip(audio);

            EditorUtility.SetDirty(canvas.gameObject);
            Selection.activeGameObject = canvas.gameObject;
            Debug.Log("[GameOverAudio] ¡GameOverAudio agregado y enlazado con gameover.wav con éxito!");
        }

        private static void ConfigurarClip(GameOverAudio audio)
        {
            if (audio.clipGameOver == null)
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/gameover.wav");
                audio.clipGameOver = clip;
            }
            EditorUtility.SetDirty(audio);
        }
#endif
    }
}

