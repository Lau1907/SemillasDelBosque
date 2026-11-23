using UnityEngine;
using UnityEditor;
using System.IO;

namespace SemillasDelBosque.Editor
{
    /// <summary>
    /// GeneradorArte: Script de editor para generar sprites procedurales en estilo pixel art.
    /// Crea iconos de HUD, elementos de decoración y capas de fondo para los niveles 1 y 3.
    /// Configura automáticamente TextureImporter con filtro Point, sin compresión, PPU 16,
    /// Pivot al Centro-Abajo (Bottom-Center) y Tiling horizontal (Repeat en X, Clamp en Y)
    /// para evitar cualquier corte o repetición vertical de los árboles.
    /// </summary>
    public static class GeneradorArte
    {
        private const string RUTA_GENERADOS = "Assets/Sprites/Generados";
        private const int PPU = 16;

        // Dimensiones de los fondos (altura de 384 px = 24 unidades en mundo a PPU 16)
        private const int ANCHO_FONDO = 256;
        private const int ALTO_FONDO = 384;

        // Paleta oficial Nivel 1 (Bosque gris-verde menta desaturado)
        // Cielo: #88B59E a #69967F
        private static readonly Color32 ColorCieloSuperior = new Color32(0x88, 0xB5, 0x9E, 0xFF); // #88B59E
        private static readonly Color32 ColorCieloHorizonte = new Color32(0x69, 0x96, 0x7F, 0xFF); // #69967F

        // Capa -20 (Pinos medianos): #4E7358
        private static readonly Color32 ColorPinoMedio = new Color32(0x4E, 0x73, 0x58, 0xFF);     // #4E7358
        private static readonly Color32 ColorPinoMedioSombra = new Color32(0x3B, 0x5B, 0x44, 0xFF);
        private static readonly Color32 ColorPinoMedioLuz = new Color32(0x60, 0x8A, 0x6C, 0xFF);

        // Capa -10 (Pinos cercanos completos): #3B5B45
        private static readonly Color32 ColorPinoCercano = new Color32(0x3B, 0x5B, 0x45, 0xFF);   // #3B5B45
        private static readonly Color32 ColorPinoCercanoSombra = new Color32(0x28, 0x3F, 0x30, 0xFF);
        private static readonly Color32 ColorPinoCercanoLuz = new Color32(0x4D, 0x75, 0x5A, 0xFF);
        private static readonly Color32 ColorTronco = new Color32(0x22, 0x33, 0x28, 0xFF);

        [MenuItem("Semillas del Bosque/Arte/Generar sprites")]
        public static void GenerarTodosLosSprites()
        {
            Debug.Log("[GeneradorArte] Iniciando generación de sprites pixel art (Fondos 256x384, Pivot Bottom-Center)...");

            if (!Directory.Exists(RUTA_GENERADOS))
            {
                Directory.CreateDirectory(RUTA_GENERADOS);
                Debug.Log($"[GeneradorArte] Carpeta creada: {RUTA_GENERADOS}");
            }

            // 1. HUD: Corazones y Semilla
            GenerarCorazonLleno();
            GenerarCorazonVacio();
            GenerarIconoSemilla();
            GenerarIconoLibroCodice();

            // 2. Elementos de decoración (props)
            GenerarArbusto();
            GenerarHongo();
            GenerarRoca();
            GenerarJunco();
            GenerarFlor();

            // 3. Fondos Nivel 1 (Bosque desaturado, sin repetición vertical)
            GenerarFondoColinasLejanasN1();
            GenerarFondoArbolesMediosN1();
            GenerarFondoArbolesCercanosN1();

            // 4. Fondos Nivel 3 (Noche azul, siluetas, agua y luciérnagas)
            GenerarFondoNocheLejanaN3();
            GenerarFondoNocheMediaN3();
            GenerarFondoNocheAguaN3();

            // 5. Fondos Nivel 2 (Bosque Profundo - verde denso con sombra suave)
            GenerarFondoBosqueProfundoLejanoN2();
            GenerarFondoBosqueProfundoMedioN2();
            GenerarFondoBosqueProfundoCercanoN2();

            // 6. Fondos Nivel 4 (Cima de los Recuerdos - atardecer dorado/púrpura y Castillo de Chapultepec)
            GenerarFondoAtardecerLejanoN4();
            GenerarFondoCastilloChapultepecMedioN4();
            GenerarFondoAtardecerCercanoN4();
            GenerarFondoCastilloLandmarkN4();

            // 7. Logo del Equipo para Splash Screen
            GenerarLogoEquipo();

            AssetDatabase.Refresh();
            Debug.Log("[GeneradorArte] ¡Todos los sprites generados e importados exitosamente con pivot Bottom-Center y clamp vertical!");
        }

        #region Generación de HUD

        private static void GenerarCorazonLleno()
        {
            string nombre = "corazon_lleno.png";
            Debug.Log($"[GeneradorArte] Generando: {nombre}...");
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color transparente = new Color(0, 0, 0, 0);
            Color borde = new Color32(70, 10, 20, 255);
            Color rojo = new Color32(230, 45, 65, 255);
            Color brillo = new Color32(255, 150, 165, 255);
            Color sombra = new Color32(170, 20, 40, 255);

            int[,] patron = new int[16, 16]
            {
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,1,1,1,0,0,0,0,0,1,1,1,0,0,0},
                {0,1,3,3,2,1,0,0,0,1,2,2,2,1,0,0},
                {1,3,3,2,2,2,1,0,1,2,2,2,2,2,1,0},
                {1,3,2,2,2,2,2,1,2,2,2,2,2,2,1,0},
                {1,2,2,2,2,2,2,2,2,2,2,2,2,4,1,0},
                {1,2,2,2,2,2,2,2,2,2,2,2,4,4,1,0},
                {0,1,2,2,2,2,2,2,2,2,2,4,4,1,0,0},
                {0,0,1,2,2,2,2,2,2,2,4,4,1,0,0,0},
                {0,0,0,1,2,2,2,2,2,4,4,1,0,0,0,0},
                {0,0,0,0,1,2,2,2,4,4,1,0,0,0,0,0},
                {0,0,0,0,0,1,2,4,4,1,0,0,0,0,0,0},
                {0,0,0,0,0,0,1,4,1,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
            };

            DibujarPatron(tex, patron, transparente, borde, rojo, brillo, sombra);
            GuardarYConfigurarSprite(tex, nombre, esFondo: false);
        }

