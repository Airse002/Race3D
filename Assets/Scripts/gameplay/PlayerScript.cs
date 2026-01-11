using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class AircraftRocketController : MonoBehaviour
{
    [Header("Speed / Throttle")]
    public float cruiseSpeed = 25f;          // cílová rychlost dopředu
    public float throttleResponse = 3f;      // jak rychle se dorovnává na cruiseSpeed

    [Header("Rates (deg/sec)")]
    public float pitchRate = 85f;            // ↑/↓
    public float rollRate  = 120f;           // ←/→
    public float yawRate   = 45f;            // A/D nebo Q/E (volitelné)

    [Header("Feel")]
    public float angularResponse = 8f;       // jak “ostře” reaguje na ovládání (větší = ostřejší)
    public float autoLevelStrength = 2.5f;   // 0 = vypnuto, jinak sám rovná náklon když nepřidržuješ roll

    public float deadzone = 0.15f;

    [Header("Physics")]
    public bool useGravity = false;
    public float linearDrag = 0f;
    public float angularDrag = 2f;

    public bool inputEnabled = true;
    public bool movementEnabled = true;


    Rigidbody rb;

    // inputs
    float pitchInput;   // + = nos nahoru
    float rollInput;    // + = roll doprava
    float yawInput;     // + = yaw doprava

    float currentSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = useGravity;
        rb.linearDamping = linearDrag;
        rb.angularDamping = angularDrag;

        // doporučené pro plynulost
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        currentSpeed = cruiseSpeed;
    }

    void Update()
    {
         if (!inputEnabled)
        {
            pitchInput = rollInput = yawInput = 0f;
            return;
        }



        var kb = Keyboard.current;
        if (kb == null)
        {
            pitchInput = rollInput = yawInput = 0f;
            return;
        }

        // Šipky = klasický “flight stick”
        pitchInput = (kb.downArrowKey.isPressed ? 1f : 0f) + (kb.upArrowKey.isPressed ? -1f : 0f);
        rollInput  = (kb.rightArrowKey.isPressed ? 1f : 0f) + (kb.leftArrowKey.isPressed ? -1f : 0f);

        // Volitelné yaw (směrovka)
        yawInput = 0f;
        if (kb.aKey.isPressed || kb.qKey.isPressed) yawInput -= 1f;
        if (kb.dKey.isPressed || kb.eKey.isPressed) yawInput += 1f;

        pitchInput = Mathf.Clamp(pitchInput, -1f, 1f);
        rollInput  = Mathf.Clamp(rollInput,  -1f, 1f);
        yawInput   = Mathf.Clamp(yawInput,   -1f, 1f);
    }

    void FixedUpdate()
        {
        if (!movementEnabled)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }


        float dt = Time.fixedDeltaTime;

        // 1) Drž rychlost dopředu (throttle)
        currentSpeed = Mathf.Lerp(currentSpeed, cruiseSpeed, 1f - Mathf.Exp(-throttleResponse * dt));
        rb.linearVelocity = transform.forward * currentSpeed;

        // 2) Cílová úhlová rychlost v lokálním prostoru (rad/s)
        float pitchRad = pitchRate * Mathf.Deg2Rad;
        float rollRad  = rollRate  * Mathf.Deg2Rad;
        float yawRad   = yawRate   * Mathf.Deg2Rad;

        // Unity osy: x=right, y=up, z=forward
        // Pitch = rotace kolem X, Yaw kolem Y, Roll kolem Z
        Vector3 targetAngVelLocal = new Vector3(
            -pitchInput * pitchRad,      // minus => ↑ dává nos nahoru přirozeněji
            yawInput * yawRad,
            -rollInput * rollRad
        );

        // 3) Auto-level (srovná bank/roll když nepřidržuješ ←/→)
        if (autoLevelStrength > deadzone && Mathf.Abs(rollInput) < deadzone)
        {
            // bank angle vůči horizontu (v deg)
            Vector3 f = transform.forward;
            Vector3 upProj = Vector3.ProjectOnPlane(transform.up, f).normalized;
            Vector3 worldUpProj = Vector3.ProjectOnPlane(Vector3.up, f).normalized;

            if (upProj.sqrMagnitude > 0.0001f && worldUpProj.sqrMagnitude > 0.0001f)
            {
                float bank = Vector3.SignedAngle(upProj, worldUpProj, f); // + když jsi nakloněný
                float correction = -bank * autoLevelStrength * Mathf.Deg2Rad; // rad/s (proti banku)
                targetAngVelLocal.z += correction;
            }
        }

        // 4) Plynule nastav úhlovou rychlost (world space)
        Vector3 targetAngVelWorld = transform.TransformDirection(targetAngVelLocal);
        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, targetAngVelWorld, 1f - Mathf.Exp(-angularResponse * dt));
    }
}
