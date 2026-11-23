using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using SemillasDelBosque.Sistema;

namespace SemillasDelBosque.UI
{
    /// <summary>
    /// LibroHistoriaController: Códice interactivo de 2 páginas ("El Códice de Chapultepec").
    /// Mantiene viva la historia y leyenda mexicana del juego, desbloqueando capítulos
    /// conforme avanza el jugador en NivelDesbloqueado. Funciona tanto en el Menú Principal
    /// como dentro de los niveles (tecla L o botón del HUD).
    /// </summary>
    public class LibroHistoriaController : MonoBehaviour
    {
        [Serializable]
        public class PaginaCodice
        {
            public string numeroCapitulo;
            public string titulo;
            public string subtituloLugar;
            [TextArea(4, 10)] public string textoHistoria;
            public string rutaSpriteIlustracion;
            public int nivelRequerido = 1;
        }

        [Header("Referencias de UI del Libro")]
        [SerializeField] private GameObject panelLibroRaiz;
        [SerializeField] private TextMeshProUGUI tmpCapitulo;
        [SerializeField] private TextMeshProUGUI tmpTitulo;
        [SerializeField] private TextMeshProUGUI tmpLugar;
        [SerializeField] private TextMeshProUGUI tmpHistoria;
        [SerializeField] private TextMeshProUGUI tmpNumeroPagina;
        [SerializeField] private Image imagenIlustracion;
        [SerializeField] private Button botonAnterior;
        [SerializeField] private Button botonSiguiente;
        [SerializeField] private Button botonCerrar;

        [Header("Ilustraciones Serializadas (para Editor y Build)")]
        [SerializeField] private Sprite[] ilustracionesPorPagina = new Sprite[6];

        [Header("Comportamiento en Nivel")]
        [SerializeField] private bool permitirTeclaEnJuego = false;
        [SerializeField] private bool pausarAlAbrir = false;

        private readonly List<PaginaCodice> paginas = new List<PaginaCodice>();
        private readonly Dictionary<string, Sprite> cacheSprites = new Dictionary<string, Sprite>();
        private int indiceActual = 0;

        public GameObject PanelLibroRaiz { get => panelLibroRaiz; set => panelLibroRaiz = value; }
        public TextMeshProUGUI TmpCapitulo { get => tmpCapitulo; set => tmpCapitulo = value; }
        public TextMeshProUGUI TmpTitulo { get => tmpTitulo; set => tmpTitulo = value; }
        public TextMeshProUGUI TmpLugar { get => tmpLugar; set => tmpLugar = value; }
        public TextMeshProUGUI TmpHistoria { get => tmpHistoria; set => tmpHistoria = value; }
        public TextMeshProUGUI TmpNumeroPagina { get => tmpNumeroPagina; set => tmpNumeroPagina = value; }
        public Image ImagenIlustracion { get => imagenIlustracion; set => imagenIlustracion = value; }
        public Button BotonAnterior { get => botonAnterior; set => botonAnterior = value; }
        public Button BotonSiguiente { get => botonSiguiente; set => botonSiguiente = value; }
        public Button BotonCerrar { get => botonCerrar; set => botonCerrar = value; }
        public Sprite[] IlustracionesPorPagina { get => ilustracionesPorPagina; set => ilustracionesPorPagina = value; }
        public bool PermitirTeclaEnJuego { get => permitirTeclaEnJuego; set => permitirTeclaEnJuego = value; }
        public bool PausarAlAbrir { get => pausarAlAbrir; set => pausarAlAbrir = value; }

        public bool EstaAbierto => panelLibroRaiz != null && panelLibroRaiz.activeSelf;

        private void Awake()
        {
            InicializarPaginasOficiales();
            EnlazarBotones();
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (permitirTeclaEnJuego && Keyboard.current.lKey.wasPressedThisFrame)
            {
                if (EstaAbierto) Cerrar();
                else AbrirEnPaginaDelNivelActual();
                return;
            }

            if (!EstaAbierto) return;

            if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
            {
                PaginaSiguiente();
            }
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
            {
                PaginaAnterior();
            }
            else if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cerrar();
            }
        }

        private void EnlazarBotones()
        {
            if (botonAnterior != null)
            {
                botonAnterior.onClick.RemoveAllListeners();
                botonAnterior.onClick.AddListener(PaginaAnterior);
            }
            if (botonSiguiente != null)
            {
                botonSiguiente.onClick.RemoveAllListeners();
                botonSiguiente.onClick.AddListener(PaginaSiguiente);
            }
            if (botonCerrar != null)
            {
                botonCerrar.onClick.RemoveAllListeners();
                botonCerrar.onClick.AddListener(Cerrar);
            }
        }

