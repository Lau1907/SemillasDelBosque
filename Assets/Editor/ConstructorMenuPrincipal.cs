using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using SemillasDelBosque.Audio;
using SemillasDelBosque.Editor;
using SemillasDelBosque.Sistema;
using SemillasDelBosque.UI;

namespace SemillasDelBosque.EditorTools
{
    /// <summary>
    /// ConstructorMenuPrincipal: Construye de forma idempotente y con Undo una escena MenuPrincipal
    /// profesional con diorama animado de Chapultepec, partículas, música ambiental,
    /// tarjetas estilo códice mexicano, efectos hover y el Libro de Historia interactivo.
    /// </summary>
    public static class ConstructorMenuPrincipal
    {
        [MenuItem("Semillas del Bosque/0. Crear Escena Menu Principal", false, 0)]
        public static void CrearYConfigurarMenuPrincipal()
        {
            string rutaMenu = UtilidadesEditorNiveles.RUTA_ESCENA_MENU;
            Scene escena;

            if (File.Exists(rutaMenu))
            {
                escena = EditorSceneManager.OpenScene(rutaMenu, OpenSceneMode.Single);
            }
            else
            {
                escena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.SaveScene(escena, rutaMenu);
            }

            // Garantizar sprites generados
            if (AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/libro_codice.png") == null ||
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/fondo_nivel4_castillo.png") == null)
            {
                GeneradorArte.GenerarTodosLosSprites();
            }

            ConfigurarCamara();
            AsegurarEventSystem();
            AsegurarGestorJuego();
            AsegurarMusicaMenu();
            ConstruirEscenarioDiorama();
            ConstruirCanvasMenu(escena);

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena, rutaMenu);
            UtilidadesEditorNiveles.ConfigurarBuildSettingsOrdenado();
            Debug.Log("[ConstructorMenuPrincipal] Menú Principal profesional y Códice de Historia configurados.");
        }

        private static void ConfigurarCamara()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            Undo.RecordObject(cam, "Configurar Cámara Menú");
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(0x1B, 0x13, 0x24, 0xFF);
            EditorUtility.SetDirty(cam);
        }

        private static void AsegurarEventSystem()
        {
            EventSystem es = Object.FindAnyObjectByType<EventSystem>();
            if (es == null)
            {
                GameObject goES = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(goES, "Crear EventSystem");
            }
            else if (es.GetComponent<InputSystemUIInputModule>() == null)
            {
                Undo.AddComponent<InputSystemUIInputModule>(es.gameObject);
            }
        }

        private static void AsegurarGestorJuego()
        {
            GestorJuego gestor = Object.FindAnyObjectByType<GestorJuego>();
            if (gestor == null)
            {
                GameObject goGestor = new GameObject("GestorJuego", typeof(GestorJuego));
                Undo.RegisterCreatedObjectUndo(goGestor, "Crear GestorJuego");
            }
        }

        private static void AsegurarMusicaMenu()
        {
            GameObject existente = GameObject.Find("MusicaMenu");
            if (existente != null) Undo.DestroyObjectImmediate(existente);

            GameObject go = new GameObject("MusicaMenu", typeof(AudioSource), typeof(GestorMusica));
            Undo.RegisterCreatedObjectUndo(go, "Crear MusicaMenu");

            AudioClip clipExp = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/musica_exploracion.wav");
            AudioSource src = go.GetComponent<AudioSource>();
            src.clip = clipExp;
            src.loop = true;
            src.playOnAwake = true;
            src.volume = 0.38f;

            GestorMusica gm = go.GetComponent<GestorMusica>();
            gm.MusicaExploracion = clipExp;
        }

