using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using SemillasDelBosque.Editor;
using SemillasDelBosque.EditorScripts;
using SemillasDelBosque.Entorno;
using SemillasDelBosque.Nivel3;

namespace SemillasDelBosque.EditorTools
{
    /// <summary>
    /// ConstructorNivel2: Crea y construye idempotente y con Undo el Nivel 2
    /// ("Bosque Profundo - Calzada de los Poetas"):
    /// 18 semillas, 2 sombras patrullando, 3 checkpoints, plataformas elevadas de sigilo,
    /// Power-Up de Doble Salto, Búho Sabio, ZonaCaida en Y = -9 y Meta hacia Nivel 3.
    /// </summary>
    public static class ConstructorNivel2
    {
        private const string NOMBRE_CONTENEDOR = "--- NIVEL 2 ---";

        [MenuItem("Semillas del Bosque/2a. Crear Escena Nivel 2 (Bosque Profundo)", false, 20)]
        public static void CrearEscenaNivel2()
        {
            UtilidadesEditorNiveles.RegistrarTagsJugador();
            UtilidadesEditorNiveles.DuplicarEscenaBase(
                UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL2,
                "ConstructorNivel2");
        }

        [MenuItem("Semillas del Bosque/2b. Construir Nivel 2 (Bosque Profundo)", false, 21)]
        public static void ConstruirNivel2()
        {
            if (!File.Exists(UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL2))
            {
                CrearEscenaNivel2();
            }

            Scene escena = SceneManager.GetActiveScene();
            if (escena.path != UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL2)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                escena = EditorSceneManager.OpenScene(UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL2, OpenSceneMode.Single);
            }

            UtilidadesEditorNiveles.RegistrarTagsJugador();

            GameObject prefabSuelo = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BloqueSuelo.prefab");
            GameObject prefabSemilla = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SemillaDeLuz.prefab");
            GameObject prefabSombra = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SombraEspinosa.prefab");
            GameObject prefabCheckpoint = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PuntoDeControl.prefab");
            GameObject prefabPowerUp = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PowerUp.prefab");
            GameObject prefabBuho = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BuhoSabio.prefab");

            if (prefabSuelo == null || prefabSemilla == null || prefabSombra == null ||
                prefabCheckpoint == null || prefabPowerUp == null)
            {
                Debug.LogError("[ConstructorNivel2] Faltan prefabs requeridos en Assets/Prefabs/.");
                return;
            }

            GameObject raiz = UtilidadesEditorNiveles.ReiniciarContenedorNivel(NOMBRE_CONTENEDOR);

