using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using SemillasDelBosque.Sistema;

namespace SemillasDelBosque.UI
{
    /// <summary>
    /// MenuPausaController: Panel de pausa funcional presente en los 4 niveles.
    /// Congela el juego (Time.timeScale = 0f), se activa con Esc / P y desde botón en pantalla.
    /// Contiene 4 botones requeridos: Reanudar, Reiniciar, Opciones y Salir al Menú.
    /// </summary>
    public class MenuPausaController : MonoBehaviour
    {
        [Header("Paneles")]
        [SerializeField] private GameObject panelPausa;
        [SerializeField] private GameObject subpanelOpciones;

        [Header("Botones de Pausa")]
        [SerializeField] private Button botonReanudar;
        [SerializeField] private Button botonReiniciar;
        [SerializeField] private Button botonOpciones;
        [SerializeField] private Button botonSalirMenu;

        [Header("Opciones de Pausa")]
        [SerializeField] private Slider sliderMusica;
        [SerializeField] private Slider sliderSFX;
        [SerializeField] private Toggle toggleControlesTactiles;
        [SerializeField] private Button botonCerrarOpciones;

        private bool estaPausado = false;
        public bool EstaPausado => estaPausado;

        private void Awake()
        {
            if (panelPausa == null) panelPausa = gameObject;

            if (botonReanudar != null) botonReanudar.onClick.AddListener(Reanudar);
            if (botonReiniciar != null) botonReiniciar.onClick.AddListener(ReiniciarNivel);
            if (botonOpciones != null) botonOpciones.onClick.AddListener(AbrirOpciones);
            if (botonSalirMenu != null) botonSalirMenu.onClick.AddListener(SalirAlMenu);
            if (botonCerrarOpciones != null) botonCerrarOpciones.onClick.AddListener(CerrarOpciones);

            if (sliderMusica != null)
            {
                sliderMusica.onValueChanged.AddListener(val => GestorJuego.AsegurarInstancia().EstablecerVolumenMusica(val));
            }
            if (sliderSFX != null)
            {
                sliderSFX.onValueChanged.AddListener(val => GestorJuego.AsegurarInstancia().EstablecerVolumenSFX(val));
            }
            if (toggleControlesTactiles != null)
            {
                toggleControlesTactiles.onValueChanged.AddListener(val =>
                {
                    PlayerPrefs.SetInt(ControlesTactiles.PREF_CONTROLES_TACTILES, val ? 1 : 0);
                    PlayerPrefs.Save();
                    var ct = FindAnyObjectByType<ControlesTactiles>();
                    if (ct != null) ct.ActualizarVisibilidad();
                });
            }

            panelPausa.SetActive(false);
            if (subpanelOpciones != null) subpanelOpciones.SetActive(false);
        }

        private void Start()
        {
            CargarValoresOpciones();
        }

        private void Update()
        {
            bool teclaPausa = false;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
                {
                    teclaPausa = true;
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                {
                    teclaPausa = true;
                }
            }

            if (teclaPausa)
            {
                TogglePausa();
            }
        }

        public void TogglePausa()
        {
            if (estaPausado) Reanudar();
            else Pausar();
        }

        public void Pausar()
        {
            estaPausado = true;
            panelPausa.SetActive(true);
            if (subpanelOpciones != null) subpanelOpciones.SetActive(false);
            Time.timeScale = 0f;
            CargarValoresOpciones();
        }

        public void Reanudar()
        {
            estaPausado = false;
            panelPausa.SetActive(false);
            if (subpanelOpciones != null) subpanelOpciones.SetActive(false);
            Time.timeScale = 1f;
        }

        public void ReiniciarNivel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void AbrirOpciones()
        {
            CargarValoresOpciones();
            if (subpanelOpciones != null) subpanelOpciones.SetActive(true);
        }

        public void CerrarOpciones()
        {
            if (subpanelOpciones != null) subpanelOpciones.SetActive(false);
        }

        public void SalirAlMenu()
        {
            Time.timeScale = 1f;
            GestorJuego.AsegurarInstancia().CargarMenuPrincipal();
        }

        private void CargarValoresOpciones()
        {
            GestorJuego gj = GestorJuego.AsegurarInstancia();
            if (sliderMusica != null) sliderMusica.value = gj.VolumenMusica;
            if (sliderSFX != null) sliderSFX.value = gj.VolumenSFX;
            if (toggleControlesTactiles != null)
            {
                toggleControlesTactiles.isOn = ControlesTactiles.DebeEstarActivo();
            }
        }
    }
}