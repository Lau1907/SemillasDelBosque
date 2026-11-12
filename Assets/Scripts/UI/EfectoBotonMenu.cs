using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SemillasDelBosque.UI
{
    /// <summary>
    /// EfectoBotonMenu: Añade respuesta visual fluida (escala suave y resplandor dorado)
    /// al pasar el cursor o seleccionar botones del menú y del códice.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class EfectoBotonMenu : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private float escalaHover = 1.05f;
        [SerializeField] private float velocidad = 12f;
        [SerializeField] private Outline bordeResplandor;
        [SerializeField] private Color colorBordeNormal = new Color(0.85f, 0.65f, 0.25f, 0.45f);
        [SerializeField] private Color colorBordeHover = new Color(1f, 0.88f, 0.42f, 1f);

        private Vector3 escalaObjetivo = Vector3.one;
        private bool resaltado = false;

        public Outline BordeResplandor { get => bordeResplandor; set => bordeResplandor = value; }

        private void Awake()
        {
            if (bordeResplandor == null) bordeResplandor = GetComponent<Outline>();
            AplicarBorde(false);
        }

        private void OnDisable()
        {
            transform.localScale = Vector3.one;
            escalaObjetivo = Vector3.one;
            resaltado = false;
            AplicarBorde(false);
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                escalaObjetivo,
                Time.unscaledDeltaTime * velocidad);
        }

        public void OnPointerEnter(PointerEventData eventData) => Activar(true);
        public void OnPointerExit(PointerEventData eventData) => Activar(false);
        public void OnSelect(BaseEventData eventData) => Activar(true);
        public void OnDeselect(BaseEventData eventData) => Activar(false);

        private void Activar(bool estado)
        {
            if (resaltado == estado) return;
            resaltado = estado;
            escalaObjetivo = estado ? Vector3.one * escalaHover : Vector3.one;
            AplicarBorde(estado);
        }

        private void AplicarBorde(bool estado)
        {
            if (bordeResplandor != null)
            {
                bordeResplandor.effectColor = estado ? colorBordeHover : colorBordeNormal;
            }
        }
    }
}

