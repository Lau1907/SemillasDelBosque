using UnityEngine;
using UnityEngine.UI;
using SemillasDelBosque.Sistema;

namespace SemillasDelBosque.UI
{
    /// <summary>
    /// BotonVolverAlMenu: Restaura Time.timeScale a 1f y carga el Menú Principal.
    /// Utilizado en PanelGameOver, PanelPausa y PanelNivelCompleto.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BotonVolverAlMenu : MonoBehaviour
    {
        private Button boton;

        private void Awake()
        {
            boton = GetComponent<Button>();
            if (boton != null)
            {
                boton.onClick.RemoveListener(VolverAlMenu);
                boton.onClick.AddListener(VolverAlMenu);
            }
        }

        public void VolverAlMenu()
        {
            Time.timeScale = 1f;
            GestorJuego.AsegurarInstancia().CargarMenuPrincipal();
        }
    }
}