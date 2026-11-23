using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace SemillasDelBosque.EditorTools
{
    /// <summary>
    /// Herramienta de Editor para duplicar la escena del Nivel 1 y preparar el Nivel 3 (Lago de las Luciérnagas).
    /// Mantiene la estructura de Canvas, Luna, Cámara, Iluminación y Variables de Escena,
    /// eliminando únicamente los objetos específicos del Nivel 1.
    /// </summary>
    public static class CrearEscenaNivel3
    {
        private const string RUTA_ESCENA_ORIGEN = "Assets/Scenes/Claro del Despertar.unity";
        private const string RUTA_ESCENA_DESTINO = "Assets/Scenes/Lago de las Luciernagas.unity";

        [MenuItem("Semillas del Bosque/3a. Crear Escena Nivel 3 (Lago de las Luciernagas)", false, 30)]
        public static void CrearEscena()
        {
            // 1. Validar que la escena base del Nivel 1 exista
            if (!File.Exists(RUTA_ESCENA_ORIGEN))
            {
                Debug.LogError($"[CrearEscenaNivel3] No se encontró la escena base en: {RUTA_ESCENA_ORIGEN}");
                return;
            }

            // 2. Si el archivo destino ya existe, avisar en consola y no sobrescribir
            if (File.Exists(RUTA_ESCENA_DESTINO))
            {
                Debug.LogWarning($"[CrearEscenaNivel3] La escena '{RUTA_ESCENA_DESTINO}' ya existe. No se sobrescribirá.");
                return;
            }

            // 3. Duplicar la escena con AssetDatabase.CopyAsset
            bool copiadoExitoso = AssetDatabase.CopyAsset(RUTA_ESCENA_ORIGEN, RUTA_ESCENA_DESTINO);
            if (!copiadoExitoso)
            {
                Debug.LogError($"[CrearEscenaNivel3] Error al duplicar la escena de {RUTA_ESCENA_ORIGEN} a {RUTA_ESCENA_DESTINO}.");
                return;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[CrearEscenaNivel3] Escena duplicada exitosamente en: {RUTA_ESCENA_DESTINO}");

            // 4. Agregar a Build Settings sin duplicarla
            AgregarEscenaABuildSettings(RUTA_ESCENA_DESTINO);

            // 5. Abrir la nueva escena
            Scene nuevaEscena = EditorSceneManager.OpenScene(RUTA_ESCENA_DESTINO, OpenSceneMode.Single);
            if (!nuevaEscena.IsValid())
            {
                Debug.LogError($"[CrearEscenaNivel3] No fue posible abrir la nueva escena: {RUTA_ESCENA_DESTINO}");
                return;
            }

            // 6. Eliminar el contenido propio del Nivel 1 conservando los elementos base y a Luna
            LimpiarContenidoNivel1(nuevaEscena);

            // 7. Guardar la escena
            EditorSceneManager.MarkSceneDirty(nuevaEscena);
            bool guardado = EditorSceneManager.SaveScene(nuevaEscena);
            if (guardado)
            {
                Debug.Log($"[CrearEscenaNivel3] Escena '{RUTA_ESCENA_DESTINO}' limpiada y guardada con éxito.");
            }
            else
            {
                Debug.LogError($"[CrearEscenaNivel3] Hubo un problema al guardar la escena '{RUTA_ESCENA_DESTINO}'.");
            }
        }

        private static void AgregarEscenaABuildSettings(string rutaEscena)
        {
            var listaEscenas = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool yaExiste = listaEscenas.Exists(e => e.path == rutaEscena);

            if (!yaExiste)
            {
                listaEscenas.Add(new EditorBuildSettingsScene(rutaEscena, true));
                EditorBuildSettings.scenes = listaEscenas.ToArray();
                Debug.Log($"[CrearEscenaNivel3] Escena agregada a Build Settings: {rutaEscena}");
            }
            else
            {
                Debug.Log($"[CrearEscenaNivel3] La escena ya figuraba en Build Settings: {rutaEscena}");
            }
        }

        private static void LimpiarContenidoNivel1(Scene escena)
        {
            var raices = escena.GetRootGameObjects();
            var objetosAEliminar = new List<GameObject>();

            foreach (var raiz in raices)
            {
                // Solo inspeccionamos los grupos indicados
                if (raiz.name == "--- SUELO Y PLATAFORMAS ---")
                {
                    for (int i = 0; i < raiz.transform.childCount; i++)
                    {
                        objetosAEliminar.Add(raiz.transform.GetChild(i).gameObject);
                    }
                }
                else if (raiz.name == "--- ELEMENTOS DEL NIVEL ---")
                {
                    for (int i = 0; i < raiz.transform.childCount; i++)
                    {
                        var hijo = raiz.transform.GetChild(i).gameObject;

                        // Verificar cuidadosamente que NO sea Luna
                        if (EsLuna(hijo))
                        {
                            Debug.Log($"[CrearEscenaNivel3] Conservando a Luna ({hijo.name}).");
                            continue;
                        }

                        objetosAEliminar.Add(hijo);
                    }
                }
            }

            // Imprimir en consola la lista completa antes de borrar
            Debug.Log($"[CrearEscenaNivel3] Se eliminarán {objetosAEliminar.Count} objetos pertenecientes al Nivel 1:");
            foreach (var go in objetosAEliminar)
            {
                string nombrePadre = go.transform.parent != null ? go.transform.parent.name : "Raíz";
                Debug.Log($" - [Eliminar] {go.name} | Padre: {nombrePadre} | Tag: {go.tag} | Pos: {go.transform.position}");
            }

            // Destruir los objetos registrados
            foreach (var go in objetosAEliminar)
            {
                Object.DestroyImmediate(go);
            }
        }

        private static bool EsLuna(GameObject go)
        {
            return UtilidadesEditorNiveles.EsObjetoJugador(go);
        }
    }
}
