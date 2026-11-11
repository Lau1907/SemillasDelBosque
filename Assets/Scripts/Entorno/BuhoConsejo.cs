using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SemillasDelBosque.Entorno
{
    /// <summary>
    /// BuhoConsejo: Muestra un globo de diálogo en World Space cuando Luna se acerca al Búho Sabio.
    /// Al alejarse, el globo se oculta automáticamente.
    /// Soporta texto configurable y un AudioClip opcional.
    /// </summary>
    public class BuhoConsejo : MonoBehaviour
    {
        [Header("Mensaje y Audio")]
        [TextArea(2, 5)]
        [Tooltip("Texto que el búho le dirá a Luna al acercarse.")]
        [SerializeField] private string mensajeConsejo = "Presiona espacio para saltar. ¡Recoge las semillas de luz!";

        [Tooltip("Sonido opcional (por ejemplo ulular o campanilla) que suena al aparecer el diálogo.")]
        [SerializeField] private AudioClip clipSonido;

        [Header("Referencias del Diálogo")]
        [Tooltip("Objeto contenedor del globo de diálogo (Canvas World Space).")]
        [SerializeField] private GameObject objetoGlobo;

        [Tooltip("Componente TextMeshProUGUI que muestra el texto dentro del globo.")]
        [SerializeField] private TextMeshProUGUI textoDialogo;

        public string MensajeConsejo { get => mensajeConsejo; set => mensajeConsejo = value; }
        public AudioClip ClipSonido { get => clipSonido; set => clipSonido = value; }
        public GameObject ObjetoGlobo { get => objetoGlobo; set => objetoGlobo = value; }
        public TextMeshProUGUI TextoDialogo { get => textoDialogo; set => textoDialogo = value; }

        private void Start()
        {
            if (textoDialogo != null && !string.IsNullOrEmpty(mensajeConsejo))
            {
                textoDialogo.text = mensajeConsejo;
            }

            // Ocultar globo al inicio
            if (objetoGlobo != null)
            {
                objetoGlobo.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (EsLuna(other))
            {
                MostrarGlobo();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (EsLuna(other))
            {
                OcultarGlobo();
            }
        }

        private bool EsLuna(Collider2D col)
        {
            return col.CompareTag("Luna") || col.CompareTag("Jugador") || col.gameObject.name == "Luna";
        }

        public void MostrarGlobo()
        {
            if (objetoGlobo != null)
            {
                if (textoDialogo != null)
                {
                    textoDialogo.text = mensajeConsejo;
                }
                objetoGlobo.SetActive(true);
            }

            if (clipSonido != null)
            {
                AudioSource.PlayClipAtPoint(clipSonido, transform.position);
            }
        }

        public void OcultarGlobo()
        {
            if (objetoGlobo != null)
            {
                objetoGlobo.SetActive(false);
            }
        }

#if UNITY_EDITOR
        [MenuItem("Semillas del Bosque/Entorno/Configurar Buho Sabio")]
        public static void MenuConfigurarBuho()
        {
            GameObject buho = GameObject.Find("BuhoSabio");
            if (buho == null)
            {
                // Buscar por si tiene otro nombre en minúsculas
                var todos = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
                foreach (var g in todos)
                {
                    if (g.name.ToLower().Contains("buho"))
                    {
                        buho = g;
                        break;
                    }
                }
            }

            if (buho == null)
            {
                Debug.LogWarning("[BuhoConsejo] No se encontró el objeto 'BuhoSabio' en la escena actual.");
                return;
            }

            Debug.Log("[BuhoConsejo] Configurando trigger y globo de diálogo en 'BuhoSabio'...");
            Undo.RegisterCompleteObjectUndo(buho, "Configurar Buho Sabio");

            // 1. Asegurar que tenga BoxCollider2D trigger
            BoxCollider2D collider = buho.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = Undo.AddComponent<BoxCollider2D>(buho);
                collider.isTrigger = true;
                collider.size = new Vector2(4.5f, 3.5f);
                collider.offset = new Vector2(0f, 0.5f);
                Debug.Log("[BuhoConsejo] BoxCollider2D trigger añadido al BuhoSabio.");
            }
            else
            {
                collider.isTrigger = true;
            }

            // 2. Componente BuhoConsejo
            BuhoConsejo consejo = buho.GetComponent<BuhoConsejo>();
            if (consejo == null)
            {
                consejo = Undo.AddComponent<BuhoConsejo>(buho);
            }

            consejo.mensajeConsejo = "Presiona espacio para saltar. ¡Recoge las semillas de luz!";

            // 3. Crear o actualizar el Globo de Diálogo (World Space Canvas)
            Transform globoExistente = buho.transform.Find("GloboDialogo");
            GameObject goGlobo;
            if (globoExistente == null)
            {
                goGlobo = new GameObject("GloboDialogo", typeof(RectTransform), typeof(Canvas));
                Undo.RegisterCreatedObjectUndo(goGlobo, "Crear Globo de Dialogo");
                goGlobo.transform.SetParent(buho.transform, false);
                goGlobo.transform.localPosition = new Vector3(0f, 2.3f, 0f);

                Canvas canvasWS = goGlobo.GetComponent<Canvas>();
                canvasWS.renderMode = RenderMode.WorldSpace;
                canvasWS.sortingOrder = 10; // Por encima de personajes

                RectTransform rtCanvas = goGlobo.GetComponent<RectTransform>();
                rtCanvas.sizeDelta = new Vector2(4f, 1.8f);
                rtCanvas.localScale = new Vector3(0.015f, 0.015f, 1f);

                // Fondo del globo (Panel oscuro semitransparente)
                GameObject goFondo = new GameObject("FondoGlobo", typeof(RectTransform), typeof(Image));
                Undo.RegisterCreatedObjectUndo(goFondo, "Crear Fondo Globo");
                goFondo.transform.SetParent(goGlobo.transform, false);

                RectTransform rtFondo = goFondo.GetComponent<RectTransform>();
                rtFondo.anchorMin = Vector2.zero;
                rtFondo.anchorMax = Vector2.one;
                rtFondo.sizeDelta = new Vector2(240f, 100f);

                Image imgFondo = goFondo.GetComponent<Image>();
                imgFondo.color = new Color(0.08f, 0.09f, 0.14f, 0.92f);
                imgFondo.raycastTarget = false;

                // Texto TMP
                GameObject goTexto = new GameObject("TextoMensaje", typeof(RectTransform), typeof(TextMeshProUGUI));
                Undo.RegisterCreatedObjectUndo(goTexto, "Crear Texto Globo");
                goTexto.transform.SetParent(goFondo.transform, false);

                RectTransform rtTexto = goTexto.GetComponent<RectTransform>();
                rtTexto.anchorMin = Vector2.zero;
                rtTexto.anchorMax = Vector2.one;
                rtTexto.offsetMin = new Vector2(12f, 8f);
                rtTexto.offsetMax = new Vector2(-12f, -8f);

                TextMeshProUGUI tmp = goTexto.GetComponent<TextMeshProUGUI>();
                tmp.text = consejo.mensajeConsejo;
                tmp.fontSize = 18;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.textWrappingMode = TextWrappingModes.Normal;
                tmp.raycastTarget = false;

                consejo.objetoGlobo = goGlobo;
                consejo.textoDialogo = tmp;
            }
            else
            {
                goGlobo = globoExistente.gameObject;
                consejo.objetoGlobo = goGlobo;
                consejo.textoDialogo = goGlobo.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            // Desactivar globo para que inicie oculto
            goGlobo.SetActive(false);

            EditorUtility.SetDirty(buho);
            Selection.activeGameObject = buho;
            Debug.Log("[BuhoConsejo] ¡BuhoSabio configurado correctamente con trigger y globo de diálogo!");
        }
#endif
    }
}
