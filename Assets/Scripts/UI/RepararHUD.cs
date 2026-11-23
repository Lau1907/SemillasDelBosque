using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace SemillasDelBosque.UI
{
    /// <summary>
    /// RepararHUD: Herramienta de emergencia para forzar la reasignación de sprites
    /// en el HUD (corazon_lleno.png, corazon_vacio.png, icono_semilla.png) cuando
    /// existan referencias nulas o cuadros blancos tras editar la escena.
    /// </summary>
    public static class RepararHUD
    {
#if UNITY_EDITOR
        [MenuItem("Semillas del Bosque/UI/Reparar sprites HUD")]
        public static void MenuRepararSpritesHUD()
        {
            Sprite sLleno = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objetos/corazon_lleno.png")
                            ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/corazon_lleno.png");
            Sprite sMedio = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objetos/corazon_medio.png");
            Sprite sVacio = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objetos/corazon_vacio.png")
                            ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/corazon_vacio.png");
            Sprite sSemilla = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objetos/semilla_luz.png")
                              ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generados/icono_semilla.png");

            if (sLleno == null || sVacio == null)
            {
                Debug.LogError("[RepararHUD] No se encontraron los sprites de corazones en " +
                               "'Assets/Sprites/Generados/'. Ejecuta primero " +
                               "'Semillas del Bosque/Arte/Generar sprites'.");
                return;
            }

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[RepararHUD] No hay Canvas en la escena.");
                return;
            }

            Transform hudT = canvas.transform.Find("ContenedorHUD");
            if (hudT == null)
            {
                Debug.LogError("[RepararHUD] No se encontró 'ContenedorHUD'. Ejecuta " +
                               "'Semillas del Bosque/UI/Agregar HUD' primero.");
                return;
            }

            HudJuego hudComp = hudT.GetComponent<HudJuego>();
            if (hudComp != null)
            {
                Undo.RecordObject(hudComp, "Reparar sprites HUD");
                hudComp.SpriteCorazonLleno = sLleno;
                hudComp.SpriteCorazonMedio = sMedio;
                hudComp.SpriteCorazonVacio = sVacio;
                if (sSemilla != null && hudComp.ImagenSemilla != null)
                {
                    hudComp.ImagenSemilla.sprite = sSemilla;
                }
                EditorUtility.SetDirty(hudComp);
            }

            Transform filaSemilla = hudT.Find("FilaSemilla");
            if (filaSemilla != null && sSemilla != null)
            {
                Transform icono = filaSemilla.Find("IconoSemilla");
                if (icono != null)
                {
                    Image imgIcono = icono.GetComponent<Image>();
                    if (imgIcono != null)
                    {
                        Undo.RecordObject(imgIcono, "Reparar icono semilla");
                        imgIcono.sprite = sSemilla;
                        EditorUtility.SetDirty(imgIcono);
                    }
                }
            }

            int reparados = 0;
            Transform filaCorazones = hudT.Find("FilaCorazones");
            if (filaCorazones != null)
            {
                Image[] imgs = new Image[filaCorazones.childCount];
                for (int i = 0; i < filaCorazones.childCount; i++)
                {
                    Image imgC = filaCorazones.GetChild(i).GetComponent<Image>();
                    imgs[i] = imgC;
                    if (imgC != null)
                    {
                        Undo.RecordObject(imgC, $"Reparar corazón {i + 1}");
                        imgC.sprite = sLleno;
                        EditorUtility.SetDirty(imgC);
                        reparados++;
                    }
                }

                if (hudComp != null)
                {
                    hudComp.ImagenesCorazones = imgs;
                    EditorUtility.SetDirty(hudComp);
                }
            }

            if (hudComp != null)
            {
                hudComp.CalcularTotalSemillas();
                hudComp.ActualizarHUD(forzar: true);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[RepararHUD] {reparados} corazones e iconos reparados correctamente. Guarda con Ctrl+S.");
        }
#endif
    }
}

