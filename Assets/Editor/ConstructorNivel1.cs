using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using SemillasDelBosque.Editor;
using SemillasDelBosque.EditorScripts;
using SemillasDelBosque.Entorno;

namespace SemillasDelBosque.EditorTools
{
    /// <summary>
    /// ConstructorNivel1: Configura de forma idempotente y con Undo el Nivel 1
    /// ("Claro del Despertar - Jardín Botánico") sin alterar las plataformas y semillas existentes.
    /// </summary>
    public static class ConstructorNivel1
    {
        [MenuItem("Semillas del Bosque/1. Configurar Nivel 1 (Claro del Despertar)", false, 10)]
        public static void ConfigurarNivel1()
        {
            Scene escena = SceneManager.GetActiveScene();
            if (escena.path != UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL1)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                escena = EditorSceneManager.OpenScene(UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL1, OpenSceneMode.Single);
            }

            Debug.Log("[ConstructorNivel1] Configurando Claro del Despertar (Nivel 1)...");

            UtilidadesEditorNiveles.RegistrarTagsJugador();
            ConstructorEntorno.ConstruirEntornoNivel();
            ConfigurarAudioSombras.EjecutarConfiguracion();
            BuhoConsejo.MenuConfigurarBuho();
            UtilidadesEditorNiveles.AsegurarZonaCaida(null, 54f, 200f);

            UtilidadesEditorNiveles.ConfigurarMetaNivel(
                padre: null,
                posicion: new Vector3(105f, 1.5f, 0f),
                nivelActual: 1,
                nombreSiguienteEscena: "Bosque Profundo",
                esNivelFinal: false);

            UtilidadesEditorNiveles.ConfigurarHUDYAudioUI();
            ConfiguradorAnimaciones.AplicarEnEscenaActiva();
            UtilidadesEditorNiveles.AgregarEscenaABuildSettings(UtilidadesEditorNiveles.RUTA_ESCENA_NIVEL1);

            EditorSceneManager.MarkSceneDirty(escena);
            Debug.Log("[ConstructorNivel1] Nivel 1 configurado correctamente (12 semillas, 2 checkpoints, Sombra Zona D, Búho, ZonaCaida Y=-9, Meta -> Nivel 2).");
        }
    }
}
