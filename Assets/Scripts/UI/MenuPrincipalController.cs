using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using SemillasDelBosque.Sistema;

namespace SemillasDelBosque.UI
{
    /// <summary>
    /// MenuPrincipalController: Controla la escena MenuPrincipal.
    /// Gestiona la Splash Screen inicial (Logo del equipo) y los paneles de Menú Principal,
    /// Opciones (AudioMixer/Volumen y Resolución) y Créditos (Laura Tapia Cid y Vanesa Medrano Hernández).
    /// </summary>
    public class MenuPrincipalController : MonoBehaviour
    {
        [Header("Paneles")]
        [SerializeField] private GameObject panelSplash;
        [SerializeField] private CanvasGroup canvasGroupSplash;
        [SerializeField] private GameObject panelMenuPrincipal;
        [SerializeField] private GameObject panelOpciones;
        [SerializeField] private GameObject panelCreditos;

        [Header("Splash Screen")]
        [SerializeField] private float duracionSplash = 2.0f;
        [SerializeField] private float duracionFadeSplash = 0.5f;

        [Header("Botones Principales")]
        [SerializeField] private Button botonJugar;
        [SerializeField] private Button botonContinuar;
        [SerializeField] private TextMeshProUGUI textoBotonContinuar;
        [SerializeField] private Button botonLibro;
        [SerializeField] private Button botonOpciones;
        [SerializeField] private Button botonCreditos;
        [SerializeField] private Button botonSalir;

        [Header("Libro de Historia (Códice)")]
        [SerializeField] private LibroHistoriaController libroHistoria;

        [Header("Opciones de Audio y Pantalla")]
        [SerializeField] private Slider sliderMusica;
        [SerializeField] private TextMeshProUGUI textoVolMusica;
        [SerializeField] private Slider sliderSFX;
        [SerializeField] private TextMeshProUGUI textoVolSFX;
        [SerializeField] private TMP_Dropdown dropdownResolucion;
        [SerializeField] private Toggle togglePantallaCompleta;
        [SerializeField] private Button botonVolverOpciones;

        [Header("Créditos")]
        [SerializeField] private TextMeshProUGUI textoCreditos;
        [SerializeField] private Button botonVolverCreditos;

        private Resolution[] resolucionesDisponibles;
        private bool splashFinalizado = false;

        public GameObject PanelSplash { get => panelSplash; set => panelSplash = value; }
        public CanvasGroup CanvasGroupSplash { get => canvasGroupSplash; set => canvasGroupSplash = value; }
        public GameObject PanelMenuPrincipal { get => panelMenuPrincipal; set => panelMenuPrincipal = value; }
        public GameObject PanelOpciones { get => panelOpciones; set => panelOpciones = value; }
        public GameObject PanelCreditos { get => panelCreditos; set => panelCreditos = value; }
        public Button BotonJugar { get => botonJugar; set => botonJugar = value; }
        public Button BotonContinuar { get => botonContinuar; set => botonContinuar = value; }
        public TextMeshProUGUI TextoBotonContinuar { get => textoBotonContinuar; set => textoBotonContinuar = value; }
        public Button BotonLibro { get => botonLibro; set => botonLibro = value; }
        public LibroHistoriaController LibroHistoria { get => libroHistoria; set => libroHistoria = value; }
        public Button BotonOpciones { get => botonOpciones; set => botonOpciones = value; }
        public Button BotonCreditos { get => botonCreditos; set => botonCreditos = value; }
        public Button BotonSalir { get => botonSalir; set => botonSalir = value; }
        public Slider SliderMusica { get => sliderMusica; set => sliderMusica = value; }
        public TextMeshProUGUI TextoVolMusica { get => textoVolMusica; set => textoVolMusica = value; }
        public Slider SliderSFX { get => sliderSFX; set => sliderSFX = value; }
        public TextMeshProUGUI TextoVolSFX { get => textoVolSFX; set => textoVolSFX = value; }
        public TMP_Dropdown DropdownResolucion { get => dropdownResolucion; set => dropdownResolucion = value; }
        public Toggle TogglePantallaCompleta { get => togglePantallaCompleta; set => togglePantallaCompleta = value; }
        public Button BotonVolverOpciones { get => botonVolverOpciones; set => botonVolverOpciones = value; }
        public TextMeshProUGUI TextoCreditos { get => textoCreditos; set => textoCreditos = value; }
        public Button BotonVolverCreditos { get => botonVolverCreditos; set => botonVolverCreditos = value; }