        public void Abrir()
        {
            if (paginas.Count == 0) InicializarPaginasOficiales();
            if (panelLibroRaiz != null) panelLibroRaiz.SetActive(true);
            if (pausarAlAbrir) Time.timeScale = 0f;
            MostrarPagina(indiceActual);
        }

        public void AbrirEnPaginaDelNivelActual()
        {
            string escena = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            int pag = 1;
            if (escena.Contains("Bosque")) pag = 2;
            else if (escena.Contains("Lago")) pag = 3;
            else if (escena.Contains("Cima")) pag = 4;

            indiceActual = Mathf.Clamp(pag, 0, Mathf.Max(0, paginas.Count - 1));
            Abrir();
        }

        public void Cerrar()
        {
            if (panelLibroRaiz != null) panelLibroRaiz.SetActive(false);
            if (pausarAlAbrir) Time.timeScale = 1f;
        }

        public void PaginaSiguiente()
        {
            if (paginas.Count == 0) return;
            indiceActual = Mathf.Min(paginas.Count - 1, indiceActual + 1);
            MostrarPagina(indiceActual);
        }

        public void PaginaAnterior()
        {
            if (paginas.Count == 0) return;
            indiceActual = Mathf.Max(0, indiceActual - 1);
            MostrarPagina(indiceActual);
        }

        public void RegistrarIlustracion(string clave, Sprite sprite)
        {
            if (!string.IsNullOrEmpty(clave) && sprite != null)
            {
                cacheSprites[clave] = sprite;
            }
        }

        private void MostrarPagina(int idx)
        {
            if (paginas.Count == 0) return;
            idx = Mathf.Clamp(idx, 0, paginas.Count - 1);
            PaginaCodice p = paginas[idx];

            int nivelDesbloqueado = PlayerPrefs.GetInt(GestorJuego.CLAVE_NIVEL_DESBLOQUEADO, 1);
            bool desbloqueada = nivelDesbloqueado >= p.nivelRequerido;

            if (tmpCapitulo != null)
            {
                tmpCapitulo.text = $"◈  {p.numeroCapitulo}  ◈";
            }
            if (tmpTitulo != null)
            {
                tmpTitulo.text = desbloqueada ? p.titulo : "Capítulo Cubierto por la Niebla";
            }
            if (tmpLugar != null)
            {
                tmpLugar.text = desbloqueada ? p.subtituloLugar : $"Desbloqueado al alcanzar el Nivel {p.nivelRequerido}";
            }
            if (tmpHistoria != null)
            {
                tmpHistoria.text = desbloqueada
                    ? p.textoHistoria
                    : "Las páginas de este códice aún permanecen ocultas entre la bruma del Bosque de Chapultepec.\n\n" +
                      $"Avanza hasta el Nivel {p.nivelRequerido} para revelar esta parte de la leyenda de Nahui y el Ahuehuete Sagrado.";
            }

            if (imagenIlustracion != null)
            {
                Sprite s = (ilustracionesPorPagina != null && idx < ilustracionesPorPagina.Length && ilustracionesPorPagina[idx] != null)
                    ? ilustracionesPorPagina[idx]
                    : ObtenerIlustracion(p.rutaSpriteIlustracion);
                imagenIlustracion.sprite = s;
                imagenIlustracion.enabled = (s != null);
                imagenIlustracion.color = desbloqueada ? Color.white : new Color(0.18f, 0.18f, 0.22f, 0.85f);
            }

            if (tmpNumeroPagina != null)
            {
                tmpNumeroPagina.text = $"Folio {idx + 1} / {paginas.Count}";
            }

            if (botonAnterior != null) botonAnterior.interactable = (idx > 0);
            if (botonSiguiente != null) botonSiguiente.interactable = (idx < paginas.Count - 1);
        }

        private Sprite ObtenerIlustracion(string clave)
        {
            if (string.IsNullOrEmpty(clave)) return null;
            if (cacheSprites.TryGetValue(clave, out Sprite cached) && cached != null) return cached;

            Sprite cargado = null;
#if UNITY_EDITOR
            cargado = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(clave);
#endif
            if (cargado != null)
            {
                cacheSprites[clave] = cargado;
            }
            return cargado;
        }

