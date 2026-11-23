using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using SemillasDelBosque.Editor;
using SemillasDelBosque.EditorScripts;
using SemillasDelBosque.Nivel3;
using System.Collections.Generic;

namespace SemillasDelBosque.EditorTools
{
    /// <summary>
    /// Herramienta de Editor que construye proceduralmente el Nivel 3 ("Lago de las Luciérnagas").
    /// Genera islas flotantes, agua oscura con daño, 6 plataformas móviles, 22 semillas de luz,
    /// 3 sombras patrullando, 3 puntos de control, la meta y efectos ambientales de luciérnagas.
    /// Es completamente idempotente: si ya existe el nivel, lo elimina y lo reconstruye con soporte Undo.
    /// </summary>
    public static class ConstructorNivel3
    {
        private const string NOMBRE_CONTENEDOR_NIVEL = "--- NIVEL 3 ---";
        private const string RUTA_PREFAB_SUELO = "Assets/Prefabs/BloqueSuelo.prefab";
        private const string RUTA_PREFAB_SEMILLA = "Assets/Prefabs/SemillaDeLuz.prefab";
        private const string RUTA_PREFAB_SOMBRA = "Assets/Prefabs/SombraEspinosa.prefab";
        private const string RUTA_PREFAB_CHECKPOINT = "Assets/Prefabs/PuntoDeControl.prefab";

        // Parámetro de diseño: altura de salto accesible para Luna (normal y doble salto)
        public const float ALTURA_MAXIMA_SALTO = 2.8f;

        [MenuItem("Semillas del Bosque/3b. Construir Nivel 3 (Lago de las Luciernagas)", false, 31)]
        public static void ConstruirNivel()
        {
            Scene escenaActiva = SceneManager.GetActiveScene();
            if (escenaActiva.path != UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL3 &&
                System.IO.File.Exists(UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL3))
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                escenaActiva = EditorSceneManager.OpenScene(UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL3, OpenSceneMode.Single);
            }

            UtilidadesEditorNiveles.RegistrarTagsJugador();
            Debug.Log($"[ConstructorNivel3] Iniciando construcción en la escena: {escenaActiva.name}");

            // 1. Validar prefabs y recursos necesarios
            GameObject prefabSuelo = AssetDatabase.LoadAssetAtPath<GameObject>(RUTA_PREFAB_SUELO);
            GameObject prefabSemilla = AssetDatabase.LoadAssetAtPath<GameObject>(RUTA_PREFAB_SEMILLA);
            GameObject prefabSombra = AssetDatabase.LoadAssetAtPath<GameObject>(RUTA_PREFAB_SOMBRA);
            GameObject prefabCheckpoint = AssetDatabase.LoadAssetAtPath<GameObject>(RUTA_PREFAB_CHECKPOINT);

            if (prefabSuelo == null || prefabSemilla == null || prefabSombra == null || prefabCheckpoint == null)
            {
                Debug.LogError("[ConstructorNivel3] Faltan prefabs en Assets/Prefabs. Verifica que BloqueSuelo, SemillaDeLuz, SombraEspinosa y PuntoDeControl existan.");
                return;
            }

            // 2. Idempotencia: Si ya existe el contenedor raíz del Nivel 3, lo eliminamos con Undo
            GameObject nivelExistente = GameObject.Find(NOMBRE_CONTENEDOR_NIVEL);
            if (nivelExistente != null)
            {
                Debug.Log("[ConstructorNivel3] Eliminando versión anterior de '--- NIVEL 3 ---'...");
                Undo.DestroyObjectImmediate(nivelExistente);
            }

            // Si existen paredes viejas heredadas del Nivel 1 en la raíz, las limpiamos para dar paso a los nuevos límites
            EliminarObjetoRaizSiExiste("ParedIzquierda");
            EliminarObjetoRaizSiExiste("ParedDerecha");

            // 3. Crear contenedor principal del Nivel 3
            GameObject raizNivel = new GameObject(NOMBRE_CONTENEDOR_NIVEL);
            Undo.RegisterCreatedObjectUndo(raizNivel, "Construir Nivel 3");

