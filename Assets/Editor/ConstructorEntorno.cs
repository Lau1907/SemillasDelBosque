using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using SemillasDelBosque.EditorTools;
using SemillasDelBosque.Entorno;
using SemillasDelBosque.Nivel3;

namespace SemillasDelBosque.Editor
{
    /// <summary>
    /// ConstructorEntorno: Menú de editor para construir el fondo, parallax (3 capas con SpriteDrawMode.Simple
    /// y escala Y = camAlto / spriteAlto), iluminación Global Light 2D y props decorativos para los 4 niveles.
    /// </summary>
    public static class ConstructorEntorno
    {
        private struct ConfigEntornoNivel
        {
            public Color ColorCamara;
            public Color ColorLuzGlobal;
            public float IntensidadLuz;
            public string RutaFar;
            public string RutaMid;
            public string RutaNear;
            public float BaseY;
            public bool IncluirJuncos;
        }

        private static readonly ConfigEntornoNivel[] ConfigsPorNivel =
        {
            // Nivel 1: Claro del Despertar (#70A389, intensidad 0.65f)
            new ConfigEntornoNivel
            {
                ColorCamara = new Color32(0x5D, 0x7A, 0x68, 0xFF),
                ColorLuzGlobal = new Color32(0x70, 0xA3, 0x89, 0xFF), // #70A389
                IntensidadLuz = 0.65f,
                RutaFar = "Assets/Sprites/Generados/fondo_colinas_lejanas.png",
                RutaMid = "Assets/Sprites/Generados/fondo_arboles_medios.png",
                RutaNear = "Assets/Sprites/Generados/fondo_arboles_cercanos.png",
                BaseY = -3.5f,
                IncluirJuncos = false
            },
            // Nivel 2: Bosque Profundo (Verde denso boscoso con sombra suave)
            new ConfigEntornoNivel
            {
                ColorCamara = new Color32(0x26, 0x3E, 0x30, 0xFF),
                ColorLuzGlobal = new Color32(0x7B, 0xA8, 0x86, 0xFF),
                IntensidadLuz = 0.55f,
                RutaFar = "Assets/Sprites/Generados/fondo_nivel2_lejana.png",
                RutaMid = "Assets/Sprites/Generados/fondo_nivel2_media.png",
                RutaNear = "Assets/Sprites/Generados/fondo_nivel2_cercana.png",
                BaseY = -3.5f,
                IncluirJuncos = false
            },
            // Nivel 3: Lago de las Luciérnagas (Noche azul con reflejos y luciérnagas)
            new ConfigEntornoNivel
            {
                ColorCamara = new Color32(0x06, 0x0E, 0x1C, 0xFF),
                ColorLuzGlobal = new Color32(0x52, 0x74, 0xA8, 0xFF),
                IntensidadLuz = 0.60f,
                RutaFar = "Assets/Sprites/Generados/fondo_nivel3_lejana.png",
                RutaMid = "Assets/Sprites/Generados/fondo_nivel3_media.png",
                RutaNear = "Assets/Sprites/Generados/fondo_nivel3_agua.png",
                BaseY = -5.5f,
                IncluirJuncos = true
            },
            // Nivel 4: Cima de los Recuerdos (Atardecer dorado/púrpura y Castillo de Chapultepec)
            new ConfigEntornoNivel
            {
                ColorCamara = new Color32(0x4A, 0x25, 0x45, 0xFF),
                ColorLuzGlobal = new Color32(0xE8, 0xA6, 0x74, 0xFF),
                IntensidadLuz = 0.72f,
                RutaFar = "Assets/Sprites/Generados/fondo_nivel4_lejana.png",
                RutaMid = "Assets/Sprites/Generados/fondo_nivel4_media.png",
                RutaNear = "Assets/Sprites/Generados/fondo_nivel4_cercana.png",
                BaseY = -3.5f,
                IncluirJuncos = false
            }
        };

