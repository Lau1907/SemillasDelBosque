using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using SemillasDelBosque.Audio;
using SemillasDelBosque.Sombra;
using SemillasDelBosque.Nivel3;

namespace SemillasDelBosque.EditorScripts
{
    /// <summary>
    /// ConfigurarAudioSombras: Herramienta de Editor para configurar automáticamente
    /// el GestorMusica y las Sombras en la escena abierta.
    /// 
    /// Se ejecuta desde el menú superior:
    /// 'Semillas del Bosque/Configurar sombras y audio'
    /// </summary>
    public static class ConfigurarAudioSombras
    {
        private const string RUTA_MENU = "Semillas del Bosque/Configurar sombras y audio";

        [MenuItem(RUTA_MENU)]
        public static void EjecutarConfiguracion()
        {
            Scene escenaActiva = EditorSceneManager.GetActiveScene();
            if (!escenaActiva.isLoaded)
            {
                Debug.LogError("[ConfigurarAudioSombras] No hay ninguna escena activa abierta.");
                return;
            }

            Debug.Log($"<color=cyan><b>[ConfigurarAudioSombras]</b> Analizando la escena abierta: {escenaActiva.name}</color>");

            // 1. Identificar objetos a modificar antes de realizar cambios
            List<string> reporteCambios = new List<string>();

            // Identificar si existe el objeto Musica
            GameObject musicaGo = BuscarObjetoPorNombreEnEscena(escenaActiva, "Musica");
            if (musicaGo == null)
            {
                reporteCambios.Add("- [CREAR OBJETO] 'Musica' con AudioSource y GestorMusica");
            }
            else
            {
                reporteCambios.Add($"- [MODIFICAR OBJETO] '{musicaGo.name}' (Asignar GestorMusica y pistas de audio)");
            }

            // Identificar todos los objetos cuyo nombre empiece con "Sombra"
            List<GameObject> sombrasEncontradas = new List<GameObject>();
            BuscarSombrasEnEscena(escenaActiva, sombrasEncontradas);

            foreach (var sombra in sombrasEncontradas)
            {
                reporteCambios.Add($"- [MODIFICAR OBJETO] '{sombra.name}' (Asignar SombraPersecucion, AudioSource, clip 'sombra' y sprites)");
            }

            // 2. Imprimir en consola la lista de objetos a modificar ANTES de tocar nada
            Debug.Log($"<b>[ConfigurarAudioSombras] Lista previa de objetos a configurar ({reporteCambios.Count}):</b>\n" + string.Join("\n", reporteCambios));

            // 3. Localizar los clips de audio requeridos en Assets/Audio mediante AssetDatabase
            AudioClip clipExploracion = CargarAudioClip("musica_exploracion");
            AudioClip clipPersecucion = CargarAudioClip("musica_persecucion");
            AudioClip clipZonaRestaurada = CargarAudioClip("musica_zona_restaurada");
            AudioClip clipAlertaSombra = CargarAudioClip("sombra");
            AudioClip clipDano = CargarAudioClip("dano");

            // Localizar los sprites de patrulla y persecución
            Sprite spritePatrulla = CargarSprite("Assets/Sprites/SombraEspinosa_spritesheet.png", "SombraEspinosa_spritesheet_0");
            Sprite spritePersecucion = CargarSprite("Assets/Sprites/SombraEspinosa_spritesheet.png", "SombraEspinosa_spritesheet_1");

            // 4. Configurar el objeto "Musica"
            if (musicaGo == null)
            {
                musicaGo = new GameObject("Musica");
                SceneManager.MoveGameObjectToScene(musicaGo, escenaActiva);
                Undo.RegisterCreatedObjectUndo(musicaGo, "Crear GameObject Musica");
            }

            AudioSource musicaAudioSource = musicaGo.GetComponent<AudioSource>();
            if (musicaAudioSource == null)
            {
                musicaAudioSource = Undo.AddComponent<AudioSource>(musicaGo);
            }
            Undo.RecordObject(musicaAudioSource, "Configurar AudioSource Musica");
            musicaAudioSource.loop = true;
            musicaAudioSource.playOnAwake = true;
            musicaAudioSource.volume = 0.4f;
            if (clipExploracion != null)
            {
                musicaAudioSource.clip = clipExploracion;
            }

            GestorMusica gestorMusica = musicaGo.GetComponent<GestorMusica>();
            if (gestorMusica == null)
            {
                gestorMusica = Undo.AddComponent<GestorMusica>(musicaGo);
            }

            // Asignar serializadamente las referencias en GestorMusica para que persistan y admitan Deshacer (Undo)
            SerializedObject soGestor = new SerializedObject(gestorMusica);
            soGestor.Update();
            if (clipExploracion != null)
                soGestor.FindProperty("musicaExploracion").objectReferenceValue = clipExploracion;
            if (clipPersecucion != null)
                soGestor.FindProperty("musicaPersecucion").objectReferenceValue = clipPersecucion;
            if (clipZonaRestaurada != null)
                soGestor.FindProperty("musicaZonaRestaurada").objectReferenceValue = clipZonaRestaurada;
            soGestor.FindProperty("audioSource").objectReferenceValue = musicaAudioSource;
            soGestor.ApplyModifiedProperties();

            EditorUtility.SetDirty(musicaGo);

            // 5. Configurar cada Sombra encontrada (sin duplicar componentes)
            int sombrasConfiguradas = 0;
            foreach (GameObject sombra in sombrasEncontradas)
            {
                // Configurar AudioSource para reproducir el clip 'sombra'
                AudioSource audioSombra = sombra.GetComponent<AudioSource>();
                if (audioSombra == null)
                {
                    audioSombra = Undo.AddComponent<AudioSource>(sombra);
                }
                Undo.RecordObject(audioSombra, "Configurar AudioSource Sombra");
                audioSombra.playOnAwake = false;
                audioSombra.loop = false;
                if (clipAlertaSombra != null)
                {
                    audioSombra.clip = clipAlertaSombra;
                }

                // Configurar componente SombraPersecucion
                SombraPersecucion sombraPersecucion = sombra.GetComponent<SombraPersecucion>();
                if (sombraPersecucion == null)
                {
                    sombraPersecucion = Undo.AddComponent<SombraPersecucion>(sombra);
                }

                SerializedObject soSombra = new SerializedObject(sombraPersecucion);
                soSombra.Update();
                soSombra.FindProperty("distanciaDeteccion").floatValue = 4.8f;
                soSombra.FindProperty("distanciaPerdida").floatValue = 7.0f;
                soSombra.FindProperty("audioSource").objectReferenceValue = audioSombra;

                if (clipAlertaSombra != null)
                {
                    soSombra.FindProperty("clipAlerta").objectReferenceValue = clipAlertaSombra;
                }
                if (clipDano != null)
                {
                    soSombra.FindProperty("clipDano").objectReferenceValue = clipDano;
                }
                if (spritePatrulla != null)
                {
                    soSombra.FindProperty("spritePatrulla").objectReferenceValue = spritePatrulla;
                }
                if (spritePersecucion != null)
                {
                    soSombra.FindProperty("spritePersecucion").objectReferenceValue = spritePersecucion;
                }

                // Sincronizar la velocidad de patrulla desde PatrullaSombra si ya está definido
                PatrullaSombra patrulla = sombra.GetComponent<PatrullaSombra>();
                if (patrulla != null)
                {
                    soSombra.FindProperty("velocidadPatrulla").floatValue = patrulla.Velocidad;
                }

                soSombra.ApplyModifiedProperties();
                EditorUtility.SetDirty(sombra);
                sombrasConfiguradas++;
            }

            // 6. Marcar la escena como modificada (Dirty) para que Unity recuerde guardarla
            EditorSceneManager.MarkSceneDirty(escenaActiva);

            Debug.Log($"<color=green><b>[ConfigurarAudioSombras] ¡Configuración completada con éxito!</b></color>\n" +
                      $"- Objeto 'Musica': GestorMusica configurado con 3 pistas de audio.\n" +
                      $"- Sombras configuradas: {sombrasConfiguradas}\n" +
                      $"- Escena '{escenaActiva.name}' marcada como modificada (recuerda guardar con Ctrl+S).");
        }