            // Crear carpetas organizadoras dentro de la jerarquía
            Transform grupoIslotes = CrearGrupo(raizNivel.transform, "1_IslotesFijos");
            Transform grupoAgua = CrearGrupo(raizNivel.transform, "2_AguaOscura");
            Transform grupoPlataformas = CrearGrupo(raizNivel.transform, "3_PlataformasMoviles");
            Transform grupoSemillas = CrearGrupo(raizNivel.transform, "4_SemillasDeLuz");
            Transform grupoEnemigos = CrearGrupo(raizNivel.transform, "5_EnemigosSombra");
            Transform grupoCheckpoints = CrearGrupo(raizNivel.transform, "6_PuntosDeControl");
            Transform grupoMeta = CrearGrupo(raizNivel.transform, "7_MetaFinal");
            Transform grupoLimites = CrearGrupo(raizNivel.transform, "8_LimitesDelMapa");
            Transform grupoDecoracion = CrearGrupo(raizNivel.transform, "9_DecoracionLuciernagas");

            // 4. Construir islotes de suelo fijos a lo largo de 120 unidades
            ConstruirIslotesFijos(grupoIslotes, prefabSuelo);

            // 5. Construir el agua oscura (trigger mortal con tag 'Enemigo')
            ConstruirAguaOscura(grupoAgua);

            // 6. Construir las 6 plataformas móviles
            ConstruirPlataformasMoviles(grupoPlataformas, prefabSuelo);

            // 7. Colocar las 22 semillas de luz
            ConstruirSemillasDeLuz(grupoSemillas, prefabSemilla);

            // 8. Colocar las 3 Sombras Espinosas patrullando
            ConstruirSombrasPatrulleras(grupoEnemigos, prefabSombra);

            // 9. Colocar los 3 Puntos de Control (inicio, mitad, antes del tramo final)
            ConstruirPuntosDeControl(grupoCheckpoints, prefabCheckpoint);

            // 10. Colocar la Meta al final
            ConstruirMeta(grupoMeta);

            // 11. Paredes invisibles para delimitar el mapa
            ConstruirLimitesMapa(grupoLimites);

            // 12. Efecto decorativo de luciérnagas ambientales
            ConstruirLuciernagas(grupoDecoracion);

            // 13. Posicionar a Luna en el inicio del nivel (sobre el primer islote)
            PosicionarJugador();

            // 14. Asegurar ZonaCaida en Y = -9, entorno, audio, HUD y Build Settings
            UtilidadesEditorNiveles.AsegurarZonaCaida(grupoLimites, 60f, 200f);
            ConstructorEntorno.ConstruirEntornoNivel();
            ConfigurarAudioSombras.EjecutarConfiguracion();
            UtilidadesEditorNiveles.ConfigurarHUDYAudioUI();
            ConfiguradorAnimaciones.AplicarEnEscenaActiva();
            UtilidadesEditorNiveles.AgregarEscenaABuildSettings(UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL3);

            // 15. Guardar cambios y marcar la escena como modificada
            EditorSceneManager.MarkSceneDirty(escenaActiva);
            Debug.Log("[ConstructorNivel3] ¡Construcción del Nivel 3 finalizada exitosamente!");
        }

        private static Transform CrearGrupo(Transform padre, string nombre)
        {
            GameObject grupo = new GameObject(nombre);
            grupo.transform.SetParent(padre);
            grupo.transform.localPosition = Vector3.zero;
            return grupo.transform;
        }

        private static void EliminarObjetoRaizSiExiste(string nombre)
        {
            GameObject obj = GameObject.Find(nombre);
            if (obj != null && obj.transform.parent == null)
            {
                Undo.DestroyObjectImmediate(obj);
            }
        }

