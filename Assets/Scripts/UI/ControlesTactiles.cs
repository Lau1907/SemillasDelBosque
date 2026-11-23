using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SemillasDelBosque.Jugador;

namespace SemillasDelBosque.UI
{
    /// <summary>
    /// ControlesTactiles: Canvas/UI móvil con botones Izquierda, Derecha, Saltar y Pausa.
    /// Se activa automáticamente en Application.isMobilePlatform o con toggle en Menú Opciones.
    /// </summary>
    public class ControlesTactiles : MonoBehaviour
    {
        public const string PREF_CONTROLES_TACTILES = "ControlesTactiles";

        [Header("Botones Móviles")]
        [SerializeField] private Button botonIzquierda;
        [SerializeField] private Button botonDerecha;
        [SerializeField] private Button botonSaltar;
        [SerializeField] private Button botonPausa;

        public static bool DebeEstarActivo()
        {
            if (Application.isMobilePlatform) return true;
            return PlayerPrefs.GetInt(PREF_CONTROLES_TACTILES, 0) == 1;
        }

        private void Awake()
        {
            ActualizarVisibilidad();
            ConfigurarEventos();
        }

        private void OnEnable()
        {
            ActualizarVisibilidad();
        }

        public void ActualizarVisibilidad()
        {
            bool activo = DebeEstarActivo();
            // Si el objeto actual es un Canvas independiente, activamos/desactivamos sus hijos o componente Canvas
            var canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = activo;
            }
            else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(activo);
                }
            }
        }

        private void ConfigurarEventos()
        {
            if (botonIzquierda != null)
            {
                AgregarEventosPuntero(botonIzquierda.gameObject, 
                    () => ControladorAnimacionJugador.EntradaHorizontalVirtual = -1f,
                    () => { if (ControladorAnimacionJugador.EntradaHorizontalVirtual < 0f) ControladorAnimacionJugador.EntradaHorizontalVirtual = 0f; });
            }

            if (botonDerecha != null)
            {
                AgregarEventosPuntero(botonDerecha.gameObject, 
                    () => ControladorAnimacionJugador.EntradaHorizontalVirtual = 1f,
                    () => { if (ControladorAnimacionJugador.EntradaHorizontalVirtual > 0f) ControladorAnimacionJugador.EntradaHorizontalVirtual = 0f; });
            }

            if (botonSaltar != null)
            {
                AgregarEventosPuntero(botonSaltar.gameObject, 
                    () => ControladorAnimacionJugador.SaltoVirtualSolicitado = true,
                    null);
            }

            if (botonPausa != null)
            {
                botonPausa.onClick.RemoveAllListeners();
                botonPausa.onClick.AddListener(() =>
                {
                    var pausa = FindAnyObjectByType<MenuPausaController>();
                    if (pausa != null) pausa.TogglePausa();
                });
            }
        }

        private void AgregarEventosPuntero(GameObject target, System.Action alPresionar, System.Action alSoltar)
        {
            EventTrigger trigger = target.GetComponent<EventTrigger>();
            if (trigger == null) trigger = target.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            if (alPresionar != null)
            {
                EventTrigger.Entry entryDown = new EventTrigger.Entry();
                entryDown.eventID = EventTriggerType.PointerDown;
                entryDown.callback.AddListener((data) => alPresionar());
                trigger.triggers.Add(entryDown);
            }

            if (alSoltar != null)
            {
                EventTrigger.Entry entryUp = new EventTrigger.Entry();
                entryUp.eventID = EventTriggerType.PointerUp;
                entryUp.callback.AddListener((data) => alSoltar());
                trigger.triggers.Add(entryUp);
            }
        }
    }
}