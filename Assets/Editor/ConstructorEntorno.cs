using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using SemillasDelBosque.Entorno;
using SemillasDelBosque.Nivel3;
using System.Collections.Generic;

namespace SemillasDelBosque.Editor
{
    /// <summary>
    /// ConstructorEntorno: Menú de editor para construir el fondo, parallax, iluminación
    /// y distribución de props decorativos según el nivel actual (Nivel 1 o Nivel 3).
    /// 
    /// Nivel 1 (Claro del Despertar):
    /// - Cámara: #5D7A68 (Gris-Verde bosque desaturado)
    /// - Global Light 2D: #ADC2C2 con Intensidad 0.65f
    /// - Capas de fondo con pinos y colinas en verde menta, pivot Bottom-Center,
    ///   con Tiled horizontal únicamente (sin repetición vertical ni árboles cortados).
    /// - Configuración de patrulla horizontal para SombraEspinosa en Plataforma_Larga_D.
    /// 
    /// Nivel 3 (Lago de las Luciérnagas):
    /// - Cámara: Azul noche profundo
    /// - Global Light 2D: Intensidad adecuada (0.60f)
    /// </summary>
    public static class ConstructorEntorno
    {
        // Colores oficiales según documento de diseño
        private static readonly Color ColorCamaraNivel1 = new Color32(0x5D, 0x7A, 0x68, 0xFF);  // #5D7A68
        private static readonly Color ColorLuzNivel1 = new Color32(0xAD, 0xC2, 0xC2, 0xFF);     // #ADC2C2
        private const float IntensidadLuzNivel1 = 0.65f;

        private static readonly Color ColorCamaraNivel3 = new Color32(0x06, 0x0E, 0x1C, 0xFF);  // Azul noche profundo
        private static readonly Color ColorLuzNivel3 = new Color32(0x52, 0x74, 0xA8, 0xFF);     // Azul noche claro
        private const float IntensidadLuzNivel3 = 0.60f;

        [MenuItem("Semillas del Bosque/Entorno/Construir entorno")]
        public static void ConstruirEntornoNivel()
        {
            Scene escena = SceneManager.GetActiveScene();
            string nombreEscena = escena.name;

            bool esNivel1 = nombreEscena.Contains("Claro") || nombreEscena.Contains("Despertar");
            bool esNivel3 = nombreEscena.Contains("Lago") || nombreEscena.Contains("Luciernagas");

            if (!esNivel1 && !esNivel3)
            {
                Debug.LogWarning($"[ConstructorEntorno] Escena '{nombreEscena}' no reconocida directamente como Nivel 1 o 3. Se aplicará configuración de Nivel 1.");
                esNivel1 = true;
            }

            Debug.Log($"[ConstructorEntorno] Iniciando construcción de entorno para {(esNivel1 ? "Nivel 1 (Claro del Despertar)" : "Nivel 3 (Lago de las Luciérnagas)")}...");

            // 1. Configurar Cámara Principal
            ConfigurarCamara(esNivel1);

            // 2. Configurar Luz 2D Global
            Light2D luzGlobal = ConfigurarLuzGlobal(esNivel1);

            // 3. Crear o limpiar contenedor "--- ENTORNO ---"
            GameObject goEntorno = GameObject.Find("--- ENTORNO ---");
            if (goEntorno != null)
            {
                Debug.Log("[ConstructorEntorno] '--- ENTORNO ---' existente encontrado. Reemplazando con Undo...");
                Undo.DestroyObjectImmediate(goEntorno);
            }

            goEntorno = new GameObject("--- ENTORNO ---");
            Undo.RegisterCreatedObjectUndo(goEntorno, "Crear Entorno");
            goEntorno.transform.position = Vector3.zero;

            // 4. Crear capas de fondo con Parallax, pivot Bottom-Center y Tiled horizontal exclusivo
            SpriteRenderer[] capasFondo = CrearCapasFondo(goEntorno, esNivel1);

            // 5. Distribuir decoración sobre las plataformas
            CrearDecoracion(goEntorno, esNivel1);

            // 6. Agregar y enlazar el script RestauracionBosque
            RestauracionBosque restauracion = goEntorno.AddComponent<RestauracionBosque>();
            restauracion.Configurar(luzGlobal, capasFondo);

            // 7. Si es Nivel 1, configurar automáticamente la patrulla de la Sombra en Zona D
            if (esNivel1)
            {
                ConfigurarPatrullaNivel1();
            }

            EditorUtility.SetDirty(goEntorno);
            Selection.activeGameObject = goEntorno;
            Debug.Log("[ConstructorEntorno] ¡Entorno construido exitosamente sin tiling vertical y con alineación Bottom-Center!");
        }