        private static void ConstruirIslotesFijos(Transform padre, GameObject prefabSuelo)
        {
            // Definición de islotes: (nombre, posX, posY, ancho, alto)
            var datosIslotes = new (string nombre, float x, float y, float ancho, float alto)[]
            {
                ("Islote_1_Inicio",      5.0f,   -0.5f, 10.0f, 1.0f),  // Superficie y = 0.0, cubre x=[0, 10]
                ("Islote_2_Descanso",    20.5f,  -0.5f, 7.0f,  1.0f),  // Superficie y = 0.0, cubre x=[17, 24]
                ("Islote_3_Escalon",     35.0f,   0.2f, 6.0f,  1.0f),  // Superficie y = 0.7, cubre x=[32, 38]
                ("Islote_4_Campamento",  52.5f,   0.0f, 15.0f, 1.0f),  // Superficie y = 0.5, cubre x=[45, 60] (con Sombra y Checkpoint)
                ("Islote_5_Pilar",       71.5f,  -0.3f, 5.0f,  1.0f),  // Superficie y = 0.2, cubre x=[69, 74]
                ("Islote_6_Ruinas",      89.5f,   0.0f, 13.0f, 1.0f),  // Superficie y = 0.5, cubre x=[83, 96] (con Sombra y Checkpoint)
                ("Islote_7_Santuario",   113.5f,  0.0f, 13.0f, 1.0f)   // Superficie y = 0.5, cubre x=[107, 120] (con Sombra y Meta)
            };

            foreach (var dato in datosIslotes)
            {
                GameObject islote = (GameObject)PrefabUtility.InstantiatePrefab(prefabSuelo, padre);
                islote.name = dato.nombre;
                islote.transform.position = new Vector3(dato.x, dato.y, 0f);
                islote.transform.localScale = new Vector3(dato.ancho, dato.alto, 1f);
            }
        }

