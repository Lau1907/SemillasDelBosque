using UnityEngine;

namespace SemillasDelBosque.Nivel3
{
    /// <summary>
    /// Script auxiliar que emparenta al jugador (Luna) cuando aterriza sobre la plataforma móvil
    /// para que viaje solidariamente sobre ella sin resbalar ni desincronizarse.
    /// Al salir de la plataforma o al desactivarse, desvincula al pasajero.
    /// </summary>
    public class PlataformaPasajero : MonoBehaviour
    {
        [Tooltip("Tag del jugador para validar la colisión.")]
        [SerializeField] private string tagJugador = "Luna";

        private Transform pasajeroActual = null;
        private Transform padreOriginalPasajero = null;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (EsElJugador(collision.gameObject))
            {
                // Comprobamos que el contacto provenga desde arriba (normal apuntando hacia abajo en la plataforma)
                if (collision.contactCount > 0 && collision.GetContact(0).normal.y < -0.4f)
                {
                    pasajeroActual = collision.transform;
                    padreOriginalPasajero = pasajeroActual.parent;
                    pasajeroActual.SetParent(transform);
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (EsElJugador(collision.gameObject) && pasajeroActual != null)
            {
                if (collision.transform == pasajeroActual)
                {
                    LiberarPasajero();
                }
            }
        }

        private void OnDisable()
        {
            // Si la plataforma se oculta, destruye o reinicia mientras transporta al jugador
            LiberarPasajero();
        }

        private void LiberarPasajero()
        {
            if (pasajeroActual != null)
            {
                // Restauramos el contenedor padre previo
                pasajeroActual.SetParent(padreOriginalPasajero);
                pasajeroActual = null;
                padreOriginalPasajero = null;
            }
        }

        private bool EsElJugador(GameObject go)
        {
            // Se comprueba tanto el tag "Luna" como "Jugador" y el nombre del GameObject por robustez
            return go.name == "Luna" || go.tag == "Jugador" || go.tag == "Luna" || go.tag == tagJugador;
        }
    }
}
