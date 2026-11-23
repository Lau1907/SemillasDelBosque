using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SemillasDelBosque.Sistema;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace SemillasDelBosque.UI
{
    /// <summary>
    /// ResumenNivel: Se coloca en el objeto 'PanelNivelCompleto'.
    /// Cuando el panel se activa (OnEnable) al alcanzar la meta,
    /// muestra el total real de semillas ('Semillas recogidas: X / Total')
    /// y el tiempo transcurrido en formato 00:00 organizado con VerticalLayoutGroup.
    /// </summary>
    public class ResumenNivel : MonoBehaviour
    {
        [Header("Referencias de UI")]
        [SerializeField] private TextMeshProUGUI textoTitulo;
        [SerializeField] private TextMeshProUGUI textoSemillas;
        [SerializeField] private TextMeshProUGUI textoTiempo;

        [Header("Datos de Semillas")]
        [SerializeField] private int totalSemillas = 0;

        public TextMeshProUGUI TextoTitulo { get => textoTitulo; set => textoTitulo = value; }
        public TextMeshProUGUI TextoSemillas { get => textoSemillas; set => textoSemillas = value; }
        public TextMeshProUGUI TextoTiempo { get => textoTiempo; set => textoTiempo = value; }
        public int TotalSemillas { get => totalSemillas; set => totalSemillas = value; }

        private void Awake()
        {
            BuscarReferencias();
            if (totalSemillas <= 0)
            {
                totalSemillas = UtilidadesJugador.ContarSemillasEnEscena(gameObject.scene);
            }
        }

        private void Start()
        {
            if (totalSemillas <= 0)
            {
                totalSemillas = UtilidadesJugador.ContarSemillasEnEscena(gameObject.scene);
            }
            if (HudJuego.Instancia != null && HudJuego.Instancia.TotalSemillas > totalSemillas)
            {
                totalSemillas = HudJuego.Instancia.TotalSemillas;
            }
        }

        private void OnEnable()
        {
            BuscarReferencias();
            if (HudJuego.Instancia != null && HudJuego.Instancia.TotalSemillas > 0)
            {
                totalSemillas = HudJuego.Instancia.TotalSemillas;
            }
            else if (totalSemillas <= 0)
            {
                totalSemillas = UtilidadesJugador.ContarSemillasEnEscena(gameObject.scene);
            }

            ActualizarTexto();
        }

        private void BuscarReferencias()
        {
            if (textoTitulo == null)
            {
                Transform t = transform.Find("Text (TMP)") ?? transform.Find("TextoTitulo");
                if (t != null) textoTitulo = t.GetComponent<TextMeshProUGUI>();
            }

            if (textoSemillas == null)
            {
                Transform t = transform.Find("TextoSemillas");
                if (t != null) textoSemillas = t.GetComponent<TextMeshProUGUI>();
            }

            if (textoTiempo == null)
            {
                Transform t = transform.Find("TextoTiempo");
                if (t != null) textoTiempo = t.GetComponent<TextMeshProUGUI>();
            }
        }

        public void ActualizarTexto()
        {
            int semillas = UtilidadesJugador.LeerVariableEscena("semillas", 0);

            if (textoSemillas != null)
            {
                textoSemillas.text = $"Semillas recogidas: {semillas} / {totalSemillas}";
            }

            if (textoTiempo != null)
            {
                float t = (HudJuego.Instancia != null) ? HudJuego.Instancia.TiempoTranscurrido : 0f;
                int min = Mathf.FloorToInt(t / 60f);
                int sec = Mathf.FloorToInt(t % 60f);
                textoTiempo.text = string.Format("Tiempo transcurrido: {0:00}:{1:00}", min, sec);
            }
        }

#if UNITY_EDITOR
        [MenuItem("Semillas del Bosque/UI/Configurar Resumen de Meta")]
        public static void MenuConfigurarResumen()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Transform panelCompleto = canvas.transform.Find("PanelNivelCompleto");
            if (panelCompleto == null) return;

            Undo.RegisterCompleteObjectUndo(panelCompleto.gameObject, "Configurar Resumen Nivel");
            ResumenNivel resumen = panelCompleto.GetComponent<ResumenNivel>();
            if (resumen == null) resumen = Undo.AddComponent<ResumenNivel>(panelCompleto.gameObject);

            resumen.BuscarReferencias();
            resumen.ActualizarTexto();
            EditorUtility.SetDirty(panelCompleto.gameObject);
        }
#endif
    }
}