        private void InicializarPaginasOficiales()
        {
            paginas.Clear();

            paginas.Add(new PaginaCodice
            {
                numeroCapitulo = "PRÓLOGO DEL CÓDICE",
                titulo = "La Leyenda del Ahuehuete y el Quinto Sol",
                subtituloLugar = "Valle del Anáhuac • Bosque de Chapultepec",
                rutaSpriteIlustracion = "Assets/Sprites/Objetos/semilla_luz.png",
                nivelRequerido = 1,
                textoHistoria =
                    "Cuentan los abuelos del Anáhuac que antes del alba de la ciudad, el gran Ahuehuete Sagrado —el Viejo del Agua— " +
                    "latía en el corazón de Chapultepec alimentando cada árbol y manantial con la luz del Quinto Sol.\n\n" +
                    "Una noche sin luna, el Gran Nahual de la Niebla rompió su corazón luminoso en Semillas de Luz y cubrió el parque de sombras. " +
                    "Solo Nahui, guardiana de los Cuatro Rumbos, puede reunir las semillas y devolver el color al bosque."
            });

            paginas.Add(new PaginaCodice
            {
                numeroCapitulo = "CAPÍTULO I",
                titulo = "El Claro del Despertar y el Búho Tecólotl",
                subtituloLugar = "Jardín Botánico de Chapultepec",
                rutaSpriteIlustracion = "Assets/Sprites/Personajes/BuhoSabio_Rama.png",
                nivelRequerido = 1,
                textoHistoria =
                    "Entre los senderos apagados del Jardín Botánico, el sabio Tecólotl despierta a Nahui desde lo alto de una rama:\n\n" +
                    "«¡Tlazohcamati, Nahui! Las sombras patrullan los caminos, pero su fuerza es ilusión: salta dos veces sobre su corona para disiparlas. " +
                    "Enciende las linternas de piedra del camino y recoge las Semillas del Sol para que el bosque vuelva a respirar.»"
            });

            paginas.Add(new PaginaCodice
            {
                numeroCapitulo = "CAPÍTULO II",
                titulo = "Los Cantos de Nezahualcóyotl y el Don de Ehécatl",
                subtituloLugar = "Calzada de los Poetas • Bosque Profundo",
                rutaSpriteIlustracion = "Assets/Sprites/Animaciones/Nahui/nahui_doble_salto_0.png",
                nivelRequerido = 2,
                textoHistoria =
                    "«No para siempre en la tierra, solo un poco aquí...» —rezan las estelas talladas en honor al Rey Poeta Nezahualcóyotl.\n\n" +
                    "Bajo la sombra densa de los ahuehuetes centenarios, Nahui descubre la luz de Ehécatl, Señor del Viento. " +
                    "Con el Doble Salto logra elevarse por las copas silenciosas, esquivando o venciendo desde lo alto a los centinelas de niebla."
            });

            paginas.Add(new PaginaCodice
            {
                numeroCapitulo = "CAPÍTULO III",
                titulo = "Las Aguas de Chalchiuhtlicue",
                subtituloLugar = "Lago Mayor • Reflejo de Tenochtitlan",
                rutaSpriteIlustracion = "Assets/Sprites/Generados/fondo_nivel3_media.png",
                nivelRequerido = 3,
                textoHistoria =
                    "Cuando la noche azul cae sobre el Lago Mayor, las luciérnagas despiertan como chispas vivas de Chalchiuhtlicue, " +
                    "guardiana de los ríos y lagos mexicas.\n\n" +
                    "Sobre plataformas flotantes que cruzan las aguas oscuras, Nahui reúne las veintidós semillas perdidas. " +
                    "Al completar la última luz, el lago entero estalla en destellos dorados y la música del bosque renace."
            });

            paginas.Add(new PaginaCodice
            {
                numeroCapitulo = "CAPÍTULO IV",
                titulo = "El Guardián del Cerro del Chapulín",
                subtituloLugar = "Castillo de Chapultepec • Cima de los Recuerdos",
                rutaSpriteIlustracion = "Assets/Sprites/Generados/fondo_nivel4_castillo.png",
                nivelRequerido = 4,
                textoHistoria =
                    "En la cumbre de Chapultépec —el ancestral Cerro del Saltamontes donde se alza el Castillo— el atardecer tiñe el cielo de oro y púrpura.\n\n" +
                    "Allí aguarda el Gran Nahual de la Niebla transformado en lobo sombrío. Tras cinco saltos certeros sobre su cabeza, " +
                    "la niebla se disipa, el Ahuehuete florece y la paz regresa a la Ciudad de México."
            });

            paginas.Add(new PaginaCodice
            {
                numeroCapitulo = "BESTIARIO Y RELIQUIAS",
                titulo = "Símbolos del Bosque de Chapultepec",
                subtituloLugar = "Guía de Campo de la Guardiana",
                rutaSpriteIlustracion = "Assets/Sprites/Animaciones/Sombra/sombra_ataque_1.png",
                nivelRequerido = 1,
                textoHistoria =
                    "• Semilla de Luz: Fragmento solar que restaura el color y la vida de cada zona al reunir el 100%.\n" +
                    "• Linterna de Piedra: Punto de control ancestral que resguarda el paso de Nahui.\n" +
                    "• Sombra Espinosa (2 golpes): Espíritu de niebla que patrulla un sendero; quita ½ corazón al roce.\n" +
                    "• Gran Nahual (5 golpes): Jefe del Castillo; ruge en furia cuando le queda poca fuerza."
            });
        }
    }
}
