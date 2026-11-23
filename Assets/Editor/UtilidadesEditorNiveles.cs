using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using SemillasDelBosque.Audio;
using SemillasDelBosque.Editor;
using SemillasDelBosque.Sistema;
using SemillasDelBosque.Sombra;
using SemillasDelBosque.UI;

namespace SemillasDelBosque.EditorTools
{
    /// <summary>
    /// UtilidadesEditorNiveles: Helpers compartidos e idempotentes para crear escenas,
    /// registrar tags/Build Settings, configurar Meta, HUD y variables de Visual Scripting.
    /// </summary>
    public static class UtilidadesEditorNiveles
    {
        public const string RUTA_ESCENA_MENU = "Assets/Scenes/MenuPrincipal.unity";
        public const string RUTA_ESCENA_NIVEL1 = "Assets/Scenes/Claro del Despertar.unity";
        public const string RUTA_ESCENA_NIVEL2 = "Assets/Scenes/Bosque Profundo.unity";
        public const string RUTA_ESCENA_NIVEL3 = "Assets/Scenes/Lago de las Luciernagas.unity";
        public const string RUTA_ESCENA_NIVEL4 = "Assets/Scenes/Cima de los Recuerdos.unity";

        public static readonly string[] RutasTodasLasEscenas =
        {
            RUTA_ESCENA_MENU,
            RUTA_ESCENA_NIVEL1,
            RUTA_ESCENA_NIVEL2,
            RUTA_ESCENA_NIVEL3,
            RUTA_ESCENA_NIVEL4
        };

        /// <summary>
        /// Registra en TagManager los tags de jugador ("Jugador", "Luna", "Nahui") si aún no existen.
        /// </summary>
        public static void RegistrarTagsJugador()
        {
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            if (tagsProp == null) return;

            string[] requeridos = { "Jugador", "Luna", "Nahui" };
            bool huboCambios = false;

            foreach (string tag in requeridos)
            {
                bool encontrado = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                    {
                        encontrado = true;
                        break;
                    }
                }

                if (!encontrado)
                {
                    int idx = tagsProp.arraySize;
                    tagsProp.InsertArrayElementAtIndex(idx);
                    tagsProp.GetArrayElementAtIndex(idx).stringValue = tag;
                    huboCambios = true;
                }
            }

            if (huboCambios)
            {
                tagManager.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Agrega una escena a Build Settings sin duplicarla.
        /// </summary>
        public static void AgregarEscenaABuildSettings(string rutaEscena)
        {
            var lista = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!lista.Exists(e => e.path == rutaEscena))
            {
                lista.Add(new EditorBuildSettingsScene(rutaEscena, true));
                EditorBuildSettings.scenes = lista.ToArray();
            }
        }

        /// <summary>
        /// Ordena las escenas principales del juego en Build Settings (MenuPrincipal primero).
        /// </summary>
        [MenuItem("Semillas del Bosque/Configurar Build Settings (Todas las Escenas)", false, 100)]
        public static void ConfigurarBuildSettingsOrdenado()
        {
            var resultado = new List<EditorBuildSettingsScene>();
            foreach (string ruta in RutasTodasLasEscenas)
            {
                if (File.Exists(ruta))
                {
                    resultado.Add(new EditorBuildSettingsScene(ruta, true));
                }
            }

            foreach (var existente in EditorBuildSettings.scenes)
            {
                if (!resultado.Exists(e => e.path == existente.path) && File.Exists(existente.path))
                {
                    resultado.Add(existente);
                }
            }

            EditorBuildSettings.scenes = resultado.ToArray();
            Debug.Log($"[BuildSettings] {resultado.Count} escenas registradas en orden correctamente.");
        }

        /// <summary>
        /// Construye y guarda en un solo paso los sprites, animaciones, MenuPrincipal y los 4 niveles.
        /// </summary>
        [MenuItem("Semillas del Bosque/Construir TODO el Proyecto (Menu + 4 Niveles + Animaciones)", false, 200)]
        public static void ConstruirTodoElProyecto()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            RegistrarTagsJugador();
            GeneradorArte.GenerarTodosLosSprites();
            ConfiguradorAnimaciones.GenerarAssetsAnimacion();

            ConstructorMenuPrincipal.CrearYConfigurarMenuPrincipal();
            EditorSceneManager.SaveOpenScenes();

            ConstructorNivel1.ConfigurarNivel1();
            EditorSceneManager.SaveOpenScenes();

            ConstructorNivel2.ConstruirNivel2();
            EditorSceneManager.SaveOpenScenes();

