using UnityEngine;

namespace SemillasDelBosque.Entorno
{
    /// <summary>
    /// ParallaxFondo: Desplaza una capa de fondo en función del movimiento de la cámara.
    /// Crea el efecto de profundidad 2D (Parallax) donde los objetos lejanos se mueven más lento
    /// que los objetos cercanos al jugador.
    /// </summary>
    public class ParallaxFondo : MonoBehaviour
    {
        [Header("Configuración de Parallax")]
        [Tooltip("Multiplicador de seguimiento. 0 = estático en el mundo, 1 = se mueve idéntico a la cámara.")]
        [SerializeField] private Vector2 factorParallax = new Vector2(0.3f, 0.05f);

        [Tooltip("Cámara objetivo a seguir. Si no se asigna, busca Camera.main automáticamente.")]
        [SerializeField] private Camera camaraObjetivo;

        // Posiciones de referencia para calcular el desplazamiento relativo
        private Vector3 posicionInicialCamara;
        private Vector3 posicionInicialPropia;

        public Vector2 FactorParallax { get => factorParallax; set => factorParallax = value; }

        private void Start()
        {
            if (camaraObjetivo == null)
            {
                camaraObjetivo = Camera.main;
            }

            if (camaraObjetivo != null)
            {
                posicionInicialCamara = camaraObjetivo.transform.position;
            }

            posicionInicialPropia = transform.position;
        }

        private void LateUpdate()
        {
            if (camaraObjetivo == null) return;

            // Calcular cuánto se ha movido la cámara desde el inicio
            Vector3 desplazamientoCamara = camaraObjetivo.transform.position - posicionInicialCamara;

            // Aplicar el factor de parallax a la posición propia
            float nuevoX = posicionInicialPropia.x + (desplazamientoCamara.x * factorParallax.x);
            float nuevoY = posicionInicialPropia.y + (desplazamientoCamara.y * factorParallax.y);

            transform.position = new Vector3(nuevoX, nuevoY, posicionInicialPropia.z);
        }
    }
}

