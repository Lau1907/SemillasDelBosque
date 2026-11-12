using UnityEngine;
using SemillasDelBosque.Jugador;
using SemillasDelBosque.Sistema;

namespace SemillasDelBosque.Sombra
{
    /// <summary>
    /// SombraDanioContacto: Collider de cuerpo para Sombras y Mini-Jefe.
    /// Al tocar al jugador lateralmente, le resta medio corazón (1 unidad) y lo empuja
    /// en lugar de teletransportarlo al checkpoint.
    /// </summary>
    public class SombraDanioContacto : MonoBehaviour
    {
        [SerializeField] private int mediosCorazonesDanio = 1;
        [SerializeField] private float fuerzaEmpujeX = 8.5f;
        [SerializeField] private float fuerzaEmpujeY = 6.5f;

        private float tiempoReactivacion = 0f;

        public int MediosCorazonesDanio { get => mediosCorazonesDanio; set => mediosCorazonesDanio = value; }
        public float FuerzaEmpujeX { get => fuerzaEmpujeX; set => fuerzaEmpujeX = value; }
        public float FuerzaEmpujeY { get => fuerzaEmpujeY; set => fuerzaEmpujeY = value; }

        public void Configurar(int mediosCorazones, float empujeX, float empujeY)
        {
            mediosCorazonesDanio = mediosCorazones;
            fuerzaEmpujeX = empujeX;
            fuerzaEmpujeY = empujeY;
        }

        public void PausarDanio(float segundos)
        {
            tiempoReactivacion = Mathf.Max(tiempoReactivacion, Time.time + segundos);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            IntentarAplicarDanio(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            IntentarAplicarDanio(other);
        }

        private void IntentarAplicarDanio(Collider2D other)
        {
            if (Time.time < tiempoReactivacion) return;
            if (!UtilidadesJugador.EsJugador(other)) return;

            SistemaVidaJugador vida = SistemaVidaJugador.ObtenerOAgregar(other.gameObject);
            if (vida != null && !vida.EstaInvulnerable)
            {
                vida.RecibirDanioMedioCorazon(mediosCorazonesDanio, transform.position, fuerzaEmpujeX, fuerzaEmpujeY);
            }
        }
    }
}