        [MenuItem("Semillas del Bosque/Entorno/Configurar Patrulla Nivel 1")]
        public static void ConfigurarPatrullaNivel1()
        {
            Debug.Log("[ConstructorEntorno] Configurando patrulla de SombraEspinosa en Plataforma_Larga_D (Nivel 1)...");

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
                Debug.LogWarning("[ConstructorEntorno] No se encontró el GameObject 'SombraEspinosa' en la escena activa.");
                return;
            }

            // Buscar la plataforma larga de la Zona D
            GameObject platLarga = GameObject.Find("Plataforma_Larga_D");
            float minX = 77.8f;
            float maxX = 88.2f;

            if (platLarga != null)
            {
                Collider2D colPlat = platLarga.GetComponent<Collider2D>();
                if (colPlat != null)
                {
                    // Margen de seguridad de 0.8 unidades hacia adentro de cada borde
                    minX = colPlat.bounds.min.x + 0.8f;
                    maxX = colPlat.bounds.max.x - 0.8f;
                }
            }

            Undo.RegisterCompleteObjectUndo(sombra, "Configurar Patrulla Sombra Nivel 1");

            PatrullaSombra patrulla = sombra.GetComponent<PatrullaSombra>();
            if (patrulla == null)
            {
                patrulla = Undo.AddComponent<PatrullaSombra>(sombra);
                Debug.Log("[ConstructorEntorno] Componente PatrullaSombra agregado a SombraEspinosa.");
            }

            // Configurar límites y velocidad
            patrulla.ConfigurarRango(minX, maxX, 2.0f);
            patrulla.XMinimo = minX;
            patrulla.XMaximo = maxX;
            patrulla.Velocidad = 2.0f;

            // Crear marcadores visuales para puntos A y B como ayuda visual en el editor
            Transform puntoA = sombra.transform.Find("PuntoPatrulla_A");
            if (puntoA == null)
            {
                GameObject goA = new GameObject("PuntoPatrulla_A");
                Undo.RegisterCreatedObjectUndo(goA, "Crear Punto Patrulla A");
                goA.transform.SetParent(sombra.transform, false);
                puntoA = goA.transform;
            }
            puntoA.position = new Vector3(minX, sombra.transform.position.y, 0f);

            Transform puntoB = sombra.transform.Find("PuntoPatrulla_B");
            if (puntoB == null)
            {
                GameObject goB = new GameObject("PuntoPatrulla_B");
                Undo.RegisterCreatedObjectUndo(goB, "Crear Punto Patrulla B");
                goB.transform.SetParent(sombra.transform, false);
                puntoB = goB.transform;
            }
            puntoB.position = new Vector3(maxX, sombra.transform.position.y, 0f);

            EditorUtility.SetDirty(sombra);
            Debug.Log($"[ConstructorEntorno] Patrulla configurada en SombraEspinosa: XMin = {minX:F2}, XMax = {maxX:F2}, Velocidad = 2.0");
        }

        private static void ConfigurarCamara(bool esNivel1)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Undo.RecordObject(cam, "Ajustar color de cámara");
            // Nivel 1: #5D7A68 (Gris-Verde bosque) | Nivel 3: Azul noche profundo
            cam.backgroundColor = esNivel1 ? ColorCamaraNivel1 : ColorCamaraNivel3;
            cam.clearFlags = CameraClearFlags.SolidColor;
            EditorUtility.SetDirty(cam);
            Debug.Log($"[ConstructorEntorno] Fondo de cámara configurado a {(esNivel1 ? "#5D7A68 (Verde bosque)" : "Azul Noche")}.");
        }

