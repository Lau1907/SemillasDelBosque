using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using SemillasDelBosque.Editor;
using SemillasDelBosque.EditorScripts;
using SemillasDelBosque.Entorno;
using SemillasDelBosque.Nivel3;
using SemillasDelBosque.Sombra;

namespace SemillasDelBosque.EditorTools
{
    /// <summary>
    /// ConstructorNivel4: Crea y construye idempotente y con Undo el Nivel 4
    /// ("Cima de los Recuerdos - Castillo de Chapultepec"):
    /// 25 semillas, 3 checkpoints, persecución final, mini-jefe Gran Sombra de Nahual,
    /// ZonaCaida en Y = -9 y Meta final "¡Bosque Restaurado!".
    /// </summary>
    public static class ConstructorNivel4
    {
        private const string NOMBRE_CONTENEDOR = "--- NIVEL 4 ---";

        [MenuItem("Semillas del Bosque/4a. Crear Escena Nivel 4 (Cima de los Recuerdos)", false, 40)]
        public static void CrearEscenaNivel4()
        {
            UtilidadesEditorNiveles.RegistrarTagsJugador();
            UtilidadesEditorNiveles.DuplicarEscenaBase(
                UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL4,
                "ConstructorNivel4");
        }

        [MenuItem("Semillas del Bosque/4b. Construir Nivel 4 (Cima de los Recuerdos)", false, 41)]
        public static void ConstruirNivel4()
        {
            if (!File.Exists(UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL4))
            {
                CrearEscenaNivel4();
            }

            Scene escena = SceneManager.GetActiveScene();
            if (escena.path != UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL4)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                escena = EditorSceneManager.OpenScene(UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL4, OpenSceneMode.Single);
            }

            UtilidadesEditorNiveles.RegistrarTagsJugador();

            GameObject prefabSuelo = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BloqueSuelo.prefab");
            GameObject prefabSemilla = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SemillaDeLuz.prefab");
            GameObject prefabSombra = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SombraEspinosa.prefab");
            GameObject prefabCheckpoint = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PuntoDeControl.prefab");
            GameObject prefabPowerUp = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PowerUp.prefab");
            GameObject prefabBuho = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BuhoSabio.prefab");

            if (prefabSuelo == null || prefabSemilla == null || prefabSombra == null || prefabCheckpoint == null)
            {
                Debug.LogError("[ConstructorNivel4] Faltan prefabs requeridos en Assets/Prefabs/.");
                return;
            }

            GameObject raiz = UtilidadesEditorNiveles.ReiniciarContenedorNivel(NOMBRE_CONTENEDOR);