        private static void ConstruirEscenarioDiorama()
        {
            GameObject viejo = GameObject.Find("--- ESCENARIO MENU ---");
            if (viejo != null) Undo.DestroyObjectImmediate(viejo);

            GameObject raiz = new GameObject("--- ESCENARIO MENU ---");
            Undo.RegisterCreatedObjectUndo(raiz, "Crear Escenario Menú");

            // Cielo atardecer + Castillo de Chapultepec + Bosque al fondo
            CrearFondoMenu(raiz.transform, "CieloAtardecer", "Assets/Sprites/Generados/fondo_nivel4_lejana.png", new Vector3(0f, -5.6f, 0f), new Vector3(1.5f, 0.50f, 1f), -30, Color.white);
            CrearFondoMenu(raiz.transform, "CastilloChapultepec", "Assets/Sprites/Generados/fondo_nivel4_castillo.png", new Vector3(3.4f, -4.4f, 0f), new Vector3(0.85f, 0.52f, 1f), -20, new Color(1f, 0.92f, 0.95f, 0.96f));
            CrearFondoMenu(raiz.transform, "ArbolesBosque", "Assets/Sprites/Generados/fondo_arboles_cercanos.png", new Vector3(0f, -5.8f, 0f), new Vector3(1.5f, 0.46f, 1f), -10, new Color(0.70f, 0.80f, 0.72f, 0.88f));

            // Terraza derecha donde reposan Nahui, Tecólotl, Linterna y Semilla
            GameObject prefabSuelo = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BloqueSuelo.prefab");
            if (prefabSuelo != null)
            {
                GameObject terraza = (GameObject)PrefabUtility.InstantiatePrefab(prefabSuelo, raiz.transform);
                Undo.RegisterCreatedObjectUndo(terraza, "Crear Terraza Menú");
                terraza.name = "TerrazaChapultepec";
                terraza.transform.position = new Vector3(4.2f, -3.4f, 0f);
                terraza.transform.localScale = new Vector3(7.5f, 1.2f, 1f);
            }

            // Personajes y reliquias decorativas (solo visuales, sin física ni grafos)
            CrearActorVisual(raiz.transform, "NahuiDiorama", "Assets/Sprites/Animaciones/Nahui/nahui_idle_0.png",
                new Vector3(3.2f, -2.12f, 0f), new Vector3(1.35f, 1.35f, 1f), 5,
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ConfiguradorAnimaciones.RUTA_CTRL_NAHUI));

            CrearActorVisual(raiz.transform, "BuhoDiorama", "Assets/Sprites/Personajes/BuhoSabio_Rama.png",
                new Vector3(5.6f, -1.75f, 0f), new Vector3(1.25f, 1.25f, 1f), 4, null);

            CrearActorVisual(raiz.transform, "LinternaDiorama", "Assets/Sprites/Objetos/linterna_checkpoint.png",
                new Vector3(1.6f, -2.05f, 0f), Vector3.one, 4, null);

            CrearActorVisual(raiz.transform, "SemillaDiorama", "Assets/Sprites/Objetos/semilla_luz.png",
                new Vector3(4.35f, -1.55f, 0f), new Vector3(0.75f, 0.75f, 1f), 6, null);

            // Partículas de luciérnagas doradas flotando en el menú
            GameObject goPs = new GameObject("LuciernagasMenu");
            Undo.RegisterCreatedObjectUndo(goPs, "Crear LuciernagasMenu");
            goPs.transform.SetParent(raiz.transform, false);
            goPs.transform.position = new Vector3(1.5f, 0f, 0f);

