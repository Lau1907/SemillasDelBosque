using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using SemillasDelBosque.Jugador;
using SemillasDelBosque.Sombra;
using SemillasDelBosque.UI;

namespace SemillasDelBosque.EditorTools
{
    /// <summary>
    /// ConfiguradorAnimaciones: Configura los importadores de los sprites extraídos,
    /// genera de forma idempotente los AnimationClips y AnimatorControllers para Nahui,
    /// Sombra Espinosa y Gran Sombra de Nahual, y los asigna a los objetos de la escena activa.
    /// </summary>
    public static class ConfiguradorAnimaciones
    {
        private const string DIR_ANIM_NAHUI = "Assets/Animaciones/Nahui";
        private const string DIR_ANIM_SOMBRA = "Assets/Animaciones/Sombra";

        public const string RUTA_CTRL_NAHUI = DIR_ANIM_NAHUI + "/Nahui.controller";
        public const string RUTA_CTRL_SOMBRA = DIR_ANIM_SOMBRA + "/SombraEspinosa.controller";

        public const string RUTA_SPRITE_SEMILLA = "Assets/Sprites/Objetos/semilla_luz.png";
        public const string RUTA_SPRITE_CORAZON_LLENO = "Assets/Sprites/Objetos/corazon_lleno.png";
        public const string RUTA_SPRITE_CORAZON_MEDIO = "Assets/Sprites/Objetos/corazon_medio.png";
        public const string RUTA_SPRITE_CORAZON_VACIO = "Assets/Sprites/Objetos/corazon_vacio.png";
        public const string RUTA_SPRITE_CHECKPOINT = "Assets/Sprites/Objetos/linterna_checkpoint.png";
        public const string RUTA_SPRITE_BUHO = "Assets/Sprites/Personajes/BuhoSabio_Rama.png";

        [MenuItem("Semillas del Bosque/Arte/Configurar Sprites y Animaciones", false, 5)]
        public static void EjecutarConfiguracionCompleta()
        {
            GenerarAssetsAnimacion();
            AplicarEnEscenaActiva();
        }

        public static void GenerarAssetsAnimacion()
        {
            ConfigurarImportadoresSprites();
            AsegurarCarpeta("Assets/Animaciones");
            AsegurarCarpeta(DIR_ANIM_NAHUI);
            AsegurarCarpeta(DIR_ANIM_SOMBRA);

            // Clips de Nahui
            AnimationClip clipNahuiIdle = CrearOReemplazarClipSprite(
                DIR_ANIM_NAHUI + "/Nahui_Idle.anim", 4f, true,
                "Assets/Sprites/Animaciones/Nahui/nahui_idle_0.png",
                "Assets/Sprites/Animaciones/Nahui/nahui_idle_1.png");

            AnimationClip clipNahuiCorrer = CrearOReemplazarClipSprite(
                DIR_ANIM_NAHUI + "/Nahui_Correr.anim", 10f, true,
                "Assets/Sprites/Animaciones/Nahui/nahui_run_0.png",
                "Assets/Sprites/Animaciones/Nahui/nahui_run_1.png",
                "Assets/Sprites/Animaciones/Nahui/nahui_run_2.png",
                "Assets/Sprites/Animaciones/Nahui/nahui_run_3.png");

            AnimationClip clipNahuiSaltar = CrearOReemplazarClipSprite(
                DIR_ANIM_NAHUI + "/Nahui_Saltar.anim", 6f, false,
                "Assets/Sprites/Animaciones/Nahui/nahui_jump_0.png",
                "Assets/Sprites/Animaciones/Nahui/nahui_jump_1.png");

            AnimationClip clipNahuiDobleSalto = CrearOReemplazarClipSprite(
                DIR_ANIM_NAHUI + "/Nahui_DobleSalto.anim", 8f, true,
                "Assets/Sprites/Animaciones/Nahui/nahui_doble_salto_0.png",
                "Assets/Sprites/Animaciones/Nahui/nahui_jump_1.png");

            ConstruirControllerNahui(clipNahuiIdle, clipNahuiCorrer, clipNahuiSaltar, clipNahuiDobleSalto);

            // Clips de Sombra
            AnimationClip clipSombraPatrulla = CrearOReemplazarClipSprite(
                DIR_ANIM_SOMBRA + "/Sombra_Patrulla.anim", 6f, true,
                "Assets/Sprites/Animaciones/Sombra/sombra_patrulla_0.png",
                "Assets/Sprites/Animaciones/Sombra/sombra_patrulla_1.png",
                "Assets/Sprites/Animaciones/Sombra/sombra_patrulla_2.png",
                "Assets/Sprites/Animaciones/Sombra/sombra_patrulla_3.png");

            AnimationClip clipSombraPersecucion = CrearOReemplazarClipSprite(
                DIR_ANIM_SOMBRA + "/Sombra_Persecucion.anim", 10f, true,
                "Assets/Sprites/Animaciones/Sombra/sombra_persecucion_0.png",
                "Assets/Sprites/Animaciones/Sombra/sombra_persecucion_1.png",
                "Assets/Sprites/Animaciones/Sombra/sombra_persecucion_2.png",
                "Assets/Sprites/Animaciones/Sombra/sombra_persecucion_3.png");

            AnimationClip clipSombraAtaque = CrearOReemplazarClipSprite(
                DIR_ANIM_SOMBRA + "/Sombra_Ataque.anim", 8f, true,
                "Assets/Sprites/Animaciones/Sombra/sombra_ataque_0.png",
                "Assets/Sprites/Animaciones/Sombra/sombra_ataque_1.png",
                "Assets/Sprites/Animaciones/Sombra/sombra_ataque_2.png",
                "Assets/Sprites/Animaciones/Sombra/sombra_ataque_3.png");

            ConstruirControllerSombra(clipSombraPatrulla, clipSombraPersecucion, clipSombraAtaque);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ConfiguradorAnimaciones] Sprites, clips y AnimatorControllers generados correctamente.");
        }

