using UnityEngine;
using SemillasDelBosque.Sistema;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace SemillasDelBosque.Nivel
{
    /// <summary>
    /// ZonaCaida: Trigger colocado en la parte inferior del nivel.
    /// Si Luna cae al vacío y toca este trigger, es teletransportada de inmediato
    /// a la posición guardada en 'ultimoCheckpoint' (de Visual Scripting)
    /// y se resetea su velocidad a cero para evitar caídas infinitas.
    /// </summary>
    public class ZonaCaida : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (EsLuna(other))
            {
                ReaparecerJugador(other.gameObject);
            }
        }

        private bool EsLuna(Collider2D col)
        {
            return UtilidadesJugador.EsJugador(col);
        }

        private void ReaparecerJugador(GameObject luna)
        {
            Vector3 destinoCheckpoint = UtilidadesJugador.LeerVariableEscena("ultimoCheckpoint", Vector3.zero);
            bool checkpointValido = destinoCheckpoint != Vector3.zero;

            // Si el checkpoint está en (0,0,0) y no se ha tocado ninguno, usar posición inicial típica
            if (!checkpointValido)
            {
                destinoCheckpoint = new Vector3(2f, 1f, 0f);
            }

            Debug.Log($"[ZonaCaida] Luna cayó al abismo. Reapareciendo en checkpoint: {destinoCheckpoint}");

            // 1. Mover al jugador al checkpoint
            luna.transform.position = destinoCheckpoint;

            // 2. Anular cualquier velocidad residual para que no siga cayendo
            Rigidbody2D rb = luna.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = Vector2.zero;
#else
                rb.velocity = Vector2.zero;
#endif
                rb.angularVelocity = 0f;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Semillas del Bosque/Nivel/Agregar zona de caída")]
        public static void MenuAgregarZonaCaida()
        {
            // Verificar si ya existe una ZonaCaida en la escena
            GameObject zonaExistente = GameObject.Find("ZonaCaida");
            if (zonaExistente != null)
            {
                Debug.LogWarning($"[ZonaCaida] Ya existe un objeto '{zonaExistente.name}' en la escena (Posición: {zonaExistente.transform.position}). No se duplicará.");
                Selection.activeGameObject = zonaExistente;
                return;
            }

            Debug.Log("[ZonaCaida] Creando objeto 'ZonaCaida' debajo del nivel con BoxCollider2D trigger...");

            GameObject goZona = new GameObject("ZonaCaida");
            Undo.RegisterCreatedObjectUndo(goZona, "Crear Zona de Caída");

            // Posicionar ampliamente por debajo de las plataformas del nivel 1 (que están entre Y = -3 y 3, X = -2 a 110)
            goZona.transform.position = new Vector3(54f, -9f, 0f);

            BoxCollider2D col = goZona.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(200f, 6f); // Cubre todo el ancho del nivel
            col.offset = Vector2.zero;

            goZona.AddComponent<ZonaCaida>();

            EditorUtility.SetDirty(goZona);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = goZona;
            Debug.Log("[ZonaCaida] ¡Zona de caída creada exitosamente en (54, -9, 0) con tamaño (200, 6)!");
        }
#endif
    }
}