            ConstructorNivel3.ConstruirNivel();
            EditorSceneManager.SaveOpenScenes();

            ConstructorNivel4.ConstruirNivel4();
            EditorSceneManager.SaveOpenScenes();

            ConfigurarBuildSettingsOrdenado();
            EditorSceneManager.OpenScene(RUTA_ESCENA_MENU, OpenSceneMode.Single);
            Debug.Log("[Semillas del Bosque] ¡Proyecto completo configurado y guardado (MenuPrincipal + Niveles 1 al 4 + Animaciones + Sprites)!");
        }

        /// <summary>
        /// Clona la escena base de Nivel 1 hacia rutaDestino de forma idempotente,
        /// conservando Cámara, Luz, Canvas, Luna/Nahui y SceneVariables, limpiando los grupos del Nivel 1.
        /// </summary>
        public static bool DuplicarEscenaBase(string rutaDestino, string etiquetaLog)
        {
            if (!File.Exists(RUTA_ESCENA_NIVEL1))
            {
                Debug.LogError($"[{etiquetaLog}] No se encontró la escena base en: {RUTA_ESCENA_NIVEL1}");
                return false;
            }

            if (File.Exists(rutaDestino))
            {
                Debug.LogWarning($"[{etiquetaLog}] La escena '{rutaDestino}' ya existe. Se abrirá sin sobrescribir.");
                AgregarEscenaABuildSettings(rutaDestino);
                EditorSceneManager.OpenScene(rutaDestino, OpenSceneMode.Single);
                return true;
            }

            if (!AssetDatabase.CopyAsset(RUTA_ESCENA_NIVEL1, rutaDestino))
            {
                Debug.LogError($"[{etiquetaLog}] Error al duplicar {RUTA_ESCENA_NIVEL1} hacia {rutaDestino}.");
                return false;
            }

            AssetDatabase.Refresh();
            AgregarEscenaABuildSettings(rutaDestino);

            Scene nuevaEscena = EditorSceneManager.OpenScene(rutaDestino, OpenSceneMode.Single);
            if (!nuevaEscena.IsValid())
            {
                Debug.LogError($"[{etiquetaLog}] No fue posible abrir la nueva escena: {rutaDestino}");
                return false;
            }

            LimpiarContenidoNivelBase(nuevaEscena, etiquetaLog);
            EditorSceneManager.MarkSceneDirty(nuevaEscena);
            EditorSceneManager.SaveScene(nuevaEscena);
            Debug.Log($"[{etiquetaLog}] Escena '{rutaDestino}' creada y preparada con éxito.");
            return true;
        }

        private static void LimpiarContenidoNivelBase(Scene escena, string etiquetaLog)
        {
            var raices = escena.GetRootGameObjects();
            var objetosAEliminar = new List<GameObject>();

            foreach (var raiz in raices)
            {
                if (raiz.name == "--- SUELO Y PLATAFORMAS ---")
                {
                    for (int i = 0; i < raiz.transform.childCount; i++)
                    {
                        objetosAEliminar.Add(raiz.transform.GetChild(i).gameObject);
                    }
                }
                else if (raiz.name == "--- ELEMENTOS DEL NIVEL ---")
                {
                    for (int i = 0; i < raiz.transform.childCount; i++)
                    {
                        var hijo = raiz.transform.GetChild(i).gameObject;
                        if (EsObjetoJugador(hijo))
                        {
                            continue;
                        }
                        objetosAEliminar.Add(hijo);
                    }
                }
                else if (raiz.name == "Meta" || raiz.name == "ZonaCaida" ||
                         raiz.name == "ParedIzquierda" || raiz.name == "ParedDerecha" ||
                         raiz.name == "--- ENTORNO ---")
                {
                    objetosAEliminar.Add(raiz);
                }
            }

            Debug.Log($"[{etiquetaLog}] Limpiando {objetosAEliminar.Count} objetos heredados de Nivel 1...");
            foreach (var go in objetosAEliminar)
            {
                if (go != null) Undo.DestroyObjectImmediate(go);
            }
        }

        public static bool EsObjetoJugador(GameObject go)
        {
            if (go == null) return false;
            return UtilidadesJugador.EsJugador(go);
        }

        public static GameObject BuscarJugadorEnEscena()
        {
            return UtilidadesJugador.BuscarJugador();
        }

