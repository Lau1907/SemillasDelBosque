using UnityEngine;
using UnityEngine.UI;

namespace SemillasDelBosque.UI
{
    [RequireComponent(typeof(Button))]
    public class BotonPausaHUD : MonoBehaviour
    {
        private Button boton;

        private void Awake()
        {
            boton = GetComponent<Button>();
            if (boton != null)
            {
                boton.onClick.RemoveListener(AlHacerClic);
                boton.onClick.AddListener(AlHacerClic);
            }
        }

        public void AlHacerClic()
        {
            MenuPausaController ctrl = Object.FindAnyObjectByType<MenuPausaController>(FindObjectsInactive.Include);
            if (ctrl != null)
            {
                ctrl.TogglePausa();
            }
            else
            {
                Transform pPausa = transform.parent != null ? transform.parent.Find("PanelPausa") : null;
                if (pPausa != null)
                {
                    bool pausar = !pPausa.gameObject.activeSelf;
                    pPausa.gameObject.SetActive(pausar);
                    Time.timeScale = pausar ? 0f : 1f;
                }
            }
        }
    }
}