        private static void GenerarCorazonVacio()
        {
            string nombre = "corazon_vacio.png";
            Debug.Log($"[GeneradorArte] Generando: {nombre}...");
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color transparente = new Color(0, 0, 0, 0);
            Color borde = new Color32(50, 45, 55, 255);
            Color grisOscuro = new Color32(30, 28, 35, 180);
            Color grisMedio = new Color32(60, 55, 68, 220);

            int[,] patron = new int[16, 16]
            {
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,1,1,1,0,0,0,0,0,1,1,1,0,0,0},
                {0,1,3,2,2,1,0,0,0,1,2,2,2,1,0,0},
                {1,3,2,2,2,2,1,0,1,2,2,2,2,2,1,0},
                {1,2,2,2,2,2,2,1,2,2,2,2,2,2,1,0},
                {1,2,2,2,2,2,2,2,2,2,2,2,2,2,1,0},
                {1,2,2,2,2,2,2,2,2,2,2,2,2,2,1,0},
                {0,1,2,2,2,2,2,2,2,2,2,2,2,1,0,0},
                {0,0,1,2,2,2,2,2,2,2,2,2,1,0,0,0},
                {0,0,0,1,2,2,2,2,2,2,2,1,0,0,0,0},
                {0,0,0,0,1,2,2,2,2,2,1,0,0,0,0,0},
                {0,0,0,0,0,1,2,2,2,1,0,0,0,0,0,0},
                {0,0,0,0,0,0,1,2,1,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
            };

            DibujarPatron(tex, patron, transparente, borde, grisOscuro, grisMedio, grisOscuro);
            GuardarYConfigurarSprite(tex, nombre, esFondo: false);
        }

        private static void GenerarIconoSemilla()
        {
            string nombre = "icono_semilla.png";
            Debug.Log($"[GeneradorArte] Generando: {nombre}...");
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color transparente = new Color(0, 0, 0, 0);
            Color borde = new Color32(140, 90, 10, 255);
            Color oro = new Color32(255, 205, 45, 255);
            Color brillo = new Color32(255, 250, 190, 255);
            Color destello = new Color32(255, 255, 255, 255);

            int[,] patron = new int[16, 16]
            {
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,1,3,3,1,0,0,0,0,0,0},
                {0,0,0,0,0,1,3,4,3,2,1,0,0,0,0,0},
                {0,0,0,0,1,3,4,4,3,2,2,1,0,0,0,0},
                {0,0,0,1,3,4,3,2,2,2,2,2,1,0,0,0},
                {0,0,1,3,3,2,2,2,2,2,2,2,2,1,0,0},
                {0,0,1,3,2,2,2,2,2,2,2,2,2,1,0,0},
                {0,0,1,2,2,2,2,2,2,2,2,2,2,1,0,0},
                {0,0,1,2,2,2,2,2,2,2,2,2,2,1,0,0},
                {0,0,0,1,2,2,2,2,2,2,2,2,1,0,0,0},
                {0,0,0,1,2,2,2,2,2,2,2,2,1,0,0,0},
                {0,0,0,0,1,2,2,2,2,2,2,1,0,0,0,0},
                {0,0,0,0,0,1,1,2,2,1,1,0,0,0,0,0},
                {0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
            };

            DibujarPatron(tex, patron, transparente, borde, oro, brillo, destello);
            GuardarYConfigurarSprite(tex, nombre, esFondo: false);
        }

        private static void GenerarIconoLibroCodice()
        {
            string nombre = "libro_codice.png";
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);
            Color borde = new Color32(45, 25, 15, 255);
            Color cuero = new Color32(125, 65, 32, 255);
            Color oro = new Color32(245, 195, 65, 255);
            Color papel = new Color32(238, 222, 185, 255);

            int[,] patron = new int[16, 16]
            {
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0},
                {0,1,2,3,3,2,2,2,2,2,2,2,2,1,0,0},
                {0,1,2,3,2,2,2,3,3,2,2,2,2,1,0,0},
                {0,1,2,2,2,2,3,4,4,3,2,2,2,1,0,0},
                {0,1,2,2,2,2,3,4,4,3,2,2,2,1,0,0},
                {0,1,2,2,2,2,2,3,3,2,2,2,2,1,0,0},
                {0,1,2,3,2,2,2,2,2,2,2,2,2,1,0,0},
                {0,1,2,3,3,2,2,2,2,2,2,2,2,1,0,0},
                {0,1,2,2,2,2,2,2,2,2,2,2,2,1,0,0},
                {0,1,2,2,2,2,2,2,2,2,2,2,2,1,0,0},
                {0,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0},
                {0,1,4,4,4,4,4,4,4,4,4,4,4,2,1,0},
                {0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
            };

            DibujarPatron(tex, patron, trans, borde, cuero, oro, papel);
            GuardarYConfigurarSprite(tex, nombre, esFondo: false);
        }

        #endregion

        #region Generación de Props (Decoración Proporcionada)

        private static void GenerarArbusto()
        {
            string nombre = "arbusto.png";
            Debug.Log($"[GeneradorArte] Generando: {nombre}...");
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);
            Color borde = ColorPinoCercanoSombra;
            Color verdeBase = ColorPinoCercano;
            Color verdeMedio = ColorPinoMedio;
            Color brillo = ColorPinoMedioLuz;