        private void Start()
        {
            GestorJuego.AsegurarInstancia();
            ConfigurarBotones();
            ConfigurarOpciones();
            ConfigurarCreditos();
            ActualizarEstadoBotonContinuar();

            if (panelSplash != null)
            {
                panelSplash.SetActive(true);
                if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
                if (panelOpciones != null) panelOpciones.SetActive(false);
                if (panelCreditos != null) panelCreditos.SetActive(false);
                StartCoroutine(SecuenciaSplash());
            }
            else
            {
                MostrarPanelPrincipal();
            }
        }

        private void Update()
        {
            if (splashFinalizado || panelSplash == null || !panelSplash.activeSelf) return;

            bool clicOTecla = false;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                clicOTecla = true;
            }
            else if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                clicOTecla = true;
            }

            if (clicOTecla)
            {
                StopAllCoroutines();
                FinalizarSplashInmediato();
            }
        }

        private IEnumerator SecuenciaSplash()
        {
            if (canvasGroupSplash != null) canvasGroupSplash.alpha = 1f;
            yield return new WaitForSeconds(duracionSplash);

            if (canvasGroupSplash != null && duracionFadeSplash > 0f)
            {
                float t = 0f;
                while (t < duracionFadeSplash)
                {
                    t += Time.deltaTime;
                    canvasGroupSplash.alpha = Mathf.Lerp(1f, 0f, t / duracionFadeSplash);
                    yield return null;
                }
            }

            FinalizarSplashInmediato();
        }

        private void FinalizarSplashInmediato()
        {
            splashFinalizado = true;
            if (panelSplash != null) panelSplash.SetActive(false);
            MostrarPanelPrincipal();
        }

        private void ConfigurarBotones()
        {
            if (botonJugar != null)
            {
                botonJugar.onClick.RemoveAllListeners();
                botonJugar.onClick.AddListener(Jugar);
            }
            if (botonContinuar != null)
            {
                botonContinuar.onClick.RemoveAllListeners();
                botonContinuar.onClick.AddListener(Continuar);
            }
            if (botonLibro != null)
            {
                botonLibro.onClick.RemoveAllListeners();
                botonLibro.onClick.AddListener(AbrirLibroHistoria);
            }
            if (botonOpciones != null)
            {
                botonOpciones.onClick.RemoveAllListeners();
                botonOpciones.onClick.AddListener(AbrirOpciones);
            }
            if (botonCreditos != null)
            {
                botonCreditos.onClick.RemoveAllListeners();
                botonCreditos.onClick.AddListener(AbrirCreditos);
            }
            if (botonSalir != null)
            {
                botonSalir.onClick.RemoveAllListeners();
                botonSalir.onClick.AddListener(Salir);
            }
            if (botonVolverOpciones != null)
            {
                botonVolverOpciones.onClick.RemoveAllListeners();
                botonVolverOpciones.onClick.AddListener(MostrarPanelPrincipal);
            }
            if (botonVolverCreditos != null)
            {
                botonVolverCreditos.onClick.RemoveAllListeners();
                botonVolverCreditos.onClick.AddListener(MostrarPanelPrincipal);
            }
        }

        private void ConfigurarOpciones()
        {
            GestorJuego gestor = GestorJuego.AsegurarInstancia();

            if (sliderMusica != null)
            {
                sliderMusica.minValue = 0.0001f;
                sliderMusica.maxValue = 1f;
                float vMus = Mathf.Max(0.0001f, PlayerPrefs.GetFloat(GestorJuego.CLAVE_VOL_MUSICA, gestor.VolumenMusica));
                sliderMusica.SetValueWithoutNotify(vMus);
                ActualizarEtiquetaPorcentaje(textoVolMusica, vMus);
                sliderMusica.onValueChanged.RemoveAllListeners();
                sliderMusica.onValueChanged.AddListener(val =>
                {
                    GestorJuego.AsegurarInstancia().EstablecerVolumenMusica(val);
                    ActualizarEtiquetaPorcentaje(textoVolMusica, val);
                });
            }

            if (sliderSFX != null)
            {
                sliderSFX.minValue = 0.0001f;
                sliderSFX.maxValue = 1f;
                float vSfx = Mathf.Max(0.0001f, PlayerPrefs.GetFloat(GestorJuego.CLAVE_VOL_SFX, gestor.VolumenSFX));
                sliderSFX.SetValueWithoutNotify(vSfx);
                ActualizarEtiquetaPorcentaje(textoVolSFX, vSfx);
                sliderSFX.onValueChanged.RemoveAllListeners();
                sliderSFX.onValueChanged.AddListener(val =>
                {
                    GestorJuego.AsegurarInstancia().EstablecerVolumenSFX(val);
                    ActualizarEtiquetaPorcentaje(textoVolSFX, val);
                });
            }

            if (togglePantallaCompleta != null)
            {
                togglePantallaCompleta.SetIsOnWithoutNotify(Screen.fullScreen);
                togglePantallaCompleta.onValueChanged.RemoveAllListeners();
                togglePantallaCompleta.onValueChanged.AddListener(AlCambiarPantallaCompleta);
            }

            if (dropdownResolucion != null)
            {
                PoblarResoluciones();
            }
        }

        private void PoblarResoluciones()
        {
            Resolution[] todas = Screen.resolutions;
            List<Resolution> unicas = new List<Resolution>();
            List<string> etiquetas = new List<string>();
            int indiceActual = 0;

            for (int i = 0; i < todas.Length; i++)
            {
                Resolution r = todas[i];
                bool duplicada = false;
                for (int j = 0; j < unicas.Count; j++)
                {
                    if (unicas[j].width == r.width && unicas[j].height == r.height)
                    {
                        duplicada = true;
                        break;
                    }
                }
                if (duplicada) continue;

                unicas.Add(r);
                etiquetas.Add($"{r.width} x {r.height}");
                if (r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height)
                {
                    indiceActual = unicas.Count - 1;
                }
            }

            if (unicas.Count == 0)
            {
                unicas.Add(new Resolution { width = 1920, height = 1080 });
                etiquetas.Add("1920 x 1080");
            }

            resolucionesDisponibles = unicas.ToArray();
            dropdownResolucion.ClearOptions();
            dropdownResolucion.AddOptions(etiquetas);
            dropdownResolucion.SetValueWithoutNotify(indiceActual);
            dropdownResolucion.RefreshShownValue();

            dropdownResolucion.onValueChanged.RemoveAllListeners();
            dropdownResolucion.onValueChanged.AddListener(AlCambiarResolucion);
        }

        private void AlCambiarResolucion(int indice)
        {
            if (resolucionesDisponibles == null || indice < 0 || indice >= resolucionesDisponibles.Length) return;
            Resolution r = resolucionesDisponibles[indice];
            bool pantallaCompleta = togglePantallaCompleta != null ? togglePantallaCompleta.isOn : Screen.fullScreen;
            GestorJuego.AsegurarInstancia().EstablecerResolucion(r.width, r.height, pantallaCompleta);
        }

        private void AlCambiarPantallaCompleta(bool activa)
        {
            Screen.fullScreen = activa;
            PlayerPrefs.SetInt(GestorJuego.CLAVE_PANTALLA_COMPLETA, activa ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void ConfigurarCreditos()
        {
            if (textoCreditos == null) return;
            if (string.IsNullOrWhiteSpace(textoCreditos.text))
            {
                textoCreditos.text = ObtenerTextoCreditosOficial();
            }
        }

        public static string ObtenerTextoCreditosOficial()
        {
            return "<b>SEMILLAS DEL BOSQUE (EDICIÓN CHAPULTEPEC)</b>\n\n" +
                   "<b>Desarrollo, Diseño y Arte:</b>\n" +
                   "• Laura Tapia Cid\n" +
                   "• Vanesa Medrano Hernández\n\n" +
                   "<b>Tecnología y Licencias:</b>\n" +
                   "• Unity 6 (URP 2D) & Unity Visual Scripting\n" +
                   "• TextMesh Pro (Unity Technologies)\n" +
                   "• Sprites y Audio originales creados para el proyecto\n" +
                   "  Semillas del Bosque - Bosque de Chapultepec (CDMX).";
        }

        private void ActualizarEstadoBotonContinuar()
        {
            int nivelDesbloqueado = PlayerPrefs.GetInt(GestorJuego.CLAVE_NIVEL_DESBLOQUEADO, 1);
            nivelDesbloqueado = Mathf.Clamp(nivelDesbloqueado, 1, GestorJuego.NombresEscenasNiveles.Length);

            if (textoBotonContinuar != null)
            {
                string nombreNivel = GestorJuego.NombresEscenasNiveles[nivelDesbloqueado - 1];
                textoBotonContinuar.text = $"Continuar • Nv.{nivelDesbloqueado}: {nombreNivel}";
            }
        }

        private static void ActualizarEtiquetaPorcentaje(TextMeshProUGUI tmp, float valor)
        {
            if (tmp == null) return;
            int pct = Mathf.RoundToInt(Mathf.Clamp01(valor) * 100f);
            tmp.text = $"{pct}%";
        }

        public void AbrirLibroHistoria()
        {
            if (libroHistoria != null)
            {
                libroHistoria.Abrir();
            }
        }

        public void MostrarPanelPrincipal()
        {
            if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
            if (panelOpciones != null) panelOpciones.SetActive(false);
            if (panelCreditos != null) panelCreditos.SetActive(false);
            ActualizarEstadoBotonContinuar();
        }

        /// <summary>
        /// Botón 1: Carga el Nivel 1 ("Claro del Despertar").
        /// </summary>
        public void Jugar()
        {
            GestorJuego.AsegurarInstancia().CargarNivel(1);
        }

        /// <summary>
        /// Botón 2: Lee PlayerPrefs.GetInt("NivelDesbloqueado", 1) y carga la escena correspondiente.
        /// </summary>
        public void Continuar()
        {
            int nivelDesbloqueado = PlayerPrefs.GetInt(GestorJuego.CLAVE_NIVEL_DESBLOQUEADO, 1);
            GestorJuego.AsegurarInstancia().CargarNivel(nivelDesbloqueado);
        }

        /// <summary>
        /// Botón 3: Abre el panel de configuración de audio y resolución.
        /// </summary>
        public void AbrirOpciones()
        {
            if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
            if (panelCreditos != null) panelCreditos.SetActive(false);
            if (panelOpciones != null) panelOpciones.SetActive(true);
        }

        /// <summary>
        /// Botón 4: Muestra la pantalla de créditos.
        /// </summary>
        public void AbrirCreditos()
        {
            if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
            if (panelOpciones != null) panelOpciones.SetActive(false);
            if (panelCreditos != null) panelCreditos.SetActive(true);
        }

        /// <summary>
        /// Botón 5: Cierra la aplicación.
        /// </summary>
        public void Salir()
        {
            Debug.Log("[MenuPrincipalController] Saliendo de Semillas del Bosque...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}

