using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using SemillasDelBosque.Audio;

namespace SemillasDelBosque.Sistema
{
    /// <summary>
    /// GestorJuego: Singleton persistente con DontDestroyOnLoad.
    /// Administra la configuración de audio (AudioMixer + PlayerPrefs), resolución,
    /// progreso de niveles desbloqueados ("NivelDesbloqueado") y carga de escenas.
    /// </summary>
    public class GestorJuego : MonoBehaviour
    {
        public const string CLAVE_NIVEL_DESBLOQUEADO = "NivelDesbloqueado";
        public const string CLAVE_VOL_MUSICA = "VolMusica";
        public const string CLAVE_VOL_SFX = "VolSFX";
        public const string CLAVE_PANTALLA_COMPLETA = "PantallaCompleta";

        public static readonly string[] NombresEscenasNiveles =
        {
            "Claro del Despertar",      // Nivel 1
            "Bosque Profundo",          // Nivel 2
            "Lago de las Luciernagas",  // Nivel 3
            "Cima de los Recuerdos"     // Nivel 4
        };

        public const string ESCENA_MENU_PRINCIPAL = "MenuPrincipal";

        public static GestorJuego Instancia { get; private set; }

        [Header("Audio y Mixer")]
        [Tooltip("AudioMixer opcional con parámetros expuestos 'VolMusica' y 'VolSFX'.")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string parametroVolMusica = "VolMusica";
        [SerializeField] private string parametroVolSFX = "VolSFX";

        [Header("Estado Actual")]
        [Range(0f, 1f)]
        [SerializeField] private float volumenMusica = 0.8f;

        [Range(0f, 1f)]
        [SerializeField] private float volumenSFX = 0.8f;

        public float VolumenMusica => volumenMusica;
        public float VolumenSFX => volumenSFX;
        public AudioMixer AudioMixerPrincipal { get => audioMixer; set => audioMixer = value; }

        public event Action<float> AlCambiarVolumenMusica;
        public event Action<float> AlCambiarVolumenSFX;

        /// <summary>
        /// Garantiza que exista una instancia activa incluso si se inicia Play Mode directamente desde un nivel.
        /// </summary>
        public static GestorJuego AsegurarInstancia()
        {
            if (Instancia != null) return Instancia;

            GestorJuego existente = FindAnyObjectByType<GestorJuego>();
            if (existente != null)
            {
                Instancia = existente;
                return Instancia;
            }

            GameObject go = new GameObject("GestorJuego");
            return go.AddComponent<GestorJuego>();
        }

        private void Awake()
        {
            if (Instancia != null && Instancia != this)
            {
                Destroy(gameObject);
                return;
            }

            Instancia = this;
            DontDestroyOnLoad(gameObject);

            CargarPreferencias();
            SceneManager.sceneLoaded += AlCargarEscena;
        }

        private void OnDestroy()
        {
            if (Instancia == this)
            {
                SceneManager.sceneLoaded -= AlCargarEscena;
                Instancia = null;
            }
        }

        private void Start()
        {
            AplicarVolumenes();
            SincronizarVariableNivelDesbloqueado();
        }

        private void AlCargarEscena(Scene escena, LoadSceneMode modo)
        {
            AplicarVolumenes();
            SincronizarVariableNivelDesbloqueado();
        }

        /// <summary>
        /// Carga las preferencias de audio, pantalla y nivel desde PlayerPrefs.
        /// </summary>
        public void CargarPreferencias()
        {
            volumenMusica = Mathf.Clamp01(PlayerPrefs.GetFloat(CLAVE_VOL_MUSICA, 0.8f));
            volumenSFX = Mathf.Clamp01(PlayerPrefs.GetFloat(CLAVE_VOL_SFX, 0.8f));

            if (PlayerPrefs.HasKey(CLAVE_PANTALLA_COMPLETA))
            {
                Screen.fullScreen = PlayerPrefs.GetInt(CLAVE_PANTALLA_COMPLETA, 1) == 1;
            }
        }

        /// <summary>
        /// Ajusta y guarda automáticamente el volumen de la música en AudioMixer y PlayerPrefs.
        /// </summary>
        public void EstablecerVolumenMusica(float valor)
        {
            volumenMusica = Mathf.Clamp01(valor);
            PlayerPrefs.SetFloat(CLAVE_VOL_MUSICA, volumenMusica);
            PlayerPrefs.Save();

            AplicarVolumenMusica();
            AlCambiarVolumenMusica?.Invoke(volumenMusica);
        }

        /// <summary>
        /// Ajusta y guarda automáticamente el volumen de efectos SFX en AudioMixer y PlayerPrefs.
        /// </summary>
        public void EstablecerVolumenSFX(float valor)
        {
            volumenSFX = Mathf.Clamp01(valor);
            PlayerPrefs.SetFloat(CLAVE_VOL_SFX, volumenSFX);
            PlayerPrefs.Save();

            AplicarVolumenSFX();
            AlCambiarVolumenSFX?.Invoke(volumenSFX);
        }

        public void AplicarVolumenes()
        {
            AplicarVolumenMusica();
            AplicarVolumenSFX();
        }

        private void AplicarVolumenMusica()
        {
            if (audioMixer != null && !string.IsNullOrEmpty(parametroVolMusica))
            {
                audioMixer.SetFloat(parametroVolMusica, ConvertirLinealADecibelios(volumenMusica));
            }

            if (GestorMusica.Instancia != null)
            {
                GestorMusica.Instancia.EstablecerMultiplicadorVolumen(volumenMusica);
            }
        }

        private void AplicarVolumenSFX()
        {
            if (audioMixer != null && !string.IsNullOrEmpty(parametroVolSFX))
            {
                audioMixer.SetFloat(parametroVolSFX, ConvertirLinealADecibelios(volumenSFX));
            }
        }

        private static float ConvertirLinealADecibelios(float valorLineal)
        {
            float seguro = Mathf.Clamp(valorLineal, 0.0001f, 1f);
            return Mathf.Log10(seguro) * 20f;
        }

        /// <summary>
        /// Actualiza la resolución y modo de pantalla completa.
        /// </summary>
        public void EstablecerResolucion(int ancho, int alto, bool pantallaCompleta)
        {
            Screen.SetResolution(ancho, alto, pantallaCompleta);
            PlayerPrefs.SetInt(CLAVE_PANTALLA_COMPLETA, pantallaCompleta ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Guarda el nivel máximo desbloqueado (1..4) en PlayerPrefs y lo sincroniza con Visual Scripting.
        /// </summary>
        public void GuardarNivelAlcanzado(int nivel)
        {
            int nivelAcotado = Mathf.Clamp(nivel, 1, NombresEscenasNiveles.Length);
            int actual = PlayerPrefs.GetInt(CLAVE_NIVEL_DESBLOQUEADO, 1);

            if (nivelAcotado > actual)
            {
                PlayerPrefs.SetInt(CLAVE_NIVEL_DESBLOQUEADO, nivelAcotado);
                PlayerPrefs.Save();
                Debug.Log($"[GestorJuego] Progreso guardado: NivelDesbloqueado = {nivelAcotado}");
            }

            SincronizarVariableNivelDesbloqueado();
        }

        /// <summary>
        /// Devuelve el nivel desbloqueado desde PlayerPrefs (mínimo 1, máximo 4),
        /// verificando también si existe una variable de Visual Scripting mayor.
        /// </summary>
        public int ObtenerNivelDesbloqueado()
        {
            int desdePrefs = PlayerPrefs.GetInt(CLAVE_NIVEL_DESBLOQUEADO, 1);
            int desdeVsApp = UtilidadesJugador.LeerVariableAplicacion(CLAVE_NIVEL_DESBLOQUEADO, 1);
            int desdeVsEscena = UtilidadesJugador.LeerVariableEscena(CLAVE_NIVEL_DESBLOQUEADO, 1);

            int mayor = Mathf.Max(desdePrefs, Mathf.Max(desdeVsApp, desdeVsEscena));
            return Mathf.Clamp(mayor, 1, NombresEscenasNiveles.Length);
        }

        private void SincronizarVariableNivelDesbloqueado()
        {
            int nivel = Mathf.Clamp(PlayerPrefs.GetInt(CLAVE_NIVEL_DESBLOQUEADO, 1), 1, NombresEscenasNiveles.Length);
            UtilidadesJugador.EscribirVariableAplicacion(CLAVE_NIVEL_DESBLOQUEADO, nivel);

            try
            {
                if (Unity.VisualScripting.Variables.ActiveScene != null &&
                    Unity.VisualScripting.Variables.ActiveScene.IsDefined(CLAVE_NIVEL_DESBLOQUEADO))
                {
                    Unity.VisualScripting.Variables.ActiveScene.Set(CLAVE_NIVEL_DESBLOQUEADO, nivel);
                }
            }
            catch
            {
                // Ignorar si no hay variables de escena en la escena actual (ej. MenuPrincipal)
            }
        }

        /// <summary>
        /// Carga la escena correspondiente al número de nivel (1..4).
        /// </summary>
        public void CargarNivel(int numeroNivel)
        {
            int indice = Mathf.Clamp(numeroNivel, 1, NombresEscenasNiveles.Length) - 1;
            CargarEscena(NombresEscenasNiveles[indice]);
        }

        /// <summary>
        /// Carga la escena del Menú Principal.
        /// </summary>
        public void CargarMenuPrincipal()
        {
            CargarEscena(ESCENA_MENU_PRINCIPAL);
        }

        /// <summary>
        /// Carga una escena por nombre si está registrada en Build Settings.
        /// </summary>
        public void CargarEscena(string nombreEscena)
        {
            Time.timeScale = 1f;
            if (string.IsNullOrEmpty(nombreEscena))
            {
                Debug.LogWarning("[GestorJuego] Nombre de escena vacío.");
                return;
            }

            if (Application.CanStreamedLevelBeLoaded(nombreEscena))
            {
                SceneManager.LoadScene(nombreEscena);
            }
            else
            {
                Debug.LogError($"[GestorJuego] La escena '{nombreEscena}' no se puede cargar. " +
                               "Asegúrate de agregarla en File > Build Profiles / Build Settings.");
            }
        }

        public static string ObtenerNombreEscenaPorNivel(int numeroNivel)
        {
            int indice = Mathf.Clamp(numeroNivel, 1, NombresEscenasNiveles.Length) - 1;
            return NombresEscenasNiveles[indice];
        }
    }
}

