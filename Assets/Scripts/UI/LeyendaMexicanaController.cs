using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace SemillasDelBosque.UI
{
    /// <summary>
    /// LeyendaMexicanaController: Presenta al inicio de cada nivel un pergamino narrativo
    /// con la leyenda mexicana de Chapultepec y la mitología nahua correspondiente al nivel.
    /// Nunca bloquea el movimiento del jugador y puede cerrarse al hacer clic.
    /// </summary>
    public class LeyendaMexicanaController : MonoBehaviour
    {
        [SerializeField] private string tituloLeyenda;
        [TextArea(2, 5)]
        [SerializeField] private string textoLeyenda;
        [SerializeField] private float duracionVisible = 6.5f;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI tmpTitulo;
        [SerializeField] private TextMeshProUGUI tmpCuerpo;

        public CanvasGroup CanvasGroupPanel { get => canvasGroup; set => canvasGroup = value; }
        public TextMeshProUGUI TmpTitulo { get => tmpTitulo; set => tmpTitulo = value; }
        public TextMeshProUGUI TmpCuerpo { get => tmpCuerpo; set => tmpCuerpo = value; }

        private void Start()
        {
            if (string.IsNullOrEmpty(tituloLeyenda) || string.IsNullOrEmpty(textoLeyenda))
            {
                AsignarLeyendaPorEscena(SceneManager.GetActiveScene().name);
            }

            if (tmpTitulo != null) tmpTitulo.text = tituloLeyenda;
            if (tmpCuerpo != null) tmpCuerpo.text = textoLeyenda;

            StartCoroutine(MostrarYOcultar());
        }

        public void ConfigurarTextos(string titulo, string cuerpo)
        {
            tituloLeyenda = titulo;
            textoLeyenda = cuerpo;
            if (tmpTitulo != null) tmpTitulo.text = tituloLeyenda;
            if (tmpCuerpo != null) tmpCuerpo.text = textoLeyenda;
        }

        public void CerrarAhora()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }

        private IEnumerator MostrarYOcultar()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                float tIn = 0f;
                while (tIn < 0.45f)
                {
                    tIn += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Clamp01(tIn / 0.45f);
                    yield return null;
                }
                canvasGroup.alpha = 1f;
            }

            yield return new WaitForSeconds(duracionVisible);

            if (canvasGroup != null)
            {
                float tOut = 0f;
                while (tOut < 0.75f)
                {
                    tOut += Time.deltaTime;
                    canvasGroup.alpha = 1f - Mathf.Clamp01(tOut / 0.75f);
                    yield return null;
                }
            }

            gameObject.SetActive(false);
        }

        public void AsignarLeyendaPorEscena(string nombreEscena)
        {
            if (nombreEscena.Contains("Bosque") || nombreEscena.Contains("Poetas"))
            {
                tituloLeyenda = "II. Los Cantos de Nezahualcóyotl — Calzada de los Poetas";
                textoLeyenda = "«No para siempre en la tierra, solo un poco aquí...»\n" +
                               "Entre los versos grabados en piedra de Chapultepec, invoca el aliento de Ehécatl (Doble Salto) y salta 2 veces sobre las sombras.";
            }
            else if (nombreEscena.Contains("Lago") || nombreEscena.Contains("Luciernagas"))
            {
                tituloLeyenda = "III. Las Aguas de Chalchiuhtlicue — Lago Mayor";
                textoLeyenda = "Cuenta la leyenda mexica que las aguas del antiguo lago reflejaban el cielo de Tenochtitlan.\n" +
                               "Sigue la luz de las luciérnagas y devuelve el color al lago sagrado.";
            }
            else if (nombreEscena.Contains("Cima") || nombreEscena.Contains("Recuerdos"))
            {
                tituloLeyenda = "IV. El Cerro del Chapulín — Castillo de Chapultepec";
                textoLeyenda = "En lo alto de Chapultépec («Cerro del Saltamontes»), el Gran Nahual de la Niebla resguarda la última luz.\n" +
                               "¡Salta 5 veces sobre su cabeza para liberar el Bosque de México!";
            }
            else
            {
                tituloLeyenda = "I. El Ahuehuete Sagrado — Jardín Botánico de Chapultepec";
                textoLeyenda = "Cuenta la leyenda del Anáhuac que el viejo Ahuehuete guardaba la luz del Quinto Sol hasta que el Nahual la dispersó.\n" +
                               "¡Despierta, Nahui! Salta sobre las sombras (2 golpes) y reúne las Semillas de Luz.";
            }
        }
    }
}

