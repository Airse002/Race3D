using UnityEngine;

public class AircraftChaseCamera : MonoBehaviour
{
    public Transform target;

    [Header("Offset in target local space")]
    public Vector3 localOffset = new Vector3(0f, 3.5f, -12f);

    [Header("Smoothing")]
    public float positionSmoothTime = 0.12f;
    public float rotationLerp = 10f;

    [Header("Look")]
    public float lookAhead = 10f;

    [Range(0f, 1f)]
    public float rollFollow = 0.6f; // 0 = kamera rovně, 1 = plně se naklání s lodí

    [Header("Boost Effects")]
    [Tooltip("Síla třesení kamery při boostu")]
    public float boostShakeIntensity = 0.15f;

    [Tooltip("Frekvence třesení kamery")]
    public float boostShakeFrequency = 25f;

    [Tooltip("Změna FOV při boostu (přidá se k základnímu FOV)")]
    public float boostFOVIncrease = 15f;

    [Tooltip("Rychlost změny FOV")]
    public float fovChangeSpeed = 8f;

    [Header("Boost Camera Distance")]
    [Tooltip("Když true, kamera se při boostu trochu oddálí")]
    public bool zoomOutOnBoost = true;

    [Tooltip("O kolik se kamera oddálí při boostu")]
    public float boostDistanceIncrease = 2f;

    // Private vars
    private Vector3 posVel;
    private Camera cam;
    private float baseFOV = 60f;
    private float targetFOV;
    private float shakeTime = 0f;
    private AircraftRocketController playerController;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            baseFOV = cam.fieldOfView;
            targetFOV = baseFOV;
        }

        // Najdi player controller
        FindPlayerController();
    }

    void FindPlayerController()
    {
        if (target == null) return;

        playerController = target.GetComponent<AircraftRocketController>();
        if (playerController == null)
        {
            // Zkus najít podle tagu
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                playerController = player.GetComponent<AircraftRocketController>();
        }
    }

    void LateUpdate()
    {
        if (!target)
        {
            Debug.LogWarning("[AircraftChaseCamera] Target is null!");
            return;
        }

        // Zkontroluj, jestli boost je aktivní
        bool isBoosting = false;
        if (playerController == null)
            FindPlayerController();

        if (playerController != null)
        {
            isBoosting = playerController.IsBoosting();

            // DEBUG - výpis jen občas (každých 60 framů)
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[ChaseCamera] IsBoosting: {isBoosting}, ShakeIntensity: {boostShakeIntensity}");
            }
        }
        else
        {
            if (Time.frameCount % 120 == 0)
            {
                Debug.LogWarning("[AircraftChaseCamera] PlayerController not found!");
            }
        }

        // === POZICE S MOŽNÝM ZOOM OUT ===
        Vector3 offsetToUse = localOffset;
        if (isBoosting && zoomOutOnBoost)
        {
            // Oddal kameru dozadu při boostu
            offsetToUse.z -= boostDistanceIncrease;
        }

        // Vypočítej pozici BEZ shake (smooth damping nesmí zahrnovat shake)
        Vector3 desiredPos = target.TransformPoint(offsetToUse);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref posVel, positionSmoothTime);

        // === CAMERA SHAKE - APLIKUJ AŽ PO SMOOTH DAMP ===
        if (isBoosting)
        {
            shakeTime += Time.deltaTime * boostShakeFrequency;

            // Perlin noise pro plynulejší shake
            float shakeX = (Mathf.PerlinNoise(shakeTime, 0f) - 0.5f) * 2f * boostShakeIntensity;
            float shakeY = (Mathf.PerlinNoise(0f, shakeTime) - 0.5f) * 2f * boostShakeIntensity;
            float shakeZ = (Mathf.PerlinNoise(shakeTime, shakeTime) - 0.5f) * 2f * boostShakeIntensity * 0.5f;

            Vector3 shakeOffset = new Vector3(shakeX, shakeY, shakeZ);

            // Přidej shake PŘÍMO k finální pozici
            transform.position += shakeOffset;

            // DEBUG - výpis shake offsetu
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[ChaseCamera] ShakeOffset: {shakeOffset}");
            }
        }
        else
        {
            shakeTime = 0f;
        }

        // === ROTACE ===
        Vector3 lookPoint = target.position + target.forward * lookAhead;
        Vector3 forwardToLook = (lookPoint - transform.position).normalized;

        // up v kameře: blend mezi světovým up a up lodě (kvůli roll follow)
        Vector3 upDir = Vector3.Slerp(Vector3.up, target.up, rollFollow);

        Quaternion desiredRot = Quaternion.LookRotation(forwardToLook, upDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationLerp * Time.deltaTime);

        // === FOV EFEKT ===
        if (cam != null)
        {
            targetFOV = isBoosting ? baseFOV + boostFOVIncrease : baseFOV;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Nastaví základní FOV (volitelné, pro runtime změny)
    /// </summary>
    public void SetBaseFOV(float fov)
    {
        baseFOV = fov;
        if (cam != null && !playerController.IsBoosting())
            cam.fieldOfView = baseFOV;
    }
}