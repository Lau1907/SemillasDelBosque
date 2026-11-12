using UnityEngine;
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
    /// calcula y muestra cuántas semillas recogió Luna en el formato 'Semillas recogidas: X / Total'.
    /// </summary>
    public class ResumenNivel : MonoBehaviour
    {
        [Header("Referencias de UI")]
        [Tooltip("Texto TMP donde se escribe el resumen de semillas.")]
        [SerializeField] private TextMeshProUGUI textoSemillas;

        [Header("Datos de Semillas")]
        [SerializeField] private int totalSemillas = 0;

        private void Awake()
        {
            CalcularTotalSemillas();

            if (textoSemillas == null)
            {
                Transform tHijo = transform.Find("TextoSemillas");
                if (tHijo != null)
                {
                    textoSemillas = tHijo.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        private void OnEnable()
        {
            CalcularTotalSemillas();
            ActualizarTexto();
        }

        public void CalcularTotalSemillas()
        {
            totalSemillas = UtilidadesJugador.ContarSemillasEnEscena(gameObject.scene);
        }

        public void ActualizarTexto()
        {
            int semillas = UtilidadesJugador.LeerVariableEscena("semillas", 0);

            if (textoSemillas != null)
            {
                textoSemillas.text = $"Semillas recogidas: {semillas} / {totalSemillas}";
            }
        }

#if UNITY_EDITOR
        [MenuItem("Semillas del Bosque/UI/Configurar Resumen de Meta")]
        public static void MenuConfigurarResumen()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[ResumenNivel] No se encontró ningún Canvas en la escena.");
                return;
            }

            Transform panelCompleto = canvas.transform.Find("PanelNivelCompleto");
            if (panelCompleto == null)
            {
                Debug.LogError("[ResumenNivel] No se encontró 'PanelNivelCompleto' como hijo del Canvas.");
                return;
            }

            Debug.Log("[ResumenNivel] Configurando Resumen de Nivel en 'PanelNivelCompleto'...");
            Undo.RegisterCompleteObjectUndo(panelCompleto.gameObject, "Configurar Resumen Nivel");

            ResumenNivel resumen = panelCompleto.GetComponent<ResumenNivel>();
            if (resumen == null)
            {
                resumen = Undo.AddComponent<ResumenNivel>(panelCompleto.gameObject);
            }

            // Buscar o crear TextoSemillas
            Transform tSemillas = panelCompleto.Find("TextoSemillas");
            TextMeshProUGUI tmpSemillas;

            if (tSemillas == null)
            {
                GameObject goTexto = new GameObject("TextoSemillas", typeof(RectTransform), typeof(TextMeshProUGUI));
                Undo.RegisterCreatedObjectUndo(goTexto, "Crear TextoSemillas en Resumen");
                goTexto.transform.SetParent(panelCompleto, false);

                RectTransform rt = goTexto.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, -40f); // Justo debajo del título ¡NIVEL COMPLETADO!
                rt.sizeDelta = new Vector2(400f, 50f);

                tmpSemillas = goTexto.GetComponent<TextMeshProUGUI>();
                tmpSemillas.fontSize = 26;
                tmpSemillas.alignment = TextAlignmentOptions.Center;
                tmpSemillas.color = new Color(1f, 0.95f, 0.6f, 1f); // Dorado suave
                tmpSemillas.raycastTarget = false;
            }
            else
            {
                tmpSemillas = tSemillas.GetComponent<TextMeshProUGUI>();
            }

            resumen.textoSemillas = tmpSemillas;
            resumen.CalcularTotalSemillas();
            resumen.ActualizarTexto();

            EditorUtility.SetDirty(panelCompleto.gameObject);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = panelCompleto.gameObject;
            Debug.Log("[ResumenNivel] ¡Resumen de nivel configurado con éxito en PanelNivelCompleto!");
        }
#endif
    }
}