            Transform grpSuelo = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "1_AscensoYCastillo");
            Transform grpSemillas = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "2_SemillasDeLuz");
            Transform grpPersecucion = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "3_PersecucionFinal");
            Transform grpCheckpoints = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "4_Checkpoints");
            Transform grpJefe = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "5_MiniJefeNahual");
            Transform grpBuho = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "6_BuhoSabio");
            Transform grpMeta = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "7_MetaFinal");
            Transform grpLimites = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "8_Limites");

            ConstruirPlataformas(grpSuelo, prefabSuelo);
            ConstruirSemillas(grpSemillas, prefabSemilla);
            ConstruirPersecucionFinal(grpPersecucion, prefabSombra);
            ConstruirCheckpoints(grpCheckpoints, prefabCheckpoint);

            if (prefabPowerUp != null)
            {
                GameObject pu = (GameObject)PrefabUtility.InstantiatePrefab(prefabPowerUp, grpSuelo);
                Undo.RegisterCreatedObjectUndo(pu, "Crear PowerUp Nivel 4");
                pu.name = "PowerUp_DobleSalto";
                pu.transform.position = new Vector3(8.5f, 1.5f, 0f);
            }

            GranSombraNahual jefe = ConstruirMiniJefeNahual(grpJefe);
            ConstruirBuho(grpBuho, prefabBuho);
            ConstruirLimites(grpLimites);

            UtilidadesEditorNiveles.AsegurarZonaCaida(grpLimites, 65f, 240f);
            UtilidadesEditorNiveles.ConfigurarMetaNivel(
                grpMeta,
                new Vector3(121.0f, 3.6f, 0f),
                nivelActual: 4,
                nombreSiguienteEscena: string.Empty,
                esNivelFinal: true,
                requiereJefe: true,
                jefe: jefe);

            UtilidadesEditorNiveles.PosicionarJugadorEInicializarVariables(new Vector3(2f, 1.0f, 0f), true);

            ConstructorEntorno.ConstruirEntornoNivel();
            ConfigurarAudioSombras.EjecutarConfiguracion();
            BuhoConsejo.MenuConfigurarBuho();
            ConfigurarTextoBuhoNivel4();
            UtilidadesEditorNiveles.ConfigurarHUDYAudioUI();
            ConfiguradorAnimaciones.AplicarEnEscenaActiva();
            UtilidadesEditorNiveles.AgregarEscenaABuildSettings(UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL4);

            EditorSceneManager.MarkSceneDirty(escena);
            Debug.Log("[ConstructorNivel4] Nivel 4 construido: 25 semillas, 3 checkpoints, persecución final, Gran Sombra de Nahual y Meta ¡Bosque Restaurado!.");
        }

        private static void ConstruirPlataformas(Transform padre, GameObject prefabSuelo)
        {
            var datos = new (string nombre, float x, float y, float ancho, float alto)[]
            {
                ("Suelo_BaseCerro",            6.0f,  -0.5f, 14.0f, 1.0f),
                ("Plataforma_Rampa_1",        17.0f,   0.6f,  6.0f, 0.8f),
                ("Plataforma_Rampa_2",        27.0f,   1.5f, 10.0f, 0.9f),
                ("Plataforma_Mirador_1",      39.0f,   2.2f,  8.0f, 0.9f),
                ("Plataforma_Persecucion_A",  53.0f,   1.8f, 15.0f, 1.0f),
                ("Plataforma_Escalon_Alto",   64.5f,   3.0f,  5.0f, 0.8f),
                ("Plataforma_Persecucion_B",  76.5f,   2.2f, 14.0f, 1.0f),
                ("Plataforma_AntesalaJefe",   89.0f,   2.5f,  7.0f, 0.9f),
                ("Arena_CastilloChapultepec", 108.5f,  2.0f, 30.0f, 1.2f),
                ("Plataforma_ApoyoJefe_Izq",  101.0f,  4.4f,  4.0f, 0.5f),
                ("Plataforma_ApoyoJefe_Der",  112.0f,  4.4f,  4.0f, 0.5f)
            };

            foreach (var d in datos)
            {
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefabSuelo, padre);
                Undo.RegisterCreatedObjectUndo(go, $"Crear {d.nombre}");
                go.name = d.nombre;
                go.transform.position = new Vector3(d.x, d.y, 0f);
                go.transform.localScale = new Vector3(d.ancho, d.alto, 1f);
            }
        }

        private static void ConstruirSemillas(Transform padre, GameObject prefabSemilla)
        {
            // Exactamente 25 semillas a lo largo del ascenso hasta la cima del Castillo
            Vector2[] posiciones =
            {
                new Vector2(5.5f, 1.3f),
                new Vector2(11.0f, 1.3f),
                new Vector2(16.0f, 2.2f),
                new Vector2(18.5f, 2.2f),
                new Vector2(24.0f, 3.1f),
                new Vector2(27.0f, 3.1f),
                new Vector2(30.0f, 3.1f),
                new Vector2(37.0f, 3.8f),
                new Vector2(41.0f, 3.8f),
                new Vector2(48.0f, 3.4f),
                new Vector2(51.5f, 3.4f),
                new Vector2(55.0f, 3.4f),
                new Vector2(58.5f, 3.4f),
                new Vector2(64.5f, 4.6f),
                new Vector2(72.0f, 3.8f),
                new Vector2(75.5f, 3.8f),
                new Vector2(79.0f, 3.8f),
                new Vector2(82.0f, 3.8f),
                new Vector2(88.5f, 4.1f),
                new Vector2(96.0f, 3.8f),
                new Vector2(101.0f, 5.8f),
                new Vector2(106.5f, 4.0f),
                new Vector2(112.0f, 5.8f),
                new Vector2(116.0f, 3.8f),
                new Vector2(118.5f, 3.8f)
            };

            for (int i = 0; i < posiciones.Length; i++)
            {
                GameObject s = (GameObject)PrefabUtility.InstantiatePrefab(prefabSemilla, padre);
                Undo.RegisterCreatedObjectUndo(s, $"Crear Semilla {i + 1}");
                s.name = $"SemillaDeLuz_{i + 1}";
                s.transform.position = new Vector3(posiciones[i].x, posiciones[i].y, 0f);
            }
        }

        private static void ConstruirPersecucionFinal(Transform padre, GameObject prefabSombra)
        {
            var datos = new (string nombre, float x, float y, float minX, float maxX, float vel)[]
            {
                ("SombraEspinosa_Persecucion_1", 28.0f, 2.8f, 23.0f, 31.0f, 2.5f),
                ("SombraEspinosa_Persecucion_2", 53.0f, 3.1f, 46.5f, 59.5f, 2.8f),
                ("SombraEspinosa_Persecucion_3", 76.5f, 3.5f, 70.5f, 82.5f, 3.0f)
            };

            foreach (var d in datos)
            {
                GameObject s = (GameObject)PrefabUtility.InstantiatePrefab(prefabSombra, padre);
                Undo.RegisterCreatedObjectUndo(s, $"Crear {d.nombre}");
                s.name = d.nombre;
                s.transform.position = new Vector3(d.x, d.y, 0f);

                PatrullaSombra patrulla = Undo.AddComponent<PatrullaSombra>(s);
                patrulla.ConfigurarRango(d.minX, d.maxX, d.vel);
            }
        }

        private static void ConstruirCheckpoints(Transform padre, GameObject prefabCheckpoint)
        {
            var datos = new (string nombre, Vector3 pos)[]
            {
                ("PuntoDeControl_1_Base",     new Vector3(3.5f, 0.8f, 0f)),
                ("PuntoDeControl_2_Mirador",  new Vector3(39.0f, 3.4f, 0f)),
                ("PuntoDeControl_3_Antesala", new Vector3(89.5f, 3.7f, 0f))
            };

            foreach (var d in datos)
            {
                GameObject cp = (GameObject)PrefabUtility.InstantiatePrefab(prefabCheckpoint, padre);
                Undo.RegisterCreatedObjectUndo(cp, $"Crear {d.nombre}");
                cp.name = d.nombre;
                cp.transform.position = d.pos;
            }
        }

        private static GranSombraNahual ConstruirMiniJefeNahual(Transform padre)
        {
            GameObject jefeGo = new GameObject("GranSombraNahual");
            Undo.RegisterCreatedObjectUndo(jefeGo, "Crear Gran Sombra de Nahual");
            jefeGo.transform.SetParent(padre, false);
            jefeGo.transform.position = new Vector3(107.0f, 3.7f, 0f);
            jefeGo.transform.localScale = new Vector3(2.2f, 2.2f, 1f);

            SpriteRenderer sr = jefeGo.AddComponent<SpriteRenderer>();
            Sprite sPatrulla = CargarSpriteSheet("Assets/Sprites/SombraEspinosa_spritesheet.png", "SombraEspinosa_spritesheet_0");
            Sprite sPersecucion = CargarSpriteSheet("Assets/Sprites/SombraEspinosa_spritesheet.png", "SombraEspinosa_spritesheet_1");
            sr.sprite = sPatrulla;
            sr.color = new Color(0.95f, 0.78f, 1.0f, 1f);
            sr.sortingOrder = 2;

            Rigidbody2D rb = jefeGo.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;

            // Cuerpo inferior con tag "Enemigo" para dañar al jugador por contacto lateral
            GameObject cuerpoDanio = new GameObject("CuerpoDanio");
            Undo.RegisterCreatedObjectUndo(cuerpoDanio, "Crear CuerpoDanio Jefe");
            cuerpoDanio.transform.SetParent(jefeGo.transform, false);
            cuerpoDanio.transform.localPosition = new Vector3(0f, -0.15f, 0f);
            cuerpoDanio.tag = "Enemigo";
            BoxCollider2D colDanio = cuerpoDanio.AddComponent<BoxCollider2D>();
            colDanio.isTrigger = true;
            colDanio.size = new Vector2(0.85f, 0.75f);

            // Punto débil superior para vencer al mini-jefe saltando encima
            GameObject cabezaDebil = new GameObject("CabezaPuntoDebil");
            Undo.RegisterCreatedObjectUndo(cabezaDebil, "Crear CabezaPuntoDebil Jefe");
            cabezaDebil.transform.SetParent(jefeGo.transform, false);
            cabezaDebil.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            BoxCollider2D colCabeza = cabezaDebil.AddComponent<BoxCollider2D>();
            colCabeza.isTrigger = true;
            colCabeza.size = new Vector2(0.9f, 0.35f);

            GranSombraNahual jefe = jefeGo.AddComponent<GranSombraNahual>();
            jefe.ConfigurarArena(96.5f, 115.5f);
            jefe.ColliderCuerpoDanio = colDanio;
            jefe.ColliderCabezaDebil = colCabeza;
            jefe.SpritePatrulla = sPatrulla;
            jefe.SpritePersecucion = sPersecucion;
            jefe.SpriteAtaque = sPersecucion;
            jefe.ClipAlerta = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sombra.wav");
            jefe.ClipDano = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/dano.wav");

            // UI World Space del Mini-Jefe
            CrearUIMiniJefe(jefeGo.transform, jefe);

            return jefe;
        }

        private static void CrearUIMiniJefe(Transform padreJefe, GranSombraNahual jefe)
        {
            GameObject goCanvas = new GameObject("CanvasJefe", typeof(RectTransform), typeof(Canvas));
            Undo.RegisterCreatedObjectUndo(goCanvas, "Crear CanvasJefe");
            goCanvas.transform.SetParent(padreJefe, false);
            goCanvas.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            goCanvas.transform.localScale = new Vector3(0.01f, 0.01f, 1f);

            Canvas c = goCanvas.GetComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            c.sortingOrder = 15;

            GameObject goNombre = new GameObject("NombreJefe", typeof(RectTransform), typeof(TextMeshProUGUI));
            goNombre.transform.SetParent(goCanvas.transform, false);
            RectTransform rtNombre = goNombre.GetComponent<RectTransform>();
            rtNombre.anchoredPosition = new Vector2(0f, 24f);
            rtNombre.sizeDelta = new Vector2(260f, 30f);

            TextMeshProUGUI tmp = goNombre.GetComponent<TextMeshProUGUI>();
            tmp.text = "Gran Nahual de la Niebla (5/5)";
            tmp.fontSize = 18;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.85f, 0.5f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;

            GameObject goSlider = new GameObject("BarraVida", typeof(RectTransform), typeof(Slider));
            goSlider.transform.SetParent(goCanvas.transform, false);
            RectTransform rtSlider = goSlider.GetComponent<RectTransform>();
            rtSlider.anchoredPosition = new Vector2(0f, 0f);
            rtSlider.sizeDelta = new Vector2(180f, 14f);

            GameObject bg = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(goSlider.transform, false);
            RectTransform rtBg = bg.GetComponent<RectTransform>();
            rtBg.anchorMin = Vector2.zero;
            rtBg.anchorMax = Vector2.one;
            rtBg.offsetMin = Vector2.zero;
            rtBg.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.1f, 0.05f, 0.15f, 0.9f);

            GameObject fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.transform.SetParent(goSlider.transform, false);
            RectTransform rtFA = fillArea.GetComponent<RectTransform>();
            rtFA.anchorMin = Vector2.zero;
            rtFA.anchorMax = Vector2.one;
            rtFA.offsetMin = Vector2.zero;
            rtFA.offsetMax = Vector2.zero;

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform rtFill = fill.GetComponent<RectTransform>();
            rtFill.anchorMin = Vector2.zero;
            rtFill.anchorMax = Vector2.one;
            rtFill.offsetMin = Vector2.zero;
            rtFill.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().color = new Color(0.85f, 0.25f, 0.45f, 1f);

            Slider slider = goSlider.GetComponent<Slider>();
            slider.fillRect = rtFill;
            slider.maxValue = 5;
            slider.value = 5;

            jefe.BarraVida = slider;
            jefe.TextoNombreJefe = tmp;
        }

        private static Sprite CargarSpriteSheet(string ruta, string nombre)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(ruta);
            foreach (Object a in assets)
            {
                if (a is Sprite s && s.name == nombre) return s;
            }
            return null;
        }

        private static void ConstruirBuho(Transform padre, GameObject prefabBuho)
        {
            if (prefabBuho == null) return;
            GameObject buho = (GameObject)PrefabUtility.InstantiatePrefab(prefabBuho, padre);
            Undo.RegisterCreatedObjectUndo(buho, "Crear BuhoSabio Nivel 4");
            buho.name = "BuhoSabio";
            buho.transform.position = new Vector3(88.0f, 3.9f, 0f);
        }

        private static void ConfigurarTextoBuhoNivel4()
        {
            BuhoConsejo consejo = Object.FindAnyObjectByType<BuhoConsejo>();
            if (consejo != null)
            {
                Undo.RecordObject(consejo, "Configurar consejo Búho Nivel 4");
                consejo.MensajeConsejo = "¡Nahui! Salta 5 veces sobre la cabeza del Gran Nahual de la Niebla para liberar el Cerro del Chapulín.";
                if (consejo.TextoDialogo != null)
                {
                    consejo.TextoDialogo.text = consejo.MensajeConsejo;
                }
                EditorUtility.SetDirty(consejo);
            }
        }

        private static void ConstruirLimites(Transform padre)
        {
            CrearPared(padre, "ParedInvisible_Izquierda", new Vector3(-1.5f, 5f, 0f));
            CrearPared(padre, "ParedInvisible_Derecha", new Vector3(124.5f, 5f, 0f));
        }

        private static void CrearPared(Transform padre, string nombre, Vector3 pos)
        {
            GameObject p = new GameObject(nombre);
            Undo.RegisterCreatedObjectUndo(p, $"Crear {nombre}");
            p.transform.SetParent(padre, false);
            p.transform.position = pos;
            BoxCollider2D col = p.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 24f);
        }
    }
}