            int[,] patron = new int[16, 16]
            {
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0},
                {0,0,0,1,1,4,4,4,1,1,1,0,0,0,0,0},
                {0,0,1,4,4,4,3,3,4,4,4,1,0,0,0,0},
                {0,1,4,4,3,3,3,3,3,4,3,3,1,0,0,0},
                {1,4,4,3,3,3,3,3,3,3,3,3,2,1,0,0},
                {1,4,3,3,1,1,3,3,3,1,1,3,2,1,1,0},
                {1,3,3,1,4,4,1,3,1,4,3,1,2,3,3,1},
                {1,3,3,1,4,3,3,1,3,3,3,2,2,2,2,1},
                {1,3,3,3,3,3,3,3,3,3,2,2,2,2,2,1},
                {1,2,3,3,3,3,3,3,2,2,2,2,2,2,2,1},
                {1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1},
                {0,1,2,2,2,2,2,2,2,2,2,2,2,2,1,0},
                {0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
            };

            DibujarPatron(tex, patron, trans, borde, verdeBase, verdeMedio, brillo);
            GuardarYConfigurarSprite(tex, nombre, esFondo: false);
        }

        private static void GenerarHongo()
        {
            string nombre = "hongo.png";
            Debug.Log($"[GeneradorArte] Generando: {nombre}...");
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);
            Color borde = new Color32(55, 25, 25, 255);
            Color rojo = new Color32(205, 55, 55, 255);
            Color mota = new Color32(245, 240, 230, 255);
            Color tallo = new Color32(215, 210, 195, 255);

            int[,] patron = new int[16, 16]
            {
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0},
                {0,0,0,1,1,2,2,3,2,2,1,1,0,0,0,0},
                {0,0,1,2,3,2,2,3,2,2,3,2,1,0,0,0},
                {0,1,2,2,3,2,2,2,2,2,3,2,2,1,0,0},
                {1,2,2,2,2,2,3,3,2,2,2,2,2,2,1,0},
                {1,2,3,2,2,2,3,3,2,2,2,3,2,2,1,0},
                {1,2,3,2,2,2,2,2,2,2,2,3,2,2,1,0},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0},
                {0,0,0,0,1,4,4,4,4,4,1,0,0,0,0,0},
                {0,0,0,0,1,4,4,4,4,4,1,0,0,0,0,0},
                {0,0,0,0,1,4,4,4,4,4,1,0,0,0,0,0},
                {0,0,0,0,1,4,4,4,4,4,1,0,0,0,0,0},
                {0,0,0,1,4,4,4,4,4,4,4,1,0,0,0,0},
                {0,0,0,1,1,1,1,1,1,1,1,1,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
            };

            DibujarPatron(tex, patron, trans, borde, rojo, mota, tallo);
            GuardarYConfigurarSprite(tex, nombre, esFondo: false);
        }

        private static void GenerarRoca()
        {
            string nombre = "roca.png";
            Debug.Log($"[GeneradorArte] Generando: {nombre}...");
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);
            Color borde = new Color32(35, 42, 45, 255);
            Color gris = new Color32(85, 98, 105, 255);
            Color musgo = ColorPinoMedio;
            Color sombra = new Color32(50, 60, 65, 255);

            int[,] patron = new int[16, 16]
            {
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0},
                {0,0,0,1,1,3,3,3,2,1,0,0,0,0,0,0},
                {0,0,1,3,3,3,3,2,2,2,1,1,0,0,0,0},
                {0,1,3,3,3,2,2,2,2,2,2,2,1,0,0,0},
                {1,3,3,3,2,2,2,2,4,4,2,2,2,1,0,0},
                {1,3,2,2,2,2,4,4,4,4,4,2,2,2,1,0},
                {1,2,2,2,4,4,4,4,4,4,4,4,2,2,1,0},
                {1,2,2,4,4,4,4,4,4,4,4,4,4,4,1,0},
                {1,2,4,4,4,4,4,4,4,4,4,4,4,4,1,0},
                {1,4,4,4,4,4,4,4,4,4,4,4,4,4,1,0},
                {1,4,4,4,4,4,4,4,4,4,4,4,4,4,1,0},
                {0,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
            };

            DibujarPatron(tex, patron, trans, borde, gris, musgo, sombra);
            GuardarYConfigurarSprite(tex, nombre, esFondo: false);
        }

        private static void GenerarJunco()
        {
            string nombre = "junco.png";
            Debug.Log($"[GeneradorArte] Generando: {nombre}...");
            Texture2D tex = new Texture2D(16, 32, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);
            Color borde = ColorPinoCercanoSombra;
            Color tallo = ColorPinoCercano;
            Color puro = new Color32(95, 60, 35, 255);
            Color puroBrillo = new Color32(140, 90, 50, 255);

            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, trans);

            for (int y = 0; y < 22; y++)
            {
                tex.SetPixel(7, y, tallo);
                tex.SetPixel(8, y, tallo);
                tex.SetPixel(6, y, borde);
                tex.SetPixel(9, y, borde);
            }

            for (int y = 2; y < 14; y++)
            {
                int x = 7 - (int)Mathf.Sin((y - 2) * 0.25f * Mathf.PI) * 3;
                tex.SetPixel(x, y, tallo);
                tex.SetPixel(x - 1, y, borde);
            }
            for (int y = 4; y < 16; y++)
            {
                int x = 8 + (int)Mathf.Sin((y - 4) * 0.25f * Mathf.PI) * 4;
                tex.SetPixel(x, y, tallo);
                tex.SetPixel(x + 1, y, borde);
            }

            for (int y = 18; y < 29; y++)
            {
                tex.SetPixel(6, y, borde);
                tex.SetPixel(7, y, puroBrillo);
                tex.SetPixel(8, y, puro);
                tex.SetPixel(9, y, borde);
            }
            tex.SetPixel(7, 29, borde);
            tex.SetPixel(8, 29, borde);
            tex.SetPixel(7, 30, tallo);
            tex.SetPixel(8, 30, tallo);

            tex.Apply();
            GuardarYConfigurarSprite(tex, nombre, esFondo: false);
        }

        private static void GenerarFlor()
        {
            string nombre = "flor.png";
            Debug.Log($"[GeneradorArte] Generando: {nombre}...");
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);
            Color borde = ColorPinoCercanoSombra;
            Color petalo = new Color32(235, 245, 238, 255);
            Color centro = new Color32(255, 215, 60, 255);
            Color tallo = ColorPinoCercano;

            int[,] patron = new int[16, 16]
            {
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,1,1,0,0,1,1,0,0,0,0,0},
                {0,0,0,0,1,2,2,1,1,2,2,1,0,0,0,0},
                {0,0,0,0,1,2,2,1,1,2,2,1,0,0,0,0},
                {0,0,1,1,0,1,1,3,3,1,1,0,1,1,0,0},
                {0,1,2,2,1,3,3,3,3,3,3,1,2,2,1,0},
                {0,1,2,2,1,3,3,3,3,3,3,1,2,2,1,0},
                {0,0,1,1,0,1,1,3,3,1,1,0,1,1,0,0},
                {0,0,0,0,1,2,2,1,1,2,2,1,0,0,0,0},
                {0,0,0,0,1,2,2,1,1,2,2,1,0,0,0,0},
                {0,0,0,0,0,1,1,4,4,1,1,0,0,0,0,0},
                {0,0,0,0,0,1,4,4,4,1,0,0,0,0,0,0},
                {0,0,0,0,1,4,4,4,4,4,1,0,0,0,0,0},
                {0,0,0,0,0,0,1,4,4,1,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
            };

            DibujarPatron(tex, patron, trans, borde, petalo, centro, tallo);
            GuardarYConfigurarSprite(tex, nombre, esFondo: false);
        }

        #endregion

        #region Fondos Nivel 1 (Claro del Despertar - 256x384, Sin Cortes Verticales)

        /// <summary>
        /// Capa -30: Gradiente suave de cielo verde menta desaturado (#88B59E a #69967F)
        /// en la parte superior con siluetas de colinas suaves en la base.
        /// </summary>
        private static void GenerarFondoColinasLejanasN1()
        {
            string nombre = "fondo_colinas_lejanas.png";
            Debug.Log($"[GeneradorArte] Generando: {nombre} (256x384, Gradiente #88B59E a #69967F)...");
            Texture2D tex = new Texture2D(ANCHO_FONDO, ALTO_FONDO, TextureFormat.RGBA32, false);

            Color32 colinaTrasera = new Color32(0x54, 0x7A, 0x65, 0xFF);
            Color32 colinaFrente = new Color32(0x46, 0x69, 0x55, 0xFF);
            Color32 pinoCresta = new Color32(0x3B, 0x59, 0x47, 0xFF);

            for (int x = 0; x < ANCHO_FONDO; x++)
            {
                float rad = (float)x / ANCHO_FONDO * Mathf.PI * 2f;
                // Colinas suaves en el tercio inferior (hasta Y=120)
                int yColina1 = (int)(95 + Mathf.Sin(rad) * 20 + Mathf.Cos(rad * 2f) * 10);
                int yColina2 = (int)(65 + Mathf.Sin(rad * 2f + 1.2f) * 15 + Mathf.Cos(rad) * 8);

                int pinoPico1 = ((x % 6 == 0 || x % 6 == 1)) ? 4 : 0;
                int pinoPico2 = ((x % 8 == 0 || x % 8 == 1)) ? 5 : 0;

                int yMax1 = yColina1 + pinoPico1;
                int yMax2 = yColina2 + pinoPico2;

                for (int y = 0; y < ALTO_FONDO; y++)
                {
                    if (y <= yMax2)
                    {
                        // Colina del frente
                        tex.SetPixel(x, y, (y == yMax2 && pinoPico2 > 0) ? pinoCresta : colinaFrente);
                    }
                    else if (y <= yMax1)
                    {
                        // Colina trasera
                        tex.SetPixel(x, y, (y == yMax1 && pinoPico1 > 0) ? pinoCresta : colinaTrasera);
                    }
                    else
                    {
                        // Gradiente de cielo verde menta desaturado (#69967F en el horizonte -> #88B59E arriba)
                        float t = (float)(y - 65) / (ALTO_FONDO - 65);
                        Color cCielo = Color.Lerp(ColorCieloHorizonte, ColorCieloSuperior, Mathf.Clamp01(t));
                        tex.SetPixel(x, y, cCielo);
                    }
                }
            }

            tex.Apply();
            GuardarYConfigurarSprite(tex, nombre, esFondo: true);
        }

        /// <summary>
        /// Capa -20: Siluetas de pinos medianos más oscuros (#4E7358) levantándose desde
        /// la base hasta el 70% de la altura de la textura (Y ~ 268 px).
        /// Arriba es 100% transparente para permitir ver el gradiente de la capa -30.
        /// </summary>
        private static void GenerarFondoArbolesMediosN1()
        {
            string nombre = "fondo_arboles_medios.png";
            Debug.Log($"[GeneradorArte] Generando: {nombre} (256x384, Pinos Medianos #4E7358 hasta 70% de altura)...");
            Texture2D tex = new Texture2D(ANCHO_FONDO, ALTO_FONDO, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);

            // Inicializar a transparente
            for (int y = 0; y < ALTO_FONDO; y++)
                for (int x = 0; x < ANCHO_FONDO; x++)
                    tex.SetPixel(x, y, trans);

            // Centros y alturas de pinos medianos (altura máxima 268 px = 70% de 384)
            int[] centros = { 14, 42, 70, 98, 126, 154, 182, 210, 238 };
            int[] alturas = { 245, 268, 235, 265, 250, 268, 240, 262, 248 };

            for (int i = 0; i < centros.Length; i++)
            {
                int cx = centros[i];
                int maxAlt = alturas[i];
                DibujarPinoProcedural(tex, ANCHO_FONDO, ALTO_FONDO, cx, maxAlt, ColorPinoMedio, ColorPinoMedioSombra, ColorPinoMedioLuz, niveles: 6, anchoMaximoBase: 24);
            }

            tex.Apply();
            GuardarYConfigurarSprite(tex, nombre, esFondo: true);
        }

        /// <summary>
        /// Capa -10: Pinos bien definidos y completos en pixel art (#3B5B45), ocupando
        /// toda la altura vertical del lienzo con copas puntiagudas completas sin cortes truncados.
        /// </summary>
        private static void GenerarFondoArbolesCercanosN1()
        {
            string nombre = "fondo_arboles_cercanos.png";
            Debug.Log($"[GeneradorArte] Generando: {nombre} (256x384, Pinos Completos #3B5B45 en toda la altura)...");
            Texture2D tex = new Texture2D(ANCHO_FONDO, ALTO_FONDO, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);

            for (int y = 0; y < ALTO_FONDO; y++)
                for (int x = 0; x < ANCHO_FONDO; x++)
                    tex.SetPixel(x, y, trans);

            // Troncos visibles en la base
            int[] centrosCercanos = { 32, 92, 156, 218 };
            int[] alturasCercanas = { 355, 370, 350, 365 };

            foreach (int cx in centrosCercanos)
            {
                for (int y = 0; y < 70; y++)
                {
                    for (int x = cx - 4; x <= cx + 4; x++)
                    {
                        int wx = (x + ANCHO_FONDO) % ANCHO_FONDO;
                        tex.SetPixel(wx, y, ColorTronco);
                    }
                }
            }

            // Dibujar las coníferas grandes completas con copas en punta perfecta
            for (int i = 0; i < centrosCercanos.Length; i++)
            {
                int cx = centrosCercanos[i];
                int maxAlt = alturasCercanas[i];
                DibujarPinoProcedural(tex, ANCHO_FONDO, ALTO_FONDO, cx, maxAlt, ColorPinoCercano, ColorPinoCercanoSombra, ColorPinoCercanoLuz, niveles: 8, anchoMaximoBase: 36);
            }

            tex.Apply();
            GuardarYConfigurarSprite(tex, nombre, esFondo: true);
        }

        /// <summary>
        /// Algoritmo de pixel art para dibujar un pino completo con ramas escalonadas y punta afilada
        /// </summary>
        private static void DibujarPinoProcedural(Texture2D tex, int w, int h, int cx, int maxAlt, Color32 colorBase, Color32 colorSombra, Color32 colorLuz, int niveles, int anchoMaximoBase)
        {
            int alturaNivel = maxAlt / niveles;

            for (int nivel = 0; nivel < niveles; nivel++)
            {
                int yInicio = nivel * (alturaNivel - 6);
                int yFin = Mathf.Min(maxAlt, yInicio + alturaNivel + 8);
                // Ancho proporcional que se ensancha hacia la base
                float factorNivel = (float)(niveles - nivel) / niveles;
                int anchoNivel = Mathf.Max(3, (int)(factorNivel * anchoMaximoBase));

                for (int y = yInicio; y <= yFin; y++)
                {
                    if (y >= h) continue;

                    // Forma cónica triangular dentro del nivel
                    float tY = (float)(yFin - y) / (yFin - yInicio);
                    int ancho = Mathf.Max(1, (int)(tY * anchoNivel));

                    for (int x = cx - ancho; x <= cx + ancho; x++)
                    {
                        int wx = (x + w) % w;

                        Color32 c = colorBase;
                        if (x < cx - 1) c = colorLuz;
                        else if (x > cx + 1) c = colorSombra;
                        else if ((wx + y) % 4 == 0) c = colorLuz;

                        tex.SetPixel(wx, y, c);
                    }
                }
            }

            // Punta afilada limpia en la copa del árbol (sin truncamientos)
            for (int y = maxAlt; y <= Mathf.Min(h - 1, maxAlt + 4); y++)
            {
                tex.SetPixel((cx + w) % w, y, colorLuz);
            }
        }

        #endregion

        #region Fondos Nivel 3 (Lago de las Luciérnagas - 256x384)

        private static void GenerarFondoNocheLejanaN3()
        {
            string nombre = "fondo_nivel3_lejana.png";
            Debug.Log($"[GeneradorArte] Generando: {nombre} (256x384, Noche Azul)...");
            Texture2D tex = new Texture2D(ANCHO_FONDO, ALTO_FONDO, TextureFormat.RGBA32, false);
            Color cieloOscuro = new Color32(6, 10, 24, 255);
            Color cieloHorizonte = new Color32(18, 32, 65, 255);
            Color montana = new Color32(10, 18, 38, 255);
            Color estrella = new Color32(200, 240, 255, 255);

            for (int y = 0; y < ALTO_FONDO; y++)
            {
                float t = (float)y / ALTO_FONDO;
                Color colCielo = Color.Lerp(cieloHorizonte, cieloOscuro, t);
                for (int x = 0; x < ANCHO_FONDO; x++)
                {
                    tex.SetPixel(x, y, colCielo);
                }
            }

            System.Random rnd = new System.Random(42);
            for (int i = 0; i < 70; i++)
            {
                int ex = rnd.Next(0, ANCHO_FONDO);
                int ey = rnd.Next(120, ALTO_FONDO);
                tex.SetPixel(ex, ey, estrella);
            }

            for (int x = 0; x < ANCHO_FONDO; x++)
            {
                float rad = (float)x / ANCHO_FONDO * Mathf.PI * 2f;
                int altMont = (int)(95 + Mathf.Sin(rad * 3f) * 22 + Mathf.Cos(rad) * 12);
                for (int y = 0; y <= altMont; y++)
                {
                    tex.SetPixel(x, y, montana);
                }
            }

            tex.Apply();
            GuardarYConfigurarSprite(tex, nombre, esFondo: true);
        }

        private static void GenerarFondoNocheMediaN3()
        {
            string nombre = "fondo_nivel3_media.png";
            Debug.Log($"[GeneradorArte] Generando: {nombre} (256x384, Siluetas con Luciérnagas)...");
            Texture2D tex = new Texture2D(ANCHO_FONDO, ALTO_FONDO, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);
            Color silueta = new Color32(8, 14, 30, 255);
            Color siluetaLuz = new Color32(14, 24, 48, 255);
            Color luciernaga = new Color32(100, 240, 220, 255);

            for (int y = 0; y < ALTO_FONDO; y++)
                for (int x = 0; x < ANCHO_FONDO; x++)
                    tex.SetPixel(x, y, trans);

            int[] centros = { 20, 58, 96, 134, 172, 210, 242 };
            int[] alturas = { 240, 265, 235, 268, 245, 260, 250 };

            for (int i = 0; i < centros.Length; i++)
            {
                DibujarPinoProcedural(tex, ANCHO_FONDO, ALTO_FONDO, centros[i], alturas[i], silueta, silueta, siluetaLuz, niveles: 6, anchoMaximoBase: 26);
            }

            System.Random rnd = new System.Random(108);
            for (int i = 0; i < 45; i++)
            {
                int lx = rnd.Next(0, ANCHO_FONDO);
                int ly = rnd.Next(50, 260);
                tex.SetPixel(lx, ly, luciernaga);
                tex.SetPixel((lx + 1) % ANCHO_FONDO, ly, new Color32(50, 180, 160, 180));
            }

            tex.Apply();
            GuardarYConfigurarSprite(tex, nombre, esFondo: true);
        }

        private static void GenerarFondoNocheAguaN3()
        {
            string nombre = "fondo_nivel3_agua.png";
            Debug.Log($"[GeneradorArte] Generando: {nombre} (256x384, Agua y Reflejos)...");
            Texture2D tex = new Texture2D(ANCHO_FONDO, ALTO_FONDO, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);
            Color aguaOscura = new Color32(5, 10, 22, 255);
            Color aguaOndulada = new Color32(12, 24, 50, 255);
            Color reflejoCyan = new Color32(50, 150, 180, 180);

            for (int y = 0; y < ALTO_FONDO; y++)
            {
                for (int x = 0; x < ANCHO_FONDO; x++)
                {
                    if (y > 110)
                    {
                        tex.SetPixel(x, y, trans);
                    }
                    else
                    {
                        if ((y + (x / 8)) % 4 == 0)
                            tex.SetPixel(x, y, aguaOndulada);
                        else if ((y % 8 == 0) && (x % 16 < 4))
                            tex.SetPixel(x, y, reflejoCyan);
                        else
                            tex.SetPixel(x, y, aguaOscura);
                    }
                }
            }

            tex.Apply();
            GuardarYConfigurarSprite(tex, nombre, esFondo: true);
        }

        #endregion

        #region Fondos Nivel 2 (Bosque Profundo - 256x384, Verde Denso y Sombra Suave)

        private static void GenerarFondoBosqueProfundoLejanoN2()
        {
            string nombre = "fondo_nivel2_lejana.png";
            Texture2D tex = new Texture2D(ANCHO_FONDO, ALTO_FONDO, TextureFormat.RGBA32, false);
            Color32 cieloAlto = new Color32(0x1E, 0x35, 0x29, 0xFF);
            Color32 cieloHorizonte = new Color32(0x2F, 0x52, 0x3E, 0xFF);
            Color32 colina = new Color32(0x20, 0x3A, 0x2C, 0xFF);

            for (int x = 0; x < ANCHO_FONDO; x++)
            {
                float rad = (float)x / ANCHO_FONDO * Mathf.PI * 2f;
                int altColina = (int)(110 + Mathf.Sin(rad * 2f) * 18 + Mathf.Cos(rad * 3f) * 8);

                for (int y = 0; y < ALTO_FONDO; y++)
                {
                    if (y <= altColina)
                    {
                        tex.SetPixel(x, y, colina);
                    }
                    else
                    {
                        float t = Mathf.Clamp01((float)(y - altColina) / (ALTO_FONDO - altColina));
                        tex.SetPixel(x, y, Color.Lerp(cieloHorizonte, cieloAlto, t));
                    }
                }
            }

            tex.Apply();
            GuardarYConfigurarSprite(tex, nombre, esFondo: true);
        }

        private static void GenerarFondoBosqueProfundoMedioN2()
        {
            string nombre = "fondo_nivel2_media.png";
            Texture2D tex = new Texture2D(ANCHO_FONDO, ALTO_FONDO, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);
            Color32 basePino = new Color32(0x1B, 0x3B, 0x28, 0xFF);
            Color32 sombraPino = new Color32(0x11, 0x26, 0x1A, 0xFF);
            Color32 luzPino = new Color32(0x2D, 0x5A, 0x3E, 0xFF);

            for (int y = 0; y < ALTO_FONDO; y++)
                for (int x = 0; x < ANCHO_FONDO; x++)
                    tex.SetPixel(x, y, trans);

            int[] centros = { 16, 48, 80, 112, 144, 176, 208, 240 };
            int[] alturas = { 285, 310, 275, 320, 290, 315, 280, 305 };

            for (int i = 0; i < centros.Length; i++)
            {
                DibujarPinoProcedural(tex, ANCHO_FONDO, ALTO_FONDO, centros[i], alturas[i], basePino, sombraPino, luzPino, niveles: 7, anchoMaximoBase: 28);
            }

            tex.Apply();
            GuardarYConfigurarSprite(tex, nombre, esFondo: true);
        }

        private static void GenerarFondoBosqueProfundoCercanoN2()
        {
            string nombre = "fondo_nivel2_cercana.png";
            Texture2D tex = new Texture2D(ANCHO_FONDO, ALTO_FONDO, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);
            Color32 tronco = new Color32(0x12, 0x20, 0x17, 0xFF);
            Color32 basePino = new Color32(0x15, 0x2F, 0x20, 0xFF);
            Color32 sombraPino = new Color32(0x0C, 0x1D, 0x13, 0xFF);
            Color32 luzPino = new Color32(0x23, 0x48, 0x31, 0xFF);

            for (int y = 0; y < ALTO_FONDO; y++)
                for (int x = 0; x < ANCHO_FONDO; x++)
                    tex.SetPixel(x, y, trans);

            int[] centros = { 28, 92, 156, 220 };
            int[] alturas = { 355, 372, 360, 368 };

            foreach (int cx in centros)
            {
                for (int y = 0; y < 80; y++)
                    for (int x = cx - 5; x <= cx + 5; x++)
                        tex.SetPixel((x + ANCHO_FONDO) % ANCHO_FONDO, y, tronco);
            }

            for (int i = 0; i < centros.Length; i++)
            {
                DibujarPinoProcedural(tex, ANCHO_FONDO, ALTO_FONDO, centros[i], alturas[i], basePino, sombraPino, luzPino, niveles: 8, anchoMaximoBase: 38);
            }

            tex.Apply();
            GuardarYConfigurarSprite(tex, nombre, esFondo: true);
        }

        #endregion

        #region Fondos Nivel 4 (Cima de los Recuerdos - Atardecer Dorado/Púrpura y Castillo de Chapultepec)

        private static void GenerarFondoAtardecerLejanoN4()
        {
            string nombre = "fondo_nivel4_lejana.png";
            Texture2D tex = new Texture2D(ANCHO_FONDO, ALTO_FONDO, TextureFormat.RGBA32, false);
            Color32 purpura = new Color32(0x3B, 0x1E, 0x4B, 0xFF);
            Color32 magenta = new Color32(0x8E, 0x3B, 0x62, 0xFF);
            Color32 dorado = new Color32(0xF2, 0xA6, 0x5A, 0xFF);
            Color32 sol = new Color32(0xFF, 0xE1, 0x8F, 0xFF);

            for (int y = 0; y < ALTO_FONDO; y++)
            {
                float t = (float)y / ALTO_FONDO;
                Color c = t < 0.45f
                    ? Color.Lerp(dorado, magenta, t / 0.45f)
                    : Color.Lerp(magenta, purpura, (t - 0.45f) / 0.55f);

                for (int x = 0; x < ANCHO_FONDO; x++)
                {
                    tex.SetPixel(x, y, c);
                }
            }

            // Sol poniente dorado
            int solX = 128;
            int solY = 175;
            int radio = 28;
            for (int y = solY - radio; y <= solY + radio; y++)
            {
                for (int x = solX - radio; x <= solX + radio; x++)
                {
                    if ((x - solX) * (x - solX) + (y - solY) * (y - solY) <= radio * radio)
                    {
                        tex.SetPixel(x, y, sol);
                    }
                }
            }

            tex.Apply();
            GuardarYConfigurarSprite(tex, nombre, esFondo: true);
        }

        private static void GenerarFondoCastilloChapultepecMedioN4()
        {
            string nombre = "fondo_nivel4_media.png";
            Texture2D tex = new Texture2D(ANCHO_FONDO, ALTO_FONDO, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);
            Color32 cerro = new Color32(0x2A, 0x17, 0x35, 0xFF);
            Color32 cerroLuz = new Color32(0x45, 0x24, 0x4E, 0xFF);

            for (int y = 0; y < ALTO_FONDO; y++)
                for (int x = 0; x < ANCHO_FONDO; x++)
                    tex.SetPixel(x, y, trans);

            // Colinas suaves de Chapultepec sin repetición del castillo
            for (int x = 0; x < ANCHO_FONDO; x++)
            {
                float rad = (float)x / ANCHO_FONDO * Mathf.PI * 2f;
                int altCerro = (int)(115 + Mathf.Sin(rad) * 20 + Mathf.Cos(rad * 2f) * 10);
                for (int y = 0; y <= altCerro; y++)
                {
                    tex.SetPixel(x, y, y >= altCerro - 3 ? cerroLuz : cerro);
                }
            }

            tex.Apply();
            GuardarYConfigurarSprite(tex, nombre, esFondo: true);
        }

        private static void GenerarFondoCastilloLandmarkN4()
        {
            string nombre = "fondo_nivel4_castillo.png";
            Texture2D tex = new Texture2D(ANCHO_FONDO, ALTO_FONDO, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);
            Color32 cerro = new Color32(0x2A, 0x17, 0x35, 0xFF);
            Color32 castillo = new Color32(0x1D, 0x0F, 0x26, 0xFF);
            Color32 ventanaLuz = new Color32(0xFF, 0xC8, 0x6B, 0xFF);

            for (int y = 0; y < ALTO_FONDO; y++)
                for (int x = 0; x < ANCHO_FONDO; x++)
                    tex.SetPixel(x, y, trans);

            // Cerro del Chapulín (elevación central)
            for (int x = 0; x < ANCHO_FONDO; x++)
            {
                float dx = Mathf.Abs(x - 128f) / 128f;
                int altCerro = (int)(135 - dx * dx * 65 + Mathf.Sin(x * 0.08f) * 4);
                for (int y = 0; y <= altCerro; y++)
                {
                    tex.SetPixel(x, y, cerro);
                }
            }

            // Silueta del Castillo de Chapultepec en la cima (X: 72..184, Y: 130..238)
            for (int y = 130; y <= 178; y++)
                for (int x = 72; x <= 184; x++)
                    tex.SetPixel(x, y, castillo);

            for (int x = 72; x <= 184; x += 6)
                for (int y = 179; y <= 185; y++)
                    for (int k = 0; k < 3 && x + k <= 184; k++)
                        tex.SetPixel(x + k, y, castillo);

            // Torreón del Caballero Alto
            for (int y = 179; y <= 228; y++)
                for (int x = 94; x <= 122; x++)
                    tex.SetPixel(x, y, castillo);
            for (int y = 229; y <= 238; y++)
                for (int x = 104; x <= 112; x++)
                    tex.SetPixel(x, y, castillo);

            // Cúpula / mirador derecho
            for (int y = 179; y <= 205; y++)
                for (int x = 148; x <= 172; x++)
                    tex.SetPixel(x, y, castillo);

            // Ventanas cálidas iluminadas
            for (int x = 84; x <= 172; x += 14)
                for (int y = 148; y <= 160; y++)
                    for (int k = 0; k < 4; k++)
                        tex.SetPixel(x + k, y, ventanaLuz);

            for (int y = 195; y <= 212; y++)
                for (int x = 105; x <= 111; x++)
                    tex.SetPixel(x, y, ventanaLuz);

            tex.Apply();
            GuardarYConfigurarSprite(tex, nombre, esFondo: true);
        }

        private static void GenerarFondoAtardecerCercanoN4()
        {
            string nombre = "fondo_nivel4_cercana.png";
            Texture2D tex = new Texture2D(ANCHO_FONDO, ALTO_FONDO, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);
            Color32 pinoOscuro = new Color32(0x18, 0x0E, 0x20, 0xFF);
            Color32 pinoSombra = new Color32(0x10, 0x08, 0x16, 0xFF);
            Color32 pinoLuz = new Color32(0x3D, 0x20, 0x3B, 0xFF);

            for (int y = 0; y < ALTO_FONDO; y++)
                for (int x = 0; x < ANCHO_FONDO; x++)
                    tex.SetPixel(x, y, trans);

            int[] centros = { 18, 72, 188, 238 };
            int[] alturas = { 280, 240, 245, 290 };

            for (int i = 0; i < centros.Length; i++)
            {
                DibujarPinoProcedural(tex, ANCHO_FONDO, ALTO_FONDO, centros[i], alturas[i], pinoOscuro, pinoSombra, pinoLuz, niveles: 6, anchoMaximoBase: 26);
            }

            tex.Apply();
            GuardarYConfigurarSprite(tex, nombre, esFondo: true);
        }

        #endregion

        #region Logo del Equipo

        private static void GenerarLogoEquipo()
        {
            string nombre = "logo_equipo.png";
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color trans = new Color(0, 0, 0, 0);
            Color32 fondoCircular = new Color32(0x1B, 0x3B, 0x2B, 0xFF);
            Color32 bordeDorado = new Color32(0xF5, 0xC4, 0x43, 0xFF);
            Color32 hojaVerde = new Color32(0x52, 0xB7, 0x88, 0xFF);
            Color32 semillaOro = new Color32(0xFF, 0xE6, 0x80, 0xFF);

            int c = size / 2;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    if (dist <= 58f && dist >= 53f)
                    {
                        tex.SetPixel(x, y, bordeDorado);
                    }
                    else if (dist < 53f)
                    {
                        tex.SetPixel(x, y, fondoCircular);
                    }
                    else
                    {
                        tex.SetPixel(x, y, trans);
                    }
                }
            }

            // Brote central (hojas + semilla de luz)
            for (int y = 34; y < 86; y++)
            {
                for (int x = c - 3; x <= c + 3; x++)
                {
                    tex.SetPixel(x, y, hojaVerde);
                }
            }
            for (int y = 52; y < 82; y++)
            {
                int dx = (y - 52);
                for (int x = 0; x < 18; x++)
                {
                    if (Mathf.Abs(x - dx * 0.5f) < 6f)
                    {
                        tex.SetPixel(c - 4 - x, y, hojaVerde);
                        tex.SetPixel(c + 4 + x, y, hojaVerde);
                    }
                }
            }
            for (int y = 74; y <= 98; y++)
            {
                for (int x = c - 12; x <= c + 12; x++)
                {
                    if ((x - c) * (x - c) + (y - 86) * (y - 86) <= 130)
                    {
                        tex.SetPixel(x, y, semillaOro);
                    }
                }
            }

            tex.Apply();
            GuardarYConfigurarSprite(tex, nombre, esFondo: false);
        }

        #endregion

        #region Utilidades y Configuración de Importer

        private static void DibujarPatron(Texture2D tex, int[,] patron, params Color[] paleta)
        {
            int h = patron.GetLength(0);
            int w = patron.GetLength(1);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = patron[h - 1 - y, x];
                    tex.SetPixel(x, y, paleta[idx]);
                }
            }
            tex.Apply();
        }

        private static void GuardarYConfigurarSprite(Texture2D tex, string nombreArchivo, bool esFondo = false)
        {
            byte[] bytes = tex.EncodeToPNG();
            string rutaCompleta = Path.Combine(RUTA_GENERADOS, nombreArchivo);
            File.WriteAllBytes(rutaCompleta, bytes);

            AssetDatabase.ImportAsset(rutaCompleta, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(rutaCompleta) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = PPU;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;

                if (esFondo)
                {
                    // Tiling únicamente en el eje horizontal (U=Repeat), sin repetición vertical (V=Clamp)
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.wrapModeU = TextureWrapMode.Repeat;
                    importer.wrapModeV = TextureWrapMode.Clamp;
                }
                else
                {
                    importer.wrapMode = TextureWrapMode.Clamp;
                }

                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spritePixelsPerUnit = PPU;
                settings.spriteMeshType = SpriteMeshType.FullRect;

                if (esFondo)
                {
                    // Pivot al centro-abajo (Bottom-Center) para alinear la base del bosque con los suelos
                    settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                    settings.spritePivot = new Vector2(0.5f, 0f);
                    importer.spritePivot = new Vector2(0.5f, 0f);
                }
                else
                {
                    settings.spriteAlignment = (int)SpriteAlignment.Center;
                    settings.spritePivot = new Vector2(0.5f, 0.5f);
                    importer.spritePivot = new Vector2(0.5f, 0.5f);
                }

                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }

            Debug.Log($"[GeneradorArte] Creado e importado: {rutaCompleta} (Fondo={esFondo}, Pivot={(esFondo ? "BottomCenter" : "Center")})");
        }

        #endregion
    }
}