        private static void ConstruirAguaOscura(Transform padre)
        {
            // Agua oscura debajo de todo el mapa. Cubre desde x = -5 hasta x = 125 (Y = -4.0 a Y = -1.0)
            // IMPORTANTE: localScale = (1, 1, 1) para que BoxCollider2D.size NO se multiplique y suba por encima de los islotes.
            GameObject agua = new GameObject("AguaOscura_LagoMortal");
            Undo.RegisterCreatedObjectUndo(agua, "Crear AguaOscura_LagoMortal");
            agua.transform.SetParent(padre, false);
            agua.transform.position = new Vector3(60.0f, -2.5f, 0f);
            agua.transform.localScale = Vector3.one;
            agua.tag = "Enemigo"; // El grafo de daño de Luna detecta el tag 'Enemigo'

            // Collider trigger exacto de 130x3 (borde superior en Y = -1.0, debajo de todos los islotes en Y >= -0.3)
            BoxCollider2D col = agua.AddComponent<BoxCollider2D>();
            col.size = new Vector2(130.0f, 3.0f);
            col.isTrigger = true;

            // Visual en objeto hijo sin collider para escalar libremente sin alterar el BoxCollider2D
            GameObject visual = new GameObject("VisualAgua");
            Undo.RegisterCreatedObjectUndo(visual, "Crear VisualAgua");
            visual.transform.SetParent(agua.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(130.0f, 3.0f, 1.0f);

            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            sr.color = new Color(0.04f, 0.09f, 0.18f, 0.92f);
            sr.sortingOrder = 5;
        }

        private static void ConstruirPlataformasMoviles(Transform padre, GameObject prefabSuelo)
        {
            // 6 plataformas móviles: distintas velocidades, pausas y trayectorias
            var datos = new (string nombre, Vector2 inicio, Vector2 fin, float vel, float pausa, Vector2 escala)[]
            {
                // 1. Fácil: horizontal lenta para aprender la mecánica
                ("PlataformaMovil_1_HorizontalSuave",  new Vector2(11.5f, 0.5f), new Vector2(15.5f, 0.5f), 1.8f, 0.5f, new Vector2(2.5f, 0.4f)),
                // 2. Fácil-Media: horizontal sobre el segundo tramo de agua
                ("PlataformaMovil_2_HorizontalMedia",  new Vector2(25.5f, 0.6f), new Vector2(29.5f, 0.6f), 2.2f, 0.5f, new Vector2(2.5f, 0.4f)),
                // 3. Ascensor vertical: introduce timing de altura
                ("PlataformaMovil_3_Ascensor1",        new Vector2(41.5f, 0.8f), new Vector2(41.5f, 2.4f), 2.0f, 0.6f, new Vector2(2.5f, 0.4f)),
                // 4. Horizontal rápida: cruce largo sobre agua amplia
                ("PlataformaMovil_4_HorizontalCruce",  new Vector2(62.0f, 0.5f), new Vector2(66.5f, 0.5f), 2.5f, 0.4f, new Vector2(2.5f, 0.4f)),
                // 5. Elevador vertical alto: salto preciso al Islote 6
                ("PlataformaMovil_5_Ascensor2",        new Vector2(78.5f, 0.5f), new Vector2(78.5f, 2.6f), 2.2f, 0.5f, new Vector2(2.5f, 0.4f)),
                // 6. Tramo final exigente: velocidad mayor sobre el lago profundo
                ("PlataformaMovil_6_FinalExigente",     new Vector2(98.0f, 0.6f), new Vector2(104.0f, 0.6f), 3.0f, 0.3f, new Vector2(2.5f, 0.4f))
            };

            foreach (var d in datos)
            {
                GameObject plat = (GameObject)PrefabUtility.InstantiatePrefab(prefabSuelo, padre);
                plat.name = d.nombre;
                plat.transform.position = d.inicio;
                plat.transform.localScale = new Vector3(d.escala.x, d.escala.y, 1f);

                // Configurar PlataformaMovil
                PlataformaMovil movil = plat.AddComponent<PlataformaMovil>();
                movil.ConfigurarPuntos(d.inicio, d.fin, d.vel, d.pausa);

                // Configurar PlataformaPasajero para que Luna no se deslice
                plat.AddComponent<PlataformaPasajero>();
            }
        }

        private static void ConstruirSemillasDeLuz(Transform padre, GameObject prefabSemilla)
        {
            // 22 semillas de luz colocadas en arcos sobre plataformas y en zonas de riesgo
            var posiciones = new Vector2[]
            {
                new Vector2(6.0f, 1.6f),    // 1: Islote 1 (tutorial)
                new Vector2(8.5f, 1.6f),    // 2: Islote 1 (borde)
                new Vector2(11.5f, 2.3f),   // 3: Arco sobre Plat Móvil 1 (inicio)
                new Vector2(13.5f, 3.2f),   // 4: Arco sobre Plat Móvil 1 (cúspide)
                new Vector2(15.5f, 2.3f),   // 5: Arco sobre Plat Móvil 1 (bajada)
                new Vector2(19.0f, 1.6f),   // 6: Islote 2
                new Vector2(22.0f, 1.6f),   // 7: Islote 2
                new Vector2(25.5f, 2.4f),   // 8: Arco sobre Plat Móvil 2 (inicio)
                new Vector2(27.5f, 3.3f),   // 9: Arco sobre Plat Móvil 2 (cúspide)
                new Vector2(29.5f, 2.4f),   // 10: Arco sobre Plat Móvil 2 (bajada)
                new Vector2(34.0f, 2.3f),   // 11: Islote 3
                new Vector2(36.5f, 2.3f),   // 12: Islote 3
                new Vector2(41.5f, 4.2f),   // 13: En el aire sobre Plat Vertical 3 (riesgo)
                new Vector2(51.5f, 2.4f),   // 14: Islote 4 (sobre patrulla de Sombra 1)
                new Vector2(54.5f, 2.4f),   // 15: Islote 4 (sobre patrulla de Sombra 1)
                new Vector2(63.0f, 2.2f),   // 16: Plat Móvil 4
                new Vector2(65.5f, 2.2f),   // 17: Plat Móvil 4
                new Vector2(71.5f, 1.8f),   // 18: Islote 5
                new Vector2(78.5f, 4.4f),   // 19: Gran salto vertical sobre Plat 5 (riesgo)
                new Vector2(87.5f, 2.4f),   // 20: Islote 6 (sobre patrulla de Sombra 2)
                new Vector2(101.0f, 2.5f),  // 21: Plat Móvil 6 (tramo final exigente)
                new Vector2(111.0f, 2.4f)   // 22: Islote 7 (guardián previo a la Meta)
            };

            for (int i = 0; i < posiciones.Length; i++)
            {
                GameObject semilla = (GameObject)PrefabUtility.InstantiatePrefab(prefabSemilla, padre);
                semilla.name = $"SemillaDeLuz_{i + 1}";
                semilla.transform.position = new Vector3(posiciones[i].x, posiciones[i].y, 0f);
            }
        }

        private static void ConstruirSombrasPatrulleras(Transform padre, GameObject prefabSombra)
        {
            // 3 sombras patrullando exclusivamente islotes fijos largos
            var datos = new (string nombre, float posX, float posY, float minX, float maxX, float vel)[]
            {
                ("SombraEspinosa_1_Campamento", 53.0f, 1.25f, 50.0f, 57.0f, 1.8f),
                ("SombraEspinosa_2_Ruinas",     87.5f, 1.25f, 85.0f, 91.0f, 2.0f),
                ("SombraEspinosa_3_Guardian",   111.0f, 1.25f, 108.5f, 113.5f, 2.2f)
            };

            foreach (var d in datos)
            {
                GameObject sombra = (GameObject)PrefabUtility.InstantiatePrefab(prefabSombra, padre);
                sombra.name = d.nombre;
                sombra.transform.position = new Vector3(d.posX, d.posY, 0f);

                PatrullaSombra patrulla = sombra.AddComponent<PatrullaSombra>();
                patrulla.ConfigurarRango(d.minX, d.maxX, d.vel);
            }
        }

        private static void ConstruirPuntosDeControl(Transform padre, GameObject prefabCheckpoint)
        {
            // 3 puntos de control: inicio (Islote 1), mitad (Islote 4) y antes del tramo final (Islote 6)
            var datos = new (string nombre, Vector3 pos)[]
            {
                ("PuntoDeControl_1_Inicio",         new Vector3(3.5f, 0.5f, 0f)),
                ("PuntoDeControl_2_Mitad",          new Vector3(47.0f, 1.0f, 0f)),
                ("PuntoDeControl_3_AntesTramoFinal", new Vector3(94.0f, 1.0f, 0f))
            };

            foreach (var d in datos)
            {
                GameObject cp = (GameObject)PrefabUtility.InstantiatePrefab(prefabCheckpoint, padre);
                cp.name = d.nombre;
                cp.transform.position = d.pos;
            }
        }

        private static void ConstruirMeta(Transform padre)
        {
            // Meta idempotente con GrafoMeta + ControladorMeta hacia Nivel 4 ("Cima de los Recuerdos")
            UtilidadesEditorNiveles.ConfigurarMetaNivel(
                padre,
                new Vector3(117.5f, 2.0f, 0f),
                nivelActual: 3,
                nombreSiguienteEscena: "Cima de los Recuerdos",
                esNivelFinal: false);
        }

        private static void ConstruirLimitesMapa(Transform padre)
        {
            // Pared invisible izquierda
            GameObject paredIzq = new GameObject("ParedInvisible_Izquierda");
            paredIzq.transform.SetParent(padre);
            paredIzq.transform.position = new Vector3(-1.5f, 5.0f, 0f);
            BoxCollider2D colIzq = paredIzq.AddComponent<BoxCollider2D>();
            colIzq.size = new Vector2(1.0f, 20.0f);
            colIzq.isTrigger = false;

            // Pared invisible derecha
            GameObject paredDer = new GameObject("ParedInvisible_Derecha");
            paredDer.transform.SetParent(padre);
            paredDer.transform.position = new Vector3(121.5f, 5.0f, 0f);
            BoxCollider2D colDer = paredDer.AddComponent<BoxCollider2D>();
            colDer.size = new Vector2(1.0f, 20.0f);
            colDer.isTrigger = false;
        }

        private static void ConstruirLuciernagas(Transform padre)
        {
            GameObject luciernagas = new GameObject("SistemaLuciernagas");
            luciernagas.transform.SetParent(padre);
            luciernagas.transform.position = new Vector3(60.0f, 2.0f, 0f);

            ParticleSystem ps = luciernagas.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = 4.0f;
            main.startSpeed = 0.4f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
            main.startColor = new Color(0.85f, 1.0f, 0.45f, 0.75f); // Verde lima luminoso suave
            main.maxParticles = 80;

            var emission = ps.emission;
            emission.rateOverTime = 20f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(120.0f, 5.0f, 1.0f);

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-0.15f, 0.35f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            var renderer = luciernagas.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 10;
        }

        private static void PosicionarJugador()
        {
            UtilidadesEditorNiveles.PosicionarJugadorEInicializarVariables(new Vector3(2.0f, 0.65f, 0.0f), false);
        }
    }
}