        [MenuItem("Semillas del Bosque/Entorno/Construir entorno")]
        public static void ConstruirEntornoNivel()
        {
            Scene escena = SceneManager.GetActiveScene();
            int nivel = UtilidadesEditorNiveles.DetectarNumeroNivel(escena);
            ConfigEntornoNivel cfg = ConfigsPorNivel[Mathf.Clamp(nivel - 1, 0, ConfigsPorNivel.Length - 1)];

            Debug.Log($"[ConstructorEntorno] Construyendo entorno para Nivel {nivel} ({escena.name})...");

            // Garantizar que los sprites procedurales de los 4 niveles existan antes de construir
            if (AssetDatabase.LoadAssetAtPath<Sprite>(cfg.RutaFar) == null ||
                AssetDatabase.LoadAssetAtPath<Sprite>(cfg.RutaMid) == null ||
                AssetDatabase.LoadAssetAtPath<Sprite>(cfg.RutaNear) == null)
            {
                GeneradorArte.GenerarTodosLosSprites();
            }

            ConfigurarCamara(cfg.ColorCamara);
            Light2D luzGlobal = ConfigurarLuzGlobal(cfg.ColorLuzGlobal, cfg.IntensidadLuz);

            GameObject goEntorno = GameObject.Find("--- ENTORNO ---");
            if (goEntorno != null)
            {
                Undo.DestroyObjectImmediate(goEntorno);
            }

            goEntorno = new GameObject("--- ENTORNO ---");
            Undo.RegisterCreatedObjectUndo(goEntorno, "Crear Entorno");
            goEntorno.transform.position = Vector3.zero;

            SpriteRenderer[] capasFondo = CrearCapasFondo(goEntorno, cfg);
            CrearDecoracion(goEntorno, cfg.IncluirJuncos);
            CrearAmbientacionEspecificaNivel(goEntorno, nivel);

            RestauracionBosque restauracion = Undo.AddComponent<RestauracionBosque>(goEntorno);
            restauracion.Configurar(luzGlobal, capasFondo);

            if (nivel == 1)
            {
                ConfigurarPatrullaNivel1();
            }

            EditorUtility.SetDirty(goEntorno);
            EditorSceneManager.MarkSceneDirty(escena);
            Selection.activeGameObject = goEntorno;
            Debug.Log($"[ConstructorEntorno] Entorno de Nivel {nivel} construido con 3 capas SpriteDrawMode.Simple.");
        }

        [MenuItem("Semillas del Bosque/Entorno/Configurar Patrulla Nivel 1")]
        public static void ConfigurarPatrullaNivel1()
        {
            GameObject sombra = GameObject.Find("SombraEspinosa");
            if (sombra == null)
            {
                var todos = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
                foreach (var g in todos)
                {
                    if (g.name.StartsWith("SombraEspinosa") && g.scene == SceneManager.GetActiveScene())
                    {
                        sombra = g;
                        break;
                    }
                }
            }

            if (sombra == null)
            {
                Debug.LogWarning("[ConstructorEntorno] No se encontró 'SombraEspinosa' en la escena activa.");
                return;
            }

            GameObject platLarga = GameObject.Find("Plataforma_Larga_D");
            float minX = 77.8f;
            float maxX = 88.2f;

            if (platLarga != null)
            {
                Collider2D colPlat = platLarga.GetComponent<Collider2D>();
                if (colPlat != null)
                {
                    minX = colPlat.bounds.min.x + 0.8f;
                    maxX = colPlat.bounds.max.x - 0.8f;
                }
            }

            Undo.RegisterCompleteObjectUndo(sombra, "Configurar Patrulla Sombra Nivel 1");

            PatrullaSombra patrulla = sombra.GetComponent<PatrullaSombra>();
            if (patrulla == null)
            {
                patrulla = Undo.AddComponent<PatrullaSombra>(sombra);
            }

            patrulla.ConfigurarRango(minX, maxX, 2.0f);

            AsegurarMarcadorPatrulla(sombra.transform, "PuntoPatrulla_A", minX, sombra.transform.position.y);
            AsegurarMarcadorPatrulla(sombra.transform, "PuntoPatrulla_B", maxX, sombra.transform.position.y);

            EditorUtility.SetDirty(sombra);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void AsegurarMarcadorPatrulla(Transform padre, string nombre, float x, float y)
        {
            Transform punto = padre.Find(nombre);
            if (punto == null)
            {
                GameObject go = new GameObject(nombre);
                Undo.RegisterCreatedObjectUndo(go, $"Crear {nombre}");
                go.transform.SetParent(padre, false);
                punto = go.transform;
            }
            punto.position = new Vector3(x, y, 0f);
        }

        private static void ConfigurarCamara(Color colorFondo)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Undo.RecordObject(cam, "Ajustar color de cámara");
            cam.backgroundColor = colorFondo;
            cam.clearFlags = CameraClearFlags.SolidColor;
            EditorUtility.SetDirty(cam);
        }