        /// <summary>
        /// Crea o actualiza el contenedor raíz de nivel de forma 100% idempotente con Undo.
        /// </summary>
        public static GameObject ReiniciarContenedorNivel(string nombreContenedor)
        {
            GameObject existente = GameObject.Find(nombreContenedor);
            if (existente != null)
            {
                Undo.DestroyObjectImmediate(existente);
            }

            GameObject raiz = new GameObject(nombreContenedor);
            Undo.RegisterCreatedObjectUndo(raiz, $"Crear {nombreContenedor}");
            return raiz;
        }

        public static Transform CrearGrupo(Transform padre, string nombre)
        {
            GameObject grupo = new GameObject(nombre);
            Undo.RegisterCreatedObjectUndo(grupo, $"Crear grupo {nombre}");
            grupo.transform.SetParent(padre, false);
            grupo.transform.localPosition = Vector3.zero;
            return grupo.transform;
        }

        /// <summary>
        /// Crea o configura el GameObject Meta con GrafoMeta y ControladorMeta de forma idempotente.
        /// </summary>
        public static GameObject ConfigurarMetaNivel(
            Transform padre,
            Vector3 posicion,
            int nivelActual,
            string nombreSiguienteEscena,
            bool esNivelFinal,
            bool requiereJefe = false,
            GranSombraNahual jefe = null)
        {
            GameObject meta = GameObject.Find("Meta");
            if (meta == null)
            {
                meta = new GameObject("Meta");
                Undo.RegisterCreatedObjectUndo(meta, "Crear Meta");
                if (padre != null) meta.transform.SetParent(padre, false);
                meta.transform.position = posicion;
            }
            else
            {
                Undo.RecordObject(meta, "Configurar Meta");
            }

            meta.tag = "Meta";

            BoxCollider2D col = meta.GetComponent<BoxCollider2D>();
            if (col == null) col = Undo.AddComponent<BoxCollider2D>(meta);
            col.size = new Vector2(2.2f, 3.2f);
            col.isTrigger = true;

            GameObject panelNivelCompleto = BuscarObjetoInactivo("PanelNivelCompleto");

            Variables vars = meta.GetComponent<Variables>();
            if (vars == null) vars = Undo.AddComponent<Variables>(meta);
            if (panelNivelCompleto != null)
            {
                vars.declarations.Set("panelNivelCompleto", panelNivelCompleto);
            }

            ScriptGraphAsset grafoMeta = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>("Assets/Graphs/GrafoMeta.asset");
            if (grafoMeta != null)
            {
                ScriptMachine sm = meta.GetComponent<ScriptMachine>();
                if (sm == null) sm = Undo.AddComponent<ScriptMachine>(meta);
                sm.nest.macro = grafoMeta;
            }

            ControladorMeta ctrl = meta.GetComponent<ControladorMeta>();
            if (ctrl == null) ctrl = Undo.AddComponent<ControladorMeta>(meta);
            Undo.RecordObject(ctrl, "Configurar ControladorMeta");
            ctrl.NumeroNivelActual = nivelActual;
            ctrl.NombreSiguienteEscena = nombreSiguienteEscena;
            ctrl.EsNivelFinal = esNivelFinal;
            ctrl.RequiereJefeDerrotado = requiereJefe;
            ctrl.JefeRequerido = jefe;
            ctrl.PanelNivelCompleto = panelNivelCompleto;

            EditorUtility.SetDirty(meta);
            return meta;
        }

        /// <summary>
        /// Asegura que exista ZonaCaida en Y = -9 cubriendo el ancho del nivel.
        /// </summary>
        public static void AsegurarZonaCaida(Transform padre = null, float centroX = 60f, float ancho = 220f)
        {
            GameObject zona = GameObject.Find("ZonaCaida");
            if (zona == null)
            {
                zona = new GameObject("ZonaCaida");
                Undo.RegisterCreatedObjectUndo(zona, "Crear ZonaCaida");
            }
            else
            {
                Undo.RecordObject(zona.transform, "Actualizar ZonaCaida");
            }

            if (padre != null && zona.transform.parent == null)
            {
                zona.transform.SetParent(padre, true);
            }

            zona.transform.position = new Vector3(centroX, -9f, 0f);

            BoxCollider2D col = zona.GetComponent<BoxCollider2D>();
            if (col == null) col = Undo.AddComponent<BoxCollider2D>(zona);
            col.isTrigger = true;
            col.size = new Vector2(ancho, 6f);
            col.offset = Vector2.zero;

            if (zona.GetComponent<SemillasDelBosque.Nivel.ZonaCaida>() == null)
            {
                Undo.AddComponent<SemillasDelBosque.Nivel.ZonaCaida>(zona);
            }

            EditorUtility.SetDirty(zona);
        }

