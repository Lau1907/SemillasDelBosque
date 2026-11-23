using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SemillasDelBosque.Audio;
using SemillasDelBosque.Sombra;

namespace SemillasDelBosque.Sistema
{
    /// <summary>
    /// ControladorMeta: Complementa el trigger de la Meta en cada nivel sin modificar GrafoMeta.
    /// Detecta al jugador ("Jugador", "Luna", "Nahui"), guarda el progreso en GestorJuego,
    /// y gestiona el paso al siguiente nivel o la pantalla final "¡Bosque Restaurado!" en Nivel 4.
    /// </summary>
    public class ControladorMeta : MonoBehaviour
    {
        [Header("Configuración de Nivel")]
        [SerializeField] private int numeroNivelActual = 1;
        [SerializeField] private string nombreSiguienteEscena = "Bosque Profundo";
        [SerializeField] private bool esNivelFinal = false;
        [SerializeField] private float tiempoEsperaAntesDeCargar = 3.0f;

        [Header("Condición de Mini-Jefe (Nivel 4)")]
        [SerializeField] private bool requiereJefeDerrotado = false;
        [SerializeField] private GranSombraNahual jefeRequerido;

        [Header("UI de Meta")]
        [SerializeField] private GameObject panelNivelCompleto;
        [SerializeField] private TextMeshProUGUI textoTituloPanel;
        [SerializeField] private GameObject panelAvisoJefe;

        private bool metaActivada = false;

        public int NumeroNivelActual { get => numeroNivelActual; set => numeroNivelActual = value; }
        public string NombreSiguienteEscena { get => nombreSiguienteEscena; set => nombreSiguienteEscena = value; }
        public bool EsNivelFinal { get => esNivelFinal; set => esNivelFinal = value; }
        public bool RequiereJefeDerrotado { get => requiereJefeDerrotado; set => requiereJefeDerrotado = value; }
        public GranSombraNahual JefeRequerido { get => jefeRequerido; set => jefeRequerido = value; }
        public GameObject PanelNivelCompleto { get => panelNivelCompleto; set => panelNivelCompleto = value; }
        public TextMeshProUGUI TextoTituloPanel { get => textoTituloPanel; set => textoTituloPanel = value; }
        public GameObject PanelAvisoJefe { get => panelAvisoJefe; set => panelAvisoJefe = value; }

        private void Awake()
        {
            if (panelNivelCompleto == null)
            {
                panelNivelCompleto = BuscarObjetoInactivoPorNombre("PanelNivelCompleto");
            }

            if (requiereJefeDerrotado && jefeRequerido == null)
            {
                jefeRequerido = FindAnyObjectByType<GranSombraNahual>();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (metaActivada) return;
            if (!UtilidadesJugador.EsJugador(other)) return;

            if (requiereJefeDerrotado && jefeRequerido != null && !jefeRequerido.EstaDerrotada)
            {
                Debug.Log("[ControladorMeta] Aún debes derrotar a la Gran Sombra de Nahual antes de cruzar la meta.");
                if (panelNivelCompleto != null)
                {
                    panelNivelCompleto.SetActive(false);
                }
                StartCoroutine(MostrarAvisoJefe());
                return;
            }

            ActivarMeta();
        }

        private void LateUpdate()
        {
            // Evita que GrafoMeta muestre PanelNivelCompleto si el mini-jefe aún sigue vivo en Nivel 4
            if (!metaActivada && requiereJefeDerrotado && jefeRequerido != null && !jefeRequerido.EstaDerrotada)
            {
                if (panelNivelCompleto != null && panelNivelCompleto.activeSelf)
                {
                    panelNivelCompleto.SetActive(false);
                }
            }
        }

        public void ActivarMeta()
        {
            metaActivada = true;

            GestorJuego gestor = GestorJuego.AsegurarInstancia();
            int nivelADesbloquear = esNivelFinal ? 4 : Mathf.Clamp(numeroNivelActual + 1, 1, 4);
            gestor.GuardarNivelAlcanzado(nivelADesbloquear);

            if (GestorMusica.Instancia != null)
            {
                GestorMusica.Instancia.ZonaRestaurada();
            }

            if (panelNivelCompleto != null)
            {
                panelNivelCompleto.SetActive(true);
                ConfigurarTextoVictoria();
            }

            if (esNivelFinal)
            {
                AsegurarBotonMenuEnFinDeJuego();
            }
            else if (!string.IsNullOrEmpty(nombreSiguienteEscena))
            {
                StartCoroutine(CargarSiguienteTrasEspera(gestor));
            }
        }

        private void ConfigurarTextoVictoria()
        {
            if (!esNivelFinal) return;

            if (textoTituloPanel == null && panelNivelCompleto != null)
            {
                TextMeshProUGUI[] textos = panelNivelCompleto.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI t in textos)
                {
                    if (t.gameObject.name != "TextoSemillas")
                    {
                        textoTituloPanel = t;
                        break;
                    }
                }
            }

            if (textoTituloPanel != null)
            {
                textoTituloPanel.text = "¡Bosque Restaurado!\nEl Ahuehuete florece y Chapultepec brilla otra vez.\n¡Tlazohcamati, Guardiana Nahui!";
            }
        }