        private static Light2D ConfigurarLuzGlobal(bool esNivel1)
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
                Undo.RecordObject(luz, "Ajustar color de luz global");
                // Nivel 1: #ADC2C2 con Intensidad 0.65f | Nivel 3: Azul noche con Intensidad 0.60f
                luz.color = esNivel1 ? ColorLuzNivel1 : ColorLuzNivel3;
                luz.intensity = esNivel1 ? IntensidadLuzNivel1 : IntensidadLuzNivel3;
                EditorUtility.SetDirty(luz);
                Debug.Log($"[ConstructorEntorno] Global Light 2D configurada a color {(esNivel1 ? "#ADC2C2 (Intensidad 0.65)" : "Azul Noche (Intensidad 0.60)")}.");
            }
            else
            {
                Debug.LogWarning("[ConstructorEntorno] No se encontró un Global Light 2D en la escena.");
            }

            return luz;
        }

        private static SpriteRenderer[] CrearCapasFondo(GameObject parent, bool esNivel1)
        {
            Debug.Log("[ConstructorEntorno] Creando 3 capas de fondo con Parallax (Tiled solo en X, alineado al suelo)...");

            string rutaFar = esNivel1 ? "Assets/Sprites/Generados/fondo_colinas_lejanas.png" : "Assets/Sprites/Generados/fondo_nivel3_lejana.png";
            string rutaMid = esNivel1 ? "Assets/Sprites/Generados/fondo_arboles_medios.png" : "Assets/Sprites/Generados/fondo_nivel3_media.png";
            string rutaNear = esNivel1 ? "Assets/Sprites/Generados/fondo_arboles_cercanos.png" : "Assets/Sprites/Generados/fondo_nivel3_agua.png";

            Sprite sFar = AssetDatabase.LoadAssetAtPath<Sprite>(rutaFar);
            Sprite sMid = AssetDatabase.LoadAssetAtPath<Sprite>(rutaMid);
            Sprite sNear = AssetDatabase.LoadAssetAtPath<Sprite>(rutaNear);

            SpriteRenderer[] renderers = new SpriteRenderer[3];

            // Rango horizontal del nivel (de X = -10 a X = 140)
            float centroX = 60f;
            float anchoTotal = 260f;

            // En Nivel 1, el suelo principal está entre Y = -3 y Y = 0.65.
            // Con el pivot en Bottom-Center (centro-abajo), colocar la posición en Y = -3.5f
            // alinea la base de los pinos y colinas directamente con el suelo del nivel,
            // y la altura de 24 unidades (384 px / 16) eleva las copas de los árboles hasta Y = +20.5f,
            // cubriendo todo el campo de visión de la cámara (Y = -2.5 a +9.5) sin repetirse en vertical.
            float baseY = esNivel1 ? -3.5f : -12.0f;

            // Capa 1: Lejana (Sorting -30, Parallax suave horizontal, casi estático vertical)
            renderers[0] = CrearCapaIndividual(parent, "Capa1_FondoLejano", sFar, -30, new Vector2(0.10f, 0.02f), new Vector3(centroX, baseY, 0f), anchoTotal);

            // Capa 2: Media (Sorting -20, Parallax medio)
            renderers[1] = CrearCapaIndividual(parent, "Capa2_FondoMedio", sMid, -20, new Vector2(0.30f, 0.04f), new Vector3(centroX, baseY, 0f), anchoTotal);

            // Capa 3: Cercana / Agua (Sorting -10, Parallax cercano)
            renderers[2] = CrearCapaIndividual(parent, "Capa3_FondoCercano", sNear, -10, new Vector2(0.60f, 0.06f), new Vector3(centroX, baseY, 0f), anchoTotal);

            return renderers;
        }

        private static SpriteRenderer CrearCapaIndividual(GameObject parent, string nombre, Sprite sprite, int sortingOrder, Vector2 factorParallax, Vector3 posicion, float ancho)
        {
            GameObject goCapa = new GameObject(nombre);
            Undo.RegisterCreatedObjectUndo(goCapa, $"Crear {nombre}");
            goCapa.transform.SetParent(parent.transform, false);
            goCapa.transform.position = posicion;

            SpriteRenderer sr = goCapa.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            // Tinte blanco para no alterar los colores generados
            sr.color = Color.white;

            if (sprite != null)
            {
                // Altura del sprite en unidades de mundo (ej: 384 px ÷ PPU 16 = 24 unidades)
                float spriteAltoUnidades = sprite.rect.height / sprite.pixelsPerUnit;

                // SpriteDrawMode.Tiled SOLO en X:
                //   sr.size.y = altura exacta del sprite → el eje Y NUNCA se repite.
                //   wrapModeU = Repeat (GeneradorArte) → patrón horizontal infinito ✓
                //   wrapModeV = Clamp  (GeneradorArte) → sin duplicación vertical ✓
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.tileMode = SpriteTileMode.Continuous;
                sr.size = new Vector2(ancho, spriteAltoUnidades);

                // Escalar el GameObject en Y para que el árbol (de base a copa) llene
                // exactamente la altura visible de la cámara ortográfica, sin que la
                // textura se empalme ni duplique verticalmente.
                float camAlto = (Camera.main != null) ? Camera.main.orthographicSize * 2f : 12f;
                float escalaY = (spriteAltoUnidades > 0f) ? (camAlto / spriteAltoUnidades) : 1f;
                goCapa.transform.localScale = new Vector3(1f, escalaY, 1f);
                Debug.Log($"[ConstructorEntorno] {nombre}: spriteAlto={spriteAltoUnidades:F2}u, camAlto={camAlto:F2}u, escalaY={escalaY:F3}");
            }

            // El factor Y de parallax se fuerza a 0: el fondo nunca se desplaza verticalmente
            // (evita que al saltar se vea la textura correrse hacia arriba revelando bordes).
            ParallaxFondo parallax = goCapa.AddComponent<ParallaxFondo>();
            parallax.FactorParallax = new Vector2(factorParallax.x, 0f);

            return sr;
        }

        private static void CrearDecoracion(GameObject parent, bool esNivel1)
        {
            Debug.Log("[ConstructorEntorno] Distribuyendo elementos de decoración proporcionados...");

            GameObject goDecoParent = new GameObject("Decoracion");
            Undo.RegisterCreatedObjectUndo(goDecoParent, "Crear Decoración");
            goDecoParent.transform.SetParent(parent.transform, false);

            Sprite sArbusto = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/arbusto.png");
            Sprite sHongo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/hongo.png");
            Sprite sRoca = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/roca.png");
            Sprite sFlor = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/flor.png");
            Sprite sJunco = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/junco.png");

            List<Sprite> listaSprites = new List<Sprite> { sArbusto, sHongo, sRoca, sFlor };
            if (!esNivel1 && sJunco != null)
            {
                listaSprites.Add(sJunco);
                listaSprites.Add(sJunco);
            }

            var todosColliders = Object.FindObjectsByType<Collider2D>();
            System.Random rnd = new System.Random(1337);

            foreach (var col in todosColliders)
            {
                if (col.isTrigger) continue;
                if (!col.CompareTag("Suelo") && !col.gameObject.name.ToLower().Contains("suelo") && !col.gameObject.name.ToLower().Contains("islote") && !col.gameObject.name.ToLower().Contains("plataforma"))
                {
                    continue;
                }

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

                    // Offset exacto según pixelsPerUnit para apoyar perfectamente sobre el suelo
                    float offsetPivote = (propSprite.rect.height / (2f * propSprite.pixelsPerUnit));
                    goProp.transform.position = new Vector3(posX, posY + offsetPivote, 0f);

                    SpriteRenderer sr = goProp.AddComponent<SpriteRenderer>();
                    sr.sprite = propSprite;
                    sr.sortingOrder = -2; // Detrás de Luna (order 0)
                }
            }

            Debug.Log("[ConstructorEntorno] Decoración distribuida sobre plataformas (sin colliders y detrás de Luna).");
        }
    }
}
