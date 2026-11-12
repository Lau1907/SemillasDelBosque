using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SemillasDelBosque.UI;

namespace SemillasDelBosque.EditorTools
{
    /// <summary>
    /// ConstructorLibroCodice: Construye de forma reutilizable e idempotente la interfaz profesional
    /// de libro abierto de 2 páginas ("El Códice de Chapultepec") para el Menú Principal y los niveles.
    /// </summary>
    public static class ConstructorLibroCodice
    {
        public static LibroHistoriaController ConstruirLibroEnCanvas(
            Transform canvasTransform,
            bool permitirTeclaEnJuego,
            bool pausarAlAbrir)
        {
            Transform existente = canvasTransform.Find("LibroCodiceRaiz");
            if (existente != null)
            {
                Undo.DestroyObjectImmediate(existente.gameObject);
            }
            Transform existenteViejo = canvasTransform.Find("PanelLibroCodice");
            if (existenteViejo != null)
            {
                Undo.DestroyObjectImmediate(existenteViejo.gameObject);
            }

            // Contenedor siempre activo para que la tecla [L] funcione en juego aunque el libro esté cerrado
            GameObject goContenedor = new GameObject("LibroCodiceRaiz", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(goContenedor, "Crear LibroCodiceRaiz");
            goContenedor.transform.SetParent(canvasTransform, false);
            RectTransform rtCont = goContenedor.GetComponent<RectTransform>();
            rtCont.anchorMin = Vector2.zero;
            rtCont.anchorMax = Vector2.one;
            rtCont.offsetMin = Vector2.zero;
            rtCont.offsetMax = Vector2.zero;

            // Fondo oscuro traslúcido (este sí inicia oculto)
            GameObject goRaiz = new GameObject("PanelLibroCodice", typeof(RectTransform), typeof(Image));
            goRaiz.transform.SetParent(goContenedor.transform, false);

            RectTransform rtRaiz = goRaiz.GetComponent<RectTransform>();
            rtRaiz.anchorMin = Vector2.zero;
            rtRaiz.anchorMax = Vector2.one;
            rtRaiz.offsetMin = Vector2.zero;
            rtRaiz.offsetMax = Vector2.zero;
            goRaiz.GetComponent<Image>().color = new Color(0.03f, 0.05f, 0.04f, 0.80f);

            // Cubierta de cuero y oro del códice
            GameObject goCubierta = new GameObject("CubiertaLibro", typeof(RectTransform), typeof(Image), typeof(Outline));
            goCubierta.transform.SetParent(goRaiz.transform, false);
            RectTransform rtCub = goCubierta.GetComponent<RectTransform>();
            rtCub.anchorMin = new Vector2(0.5f, 0.5f);
            rtCub.anchorMax = new Vector2(0.5f, 0.5f);
            rtCub.sizeDelta = new Vector2(1060f, 640f);
            goCubierta.GetComponent<Image>().color = new Color32(0x2B, 0x1A, 0x12, 0xFF);
            Outline bordeOro = goCubierta.GetComponent<Outline>();
            bordeOro.effectColor = new Color32(0xD9, 0xA4, 0x41, 0xFF);
            bordeOro.effectDistance = new Vector2(4f, -4f);

            // Página Izquierda (Ilustración y Título)
            GameObject pagIzq = CrearPagina(goCubierta.transform, "PaginaIzquierda", new Vector2(-252f, 0f), new Vector2(486f, 592f));
            TextMeshProUGUI tmpCap = CrearTexto(pagIzq.transform, "Capitulo", "◈ PRÓLOGO ◈", 18, FontStyles.Bold,
                new Color32(0x8C, 0x3B, 0x1E, 0xFF), new Vector2(0f, 250f), new Vector2(440f, 32f), TextAlignmentOptions.Center);
            TextMeshProUGUI tmpTit = CrearTexto(pagIzq.transform, "TituloPagina", "Leyenda", 26, FontStyles.Bold,
                new Color32(0x26, 0x16, 0x0C, 0xFF), new Vector2(0f, 198f), new Vector2(440f, 68f), TextAlignmentOptions.Center);

            // Marco de ilustración
            GameObject marcoIlust = new GameObject("MarcoIlustracion", typeof(RectTransform), typeof(Image), typeof(Outline));
            marcoIlust.transform.SetParent(pagIzq.transform, false);
            RectTransform rtMarco = marcoIlust.GetComponent<RectTransform>();
            rtMarco.anchorMin = new Vector2(0.5f, 0.5f);
            rtMarco.anchorMax = new Vector2(0.5f, 0.5f);
            rtMarco.anchoredPosition = new Vector2(0f, -15f);
            rtMarco.sizeDelta = new Vector2(300f, 280f);
            marcoIlust.GetComponent<Image>().color = new Color32(0x1C, 0x24, 0x1F, 0xFF);
            Outline outlineMarco = marcoIlust.GetComponent<Outline>();
            outlineMarco.effectColor = new Color32(0xB5, 0x83, 0x30, 0xFF);
            outlineMarco.effectDistance = new Vector2(3f, -3f);

            GameObject goImg = new GameObject("Ilustracion", typeof(RectTransform), typeof(Image));
            goImg.transform.SetParent(marcoIlust.transform, false);
            RectTransform rtImg = goImg.GetComponent<RectTransform>();
            rtImg.anchorMin = Vector2.zero;
            rtImg.anchorMax = Vector2.one;
            rtImg.offsetMin = new Vector2(14f, 14f);
            rtImg.offsetMax = new Vector2(-14f, -14f);
            Image imgIlust = goImg.GetComponent<Image>();
            imgIlust.preserveAspect = true;

            TextMeshProUGUI tmpLugar = CrearTexto(pagIzq.transform, "SubtituloLugar", "Bosque de Chapultepec", 17, FontStyles.Italic,
                new Color32(0x5A, 0x3C, 0x28, 0xFF), new Vector2(0f, -210f), new Vector2(440f, 52f), TextAlignmentOptions.Center);

            // Lomo central del libro
            GameObject lomo = new GameObject("LomoCentral", typeof(RectTransform), typeof(Image));
            lomo.transform.SetParent(goCubierta.transform, false);
            RectTransform rtLomo = lomo.GetComponent<RectTransform>();
            rtLomo.anchorMin = new Vector2(0.5f, 0.5f);
            rtLomo.anchorMax = new Vector2(0.5f, 0.5f);
            rtLomo.sizeDelta = new Vector2(20f, 598f);
            lomo.GetComponent<Image>().color = new Color(0.08f, 0.04f, 0.02f, 0.88f);

            // Página Derecha (Texto narrativo + Navegación)
            GameObject pagDer = CrearPagina(goCubierta.transform, "PaginaDerecha", new Vector2(252f, 0f), new Vector2(486f, 592f));
            TextMeshProUGUI tmpHistoria = CrearTexto(pagDer.transform, "TextoHistoria", "...", 21, FontStyles.Normal,
                new Color32(0x2B, 0x1A, 0x0E, 0xFF), new Vector2(0f, 42f), new Vector2(420f, 410f), TextAlignmentOptions.TopLeft);
            tmpHistoria.textWrappingMode = TextWrappingModes.Normal;

            TextMeshProUGUI tmpNum = CrearTexto(pagDer.transform, "NumeroPagina", "Folio 1 / 6", 17, FontStyles.Bold,
                new Color32(0x6E, 0x48, 0x2A, 0xFF), new Vector2(0f, -190f), new Vector2(220f, 28f), TextAlignmentOptions.Center);

            Button btnAnt = CrearBotonLibro(pagDer.transform, "BotonAnterior", "◄ Anterior", new Vector2(-145f, -245f), new Vector2(150f, 44f), new Color32(0x5E, 0x3D, 0x26, 0xFF));
            Button btnSig = CrearBotonLibro(pagDer.transform, "BotonSiguiente", "Siguiente ►", new Vector2(15f, -245f), new Vector2(150f, 44f), new Color32(0x1F, 0x54, 0x3A, 0xFF));
            Button btnCerrar = CrearBotonLibro(pagDer.transform, "BotonCerrar", "Cerrar ✕", new Vector2(165f, -245f), new Vector2(120f, 44f), new Color32(0x6E, 0x25, 0x25, 0xFF));

            // Controlador en el contenedor activo + sprites serializados para builds
            LibroHistoriaController ctrl = goContenedor.AddComponent<LibroHistoriaController>();
            ctrl.PanelLibroRaiz = goRaiz;
            ctrl.TmpCapitulo = tmpCap;
            ctrl.TmpTitulo = tmpTit;
            ctrl.TmpLugar = tmpLugar;
            ctrl.TmpHistoria = tmpHistoria;
            ctrl.TmpNumeroPagina = tmpNum;
            ctrl.ImagenIlustracion = imgIlust;
            ctrl.BotonAnterior = btnAnt;
            ctrl.BotonSiguiente = btnSig;
            ctrl.BotonCerrar = btnCerrar;
            ctrl.PermitirTeclaEnJuego = permitirTeclaEnJuego;
            ctrl.PausarAlAbrir = pausarAlAbrir;

            ctrl.IlustracionesPorPagina = new Sprite[]
            {
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objetos/semilla_luz.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Personajes/BuhoSabio_Rama.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Animaciones/Nahui/nahui_doble_salto_0.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/fondo_nivel3_media.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/fondo_nivel4_castillo.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Animaciones/Sombra/sombra_ataque_1.png")
            };

            goRaiz.SetActive(false);
            EditorUtility.SetDirty(goContenedor);
            return ctrl;
        }

        private static GameObject CrearPagina(Transform padre, string nombre, Vector2 pos, Vector2 size)
        {
            GameObject pag = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            pag.transform.SetParent(padre, false);
            RectTransform rt = pag.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            pag.GetComponent<Image>().color = new Color32(0xF1, 0xE3, 0xC4, 0xFF);
            return pag;
        }

        private static TextMeshProUGUI CrearTexto(Transform padre, string nombre, string contenido, float tam, FontStyles estilo, Color col, Vector2 pos, Vector2 size, TextAlignmentOptions alineacion)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(padre, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = contenido;
            tmp.fontSize = tam;
            tmp.fontStyle = estilo;
            tmp.color = col;
            tmp.alignment = alineacion;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CrearBotonLibro(Transform padre, string nombre, string texto, Vector2 pos, Vector2 size, Color colorFondo)
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

            Outline outl = go.GetComponent<Outline>();
            outl.effectColor = new Color(0.9f, 0.72f, 0.3f, 0.6f);
            outl.effectDistance = new Vector2(2f, -2f);

            Button btn = go.GetComponent<Button>();
            btn.targetGraphic = img;

            TextMeshProUGUI tmp = CrearTexto(go.transform, "Texto", texto, 17, FontStyles.Bold, Color.white, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);
            RectTransform rtTxt = tmp.GetComponent<RectTransform>();
            rtTxt.anchorMin = Vector2.zero;
            rtTxt.anchorMax = Vector2.one;
            rtTxt.offsetMin = Vector2.zero;
            rtTxt.offsetMax = Vector2.zero;

            return btn;
        }
    }
}