        private static GameObject BuscarObjetoPorNombreEnEscena(Scene escena, string nombre)
        {
            foreach (GameObject raiz in escena.GetRootGameObjects())
            {
                if (raiz.name == nombre) return raiz;
                Transform hijo = raiz.transform.Find(nombre);
                if (hijo != null) return hijo.gameObject;
            }
            return null;
        }

        private static void BuscarSombrasEnEscena(Scene escena, List<GameObject> listaResultado)
        {
            foreach (GameObject raiz in escena.GetRootGameObjects())
            {
                BuscarSombrasRecursivo(raiz.transform, listaResultado);
            }
        }

        private static void BuscarSombrasRecursivo(Transform t, List<GameObject> listaResultado)
        {
            if (t.name.StartsWith("Sombra"))
            {
                listaResultado.Add(t.gameObject);
            }

            for (int i = 0; i < t.childCount; i++)
            {
                BuscarSombrasRecursivo(t.GetChild(i), listaResultado);
            }
        }

        private static AudioClip CargarAudioClip(string nombreExacto)
        {
            string[] guids = AssetDatabase.FindAssets($"{nombreExacto} t:AudioClip", new[] { "Assets/Audio" });
            if (guids.Length == 0)
            {
                guids = AssetDatabase.FindAssets($"{nombreExacto} t:AudioClip");
            }

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string filename = Path.GetFileNameWithoutExtension(path);
                if (string.Equals(filename, nombreExacto, System.StringComparison.OrdinalIgnoreCase))
                {
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                }
            }

            Debug.LogWarning($"[ConfigurarAudioSombras] No se encontró el AudioClip '{nombreExacto}'.");
            return null;
        }

        private static Sprite CargarSprite(string pathSpriteSheet, string nombreSprite)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(pathSpriteSheet);
            foreach (Object asset in assets)
            {
                if (asset is Sprite s && s.name == nombreSprite)
                {
                    return s;
                }
            }

            Debug.LogWarning($"[ConfigurarAudioSombras] No se encontró el Sprite '{nombreSprite}' en {pathSpriteSheet}.");
            return null;
        }
    }
}

