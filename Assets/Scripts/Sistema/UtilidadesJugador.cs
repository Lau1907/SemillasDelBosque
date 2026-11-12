using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

namespace SemillasDelBosque.Sistema
{
    /// <summary>
    /// Utilidades compartidas para detectar al jugador (tags "Jugador", "Luna", "Nahui")
    /// y leer/escribir variables de escena de Visual Scripting de forma segura.
    /// </summary>
    public static class UtilidadesJugador
    {
        private static readonly string[] TagsJugador = { "Jugador", "Luna", "Nahui" };

        /// <summary>
        /// Verifica si el Collider2D pertenece al jugador (soporta tags "Jugador", "Luna", "Nahui" y nombres "Luna"/"Nahui").
        /// </summary>
        public static bool EsJugador(Collider2D col)
        {
            return col != null && EsJugador(col.gameObject);
        }

        /// <summary>
        /// Verifica si el GameObject pertenece al jugador sin lanzar excepciones si algún tag no está registrado.
        /// </summary>
        public static bool EsJugador(GameObject go)
        {
            if (go == null) return false;

            string tagActual = go.tag;
            for (int i = 0; i < TagsJugador.Length; i++)
            {
                if (string.Equals(tagActual, TagsJugador[i], StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return string.Equals(go.name, "Luna", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(go.name, "Nahui", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Busca al jugador activo en la escena comprobando los tags "Jugador", "Luna", "Nahui" y luego por nombre.
        /// </summary>
        public static GameObject BuscarJugador()
        {
            for (int i = 0; i < TagsJugador.Length; i++)
            {
                try
                {
                    GameObject encontrado = GameObject.FindWithTag(TagsJugador[i]);
                    if (encontrado != null) return encontrado;
                }
                catch
                {
                    // El tag aún no existe en TagManager; continuar con los demás.
                }
            }

            return GameObject.Find("Nahui") ?? GameObject.Find("Luna");
        }

        /// <summary>
        /// Lee una variable de escena de Visual Scripting devolviendo valorPorDefecto si no existe o falla.
        /// </summary>
        public static T LeerVariableEscena<T>(string nombreVariable, T valorPorDefecto = default)
        {
            try
            {
                var escenaVars = Variables.ActiveScene;
                if (escenaVars != null && escenaVars.IsDefined(nombreVariable))
                {
                    return escenaVars.Get<T>(nombreVariable);
                }
            }
            catch
            {
                // En modo edición o si Visual Scripting aún no inicializó la escena
            }
            return valorPorDefecto;
        }

        /// <summary>
        /// Escribe una variable de escena de Visual Scripting si el contenedor de escena está activo.
        /// </summary>
        public static bool EscribirVariableEscena(string nombreVariable, object valor)
        {
            try
            {
                var escenaVars = Variables.ActiveScene;
                if (escenaVars != null)
                {
                    escenaVars.Set(nombreVariable, valor);
                    return true;
                }
            }
            catch
            {
                // Silencioso en modo edición si no hay SceneVariables
            }
            return false;
        }

        /// <summary>
        /// Lee una variable de aplicación de Visual Scripting (por ejemplo, NivelDesbloqueado).
        /// </summary>
        public static T LeerVariableAplicacion<T>(string nombreVariable, T valorPorDefecto = default)
        {
            try
            {
                var appVars = Variables.Application;
                if (appVars != null && appVars.IsDefined(nombreVariable))
                {
                    return appVars.Get<T>(nombreVariable);
                }
            }
            catch
            {
                // Ignorar si no está disponible
            }
            return valorPorDefecto;
        }

        /// <summary>
        /// Escribe una variable de aplicación de Visual Scripting.
        /// </summary>
        public static void EscribirVariableAplicacion(string nombreVariable, object valor)
        {
            try
            {
                var appVars = Variables.Application;
                if (appVars != null)
                {
                    appVars.Set(nombreVariable, valor);
                }
            }
            catch
            {
                // Ignorar si no está disponible
            }
        }

        /// <summary>
        /// Cuenta todas las semillas presentes en la escena indicada sin usar APIs obsoletas.
        /// </summary>
        public static int ContarSemillasEnEscena(Scene escena)
        {
            if (!escena.IsValid()) return 0;

            Transform[] todos = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);

            int conteoNombre = 0;
            int conteoTag = 0;

            for (int i = 0; i < todos.Length; i++)
            {
                GameObject go = todos[i].gameObject;
                if (go.scene != escena) continue;

                if (go.name.StartsWith("SemillaDeLuz", StringComparison.Ordinal))
                {
                    conteoNombre++;
                }
                else if (string.Equals(go.tag, "Semilla", StringComparison.Ordinal))
                {
                    conteoTag++;
                }
            }

            return conteoNombre > 0 ? conteoNombre : conteoTag;
        }
    }
}