            ParticleSystem ps = goPs.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = 4.5f;
            main.startSpeed = 0.25f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.20f);
            main.startColor = new Color(1f, 0.88f, 0.45f, 0.85f);
            main.maxParticles = 60;

            var emission = ps.emission;
            emission.rateOverTime = 12f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(18f, 10f, 1f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
            vel.y = new ParticleSystem.MinMaxCurve(0.05f, 0.35f);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            goPs.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;
        }

        private static void CrearFondoMenu(Transform padre, string nombre, string rutaSprite, Vector3 pos, Vector3 escala, int orden, Color color)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(rutaSprite);
            if (s == null) return;

            GameObject go = new GameObject(nombre, typeof(SpriteRenderer));
            Undo.RegisterCreatedObjectUndo(go, $"Crear {nombre}");
            go.transform.SetParent(padre, false);
            go.transform.position = pos;
            go.transform.localScale = escala;

            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = s;
            sr.drawMode = SpriteDrawMode.Simple;
            sr.sortingOrder = orden;
            sr.color = color;
        }

        private static void CrearActorVisual(Transform padre, string nombre, string rutaSprite, Vector3 pos, Vector3 escala, int orden, RuntimeAnimatorController ctrl)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(rutaSprite);
            if (s == null) return;

            GameObject go = new GameObject(nombre, typeof(SpriteRenderer));
            Undo.RegisterCreatedObjectUndo(go, $"Crear {nombre}");
            go.transform.SetParent(padre, false);
            go.transform.position = pos;
            go.transform.localScale = escala;

            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = s;
            sr.sortingOrder = orden;

            if (ctrl != null)
            {
                Animator anim = go.AddComponent<Animator>();
                anim.runtimeAnimatorController = ctrl;
            }
        }

        private static void ConstruirCanvasMenu(Scene escena)
        {
            GameObject canvasExistente = GameObject.Find("CanvasMenuPrincipal");
            if (canvasExistente != null)
            {
                Undo.DestroyObjectImmediate(canvasExistente);
            }

            GameObject goCanvas = new GameObject(
                "CanvasMenuPrincipal",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(goCanvas, "Crear Canvas Menú Principal");

            Canvas canvas = goCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = goCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            MenuPrincipalController ctrl = Undo.AddComponent<MenuPrincipalController>(goCanvas);

            // Degradado sutil izquierdo para legibilidad sobre el diorama
            GameObject veloIzq = new GameObject("VeloLegibilidad", typeof(RectTransform), typeof(Image));
            veloIzq.transform.SetParent(goCanvas.transform, false);
            RectTransform rtVelo = veloIzq.GetComponent<RectTransform>();
            rtVelo.anchorMin = new Vector2(0f, 0f);
            rtVelo.anchorMax = new Vector2(0.55f, 1f);
            rtVelo.offsetMin = Vector2.zero;
            rtVelo.offsetMax = Vector2.zero;
            Image imgVelo = veloIzq.GetComponent<Image>();
            imgVelo.color = new Color(0.04f, 0.08f, 0.06f, 0.58f);
            imgVelo.raycastTarget = false;

            // 1. Panel Menú Principal (Alineación editorial profesional a la izquierda)
            GameObject panelMenu = CrearPanelTransparente(goCanvas.transform, "PanelMenuPrincipal");
            ctrl.PanelMenuPrincipal = panelMenu;

            // Encabezado con Logo y Título
            Sprite logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/logo_equipo.png");
            GameObject goCrest = new GameObject("EmblemaTitulo", typeof(RectTransform), typeof(Image));
            goCrest.transform.SetParent(panelMenu.transform, false);
            RectTransform rtCrest = goCrest.GetComponent<RectTransform>();
            rtCrest.anchorMin = new Vector2(0f, 1f);
            rtCrest.anchorMax = new Vector2(0f, 1f);
            rtCrest.pivot = new Vector2(0f, 1f);
            rtCrest.anchoredPosition = new Vector2(90f, -68f);
            rtCrest.sizeDelta = new Vector2(108f, 108f);
            Image imgCrest = goCrest.GetComponent<Image>();
            imgCrest.sprite = logoSprite;
            imgCrest.preserveAspect = true;

            TextMeshProUGUI tit = CrearTexto(panelMenu.transform, "TituloJuego", "SEMILLAS DEL BOSQUE", 56, FontStyles.Bold,
                new Color(1f, 0.90f, 0.48f, 1f), new Vector2(-210f, 388f), new Vector2(820f, 75f));
            tit.alignment = TextAlignmentOptions.MidlineLeft;
            tit.rectTransform.anchoredPosition = new Vector2(220f, -88f);
            tit.rectTransform.anchorMin = new Vector2(0f, 1f);
            tit.rectTransform.anchorMax = new Vector2(0f, 1f);
            tit.rectTransform.pivot = new Vector2(0f, 0.5f);

            TextMeshProUGUI sub = CrearTexto(panelMenu.transform, "SubtituloJuego", "◈  Leyenda del Ahuehuete • Chapultepec, México  ◈", 24, FontStyles.Italic,
                new Color(0.78f, 0.94f, 0.82f, 1f), Vector2.zero, new Vector2(820f, 40f));
            sub.alignment = TextAlignmentOptions.MidlineLeft;
            sub.rectTransform.anchorMin = new Vector2(0f, 1f);
            sub.rectTransform.anchorMax = new Vector2(0f, 1f);
            sub.rectTransform.pivot = new Vector2(0f, 0.5f);
            sub.rectTransform.anchoredPosition = new Vector2(220f, -142f);

            // Tarjeta oscura con borde dorado para los botones
            GameObject tarjetaMenu = CrearTarjetaConBorde(panelMenu.transform, "TarjetaBotones", new Vector2(90f, -220f), new Vector2(490f, 520f), true);

            GameObject contenedorBotones = new GameObject("ContenedorBotones", typeof(RectTransform), typeof(VerticalLayoutGroup));
            contenedorBotones.transform.SetParent(tarjetaMenu.transform, false);
            RectTransform rtBotones = contenedorBotones.GetComponent<RectTransform>();
            rtBotones.anchorMin = Vector2.zero;
            rtBotones.anchorMax = Vector2.one;
            rtBotones.offsetMin = new Vector2(26f, 26f);
            rtBotones.offsetMax = new Vector2(-26f, -26f);

            VerticalLayoutGroup vlg = contenedorBotones.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 14f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            Sprite sLibro = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/libro_codice.png");

            ctrl.BotonJugar = CrearBotonEstilizado(contenedorBotones.transform, "BotonJugar", "Jugar • Nueva Aventura", new Color32(0x1E, 0x5C, 0x3A, 0xFF), null, out _);
            ctrl.BotonContinuar = CrearBotonEstilizado(contenedorBotones.transform, "BotonContinuar", "Continuar", new Color32(0x1F, 0x4B, 0x5C, 0xFF), null, out TextMeshProUGUI txtCont);
            ctrl.TextoBotonContinuar = txtCont;
            ctrl.BotonLibro = CrearBotonEstilizado(contenedorBotones.transform, "BotonLibro", "Códice de Historia (Libro)", new Color32(0x5A, 0x39, 0x1E, 0xFF), sLibro, out _);
            ctrl.BotonOpciones = CrearBotonEstilizado(contenedorBotones.transform, "BotonOpciones", "Opciones", new Color32(0x2D, 0x36, 0x3E, 0xFF), null, out _);
            ctrl.BotonCreditos = CrearBotonEstilizado(contenedorBotones.transform, "BotonCreditos", "Créditos", new Color32(0x3D, 0x2F, 0x4E, 0xFF), null, out _);
            ctrl.BotonSalir = CrearBotonEstilizado(contenedorBotones.transform, "BotonSalir", "Salir", new Color32(0x5E, 0x22, 0x26, 0xFF), null, out _);

            // Pie de pantalla con ayuda rápida
            TextMeshProUGUI pie = CrearTexto(panelMenu.transform, "TextoPie", "Semillas del Bosque • Edición Chapultepec (CDMX)", 18, FontStyles.Normal,
                new Color(0.85f, 0.88f, 0.82f, 0.78f), Vector2.zero, new Vector2(650f, 32f));
            pie.alignment = TextAlignmentOptions.MidlineLeft;
            pie.rectTransform.anchorMin = new Vector2(0f, 0f);
            pie.rectTransform.anchorMax = new Vector2(0f, 0f);
            pie.rectTransform.pivot = new Vector2(0f, 0f);
            pie.rectTransform.anchoredPosition = new Vector2(90f, 40f);

            // 2. Panel Opciones (Tarjeta centrada con borde dorado y porcentajes en vivo)
            GameObject panelOpciones = CrearPanelFondoOscuro(goCanvas.transform, "PanelOpciones");
            ctrl.PanelOpciones = panelOpciones;
            GameObject cardOpc = CrearTarjetaConBorde(panelOpciones.transform, "TarjetaOpciones", Vector2.zero, new Vector2(780f, 680f), false);

            CrearTexto(cardOpc.transform, "TituloOpciones", "◈  OPCIONES  ◈", 42, FontStyles.Bold,
                new Color(1f, 0.90f, 0.48f, 1f), new Vector2(0f, 265f), new Vector2(680f, 60f));

            ctrl.SliderMusica = CrearSlider(cardOpc.transform, "SliderMusica", "Volumen Música", new Vector2(0f, 145f), out TextMeshProUGUI pctMus);
            ctrl.TextoVolMusica = pctMus;
            ctrl.SliderSFX = CrearSlider(cardOpc.transform, "SliderSFX", "Volumen Efectos (SFX)", new Vector2(0f, 35f), out TextMeshProUGUI pctSfx);
            ctrl.TextoVolSFX = pctSfx;
            ctrl.DropdownResolucion = CrearDropdown(cardOpc.transform, "DropdownResolucion", "Resolución", new Vector2(0f, -80f));
            ctrl.TogglePantallaCompleta = CrearToggle(cardOpc.transform, "TogglePantallaCompleta", "Pantalla Completa", new Vector2(0f, -170f));
            ctrl.BotonVolverOpciones = CrearBotonAnclado(cardOpc.transform, "BotonVolverOpciones", "Volver al Menú", new Vector2(0f, -265f), new Vector2(300f, 58f), new Color32(0x1E, 0x5C, 0x3A, 0xFF));
            panelOpciones.SetActive(false);

            // 3. Panel Créditos (Tarjeta centrada con borde dorado)
            GameObject panelCreditos = CrearPanelFondoOscuro(goCanvas.transform, "PanelCreditos");
            ctrl.PanelCreditos = panelCreditos;
            GameObject cardCred = CrearTarjetaConBorde(panelCreditos.transform, "TarjetaCreditos", Vector2.zero, new Vector2(920f, 660f), false);

            CrearTexto(cardCred.transform, "TituloCreditos", "◈  CRÉDITOS  ◈", 42, FontStyles.Bold,
                new Color(1f, 0.90f, 0.48f, 1f), new Vector2(0f, 255f), new Vector2(750f, 60f));

            TextMeshProUGUI tmpCred = CrearTexto(cardCred.transform, "TextoCreditos",
                MenuPrincipalController.ObtenerTextoCreditosOficial(), 25, FontStyles.Normal,
                new Color(0.95f, 0.93f, 0.88f, 1f), new Vector2(0f, 5f), new Vector2(820f, 400f));
            ctrl.TextoCreditos = tmpCred;

            ctrl.BotonVolverCreditos = CrearBotonAnclado(cardCred.transform, "BotonVolverCreditos", "Volver al Menú", new Vector2(0f, -255f), new Vector2(300f, 58f), new Color32(0x1E, 0x5C, 0x3A, 0xFF));
            panelCreditos.SetActive(false);

            // 4. Libro de Historia Interactivo (Códice de Chapultepec)
            ctrl.LibroHistoria = ConstructorLibroCodice.ConstruirLibroEnCanvas(goCanvas.transform, permitirTeclaEnJuego: false, pausarAlAbrir: false);

            // 5. Splash Screen (Logo del equipo y título)
            GameObject panelSplash = new GameObject("PanelSplash", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            Undo.RegisterCreatedObjectUndo(panelSplash, "Crear PanelSplash");
            panelSplash.transform.SetParent(goCanvas.transform, false);
            RectTransform rtSplash = panelSplash.GetComponent<RectTransform>();
            rtSplash.anchorMin = Vector2.zero;
            rtSplash.anchorMax = Vector2.one;
            rtSplash.offsetMin = Vector2.zero;
            rtSplash.offsetMax = Vector2.zero;
            panelSplash.GetComponent<Image>().color = new Color32(0x0B, 0x15, 0x11, 0xFF);

            ctrl.PanelSplash = panelSplash;
            ctrl.CanvasGroupSplash = panelSplash.GetComponent<CanvasGroup>();

            GameObject goLogo = new GameObject("LogoEquipo", typeof(RectTransform), typeof(Image));
            goLogo.transform.SetParent(panelSplash.transform, false);
            RectTransform rtLogo = goLogo.GetComponent<RectTransform>();
            rtLogo.anchorMin = new Vector2(0.5f, 0.5f);
            rtLogo.anchorMax = new Vector2(0.5f, 0.5f);
            rtLogo.anchoredPosition = new Vector2(0f, 80f);
            rtLogo.sizeDelta = new Vector2(230f, 230f);
            Image imgLogo = goLogo.GetComponent<Image>();
            imgLogo.sprite = logoSprite;
            imgLogo.preserveAspect = true;

            CrearTexto(panelSplash.transform, "TextoSplashTitulo", "SEMILLAS DEL BOSQUE", 50, FontStyles.Bold,
                new Color(1f, 0.90f, 0.48f, 1f), new Vector2(0f, -95f), new Vector2(900f, 70f));
            CrearTexto(panelSplash.transform, "TextoSplashSub", "◈  Leyenda del Ahuehuete • Chapultepec, CDMX  ◈", 24, FontStyles.Italic,
                new Color(0.76f, 0.92f, 0.80f, 1f), new Vector2(0f, -150f), new Vector2(900f, 40f));
            CrearTexto(panelSplash.transform, "TextoSplashEquipo", "Laura Tapia Cid  •  Vanesa Medrano Hernández", 22, FontStyles.Normal,
                new Color(0.90f, 0.90f, 0.88f, 0.85f), new Vector2(0f, -205f), new Vector2(900f, 36f));

            EditorUtility.SetDirty(goCanvas);
            EditorSceneManager.MarkSceneDirty(escena);
            Selection.activeGameObject = goCanvas;
        }

        private static GameObject CrearPanelTransparente(Transform padre, string nombre)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform));
            go.transform.SetParent(padre, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        private static GameObject CrearPanelFondoOscuro(Transform padre, string nombre)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(padre, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.03f, 0.06f, 0.05f, 0.82f);
            return go;
        }

        private static GameObject CrearTarjetaConBorde(Transform padre, string nombre, Vector2 pos, Vector2 size, bool ancladoArribaIzquierda)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(Outline));
            go.transform.SetParent(padre, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            if (ancladoArribaIzquierda)
            {
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
            }
            else
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
            }
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.06f, 0.11f, 0.09f, 0.90f);

            Outline borde = go.GetComponent<Outline>();
            borde.effectColor = new Color32(0xD9, 0xA4, 0x41, 0xCC);
            borde.effectDistance = new Vector2(3f, -3f);
            return go;
        }

        private static TextMeshProUGUI CrearTexto(Transform padre, string nombre, string contenido, float tamano, FontStyles estilo, Color color, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(padre, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = contenido;
            tmp.fontSize = tamano;
            tmp.fontStyle = estilo;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CrearBotonEstilizado(Transform padre, string nombre, string texto, Color colorFondo, Sprite iconoOpcional, out TextMeshProUGUI tmpOut)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(LayoutElement), typeof(EfectoBotonMenu));
            go.transform.SetParent(padre, false);

            go.GetComponent<LayoutElement>().preferredHeight = 62f;

            Image img = go.GetComponent<Image>();
            img.color = colorFondo;

            Button btn = go.GetComponent<Button>();
            btn.targetGraphic = img;

            Outline borde = go.GetComponent<Outline>();
            borde.effectDistance = new Vector2(2f, -2f);
            go.GetComponent<EfectoBotonMenu>().BordeResplandor = borde;

            tmpOut = CrearTexto(go.transform, "Texto", texto, 23, FontStyles.Bold, Color.white, Vector2.zero, Vector2.zero);
            RectTransform rtTxt = tmpOut.GetComponent<RectTransform>();
            rtTxt.anchorMin = Vector2.zero;
            rtTxt.anchorMax = Vector2.one;
            rtTxt.offsetMin = new Vector2(iconoOpcional != null ? 56f : 16f, 0f);
            rtTxt.offsetMax = new Vector2(-16f, 0f);

            if (iconoOpcional != null)
            {
                GameObject goIco = new GameObject("Icono", typeof(RectTransform), typeof(Image));
                goIco.transform.SetParent(go.transform, false);
                RectTransform rtIco = goIco.GetComponent<RectTransform>();
                rtIco.anchorMin = new Vector2(0f, 0.5f);
                rtIco.anchorMax = new Vector2(0f, 0.5f);
                rtIco.anchoredPosition = new Vector2(30f, 0f);
                rtIco.sizeDelta = new Vector2(34f, 34f);
                Image imgIco = goIco.GetComponent<Image>();
                imgIco.sprite = iconoOpcional;
                imgIco.preserveAspect = true;
                imgIco.raycastTarget = false;
            }

            return btn;
        }

        private static Button CrearBotonAnclado(Transform padre, string nombre, string texto, Vector2 pos, Vector2 size, Color colorFondo)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(EfectoBotonMenu));
            go.transform.SetParent(padre, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.GetComponent<Image>();
            img.color = colorFondo;

            Button btn = go.GetComponent<Button>();
            btn.targetGraphic = img;

            Outline borde = go.GetComponent<Outline>();
            borde.effectDistance = new Vector2(2f, -2f);
            go.GetComponent<EfectoBotonMenu>().BordeResplandor = borde;

            TextMeshProUGUI tmp = CrearTexto(go.transform, "Texto", texto, 24, FontStyles.Bold, Color.white, Vector2.zero, Vector2.zero);
            RectTransform rtTxt = tmp.GetComponent<RectTransform>();
            rtTxt.anchorMin = Vector2.zero;
            rtTxt.anchorMax = Vector2.one;
            rtTxt.offsetMin = Vector2.zero;
            rtTxt.offsetMax = Vector2.zero;

            return btn;
        }

        private static Slider CrearSlider(Transform padre, string nombre, string etiqueta, Vector2 pos, out TextMeshProUGUI tmpPorcentaje)
        {
            GameObject contenedor = new GameObject(nombre + "_Contenedor", typeof(RectTransform));
            contenedor.transform.SetParent(padre, false);

            RectTransform rtCont = contenedor.GetComponent<RectTransform>();
            rtCont.anchorMin = new Vector2(0.5f, 0.5f);
            rtCont.anchorMax = new Vector2(0.5f, 0.5f);
            rtCont.anchoredPosition = pos;
            rtCont.sizeDelta = new Vector2(640f, 75f);

            TextMeshProUGUI lbl = CrearTexto(contenedor.transform, "Etiqueta", etiqueta, 22, FontStyles.Bold, Color.white, new Vector2(-80f, 22f), new Vector2(440f, 32f));
            lbl.alignment = TextAlignmentOptions.MidlineLeft;

            tmpPorcentaje = CrearTexto(contenedor.transform, "Porcentaje", "80%", 22, FontStyles.Bold, new Color(1f, 0.90f, 0.48f, 1f), new Vector2(230f, 22f), new Vector2(100f, 32f));
            tmpPorcentaje.alignment = TextAlignmentOptions.MidlineRight;

            GameObject goSlider = new GameObject(nombre, typeof(RectTransform), typeof(Slider));
            goSlider.transform.SetParent(contenedor.transform, false);
            RectTransform rtSlider = goSlider.GetComponent<RectTransform>();
            rtSlider.anchorMin = new Vector2(0.5f, 0.5f);
            rtSlider.anchorMax = new Vector2(0.5f, 0.5f);
            rtSlider.anchoredPosition = new Vector2(0f, -15f);
            rtSlider.sizeDelta = new Vector2(560f, 26f);

            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(goSlider.transform, false);
            RectTransform rtBg = bg.GetComponent<RectTransform>();
            rtBg.anchorMin = Vector2.zero;
            rtBg.anchorMax = Vector2.one;
            rtBg.offsetMin = Vector2.zero;
            rtBg.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.18f, 0.24f, 0.20f, 1f);

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(goSlider.transform, false);
            RectTransform rtFA = fillArea.GetComponent<RectTransform>();
            rtFA.anchorMin = Vector2.zero;
            rtFA.anchorMax = Vector2.one;
            rtFA.offsetMin = new Vector2(5f, 0f);
            rtFA.offsetMax = new Vector2(-5f, 0f);

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform rtFill = fill.GetComponent<RectTransform>();
            rtFill.anchorMin = Vector2.zero;
            rtFill.anchorMax = Vector2.one;
            rtFill.offsetMin = Vector2.zero;
            rtFill.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().color = new Color(0.30f, 0.78f, 0.48f, 1f);

            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(goSlider.transform, false);
            RectTransform rtHA = handleArea.GetComponent<RectTransform>();
            rtHA.anchorMin = Vector2.zero;
            rtHA.anchorMax = Vector2.one;
            rtHA.offsetMin = new Vector2(10f, 0f);
            rtHA.offsetMax = new Vector2(-10f, 0f);

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform rtHandle = handle.GetComponent<RectTransform>();
            rtHandle.sizeDelta = new Vector2(24f, 34f);
            Image imgHandle = handle.GetComponent<Image>();
            imgHandle.color = new Color(1f, 0.90f, 0.48f, 1f);

            Slider slider = goSlider.GetComponent<Slider>();
            slider.fillRect = rtFill;
            slider.handleRect = rtHandle;
            slider.targetGraphic = imgHandle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.value = 0.8f;
            return slider;
        }

        private static TMP_Dropdown CrearDropdown(Transform padre, string nombre, string etiqueta, Vector2 pos)
        {
            GameObject contenedor = new GameObject(nombre + "_Contenedor", typeof(RectTransform));
            contenedor.transform.SetParent(padre, false);
            RectTransform rtCont = contenedor.GetComponent<RectTransform>();
            rtCont.anchorMin = new Vector2(0.5f, 0.5f);
            rtCont.anchorMax = new Vector2(0.5f, 0.5f);
            rtCont.anchoredPosition = pos;
            rtCont.sizeDelta = new Vector2(640f, 80f);

            TextMeshProUGUI lbl = CrearTexto(contenedor.transform, "Etiqueta", etiqueta, 22, FontStyles.Bold, Color.white, new Vector2(-175f, 0f), new Vector2(210f, 40f));
            lbl.alignment = TextAlignmentOptions.MidlineLeft;

            GameObject goDrop = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(TMP_Dropdown), typeof(Outline));
            goDrop.transform.SetParent(contenedor.transform, false);
            RectTransform rtDrop = goDrop.GetComponent<RectTransform>();
            rtDrop.anchorMin = new Vector2(0.5f, 0.5f);
            rtDrop.anchorMax = new Vector2(0.5f, 0.5f);
            rtDrop.anchoredPosition = new Vector2(105f, 0f);
            rtDrop.sizeDelta = new Vector2(340f, 46f);
            goDrop.GetComponent<Image>().color = new Color(0.15f, 0.23f, 0.19f, 1f);
            goDrop.GetComponent<Outline>().effectColor = new Color(0.85f, 0.65f, 0.25f, 0.55f);

            TextMeshProUGUI label = CrearTexto(goDrop.transform, "Label", "1920 x 1080", 21, FontStyles.Normal, Color.white, Vector2.zero, Vector2.zero);
            RectTransform rtLab = label.GetComponent<RectTransform>();
            rtLab.anchorMin = Vector2.zero;
            rtLab.anchorMax = Vector2.one;
            rtLab.offsetMin = new Vector2(12f, 4f);
            rtLab.offsetMax = new Vector2(-28f, -4f);

            GameObject template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            template.transform.SetParent(goDrop.transform, false);
            RectTransform rtTpl = template.GetComponent<RectTransform>();
            rtTpl.anchorMin = new Vector2(0f, 0f);
            rtTpl.anchorMax = new Vector2(1f, 0f);
            rtTpl.pivot = new Vector2(0.5f, 1f);
            rtTpl.sizeDelta = new Vector2(0f, 160f);
            template.GetComponent<Image>().color = new Color(0.10f, 0.16f, 0.14f, 1f);

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(template.transform, false);
            RectTransform rtVp = viewport.GetComponent<RectTransform>();
            rtVp.anchorMin = Vector2.zero;
            rtVp.anchorMax = Vector2.one;
            rtVp.offsetMin = Vector2.zero;
            rtVp.offsetMax = Vector2.zero;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            RectTransform rtCnt = content.GetComponent<RectTransform>();
            rtCnt.anchorMin = new Vector2(0f, 1f);
            rtCnt.anchorMax = new Vector2(1f, 1f);
            rtCnt.pivot = new Vector2(0.5f, 1f);
            rtCnt.sizeDelta = new Vector2(0f, 40f);

            GameObject item = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
            item.transform.SetParent(content.transform, false);
            RectTransform rtItem = item.GetComponent<RectTransform>();
            rtItem.anchorMin = new Vector2(0f, 0.5f);
            rtItem.anchorMax = new Vector2(1f, 0.5f);
            rtItem.sizeDelta = new Vector2(0f, 38f);

            GameObject itemBg = new GameObject("Item Background", typeof(RectTransform), typeof(Image));
            itemBg.transform.SetParent(item.transform, false);
            RectTransform rtIBg = itemBg.GetComponent<RectTransform>();
            rtIBg.anchorMin = Vector2.zero;
            rtIBg.anchorMax = Vector2.one;
            rtIBg.offsetMin = Vector2.zero;
            rtIBg.offsetMax = Vector2.zero;
            itemBg.GetComponent<Image>().color = new Color(0.20f, 0.36f, 0.28f, 1f);

            TextMeshProUGUI itemLabel = CrearTexto(item.transform, "Item Label", "Option", 20, FontStyles.Normal, Color.white, Vector2.zero, Vector2.zero);
            RectTransform rtILab = itemLabel.GetComponent<RectTransform>();
            rtILab.anchorMin = Vector2.zero;
            rtILab.anchorMax = Vector2.one;
            rtILab.offsetMin = new Vector2(10f, 2f);
            rtILab.offsetMax = new Vector2(-10f, -2f);

            Toggle itemToggle = item.GetComponent<Toggle>();
            itemToggle.targetGraphic = itemBg.GetComponent<Image>();

            ScrollRect sr = template.GetComponent<ScrollRect>();
            sr.content = rtCnt;
            sr.viewport = rtVp;

            template.SetActive(false);

            TMP_Dropdown drop = goDrop.GetComponent<TMP_Dropdown>();
            drop.template = rtTpl;
            drop.captionText = label;
            drop.itemText = itemLabel;
            return drop;
        }

        private static Toggle CrearToggle(Transform padre, string nombre, string etiqueta, Vector2 pos)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Toggle));
            go.transform.SetParent(padre, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(360f, 44f);

            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            RectTransform rtBg = bg.GetComponent<RectTransform>();
            rtBg.anchorMin = new Vector2(0f, 0.5f);
            rtBg.anchorMax = new Vector2(0f, 0.5f);
            rtBg.anchoredPosition = new Vector2(22f, 0f);
            rtBg.sizeDelta = new Vector2(32f, 32f);
            bg.GetComponent<Image>().color = new Color(0.18f, 0.26f, 0.22f, 1f);

            GameObject check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            check.transform.SetParent(bg.transform, false);
            RectTransform rtChk = check.GetComponent<RectTransform>();
            rtChk.anchorMin = new Vector2(0.15f, 0.15f);
            rtChk.anchorMax = new Vector2(0.85f, 0.85f);
            rtChk.offsetMin = Vector2.zero;
            rtChk.offsetMax = Vector2.zero;
            Image imgChk = check.GetComponent<Image>();
            imgChk.color = new Color(0.38f, 0.85f, 0.52f, 1f);

            TextMeshProUGUI txt = CrearTexto(go.transform, "Label", etiqueta, 22, FontStyles.Bold, Color.white, new Vector2(28f, 0f), new Vector2(280f, 40f));
            txt.alignment = TextAlignmentOptions.MidlineLeft;

            Toggle t = go.GetComponent<Toggle>();
            t.targetGraphic = bg.GetComponent<Image>();
            t.graphic = imgChk;
            t.isOn = Screen.fullScreen;
            return t;
        }
    }
}