        private static Light2D ConfigurarLuzGlobal(Color colorLuz, float intensidad)
        {
            Light2D luz = null;
            var luces = Object.FindObjectsByType<Light2D>();
            foreach (var l in luces)
            {
                if (l.lightType == Light2D.LightType.Global)
                {
                    luz = l;
                    break;
                }
            }

            if (luz != null)
            {
                Undo.RecordObject(luz, "Ajustar Global Light 2D");
                luz.color = colorLuz;
                luz.intensity = intensidad;
                EditorUtility.SetDirty(luz);
            }
            else
            {
                Debug.LogWarning("[ConstructorEntorno] No se encontró un Global Light 2D en la escena.");
            }

            return luz;
        }

        private static SpriteRenderer[] CrearCapasFondo(GameObject parent, ConfigEntornoNivel cfg)
        {
            Sprite sFar = AssetDatabase.LoadAssetAtPath<Sprite>(cfg.RutaFar);
            Sprite sMid = AssetDatabase.LoadAssetAtPath<Sprite>(cfg.RutaMid);
            Sprite sNear = AssetDatabase.LoadAssetAtPath<Sprite>(cfg.RutaNear);

            List<SpriteRenderer> todosRenderers = new List<SpriteRenderer>();

            float inicioX = -40f;
            float finX = 180f;

            CrearCapaIndividual(parent, "Capa1_FondoLejano", sFar, -30, 0.10f, cfg.BaseY, inicioX, finX, todosRenderers);
            CrearCapaIndividual(parent, "Capa2_FondoMedio", sMid, -20, 0.30f, cfg.BaseY, inicioX, finX, todosRenderers);
            CrearCapaIndividual(parent, "Capa3_FondoCercano", sNear, -10, 0.60f, cfg.BaseY, inicioX, finX, todosRenderers);

            return todosRenderers.ToArray();
        }

        private static void CrearCapaIndividual(
            GameObject parent,
            string nombre,
            Sprite sprite,
            int sortingOrder,
            float factorParallaxX,
            float baseY,
            float inicioX,
            float finX,
            List<SpriteRenderer> acumulador)
        {
            GameObject goCapa = new GameObject(nombre);
            Undo.RegisterCreatedObjectUndo(goCapa, $"Crear {nombre}");
            goCapa.transform.SetParent(parent.transform, false);
            goCapa.transform.position = new Vector3(0f, baseY, 0f);

            float camAlto = (Camera.main != null) ? Camera.main.orthographicSize * 2f : 12f;
            float spriteAlto = (sprite != null) ? (sprite.rect.height / sprite.pixelsPerUnit) : 24f;
            float spriteAncho = (sprite != null) ? (sprite.rect.width / sprite.pixelsPerUnit) : 16f;
            float escalaY = (spriteAlto > 0f) ? (camAlto / spriteAlto) : 1f;

            // Ajuste automático de escala en Y para cubrir la pantalla de base a copa sin repetición vertical
            goCapa.transform.localScale = new Vector3(1f, escalaY, 1f);

            ParallaxFondo parallax = Undo.AddComponent<ParallaxFondo>(goCapa);
            parallax.FactorParallax = new Vector2(factorParallaxX, 0f);

            if (sprite == null || spriteAncho <= 0f) return;

            int indice = 0;
            for (float x = inicioX; x <= finX; x += spriteAncho)
            {
                GameObject seg = new GameObject($"{nombre}_Seg_{indice++}");
                Undo.RegisterCreatedObjectUndo(seg, $"Crear segmento {nombre}");
                seg.transform.SetParent(goCapa.transform, false);
                seg.transform.localPosition = new Vector3(x, 0f, 0f);

                SpriteRenderer sr = seg.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.drawMode = SpriteDrawMode.Simple;
                sr.sortingOrder = sortingOrder;
                sr.color = Color.white;

                acumulador.Add(sr);
            }
        }