        public static void AplicarEnEscenaActiva()
        {
            if (!File.Exists(RUTA_CTRL_NAHUI) || !File.Exists(RUTA_CTRL_SOMBRA))
            {
                GenerarAssetsAnimacion();
            }

            Scene escena = SceneManager.GetActiveScene();
            RuntimeAnimatorController ctrlNahui = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(RUTA_CTRL_NAHUI);
            RuntimeAnimatorController ctrlSombra = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(RUTA_CTRL_SOMBRA);

            Sprite spriteNahuiIdle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Animaciones/Nahui/nahui_idle_0.png");
            Sprite spriteSombraPatrulla = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Animaciones/Sombra/sombra_patrulla_0.png");
            Sprite spriteSombraPersecucion = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Animaciones/Sombra/sombra_persecucion_0.png");
            Sprite spriteSombraAtaque = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Animaciones/Sombra/sombra_ataque_0.png");

            Sprite spriteSemilla = AssetDatabase.LoadAssetAtPath<Sprite>(RUTA_SPRITE_SEMILLA);
            Sprite spriteCheckpoint = AssetDatabase.LoadAssetAtPath<Sprite>(RUTA_SPRITE_CHECKPOINT);
            Sprite spriteBuho = AssetDatabase.LoadAssetAtPath<Sprite>(RUTA_SPRITE_BUHO);

            // 1. Configurar Nahui / Luna
            GameObject jugador = UtilidadesEditorNiveles.BuscarJugadorEnEscena();
            if (jugador != null)
            {
                Undo.RecordObject(jugador, "Configurar Animaciones Nahui");
                SpriteRenderer sr = jugador.GetComponent<SpriteRenderer>();
                if (sr != null && spriteNahuiIdle != null)
                {
                    Undo.RecordObject(sr, "Asignar Sprite Nahui");
                    sr.sprite = spriteNahuiIdle;
                    EditorUtility.SetDirty(sr);
                }

                Animator anim = jugador.GetComponent<Animator>();
                if (anim == null) anim = Undo.AddComponent<Animator>(jugador);
                Undo.RecordObject(anim, "Asignar Controller Nahui");
                anim.runtimeAnimatorController = ctrlNahui;
                EditorUtility.SetDirty(anim);

                if (jugador.GetComponent<ControladorAnimacionJugador>() == null)
                {
                    Undo.AddComponent<ControladorAnimacionJugador>(jugador);
                }

                SistemaVidaJugador vidaJugador = jugador.GetComponent<SistemaVidaJugador>();
                if (vidaJugador == null) vidaJugador = Undo.AddComponent<SistemaVidaJugador>(jugador);
                AudioClip clipDano = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/dano.wav");
                if (clipDano != null)
                {
                    Undo.RecordObject(vidaJugador, "Asignar ClipDano Jugador");
                    vidaJugador.ClipDano = clipDano;
                    EditorUtility.SetDirty(vidaJugador);
                }
                EditorUtility.SetDirty(jugador);
            }

            AudioClip clipDanoSombra = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/dano.wav");

            // 2. Configurar Sombras Espinosas y Gran Sombra de Nahual
            GameObject[] todos = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            foreach (GameObject go in todos)
            {
                if (go.scene != escena) continue;

                if (go.name.StartsWith("Sombra"))
                {
                    SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                    if (sr != null && spriteSombraPatrulla != null)
                    {
                        Undo.RecordObject(sr, "Asignar Sprite Sombra");
                        sr.sprite = spriteSombraPatrulla;
                        EditorUtility.SetDirty(sr);
                    }

                    Animator anim = go.GetComponent<Animator>();
                    if (anim == null) anim = Undo.AddComponent<Animator>(go);
                    Undo.RecordObject(anim, "Asignar Controller Sombra");
                    anim.runtimeAnimatorController = ctrlSombra;
                    EditorUtility.SetDirty(anim);

                    SombraPersecucion sp = go.GetComponent<SombraPersecucion>();
                    if (sp != null)
                    {
                        Undo.RecordObject(sp, "Actualizar Sprites SombraPersecucion");
                        sp.SpritePatrulla = spriteSombraPatrulla;
                        sp.SpritePersecucion = spriteSombraPersecucion;
                        if (clipDanoSombra != null) sp.ClipDano = clipDanoSombra;
                        EditorUtility.SetDirty(sp);
                    }
                }
                else if (go.name == "GranSombraNahual")
                {
                    Animator anim = go.GetComponent<Animator>();
                    if (anim == null) anim = Undo.AddComponent<Animator>(go);
                    Undo.RecordObject(anim, "Asignar Controller Jefe");
                    anim.runtimeAnimatorController = ctrlSombra;
                    EditorUtility.SetDirty(anim);

                    GranSombraNahual jefe = go.GetComponent<GranSombraNahual>();
                    if (jefe != null)
                    {
                        Undo.RecordObject(jefe, "Actualizar Sprites Jefe");
                        jefe.SpritePatrulla = spriteSombraPatrulla;
                        jefe.SpritePersecucion = spriteSombraPersecucion;
                        jefe.SpriteAtaque = spriteSombraAtaque;
                        EditorUtility.SetDirty(jefe);
                    }
                }
                else if (go.name.Contains("Buho") && spriteBuho != null)
                {
                    SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        Undo.RecordObject(sr, "Asignar Sprite Búho");
                        sr.sprite = spriteBuho;
                        EditorUtility.SetDirty(sr);
                    }
                }
                else if (go.name.StartsWith("SemillaDeLuz") && spriteSemilla != null)
                {
                    SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        Undo.RecordObject(sr, "Asignar Sprite Semilla");
                        sr.sprite = spriteSemilla;
                        EditorUtility.SetDirty(sr);
                    }
                }
                else if (go.name.StartsWith("PuntoDeControl") && spriteCheckpoint != null)
                {
                    SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        Undo.RecordObject(sr, "Asignar Sprite Checkpoint");
                        sr.sprite = spriteCheckpoint;
                        sr.color = Color.white;
                        EditorUtility.SetDirty(sr);
                    }