        private void AsegurarBotonMenuEnFinDeJuego()
        {
            if (panelNivelCompleto == null) return;

            Transform existente = panelNivelCompleto.transform.Find("BotonMenuPrincipal");
            if (existente != null)
            {
                Button btnExistente = existente.GetComponent<Button>();
                if (btnExistente != null)
                {
                    btnExistente.onClick.RemoveAllListeners();
                    btnExistente.onClick.AddListener(() => GestorJuego.AsegurarInstancia().CargarMenuPrincipal());
                }
                return;
            }

            GameObject goBoton = new GameObject("BotonMenuPrincipal", typeof(RectTransform), typeof(Image), typeof(Button));
            goBoton.transform.SetParent(panelNivelCompleto.transform, false);

            RectTransform rt = goBoton.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, -110f);
            rt.sizeDelta = new Vector2(260f, 52f);

            Image img = goBoton.GetComponent<Image>();
            img.color = new Color(0.18f, 0.52f, 0.34f, 1f);

            GameObject goTxt = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
            goTxt.transform.SetParent(goBoton.transform, false);
            RectTransform rtTxt = goTxt.GetComponent<RectTransform>();
            rtTxt.anchorMin = Vector2.zero;
            rtTxt.anchorMax = Vector2.one;
            rtTxt.offsetMin = Vector2.zero;
            rtTxt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = goTxt.GetComponent<TextMeshProUGUI>();
            tmp.text = "Volver al Menú";
            tmp.fontSize = 22;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            Button btn = goBoton.GetComponent<Button>();
            btn.onClick.AddListener(() => GestorJuego.AsegurarInstancia().CargarMenuPrincipal());
        }

        private IEnumerator CargarSiguienteTrasEspera(GestorJuego gestor)
        {
            yield return new WaitForSeconds(tiempoEsperaAntesDeCargar);
            gestor.CargarEscena(nombreSiguienteEscena);
        }

        private IEnumerator MostrarAvisoJefe()
        {
            if (panelAvisoJefe != null)
            {
                panelAvisoJefe.SetActive(true);
                yield return new WaitForSeconds(2.2f);
                panelAvisoJefe.SetActive(false);
            }
            else
            {
                yield break;
            }
        }

        private static GameObject BuscarObjetoInactivoPorNombre(string nombre)
        {
            Transform[] todos = FindObjectsByType<Transform>(FindObjectsInactive.Include);
            for (int i = 0; i < todos.Length; i++)
            {
                if (todos[i].name == nombre)
                {
                    return todos[i].gameObject;
                }
            }
            return null;
        }
    }
}
