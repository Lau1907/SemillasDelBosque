using UnityEngine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using SemillasDelBosque.Sistema;

namespace SemillasDelBosque.Jugador
{
    /// <summary>
    /// ControladorAnimacionJugador:
    /// 1) Estabiliza la detección de suelo ('enSuelo' y 'saltosRestantes') para evitar que
    ///    OnCollisionExit2D en uniones entre bloques o plataformas móviles bloquee el salto.
    /// 2) Añade Coyote Time y Jump Buffer para que el salto responda siempre con fluidez.
    /// 3) Sincroniza el Animator y la orientación horizontal (flipX) de Nahui / Luna.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ControladorAnimacionJugador : MonoBehaviour
    {
        private static readonly int HashVelocidad = Animator.StringToHash("Velocidad");
        private static readonly int HashEnSuelo = Animator.StringToHash("EnSuelo");
        private static readonly int HashDobleSalto = Animator.StringToHash("DobleSaltoActivo");

        [Header("Asistencia de Salto")]
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.12f;
        [SerializeField] private float fuerzaSaltoFallback = 12f;

        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private Rigidbody2D rb;
        private Collider2D colJugador;

        private readonly Collider2D[] bufferSuelo = new Collider2D[12];
        private bool enSueloFisico = false;
        private float tiempoDesdeSuelo = 99f;
        private float bufferSaltoRestante = 0f;
        private float cooldownSaltoCSharp = 0f;

        private bool usandoDobleSaltoEnAire = false;
        private int saltosRestantesPrevios = -1;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            colJugador = GetComponent<Collider2D>();
        }

        private void FixedUpdate()
        {
            SincronizarSuelo();
        }

        private void Update()
        {
            if (rb == null || animator == null) return;

            if (cooldownSaltoCSharp > 0f) cooldownSaltoCSharp -= Time.deltaTime;
            if (bufferSaltoRestante > 0f) bufferSaltoRestante -= Time.deltaTime;

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                bufferSaltoRestante = jumpBufferTime;
            }

            // Re-sincronizar antes de que ScriptMachine.Update() lea 'enSuelo'
            SincronizarSuelo();

            if (enSueloFisico)
            {
                tiempoDesdeSuelo = 0f;
            }
            else
            {
                tiempoDesdeSuelo += Time.deltaTime;
            }

#if UNITY_6000_0_OR_NEWER
            Vector2 vel = rb.linearVelocity;
#else
            Vector2 vel = rb.velocity;
#endif

            if (spriteRenderer != null)
            {
                if (vel.x < -0.1f) spriteRenderer.flipX = true;
                else if (vel.x > 0.1f) spriteRenderer.flipX = false;
            }

            bool enSuelo = enSueloFisico || LeerEnSueloVisualScripting(vel.y);
            bool tieneDobleSalto = UtilidadesJugador.LeerVariableEscena("tieneDobleSalto", false);
            int saltosRestantes = UtilidadesJugador.LeerVariableEscena("saltosRestantes", 0);

            if (enSuelo)
            {
                usandoDobleSaltoEnAire = false;
            }
            else if (tieneDobleSalto && saltosRestantesPrevios > 0 && saltosRestantes == 0)
            {
                usandoDobleSaltoEnAire = true;
            }

            saltosRestantesPrevios = saltosRestantes;

            animator.SetFloat(HashVelocidad, Mathf.Abs(vel.x));
            animator.SetBool(HashEnSuelo, enSuelo);
            animator.SetBool(HashDobleSalto, !enSuelo && usandoDobleSaltoEnAire);
        }

        private void LateUpdate()
        {
            if (rb == null || bufferSaltoRestante <= 0f || cooldownSaltoCSharp > 0f) return;

#if UNITY_6000_0_OR_NEWER
            Vector2 vel = rb.linearVelocity;
#else
            Vector2 vel = rb.velocity;
#endif

            // Si LunaGraph ya aplicó el salto en este frame, limpiamos el buffer sin duplicar
            if (vel.y > 4.0f)
            {
                bufferSaltoRestante = 0f;
                return;
            }

            // Ejecutar salto por Jump Buffer o Coyote Time si el grafo no pudo dispararlo
            if ((enSueloFisico || tiempoDesdeSuelo <= coyoteTime) && vel.y <= 1.2f)
            {
                float fuerza = LeerFuerzaSalto();
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = new Vector2(vel.x, fuerza);
#else
                rb.velocity = new Vector2(vel.x, fuerza);
#endif
                EscribirEnSuelo(false);
                enSueloFisico = false;
                tiempoDesdeSuelo = coyoteTime + 0.1f;
                bufferSaltoRestante = 0f;
                cooldownSaltoCSharp = 0.15f;
            }
        }

        private void SincronizarSuelo()
        {
            if (rb == null) return;

#if UNITY_6000_0_OR_NEWER
            float velY = rb.linearVelocity.y;
#else
            float velY = rb.velocity.y;
#endif

            enSueloFisico = DetectarSueloBajoPies() && velY <= 0.6f;

            if (enSueloFisico)
            {
                EscribirEnSuelo(true);
                bool tieneDobleSalto = UtilidadesJugador.LeerVariableEscena("tieneDobleSalto", false);
                if (tieneDobleSalto && UtilidadesJugador.LeerVariableEscena("saltosRestantes", 0) <= 0)
                {
                    UtilidadesJugador.EscribirVariableEscena("saltosRestantes", 1);
                }
            }
        }

        private bool DetectarSueloBajoPies()
        {
            if (colJugador == null) return false;

            Bounds b = colJugador.bounds;
            Vector2 centro = new Vector2(b.center.x, b.min.y - 0.04f);
            Vector2 tamano = new Vector2(Mathf.Max(0.2f, b.size.x * 0.78f), 0.12f);

            int cantidad = Physics2D.OverlapBox(centro, tamano, 0f, ContactFilter2D.noFilter, bufferSuelo);
            for (int i = 0; i < cantidad; i++)
            {
                Collider2D hit = bufferSuelo[i];
                if (hit == null || hit == colJugador || hit.isTrigger) continue;
                if (hit.transform.IsChildOf(transform)) continue;

                // Solo contar superficies que estén debajo o a la altura de los pies (ignorar paredes laterales)
                if (hit.bounds.max.y <= b.min.y + 0.30f || hit.CompareTag("Suelo"))
                {
                    return true;
                }
            }
            return false;
        }

        private float LeerFuerzaSalto()
        {
            try
            {
                var objVars = Variables.Object(gameObject);
                if (objVars != null && objVars.IsDefined("fuerzaSalto"))
                {
                    float f = objVars.Get<float>("fuerzaSalto");
                    if (f > 1f) return f;
                }
            }
            catch
            {
                // Usar valor por defecto
            }
            return fuerzaSaltoFallback;
        }

        private void EscribirEnSuelo(bool valor)
        {
            try
            {
                var objVars = Variables.Object(gameObject);
                if (objVars != null && objVars.IsDefined("enSuelo"))
                {
                    objVars.Set("enSuelo", valor);
                }
            }
            catch
            {
                // Ignorar si no existe
            }
        }

        private bool LeerEnSueloVisualScripting(float velY)
        {
            try
            {
                var objVars = Variables.Object(gameObject);
                if (objVars != null && objVars.IsDefined("enSuelo"))
                {
                    return objVars.Get<bool>("enSuelo");
                }
            }
            catch
            {
                // Fallback por velocidad vertical si no existe la variable de objeto
            }

            return Mathf.Abs(velY) < 0.05f;
        }
    }
}