        /// <summary>
        /// Posiciona al jugador y reinicializa las variables de escena de Visual Scripting.
        /// </summary>
        public static void PosicionarJugadorEInicializarVariables(Vector3 posicionInicio, bool tieneDobleSaltoInicial = false)
        {
            GameObject jugador = BuscarJugadorEnEscena();
            if (jugador != null)
            {
                Undo.RecordObject(jugador.transform, "Posicionar Jugador");
                jugador.transform.position = posicionInicio;
                EditorUtility.SetDirty(jugador);
            }

            GameObject sceneVarsGo = GameObject.Find("VisualScripting SceneVariables");
            if (sceneVarsGo != null)
            {
                Variables vars = sceneVarsGo.GetComponent<Variables>();
                if (vars != null)
                {
                    Undo.RecordObject(vars, "Inicializar SceneVariables");
                    vars.declarations.Set("ultimoCheckpoint", posicionInicio);
                    vars.declarations.Set("semillas", 0);
                    vars.declarations.Set("vidas", 3);
                    vars.declarations.Set("tieneDobleSalto", tieneDobleSaltoInicial);
                    vars.declarations.Set("saltosRestantes", tieneDobleSaltoInicial ? 1 : 0);
                    EditorUtility.SetDirty(vars);
                }
            }
        }

        /// <summary>
        /// Configura el HUD, Resumen de Meta y Audio de Game Over en la escena activa.
        /// </summary>
        public static void ConfigurarHUDYAudioUI()
        {
            HudJuego.MenuAgregarHUD();
            RepararHUD.MenuRepararSpritesHUD();
            ResumenNivel.MenuConfigurarResumen();
            GameOverAudio.MenuAgregarGameOverAudio();
            BotonReiniciarNivel.MenuCorregirBotonReiniciar();
            AsegurarPanelLeyendaMexicana();
            AsegurarLibroCodiceEnNivel();
        }

        private static void AsegurarLibroCodiceEnNivel()
        {
            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            LibroHistoriaController libro = ConstructorLibroCodice.ConstruirLibroEnCanvas(
                canvas.transform,
                permitirTeclaEnJuego: true,
                pausarAlAbrir: true);

            Transform btnExistente = canvas.transform.Find("BotonCodiceHUD");
            if (btnExistente != null)
            {
                Undo.DestroyObjectImmediate(btnExistente.gameObject);
            }

            GameObject goBtn = new GameObject("BotonCodiceHUD", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(EfectoBotonMenu));
            Undo.RegisterCreatedObjectUndo(goBtn, "Crear BotonCodiceHUD");
            goBtn.transform.SetParent(canvas.transform, false);

            RectTransform rt = goBtn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-24f, -24f);
            rt.sizeDelta = new Vector2(158f, 46f);

            Image bg = goBtn.GetComponent<Image>();
            bg.color = new Color32(0x3B, 0x24, 0x15, 0xE6);

            Outline borde = goBtn.GetComponent<Outline>();
            borde.effectColor = new Color32(0xD9, 0xA4, 0x41, 0xCC);
            borde.effectDistance = new Vector2(2f, -2f);
            goBtn.GetComponent<EfectoBotonMenu>().BordeResplandor = borde;

            Sprite sLibro = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/libro_codice.png");
            if (sLibro != null)
            {
                GameObject goIco = new GameObject("Icono", typeof(RectTransform), typeof(Image));
                goIco.transform.SetParent(goBtn.transform, false);
                RectTransform rtIco = goIco.GetComponent<RectTransform>();
                rtIco.anchorMin = new Vector2(0f, 0.5f);
                rtIco.anchorMax = new Vector2(0f, 0.5f);
                rtIco.anchoredPosition = new Vector2(22f, 0f);
                rtIco.sizeDelta = new Vector2(26f, 26f);
                Image imgIco = goIco.GetComponent<Image>();
                imgIco.sprite = sLibro;
                imgIco.preserveAspect = true;
                imgIco.raycastTarget = false;
            }

            GameObject goTxt = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
            goTxt.transform.SetParent(goBtn.transform, false);
            RectTransform rtTxt = goTxt.GetComponent<RectTransform>();
            rtTxt.anchorMin = Vector2.zero;
            rtTxt.anchorMax = Vector2.one;
            rtTxt.offsetMin = new Vector2(sLibro != null ? 40f : 8f, 0f);
            rtTxt.offsetMax = new Vector2(-8f, 0f);