            Transform grpSuelo = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "1_SueloYPlataformasElevadas");
            Transform grpSemillas = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "2_SemillasDeLuz");
            Transform grpSombras = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "3_SombrasPatrulla");
            Transform grpCheckpoints = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "4_Checkpoints");
            Transform grpPowerUp = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "5_PowerUpDobleSalto");
            Transform grpBuho = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "6_BuhoSabio");
            Transform grpMeta = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "7_Meta");
            Transform grpLimites = UtilidadesEditorNiveles.CrearGrupo(raiz.transform, "8_Limites");

            ConstruirPlataformas(grpSuelo, prefabSuelo);
            ConstruirSemillas(grpSemillas, prefabSemilla);
            ConstruirSombras(grpSombras, prefabSombra);
            ConstruirCheckpoints(grpCheckpoints, prefabCheckpoint);
            ConstruirPowerUpDobleSalto(grpPowerUp, prefabPowerUp);
            ConstruirBuho(grpBuho, prefabBuho);
            ConstruirLimites(grpLimites);

            UtilidadesEditorNiveles.AsegurarZonaCaida(grpLimites, 60f, 220f);
            UtilidadesEditorNiveles.ConfigurarMetaNivel(
                grpMeta,
                new Vector3(114f, 2.0f, 0f),
                nivelActual: 2,
                nombreSiguienteEscena: "Lago de las Luciernagas",
                esNivelFinal: false);

            UtilidadesEditorNiveles.PosicionarJugadorEInicializarVariables(new Vector3(2f, 1.0f, 0f), false);

            ConstructorEntorno.ConstruirEntornoNivel();
            ConfigurarAudioSombras.EjecutarConfiguracion();
            BuhoConsejo.MenuConfigurarBuho();
            ConfigurarTextoBuhoNivel2();
            UtilidadesEditorNiveles.ConfigurarHUDYAudioUI();
            ConfiguradorAnimaciones.AplicarEnEscenaActiva();
            UtilidadesEditorNiveles.AgregarEscenaABuildSettings(UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL2);

            EditorSceneManager.MarkSceneDirty(escena);
            Debug.Log("[ConstructorNivel2] Nivel 2 construido: 18 semillas, 2 sombras, 3 checkpoints, plataformas elevadas de sigilo, Power-Up Doble Salto y Meta -> Nivel 3.");
        }

        private static void ConstruirPlataformas(Transform padre, GameObject prefabSuelo)
        {
            var datos = new (string nombre, float x, float y, float ancho, float alto)[]
            {
                // Tramo inicial y PowerUp
                ("Suelo_Inicio",                6.0f,  -0.5f, 14.0f, 1.0f),
                ("Plataforma_Escalon_1",       16.5f,   0.8f,  5.0f, 0.8f),
                ("Plataforma_PowerUp",         25.0f,   1.6f,  7.0f, 0.8f),

                // Zona Sigilo 1: Suelo bajo con Sombra 1 + Ruta elevada de copas
                ("Suelo_Calzada_Sombra1",      41.0f,  -0.5f, 22.0f, 1.0f),
                ("Plataforma_Sigilo_Acceso1",  31.2f,   2.4f,  3.6f, 0.6f),
                ("Plataforma_Sigilo_Alta1A",   36.5f,   3.2f,  4.5f, 0.6f),
                ("Plataforma_Sigilo_Alta1B",   43.0f,   4.2f,  5.0f, 0.6f),
                ("Plataforma_Sigilo_Alta1C",   49.5f,   3.2f,  4.5f, 0.6f),

                // Zona intermedia (Checkpoint 2)
                ("Suelo_ClaroPoetas",          59.5f,   0.2f, 11.0f, 1.0f),

                // Zona Sigilo 2: Suelo con Sombra 2 + Pasarela elevada de doble salto
                ("Suelo_Calzada_Sombra2",      79.0f,  -0.5f, 24.0f, 1.0f),
                ("Plataforma_Sigilo_Acceso2",  68.0f,   2.2f,  3.6f, 0.6f),
                ("Plataforma_Sigilo_Alta2A",   73.0f,   3.0f,  4.5f, 0.6f),
                ("Plataforma_Sigilo_Alta2B",   80.0f,   4.3f,  5.5f, 0.6f),
                ("Plataforma_Sigilo_Alta2C",   87.0f,   3.2f,  4.5f, 0.6f),

                // Tramo final hacia la Meta
                ("Plataforma_PuenteFinal",     96.5f,   1.2f,  8.0f, 0.8f),
                ("Suelo_MetaPoetas",          110.0f,   0.0f, 15.0f, 1.0f)
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
            // Exactamente 18 semillas distribuidas entre rutas bajas y elevadas
            Vector2[] posiciones =
            {
                new Vector2(6.0f, 1.3f),
                new Vector2(10.0f, 1.3f),
                new Vector2(16.5f, 2.4f),
                new Vector2(23.5f, 3.2f),
                new Vector2(26.5f, 3.2f),
                new Vector2(36.0f, 4.7f),
                new Vector2(41.5f, 5.9f),
                new Vector2(44.5f, 5.9f),
                new Vector2(50.0f, 4.8f),
                new Vector2(42.0f, 1.2f),
                new Vector2(59.5f, 1.8f),
                new Vector2(72.5f, 4.5f),
                new Vector2(78.5f, 6.0f),
                new Vector2(81.5f, 6.0f),
                new Vector2(87.5f, 4.7f),
                new Vector2(80.0f, 1.2f),
                new Vector2(96.5f, 2.8f),
                new Vector2(108.0f, 1.6f)
            };

            for (int i = 0; i < posiciones.Length; i++)
            {
                GameObject s = (GameObject)PrefabUtility.InstantiatePrefab(prefabSemilla, padre);
                Undo.RegisterCreatedObjectUndo(s, $"Crear Semilla {i + 1}");
                s.name = $"SemillaDeLuz_{i + 1}";
                s.transform.position = new Vector3(posiciones[i].x, posiciones[i].y, 0f);
            }
        }

        private static void ConstruirSombras(Transform padre, GameObject prefabSombra)
        {
            var datos = new (string nombre, float x, float y, float minX, float maxX, float vel)[]
            {
                ("SombraEspinosa_1_Poetas",  42.0f, 0.75f, 34.5f, 49.5f, 2.0f),
                ("SombraEspinosa_2_Calzada", 80.0f, 0.75f, 71.5f, 88.5f, 2.3f)
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
                ("PuntoDeControl_1", new Vector3(3.5f, 0.8f, 0f)),
                ("PuntoDeControl_2", new Vector3(59.0f, 1.4f, 0f)),
                ("PuntoDeControl_3", new Vector3(104.5f, 1.2f, 0f))
            };

            foreach (var d in datos)
            {
                GameObject cp = (GameObject)PrefabUtility.InstantiatePrefab(prefabCheckpoint, padre);
                Undo.RegisterCreatedObjectUndo(cp, $"Crear {d.nombre}");
                cp.name = d.nombre;
                cp.transform.position = d.pos;
            }
        }

        private static void ConstruirPowerUpDobleSalto(Transform padre, GameObject prefabPowerUp)
        {
            GameObject pu = (GameObject)PrefabUtility.InstantiatePrefab(prefabPowerUp, padre);
            Undo.RegisterCreatedObjectUndo(pu, "Crear PowerUp Doble Salto");
            pu.name = "PowerUp_DobleSalto";
            pu.transform.position = new Vector3(25.0f, 2.8f, 0f);
        }

        private static void ConstruirBuho(Transform padre, GameObject prefabBuho)
        {
            if (prefabBuho == null) return;
            GameObject buho = (GameObject)PrefabUtility.InstantiatePrefab(prefabBuho, padre);
            Undo.RegisterCreatedObjectUndo(buho, "Crear BuhoSabio Nivel 2");
            buho.name = "BuhoSabio";
            buho.transform.position = new Vector3(20.5f, 1.8f, 0f);
        }

        private static void ConfigurarTextoBuhoNivel2()
        {
            BuhoConsejo consejo = Object.FindAnyObjectByType<BuhoConsejo>();
            if (consejo != null)
            {
                Undo.RecordObject(consejo, "Configurar consejo Búho Nivel 2");
                consejo.MensajeConsejo = "«No para siempre en la tierra...» Toma el aliento de Ehécatl para usar el Doble Salto por las copas.";
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
            CrearPared(padre, "ParedInvisible_Derecha", new Vector3(118.5f, 5f, 0f));
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
