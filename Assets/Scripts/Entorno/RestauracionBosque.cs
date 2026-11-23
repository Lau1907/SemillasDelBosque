using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using SemillasDelBosque.Audio;
using SemillasDelBosque.Sistema;

namespace SemillasDelBosque.Entorno
{
    /// <summary>
    /// RestauracionBosque: Monitorea las semillas recogidas en la escena.
    /// Cuando Luna recoge todas las semillas de luz, inicia una transición suave (2 segundos)
    /// donde el Global Light 2D y el tinte de los fondos cambian de tonos grises/apagados
    /// a tonos cálidos verdes y dorados. También notifica a GestorMusica.Instancia.ZonaRestaurada().
    /// </summary>
    public class RestauracionBosque : MonoBehaviour
    {
        [Header("Componentes a Modificar")]
        [Tooltip("Luz 2D Global del nivel.")]
        [SerializeField] private Light2D luzGlobal;

        [Tooltip("Renderers de las capas de fondo a tintar al revivir el bosque.")]
        [SerializeField] private SpriteRenderer[] capasFondo;

        [Header("Colores Restaurados (Verde y Dorado Cálido)")]
        [Tooltip("Color objetivo para la luz global al restaurarse.")]
        [SerializeField] private Color colorLuzRestaurada = new Color(1.05f, 1.0f, 0.88f, 1f);

        [Tooltip("Color de tinte para los fondos al restaurarse el bosque.")]
        [SerializeField] private Color colorFondoRestaurado = new Color(0.95f, 1.15f, 0.95f, 1f);

        [Header("Parámetros de Transición")]
        [Tooltip("Duración en segundos de la transición de color.")]
        [SerializeField] private float duracionTransicion = 2.0f;

        [Header("Datos de Semillas")]
        [SerializeField] private int totalSemillas = 0;
        [SerializeField] private bool zonaRestaurada = false;

        public bool ZonaRestaurada => zonaRestaurada;

        private void Start()
        {
            if (luzGlobal == null)
            {
                Light2D[] luces = FindObjectsByType<Light2D>();
                foreach (Light2D l in luces)
                {
                    if (l.lightType == Light2D.LightType.Global)
                    {
                        luzGlobal = l;
                        break;
                    }
                }
            }

            if (capasFondo == null || capasFondo.Length == 0)
            {
                capasFondo = GetComponentsInChildren<SpriteRenderer>(true);
            }

            CalcularTotalSemillas();
        }

        private void Update()
        {
            if (zonaRestaurada) return;

            int semillasActuales = UtilidadesJugador.LeerVariableEscena("semillas", 0);

            // Si se han recogido todas las semillas del nivel
            if (totalSemillas > 0 && semillasActuales >= totalSemillas)
            {
                zonaRestaurada = true;
                IniciarRestauracion();
            }
        }

        /// <summary>
        /// Cuenta cuántas semillas existen en la escena
        /// </summary>
        public void CalcularTotalSemillas()
        {
            totalSemillas = UtilidadesJugador.ContarSemillasEnEscena(gameObject.scene);
        }

        /// <summary>
        /// Configura manualmente las capas de fondo y la luz desde el constructor de entorno
        /// </summary>
        public void Configurar(Light2D luz, SpriteRenderer[] fondos)
        {
            luzGlobal = luz;
            capasFondo = fondos;
            CalcularTotalSemillas();
        }

        /// <summary>
        /// Inicia la corrutina de transición visual y activa la música de zona restaurada.
        /// </summary>
        public void IniciarRestauracion()
        {
            Debug.Log("[RestauracionBosque] ¡Todas las semillas recogidas! Restaurando el bosque con luz y color...");

            // Notificar al gestor de música
            if (GestorMusica.Instancia != null)
            {
                GestorMusica.Instancia.ZonaRestaurada();
            }

            StartCoroutine(CorrutinaTransicion());
        }

        private IEnumerator CorrutinaTransicion()
        {
            Color colorInicialLuz = (luzGlobal != null) ? luzGlobal.color : Color.white;
            float intensidadInicial = (luzGlobal != null) ? luzGlobal.intensity : 0.65f;
            float intensidadObjetivo = 0.95f;
            
            Color[] coloresInicialesFondos = null;
            if (capasFondo != null && capasFondo.Length > 0)
            {
                coloresInicialesFondos = new Color[capasFondo.Length];
                for (int i = 0; i < capasFondo.Length; i++)
                {
                    coloresInicialesFondos[i] = (capasFondo[i] != null) ? capasFondo[i].color : Color.white;
                }
            }

            float tiempo = 0f;
            while (tiempo < duracionTransicion)
            {
                tiempo += Time.deltaTime;
                float t = Mathf.Clamp01(tiempo / duracionTransicion);

                // Interpolar color e intensidad de la luz global
                if (luzGlobal != null)
                {
                    luzGlobal.color = Color.Lerp(colorInicialLuz, colorLuzRestaurada, t);
                    luzGlobal.intensity = Mathf.Lerp(intensidadInicial, intensidadObjetivo, t);
                }

                // Interpolar tinte de los fondos
                if (capasFondo != null && coloresInicialesFondos != null)
                {
                    for (int i = 0; i < capasFondo.Length; i++)
                    {
                        if (capasFondo[i] != null)
                        {
                            capasFondo[i].color = Color.Lerp(coloresInicialesFondos[i], colorFondoRestaurado, t);
                        }
                    }
                }

                yield return null;
            }

            // Asegurar valores finales exactos
            if (luzGlobal != null)
            {
                luzGlobal.color = colorLuzRestaurada;
                luzGlobal.intensity = intensidadObjetivo;
            }
            if (capasFondo != null)
            {
                foreach (var f in capasFondo)
                {
                    if (f != null) f.color = colorFondoRestaurado;
                }
            }

            Debug.Log("[RestauracionBosque] ¡Zona completamente restaurada!");
        }
    }
}
