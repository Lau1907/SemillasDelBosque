using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace SemillasDelBosque.UI
{
    /// <summary>
    /// BotonReiniciarNivel: Reemplaza la lógica hardcodeada de GrafoBotonReiniciar
    /// (que cargaba "Lago de las Luciernagas") para reiniciar siempre la escena actual.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Button))]
    public class BotonReiniciarNivel : MonoBehaviour
    {
        private Button boton;

        private void Awake()
        {
            DesactivarGrafoAnterior();
            EnlazarBoton();
        }

        private void OnEnable()
        {
            DesactivarGrafoAnterior();
            EnlazarBoton();
        }

        private void DesactivarGrafoAnterior()
        {
            ScriptMachine[] maquinas = GetComponents<ScriptMachine>();
            for (int i = 0; i < maquinas.Length; i++)
            {
                if (maquinas[i] != null && maquinas[i].enabled)
                {
                    maquinas[i].enabled = false;
                }
            }
        }

        private void EnlazarBoton()
        {
            if (boton == null) boton = GetComponent<Button>();
            if (boton == null) return;

            boton.onClick.RemoveListener(ReiniciarNivelActual);
            boton.onClick.AddListener(ReiniciarNivelActual);
        }

        public void ReiniciarNivelActual()
        {
            Time.timeScale = 1f;
            Scene escenaActual = SceneManager.GetActiveScene();
            Debug.Log($"[BotonReiniciarNivel] Reiniciando nivel actual: {escenaActual.name}");
            SceneManager.LoadScene(escenaActual.name);
        }

#if UNITY_EDITOR
        [MenuItem("Semillas del Bosque/UI/Corregir Boton Reiniciar")]
        public static void MenuCorregirBotonReiniciar()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[BotonReiniciarNivel] No se encontró Canvas en la escena.");
                return;
            }

            Button[] botones = canvas.GetComponentsInChildren<Button>(true);
            int corregidos = 0;
            foreach (Button b in botones)
            {
                if (b.gameObject.name.Contains("Reiniciar"))
                {
                    Undo.RecordObject(b.gameObject, "Corregir Botón Reiniciar");

                    ScriptMachine[] sms = b.GetComponents<ScriptMachine>();
                    foreach (ScriptMachine sm in sms)
                    {
                        Undo.RecordObject(sm, "Desactivar GrafoBotonReiniciar");
                        sm.enabled = false;
                        EditorUtility.SetDirty(sm);
                    }

                    if (b.GetComponent<BotonReiniciarNivel>() == null)
                    {
                        Undo.AddComponent<BotonReiniciarNivel>(b.gameObject);
                    }

                    EditorUtility.SetDirty(b.gameObject);
                    corregidos++;
                }
            }

            if (corregidos > 0)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log($"[BotonReiniciarNivel] {corregidos} botón(es) de reinicio corregido(s) para recargar la escena actual.");
            }
        }
#endif
    }
}
