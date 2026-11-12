using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SemillasDelBosque.Jugador;
using SemillasDelBosque.Sistema;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SemillasDelBosque.UI
{
    /// <summary>
    /// HudJuego: Administra el HUD en la esquina superior izquierda de la pantalla.
    /// Muestra el contador de semillas recolectadas sobre el total del nivel
    /// y una fila de 3 corazones que reflejan las vidas actuales de Luna.
    /// Lee las variables de escena de Visual Scripting ("semillas" y "vidas").
    /// </summary>
    public class HudJuego : MonoBehaviour
    {
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

        [Header("Datos del Nivel")]
        [SerializeField] private int totalSemillas = 0;

        [Header("Indicador de Poder (Opcional)")]
        [SerializeField] private GameObject indicadorDobleSalto;

        // Cache para comprobar cambios y evitar recalcular cada frame
        private int ultimasSemillas = -1;
        private int ultimosMediosCorazones = -1;
        private bool ultimoDobleSalto = false;
        private SistemaVidaJugador vidaJugadorCache;

        public int TotalSemillas => totalSemillas;
        public TextMeshProUGUI TextoSemillas { get => textoSemillas; set => textoSemillas = value; }
        public Image ImagenSemilla { get => imagenSemilla; set => imagenSemilla = value; }
        public Image[] ImagenesCorazones { get => imagenesCorazones; set => imagenesCorazones = value; }
        public Sprite SpriteCorazonLleno { get => spriteCorazonLleno; set => spriteCorazonLleno = value; }
        public Sprite SpriteCorazonMedio { get => spriteCorazonMedio; set => spriteCorazonMedio = value; }
        public Sprite SpriteCorazonVacio { get => spriteCorazonVacio; set => spriteCorazonVacio = value; }
        public GameObject IndicadorDobleSalto { get => indicadorDobleSalto; set => indicadorDobleSalto = value; }

        private void Start()
        {
            // Calcular el total de semillas presentes al inicio de la escena
            CalcularTotalSemillas();
            ActualizarHUD(forzar: true);
        }

        private void Update()
        {
            ActualizarHUD(forzar: false);
        }

        /// <summary>
        /// Cuenta todas las semillas de la escena activa por nombre que comience con 'SemillaDeLuz' o tag 'Semilla'.
        /// </summary>
        public void CalcularTotalSemillas()
        {
            totalSemillas = UtilidadesJugador.ContarSemillasEnEscena(gameObject.scene);
        }

        /// <summary>
        /// Lee las variables de escena y refresca la UI si han cambiado los valores.
        /// </summary>
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

            // Comprobación barata: solo actualizar componentes gráficos si hubo un cambio
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

            // 1. Actualizar texto de semillas
            if (textoSemillas != null)
            {
                textoSemillas.text = $"{semillasActuales} / {totalSemillas}";
            }

            // 2. Actualizar fila de corazones (soportando medios corazones)
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

            // 3. Actualizar indicador de Doble Salto si está asignado
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
            if (canvas == null)
            {
                Debug.LogError("[HudJuego] No se encontró ningún Canvas en la escena. Asegúrate de tener uno antes de agregar el HUD.");
                return;
            }

            // Verificar si ya existe ContenedorHUD
            Transform contenedorExistente = canvas.transform.Find("ContenedorHUD");
            if (contenedorExistente != null)
            {
                Debug.LogWarning("[HudJuego] El objeto 'ContenedorHUD' ya existe en el Canvas. Reconfigurando referencias sin duplicar...");
                ConfigurarComponentesHUD(contenedorExistente.gameObject);
                return;
            }

            Debug.Log("[HudJuego] Creando ContenedorHUD en la esquina superior izquierda del Canvas...");

            // Cargar sprites (priorizando los sprites oficiales extraídos en Objetos/)
            Sprite sSemilla = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objetos/semilla_luz.png")
                              ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/icono_semilla.png");
            Sprite sCorazonLleno = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objetos/corazon_lleno.png")
                                   ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/corazon_lleno.png");
            Sprite sCorazonMedio = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objetos/corazon_medio.png");
            Sprite sCorazonVacio = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objetos/corazon_vacio.png")
                                   ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/corazon_vacio.png");

            // 1. Contenedor Principal (Arriba Izquierda)
            GameObject goHUD = new GameObject("ContenedorHUD", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(goHUD, "Crear Contenedor HUD");
            goHUD.transform.SetParent(canvas.transform, false);

            RectTransform rtHUD = goHUD.GetComponent<RectTransform>();
            rtHUD.anchorMin = new Vector2(0f, 1f);
            rtHUD.anchorMax = new Vector2(0f, 1f);
            rtHUD.pivot = new Vector2(0f, 1f);
            rtHUD.anchoredPosition = new Vector2(25f, -25f);
            rtHUD.sizeDelta = new Vector2(240f, 90f);

            // 2. Fila de Corazones (Arriba)
            GameObject goFilaCorazones = new GameObject("FilaCorazones", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            Undo.RegisterCreatedObjectUndo(goFilaCorazones, "Crear Fila Corazones");
            goFilaCorazones.transform.SetParent(goHUD.transform, false);

            RectTransform rtCorazones = goFilaCorazones.GetComponent<RectTransform>();
            rtCorazones.anchorMin = new Vector2(0f, 1f);
            rtCorazones.anchorMax = new Vector2(0f, 1f);
            rtCorazones.pivot = new Vector2(0f, 1f);
            rtCorazones.anchoredPosition = new Vector2(0f, 0f);
            rtCorazones.sizeDelta = new Vector2(150f, 36f);

            HorizontalLayoutGroup layoutCorazones = goFilaCorazones.GetComponent<HorizontalLayoutGroup>();
            layoutCorazones.spacing = 8f;
            layoutCorazones.childAlignment = TextAnchor.MiddleLeft;
            layoutCorazones.childControlWidth = false;
            layoutCorazones.childControlHeight = false;
            layoutCorazones.childForceExpandWidth = false;
            layoutCorazones.childForceExpandHeight = false;

            Image[] corazones = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject goCorazon = new GameObject($"Corazon_{i + 1}", typeof(RectTransform), typeof(Image));
                Undo.RegisterCreatedObjectUndo(goCorazon, $"Crear Corazón {i + 1}");
                goCorazon.transform.SetParent(goFilaCorazones.transform, false);

                RectTransform rtC = goCorazon.GetComponent<RectTransform>();
                rtC.sizeDelta = new Vector2(32f, 32f);

                Image imgC = goCorazon.GetComponent<Image>();
                // Registrar la modificación ANTES de asignar el sprite para que Unity
                // incluya el cambio en el sistema de Undo y lo serialice correctamente.
                Undo.RecordObject(imgC, $"Asignar sprite corazón {i + 1}");
                imgC.sprite = sCorazonLleno;
                imgC.raycastTarget = false; // Sin bloqueo de clics
                // SetDirty es OBLIGATORIO para que Unity persista la referencia al sprite
                // en la escena. Sin esto el Image muestra un cuadro blanco tras salvar.
                EditorUtility.SetDirty(imgC);
                corazones[i] = imgC;
            }

            // 3. Fila de Semillas (Debajo de los corazones)
            GameObject goFilaSemilla = new GameObject("FilaSemilla", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            Undo.RegisterCreatedObjectUndo(goFilaSemilla, "Crear Fila Semilla");
            goFilaSemilla.transform.SetParent(goHUD.transform, false);

            RectTransform rtFilaSemilla = goFilaSemilla.GetComponent<RectTransform>();
            rtFilaSemilla.anchorMin = new Vector2(0f, 1f);
            rtFilaSemilla.anchorMax = new Vector2(0f, 1f);
            rtFilaSemilla.pivot = new Vector2(0f, 1f);
            rtFilaSemilla.anchoredPosition = new Vector2(0f, -44f);
            rtFilaSemilla.sizeDelta = new Vector2(180f, 36f);

            HorizontalLayoutGroup layoutSemilla = goFilaSemilla.GetComponent<HorizontalLayoutGroup>();
            layoutSemilla.spacing = 10f;
            layoutSemilla.childAlignment = TextAnchor.MiddleLeft;
            layoutSemilla.childControlWidth = false;
            layoutSemilla.childControlHeight = false;
            layoutSemilla.childForceExpandWidth = false;
            layoutSemilla.childForceExpandHeight = false;

            // Icono de la semilla
            GameObject goIconoSemilla = new GameObject("IconoSemilla", typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(goIconoSemilla, "Crear Icono Semilla");
            goIconoSemilla.transform.SetParent(goFilaSemilla.transform, false);

            RectTransform rtIcono = goIconoSemilla.GetComponent<RectTransform>();
            rtIcono.sizeDelta = new Vector2(30f, 30f);

            Image imgSemilla = goIconoSemilla.GetComponent<Image>();
            imgSemilla.sprite = sSemilla;
            imgSemilla.raycastTarget = false;

            // Texto contador de semillas (TextMeshProUGUI)
            GameObject goTextoSemillas = new GameObject("TextoSemillas", typeof(RectTransform), typeof(TextMeshProUGUI));
            Undo.RegisterCreatedObjectUndo(goTextoSemillas, "Crear Texto Semillas");
            goTextoSemillas.transform.SetParent(goFilaSemilla.transform, false);

            RectTransform rtTexto = goTextoSemillas.GetComponent<RectTransform>();
            rtTexto.sizeDelta = new Vector2(140f, 32f);

            TextMeshProUGUI tmp = goTextoSemillas.GetComponent<TextMeshProUGUI>();
            tmp.text = "0 / 0";
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.92f, 0.45f, 1f); // Dorado luminoso
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;

            // 4. Agregar y configurar el componente HudJuego
            HudJuego hud = goHUD.AddComponent<HudJuego>();
            hud.textoSemillas = tmp;
            hud.imagenSemilla = imgSemilla;
            hud.imagenesCorazones = corazones;
            hud.spriteCorazonLleno = sCorazonLleno;
            hud.spriteCorazonMedio = sCorazonMedio;
            hud.spriteCorazonVacio = sCorazonVacio;
            hud.CalcularTotalSemillas();
            hud.ActualizarHUD(forzar: true);

            Selection.activeGameObject = goHUD;
            EditorUtility.SetDirty(goHUD);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("[HudJuego] ¡HUD creado e integrado con éxito en el Canvas! Guarda la escena con Ctrl+S.");
        }

        private static void ConfigurarComponentesHUD(GameObject goHUD)
        {
            HudJuego hud = goHUD.GetComponent<HudJuego>();
            if (hud == null)
            {
                hud = Undo.AddComponent<HudJuego>(goHUD);
            }

            Sprite sSemilla = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objetos/semilla_luz.png")
                              ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/icono_semilla.png");
            Sprite sCorazonLleno = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objetos/corazon_lleno.png")
                                   ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/corazon_lleno.png");
            Sprite sCorazonMedio = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objetos/corazon_medio.png");
            Sprite sCorazonVacio = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objetos/corazon_vacio.png")
                                   ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/corazon_vacio.png");

            hud.spriteCorazonLleno = sCorazonLleno;
            hud.spriteCorazonMedio = sCorazonMedio;
            hud.spriteCorazonVacio = sCorazonVacio;

            var filaSemilla = goHUD.transform.Find("FilaSemilla");
            if (filaSemilla != null)
            {
                var icono = filaSemilla.Find("IconoSemilla");
                if (icono != null) hud.imagenSemilla = icono.GetComponent<Image>();

                var txt = filaSemilla.Find("TextoSemillas");
                if (txt != null) hud.textoSemillas = txt.GetComponent<TextMeshProUGUI>();
            }

            var filaCorazones = goHUD.transform.Find("FilaCorazones");
            if (filaCorazones != null)
            {
                var imgs = filaCorazones.GetComponentsInChildren<Image>();
                hud.imagenesCorazones = imgs;
                // Reasignar el sprite DIRECTAMENTE en cada Image para que no quede en blanco.
                // El componente HudJuego sólo guarda el sprite para usar en runtime;
                // la referencia de cada Image en la escena debe también estar serializada.
                foreach (var imgC in imgs)
                {
                    if (imgC == null) continue;
                    Undo.RecordObject(imgC, "Reasignar sprite corazón");
                    imgC.sprite = sCorazonLleno;
                    EditorUtility.SetDirty(imgC);
                }
            }

            hud.CalcularTotalSemillas();
            hud.ActualizarHUD(forzar: true);
            EditorUtility.SetDirty(goHUD);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("[HudJuego] HUD reconfigurado. Guarda la escena con Ctrl+S.");
        }
#endif
    }
}