            TextMeshProUGUI tmp = goTxt.GetComponent<TextMeshProUGUI>();
            tmp.text = "[L] Códice";
            tmp.fontSize = 17;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.92f, 0.60f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            Button btn = goBtn.GetComponent<Button>();
            btn.targetGraphic = bg;
            UnityEventTools.AddVoidPersistentListener(btn.onClick, libro.AbrirEnPaginaDelNivelActual);

            EditorUtility.SetDirty(goBtn);
        }

        private static void AsegurarPanelLeyendaMexicana()
        {
            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Transform existente = canvas.transform.Find("PanelLeyendaMexicana");
            if (existente != null)
            {
                Undo.DestroyObjectImmediate(existente.gameObject);
            }

            GameObject panel = new GameObject(
                "PanelLeyendaMexicana",
                typeof(RectTransform),
                typeof(Image),
                typeof(CanvasGroup),
                typeof(Button));
            Undo.RegisterCreatedObjectUndo(panel, "Crear Panel Leyenda Mexicana");
            panel.transform.SetParent(canvas.transform, false);

            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 22f);
            rt.sizeDelta = new Vector2(780f, 128f);

            Image bg = panel.GetComponent<Image>();
            bg.color = new Color(0.10f, 0.08f, 0.06f, 0.88f);

            GameObject goTitulo = new GameObject("TituloLeyenda", typeof(RectTransform), typeof(TextMeshProUGUI));
            goTitulo.transform.SetParent(panel.transform, false);
            RectTransform rtTit = goTitulo.GetComponent<RectTransform>();
            rtTit.anchorMin = new Vector2(0f, 1f);
            rtTit.anchorMax = new Vector2(1f, 1f);
            rtTit.pivot = new Vector2(0.5f, 1f);
            rtTit.anchoredPosition = new Vector2(0f, -10f);
            rtTit.sizeDelta = new Vector2(-32f, 30f);

            TextMeshProUGUI tmpTit = goTitulo.GetComponent<TextMeshProUGUI>();
            tmpTit.fontSize = 18;
            tmpTit.fontStyle = FontStyles.Bold;
            tmpTit.color = new Color(1f, 0.86f, 0.42f, 1f);
            tmpTit.alignment = TextAlignmentOptions.Center;
            tmpTit.raycastTarget = false;

            GameObject goCuerpo = new GameObject("TextoLeyenda", typeof(RectTransform), typeof(TextMeshProUGUI));
            goCuerpo.transform.SetParent(panel.transform, false);
            RectTransform rtCue = goCuerpo.GetComponent<RectTransform>();
            rtCue.anchorMin = Vector2.zero;
            rtCue.anchorMax = Vector2.one;
            rtCue.offsetMin = new Vector2(20f, 12f);
            rtCue.offsetMax = new Vector2(-20f, -42f);

            TextMeshProUGUI tmpCue = goCuerpo.GetComponent<TextMeshProUGUI>();
            tmpCue.fontSize = 15;
            tmpCue.color = new Color(0.96f, 0.94f, 0.88f, 1f);
            tmpCue.alignment = TextAlignmentOptions.Center;
            tmpCue.textWrappingMode = TextWrappingModes.Normal;
            tmpCue.raycastTarget = false;

            LeyendaMexicanaController ctrl = panel.AddComponent<LeyendaMexicanaController>();
            ctrl.CanvasGroupPanel = panel.GetComponent<CanvasGroup>();
            ctrl.TmpTitulo = tmpTit;
            ctrl.TmpCuerpo = tmpCue;
            ctrl.AsignarLeyendaPorEscena(SceneManager.GetActiveScene().name);

            Button btn = panel.GetComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(ctrl.CerrarAhora);

            EditorUtility.SetDirty(panel);
        }

        /// <summary>
        /// Detecta el número de nivel (1..4) según el nombre de la escena.
        /// </summary>
        public static int DetectarNumeroNivel(Scene escena)
        {
            string nombre = escena.name;
            if (nombre.Contains("Claro") || nombre.Contains("Despertar")) return 1;
            if (nombre.Contains("Bosque") || nombre.Contains("Poetas")) return 2;
            if (nombre.Contains("Lago") || nombre.Contains("Luciernagas")) return 3;
            if (nombre.Contains("Cima") || nombre.Contains("Recuerdos") || nombre.Contains("Castillo")) return 4;
            return 1;
        }

        public static GameObject BuscarObjetoInactivo(string nombre)
        {
            Transform[] todos = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);

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