        private static void CrearDecoracion(GameObject parent, bool incluirJuncos)
        {
            GameObject goDecoParent = new GameObject("Decoracion");
            Undo.RegisterCreatedObjectUndo(goDecoParent, "Crear Decoración");
            goDecoParent.transform.SetParent(parent.transform, false);

            Sprite sArbusto = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/arbusto.png");
            Sprite sHongo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/hongo.png");
            Sprite sRoca = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/roca.png");
            Sprite sFlor = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/flor.png");
            Sprite sJunco = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/junco.png");

            List<Sprite> listaSprites = new List<Sprite>();
            if (sArbusto != null) listaSprites.Add(sArbusto);
            if (sHongo != null) listaSprites.Add(sHongo);
            if (sRoca != null) listaSprites.Add(sRoca);
            if (sFlor != null) listaSprites.Add(sFlor);
            if (incluirJuncos && sJunco != null)
            {
                listaSprites.Add(sJunco);
                listaSprites.Add(sJunco);
            }

            if (listaSprites.Count == 0) return;

            var todosColliders = Object.FindObjectsByType<Collider2D>();
            System.Random rnd = new System.Random(1337);

            foreach (var col in todosColliders)
            {
                if (col.isTrigger) continue;
                string nombreLower = col.gameObject.name.ToLower();
                if (!string.Equals(col.gameObject.tag, "Suelo", System.StringComparison.Ordinal) &&
                    !nombreLower.Contains("suelo") &&
                    !nombreLower.Contains("islote") &&
                    !nombreLower.Contains("plataforma"))
                {
                    continue;
                }

                // No decorar plataformas móviles
                if (col.GetComponent<PlataformaMovil>() != null) continue;

                Bounds b = col.bounds;
                float ancho = b.size.x;
                if (ancho < 1.2f) continue;

                int cantidad = Mathf.Clamp((int)(ancho / 3.5f), 1, 4);

                for (int i = 0; i < cantidad; i++)
                {
                    float paso = ancho / (cantidad + 1);
                    float posX = b.min.x + paso * (i + 1) + ((float)rnd.NextDouble() * 0.4f - 0.2f);
                    float posY = b.max.y;

                    Sprite propSprite = listaSprites[rnd.Next(0, listaSprites.Count)];
                    if (propSprite == null) continue;

                    GameObject goProp = new GameObject($"Prop_{propSprite.name}_{i + 1}");
                    Undo.RegisterCreatedObjectUndo(goProp, "Crear Prop Decorativo");
                    goProp.transform.SetParent(goDecoParent.transform, false);

                    float offsetPivote = propSprite.rect.height / (2f * propSprite.pixelsPerUnit);
                    goProp.transform.position = new Vector3(posX, posY + offsetPivote, 0f);

                    SpriteRenderer sr = goProp.AddComponent<SpriteRenderer>();
                    sr.sprite = propSprite;
                    sr.sortingOrder = -2;
                }
            }
        }

