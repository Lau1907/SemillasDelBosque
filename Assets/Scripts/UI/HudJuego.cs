using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SemillasDelBosque.Jugador;
using SemillasDelBosque.Sistema;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace SemillasDelBosque.UI
{
    /// <summary>
    /// HudJuego: Administra el HUD en la esquina superior izquierda de la pantalla.
    /// Muestra el contador de semillas recolectadas sobre el total del nivel,
    /// cronómetro en formato 00:00 y fila de 3 corazones que reflejan las vidas actuales.
    /// </summary>
    public class HudJuego : MonoBehaviour
    {
        public static HudJuego Instancia { get; private set; }

        [Header("Referencias de Semillas")]
        [Tooltip("Texto TMP que muestra 'X / Total'.")]
        [SerializeField] private TextMeshProUGUI textoSemillas;

        [Tooltip("Icono gráfico de la semilla.")]
        [SerializeField] private Image imagenSemilla;

        [Header("Referencias de Vidas (Corazones)")]
        [Tooltip("Las 3 imágenes de los corazones en orden de izquierda a derecha.")]
        [SerializeField] private Image[] imagenesCorazones = new Image[3];

        [Tooltip("Sprite de corazón lleno.")]
        [SerializeField] private Sprite spriteCorazonLleno;

        [Tooltip("Sprite de medio corazón.")]
        [SerializeField] private Sprite spriteCorazonMedio;

        [Tooltip("Sprite de corazón vacío.")]
        [SerializeField] private Sprite spriteCorazonVacio;

        [Header("Cronómetro de Nivel")]
        [Tooltip("Texto TMP que muestra el tiempo transcurrido en formato 00:00.")]
        [SerializeField] private TextMeshProUGUI textoCronometro;
        [SerializeField] private Image iconoReloj;

        [Header("Datos del Nivel")]
        [SerializeField] private int totalSemillas = 0;

        [Header("Indicador de Poder (Opcional)")]
        [SerializeField] private GameObject indicadorDobleSalto;

        private int ultimasSemillas = -1;
        private int ultimosMediosCorazones = -1;
        private bool ultimoDobleSalto = false;
        private SistemaVidaJugador vidaJugadorCache;
        private float tiempoTranscurrido = 0f;

        public int TotalSemillas => totalSemillas;
        public float TiempoTranscurrido => tiempoTranscurrido;
        public TextMeshProUGUI TextoSemillas { get => textoSemillas; set => textoSemillas = value; }
        public TextMeshProUGUI TextoCronometro { get => textoCronometro; set => textoCronometro = value; }
        public Image ImagenSemilla { get => imagenSemilla; set => imagenSemilla = value; }
        public Image[] ImagenesCorazones { get => imagenesCorazones; set => imagenesCorazones = value; }
        public Sprite SpriteCorazonLleno { get => spriteCorazonLleno; set => spriteCorazonLleno = value; }
        public Sprite SpriteCorazonMedio { get => spriteCorazonMedio; set => spriteCorazonMedio = value; }
        public Sprite SpriteCorazonVacio { get => spriteCorazonVacio; set => spriteCorazonVacio = value; }
        public GameObject IndicadorDobleSalto { get => indicadorDobleSalto; set => indicadorDobleSalto = value; }

        private void Awake()
        {
            Instancia = this;
            CalcularTotalSemillas();
        }

        private void OnDestroy()
        {
            if (Instancia == this) Instancia = null;
        }

        private void Start()
        {
            if (totalSemillas <= 0) CalcularTotalSemillas();
            ActualizarHUD(forzar: true);
        }

        private void Update()
        {
            if (Time.timeScale > 0f)
            {
                tiempoTranscurrido += Time.deltaTime;
                ActualizarCronometro();
            }
            ActualizarHUD(forzar: false);
        }

        public void CalcularTotalSemillas()
        {
            int conteo = UtilidadesJugador.ContarSemillasEnEscena(gameObject.scene);
            if (conteo > totalSemillas)
            {
                totalSemillas = conteo;
            }
        }

        private void ActualizarCronometro()
        {
            if (textoCronometro != null)
            {
                int minutos = Mathf.FloorToInt(tiempoTranscurrido / 60f);
                int segundos = Mathf.FloorToInt(tiempoTranscurrido % 60f);
                textoCronometro.text = string.Format("{0:00}:{1:00}", minutos, segundos);
            }
        }

        public void ActualizarHUD(bool forzar = false)
        {
            int semillasActuales = UtilidadesJugador.LeerVariableEscena("semillas", 0);
            int vidasActuales = UtilidadesJugador.LeerVariableEscena("vidas", 3);
            bool tieneDobleSalto = UtilidadesJugador.LeerVariableEscena("tieneDobleSalto", false);

            if (vidaJugadorCache == null && Application.isPlaying)
            {
                vidaJugadorCache = FindAnyObjectByType<SistemaVidaJugador>();
            }

            int mediosActuales = (vidaJugadorCache != null)
                ? vidaJugadorCache.MediosCorazones
                : Mathf.Clamp(vidasActuales, 0, 3) * 2;

            if (!forzar &&
                semillasActuales == ultimasSemillas &&
                mediosActuales == ultimosMediosCorazones &&
                tieneDobleSalto == ultimoDobleSalto)
            {
                return;
            }

            ultimasSemillas = semillasActuales;
            ultimosMediosCorazones = mediosActuales;
            ultimoDobleSalto = tieneDobleSalto;

            if (textoSemillas != null)
            {
                textoSemillas.text = $"{semillasActuales} / {totalSemillas}";
            }

            if (imagenesCorazones != null)
            {
                for (int i = 0; i < imagenesCorazones.Length; i++)
                {
                    if (imagenesCorazones[i] == null) continue;

                    int umbralLleno = (i + 1) * 2;
                    int umbralMedio = i * 2 + 1;

                    if (mediosActuales >= umbralLleno)
                    {
                        imagenesCorazones[i].sprite = spriteCorazonLleno;
                    }
                    else if (mediosActuales == umbralMedio && spriteCorazonMedio != null)
                    {
                        imagenesCorazones[i].sprite = spriteCorazonMedio;
                    }
                    else
                    {
                        imagenesCorazones[i].sprite = spriteCorazonVacio;
                    }
                }
            }

            if (indicadorDobleSalto != null)
            {
                indicadorDobleSalto.SetActive(tieneDobleSalto);
            }
        }

#if UNITY_EDITOR
        [MenuItem("Semillas del Bosque/UI/Agregar HUD")]
        public static void MenuAgregarHUD()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Transform contenedorExistente = canvas.transform.Find("ContenedorHUD");
            if (contenedorExistente != null)
            {
                HudJuego hudExistente = contenedorExistente.GetComponent<HudJuego>();
                if (hudExistente == null) hudExistente = contenedorExistente.gameObject.AddComponent<HudJuego>();
                hudExistente.CalcularTotalSemillas();
                hudExistente.ActualizarHUD(forzar: true);
                EditorUtility.SetDirty(contenedorExistente.gameObject);
                return;
            }
        }
#endif
    }
}