                    // Corregir escala (0.5, 1, 1) heredada del cuadrado placeholder para que la linterna no se aplaste
                    if (Mathf.Approximately(go.transform.localScale.x, 0.5f))
                    {
                        Undo.RecordObject(go.transform, "Ajustar Escala Linterna");
                        go.transform.localScale = Vector3.one;
                        EditorUtility.SetDirty(go.transform);
                    }
                }
            }

            // 3. Actualizar sprites del HUD si existe en la escena
            if (Object.FindAnyObjectByType<HudJuego>() != null)
            {
                RepararHUD.MenuRepararSpritesHUD();
            }

            EditorSceneManager.MarkSceneDirty(escena);
            Debug.Log($"[ConfiguradorAnimaciones] Sprites y animaciones aplicados en escena '{escena.name}'.");
        }

        private static void ConfigurarImportadoresSprites()
        {
            ConfigurarCarpetaSprites("Assets/Sprites/Animaciones/Nahui", 90f);
            ConfigurarCarpetaSprites("Assets/Sprites/Animaciones/Sombra", 84f);
            ConfigurarSpriteIndividual(RUTA_SPRITE_BUHO, 100f);
            ConfigurarSpriteIndividual(RUTA_SPRITE_SEMILLA, 64f);
            ConfigurarSpriteIndividual(RUTA_SPRITE_CORAZON_LLENO, 64f);
            ConfigurarSpriteIndividual(RUTA_SPRITE_CORAZON_MEDIO, 64f);
            ConfigurarSpriteIndividual(RUTA_SPRITE_CORAZON_VACIO, 64f);
            ConfigurarSpriteIndividual(RUTA_SPRITE_CHECKPOINT, 64f);
        }

        private static void ConfigurarCarpetaSprites(string carpeta, float ppu)
        {
            if (!Directory.Exists(carpeta)) return;
            foreach (string archivo in Directory.GetFiles(carpeta, "*.png"))
            {
                ConfigurarSpriteIndividual(archivo.Replace('\\', '/'), ppu);
            }
        }

        private static void ConfigurarSpriteIndividual(string ruta, float ppu)
        {
            if (!File.Exists(ruta)) return;
            AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (importer == null) return;

            bool cambio = importer.textureType != TextureImporterType.Sprite ||
                          importer.spriteImportMode != SpriteImportMode.Single ||
                          !Mathf.Approximately(importer.spritePixelsPerUnit, ppu) ||
                          importer.filterMode != FilterMode.Point ||
                          importer.textureCompression != TextureImporterCompression.Uncompressed;

            if (cambio)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = ppu;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }

        private static AnimationClip CrearOReemplazarClipSprite(string rutaClip, float frameRate, bool loop, params string[] rutasFrames)
        {
            List<Sprite> sprites = new List<Sprite>();
            foreach (string r in rutasFrames)
            {
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(r);
                if (s != null) sprites.Add(s);
            }

            if (sprites.Count == 0)
            {
                Debug.LogWarning($"[ConfiguradorAnimaciones] No se encontraron sprites para {rutaClip}.");
                return null;
            }

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(rutaClip);
            bool esNuevo = (clip == null);
            if (esNuevo)
            {
                clip = new AnimationClip();
            }
            else
            {
                clip.ClearCurves();
            }

            clip.frameRate = frameRate;

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
            for (int i = 0; i < sprites.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / frameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            if (esNuevo)
            {
                AssetDatabase.CreateAsset(clip, rutaClip);
            }
            else
            {
                EditorUtility.SetDirty(clip);
            }

            return clip;
        }

        private static void ConstruirControllerNahui(AnimationClip idle, AnimationClip correr, AnimationClip saltar, AnimationClip dobleSalto)
        {
            if (File.Exists(RUTA_CTRL_NAHUI))
            {
                AssetDatabase.DeleteAsset(RUTA_CTRL_NAHUI);
            }

            AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(RUTA_CTRL_NAHUI);
            ctrl.AddParameter("Velocidad", AnimatorControllerParameterType.Float);
            ctrl.AddParameter("EnSuelo", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("DobleSaltoActivo", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
            AnimatorState stIdle = sm.AddState("Idle");
            stIdle.motion = idle;
            sm.defaultState = stIdle;

            AnimatorState stCorrer = sm.AddState("Correr");
            stCorrer.motion = correr;

            AnimatorState stSaltar = sm.AddState("Saltar");
            stSaltar.motion = saltar;

            AnimatorState stDobleSalto = sm.AddState("DobleSalto");
            stDobleSalto.motion = dobleSalto;

            // Idle <-> Correr
            var tIdleACorrer = CrearTransicion(stIdle, stCorrer);
            tIdleACorrer.AddCondition(AnimatorConditionMode.If, 0f, "EnSuelo");
            tIdleACorrer.AddCondition(AnimatorConditionMode.Greater, 0.15f, "Velocidad");

            var tCorrerAIdle = CrearTransicion(stCorrer, stIdle);
            tCorrerAIdle.AddCondition(AnimatorConditionMode.Less, 0.15f, "Velocidad");

            // Salto y Doble Salto desde suelo
            var tIdleASaltar = CrearTransicion(stIdle, stSaltar);
            tIdleASaltar.AddCondition(AnimatorConditionMode.IfNot, 0f, "EnSuelo");
            tIdleASaltar.AddCondition(AnimatorConditionMode.IfNot, 0f, "DobleSaltoActivo");

            var tCorrerASaltar = CrearTransicion(stCorrer, stSaltar);
            tCorrerASaltar.AddCondition(AnimatorConditionMode.IfNot, 0f, "EnSuelo");
            tCorrerASaltar.AddCondition(AnimatorConditionMode.IfNot, 0f, "DobleSaltoActivo");

            var tSaltarADoble = CrearTransicion(stSaltar, stDobleSalto);
            tSaltarADoble.AddCondition(AnimatorConditionMode.If, 0f, "DobleSaltoActivo");

            var tIdleADoble = CrearTransicion(stIdle, stDobleSalto);
            tIdleADoble.AddCondition(AnimatorConditionMode.IfNot, 0f, "EnSuelo");
            tIdleADoble.AddCondition(AnimatorConditionMode.If, 0f, "DobleSaltoActivo");

            var tCorrerADoble = CrearTransicion(stCorrer, stDobleSalto);
            tCorrerADoble.AddCondition(AnimatorConditionMode.IfNot, 0f, "EnSuelo");
            tCorrerADoble.AddCondition(AnimatorConditionMode.If, 0f, "DobleSaltoActivo");

            // Regreso al suelo
            var tSaltarAIdle = CrearTransicion(stSaltar, stIdle);
            tSaltarAIdle.AddCondition(AnimatorConditionMode.If, 0f, "EnSuelo");

            var tDobleAIdle = CrearTransicion(stDobleSalto, stIdle);
            tDobleAIdle.AddCondition(AnimatorConditionMode.If, 0f, "EnSuelo");

            EditorUtility.SetDirty(ctrl);
        }

        private static void ConstruirControllerSombra(AnimationClip patrulla, AnimationClip persecucion, AnimationClip ataque)
        {
            if (File.Exists(RUTA_CTRL_SOMBRA))
            {
                AssetDatabase.DeleteAsset(RUTA_CTRL_SOMBRA);
            }

            AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(RUTA_CTRL_SOMBRA);
            ctrl.AddParameter("Persecucion", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("Ataque", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
            AnimatorState stPatrulla = sm.AddState("Patrulla");
            stPatrulla.motion = patrulla;
            sm.defaultState = stPatrulla;

            AnimatorState stPersecucion = sm.AddState("Persecucion");
            stPersecucion.motion = persecucion;

            AnimatorState stAtaque = sm.AddState("Ataque");
            stAtaque.motion = ataque;

            var tPatAPers = CrearTransicion(stPatrulla, stPersecucion);
            tPatAPers.AddCondition(AnimatorConditionMode.If, 0f, "Persecucion");
            tPatAPers.AddCondition(AnimatorConditionMode.IfNot, 0f, "Ataque");

            var tPersAPat = CrearTransicion(stPersecucion, stPatrulla);
            tPersAPat.AddCondition(AnimatorConditionMode.IfNot, 0f, "Persecucion");

            var tPersAAtaque = CrearTransicion(stPersecucion, stAtaque);
            tPersAAtaque.AddCondition(AnimatorConditionMode.If, 0f, "Ataque");

            var tPatAAtaque = CrearTransicion(stPatrulla, stAtaque);
            tPatAAtaque.AddCondition(AnimatorConditionMode.If, 0f, "Ataque");

            var tAtaqueAPers = CrearTransicion(stAtaque, stPersecucion);
            tAtaqueAPers.AddCondition(AnimatorConditionMode.IfNot, 0f, "Ataque");
            tAtaqueAPers.AddCondition(AnimatorConditionMode.If, 0f, "Persecucion");

            var tAtaqueAPat = CrearTransicion(stAtaque, stPatrulla);
            tAtaqueAPat.AddCondition(AnimatorConditionMode.IfNot, 0f, "Ataque");
            tAtaqueAPat.AddCondition(AnimatorConditionMode.IfNot, 0f, "Persecucion");

            EditorUtility.SetDirty(ctrl);
        }

        private static AnimatorStateTransition CrearTransicion(AnimatorState origen, AnimatorState destino)
        {
            AnimatorStateTransition t = origen.AddTransition(destino);
            t.hasExitTime = false;
            t.duration = 0f;
            t.exitTime = 0f;
            return t;
        }

        private static void AsegurarCarpeta(string ruta)
        {
            if (AssetDatabase.IsValidFolder(ruta)) return;
            string padre = Path.GetDirectoryName(ruta)?.Replace('\\', '/');
            string nombre = Path.GetFileName(ruta);
            if (!string.IsNullOrEmpty(padre) && !string.IsNullOrEmpty(nombre))
            {
                AssetDatabase.CreateFolder(padre, nombre);
            }
        }
    }
}