        private static void CrearAmbientacionEspecificaNivel(GameObject parent, int nivel)
        {
            GameObject goAmb = new GameObject("AmbientacionNivel");
            Undo.RegisterCreatedObjectUndo(goAmb, "Crear Ambientación Nivel");
            goAmb.transform.SetParent(parent.transform, false);

            if (nivel == 2)
            {
                // Hojas cayendo suavemente entre las copas del Bosque Profundo
                CrearSistemaParticulas(
                    goAmb.transform,
                    "SistemaHojasBosque",
                    new Vector3(60f, 8f, 0f),
                    new Vector3(130f, 2f, 1f),
                    new Color(0.45f, 0.78f, 0.42f, 0.75f),
                    new Vector2(0.14f, 0.28f),
                    new Vector2(-0.6f, -0.25f),
                    24f,
                    5.5f);

                // Luces suaves en zonas elevadas y Power-Up
                CrearLuzPuntual2D(goAmb.transform, "LuzPowerUp_Bosque", new Vector3(25f, 3.2f, 0f), new Color(1f, 0.9f, 0.45f), 1.4f, 6f);
                CrearLuzPuntual2D(goAmb.transform, "LuzClaroPoetas", new Vector3(59.5f, 2.5f, 0f), new Color(0.65f, 1f, 0.75f), 1.1f, 8f);
            }
            else if (nivel == 3)
            {
                // Luces puntuales cyan/lima de luciérnagas a lo largo del Lago Mayor
                float[] posicionesX = { 10f, 28f, 48f, 68f, 88f, 112f };
                for (int i = 0; i < posicionesX.Length; i++)
                {
                    CrearLuzPuntual2D(
                        goAmb.transform,
                        $"LuzLuciernaga_{i + 1}",
                        new Vector3(posicionesX[i], 2.2f, 0f),
                        new Color(0.55f, 0.95f, 0.85f),
                        1.0f,
                        7.5f);
                }
            }
            else if (nivel == 4)
            {
                // Destellos dorados de atardecer en la Cima de los Recuerdos
                CrearSistemaParticulas(
                    goAmb.transform,
                    "SistemaCenizasDoradas",
                    new Vector3(65f, 4f, 0f),
                    new Vector3(130f, 8f, 1f),
                    new Color(1f, 0.78f, 0.35f, 0.78f),
                    new Vector2(0.10f, 0.22f),
                    new Vector2(0.25f, 0.35f),
                    28f,
                    4.0f);

                // Silueta monumental del Castillo de Chapultepec centrada detrás de la arena final
                Sprite sCastillo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/fondo_nivel4_castillo.png")
                                   ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/fondo_nivel4_media.png");
                if (sCastillo != null)
                {
                    GameObject goCastillo = new GameObject("CastilloChapultepec_Fondo");
                    Undo.RegisterCreatedObjectUndo(goCastillo, "Crear Castillo Chapultepec");
                    goCastillo.transform.SetParent(goAmb.transform, false);
                    goCastillo.transform.position = new Vector3(95f, -1.8f, 0f);
                    goCastillo.transform.localScale = new Vector3(1.6f, 0.85f, 1f);

                    SpriteRenderer sr = goCastillo.AddComponent<SpriteRenderer>();
                    sr.sprite = sCastillo;
                    sr.drawMode = SpriteDrawMode.Simple;
                    sr.sortingOrder = -15;
                    sr.color = new Color(1f, 0.92f, 0.95f, 0.95f);

                    ParallaxFondo pf = goCastillo.AddComponent<ParallaxFondo>();
                    pf.FactorParallax = new Vector2(0.15f, 0f);
                }

                // Luces cálidas de atardecer en la arena del Castillo
                CrearLuzPuntual2D(goAmb.transform, "LuzArena_Izq", new Vector3(98f, 4.5f, 0f), new Color(1f, 0.72f, 0.38f), 1.3f, 8f);
                CrearLuzPuntual2D(goAmb.transform, "LuzArena_Der", new Vector3(116f, 4.5f, 0f), new Color(1f, 0.65f, 0.45f), 1.3f, 8f);
            }
        }

        private static void CrearSistemaParticulas(
            Transform padre,
            string nombre,
            Vector3 posicion,
            Vector3 escalaCaja,
            Color colorInicio,
            Vector2 rangoTamano,
            Vector2 velocidadXY,
            float tasaEmision,
            float vida)
        {
            GameObject go = new GameObject(nombre);
            Undo.RegisterCreatedObjectUndo(go, $"Crear {nombre}");
            go.transform.SetParent(padre, false);
            go.transform.position = posicion;

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = vida;
            main.startSpeed = 0.25f;
            main.startSize = new ParticleSystem.MinMaxCurve(rangoTamano.x, rangoTamano.y);
            main.startColor = colorInicio;
            main.maxParticles = 90;

            var emission = ps.emission;
            emission.rateOverTime = tasaEmision;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = escalaCaja;

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = new ParticleSystem.MinMaxCurve(velocidadXY.x - 0.15f, velocidadXY.x + 0.15f);
            vel.y = new ParticleSystem.MinMaxCurve(velocidadXY.y - 0.1f, velocidadXY.y + 0.1f);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 8;
        }

        private static void CrearLuzPuntual2D(Transform padre, string nombre, Vector3 pos, Color color, float intensidad, float radioExterior)
        {
            GameObject go = new GameObject(nombre);
            Undo.RegisterCreatedObjectUndo(go, $"Crear {nombre}");
            go.transform.SetParent(padre, false);
            go.transform.position = pos;

            Light2D luz = go.AddComponent<Light2D>();
            luz.lightType = Light2D.LightType.Point;
            luz.color = color;
            luz.intensity = intensidad;
            luz.pointLightOuterRadius = radioExterior;
            luz.pointLightInnerRadius = radioExterior * 0.25f;
        }
    }
}
