using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrackGenerator : MonoBehaviour
{
    [Header("Bootstrap")]
    [Tooltip("Když true, v Play módu se trať vygeneruje automaticky v Start().\n" +
             "Když false, trať generuj jen z Menu/LevelManageru (doporučeno).")]
    public bool generateOnStart = false;

    [Tooltip("Když true, při spuštění hry smaže případný starý GeneratedTrack/Player/Camera.")]
    public bool clearOnPlayStart = true;

    [Header("Race Time")]
    public float timeLimitSeconds = 180f;

    [Header("References")]
    public GameObject gatePrefab;
    public GameObject playerPrefab;
    public GameObject cameraPrefab;

    [Header("Camera Settings")]
    public Vector3 cameraOffsetFromPlayer = new Vector3(0, 5, -10);
    public bool setCameraAsMainCamera = true;

    [Header("Track Settings")]
    public int gateCount = 20;
    public float gateSpacing = 50f;
    public float startOffset = -40f;

    [Header("Track Type")]
    public TrackType trackType = TrackType.Sine;

    [Header("Sine Wave Parameters")]
    public float sineAmplitude = 10f;
    public float sineFrequency = 0.1f;
    public float sineHorizontalOffset = 5f;

    [Header("Linear Parameters")]
    public Vector3 linearDirection = Vector3.forward;

    [Header("Zigzag Parameters")]
    public float zigzagAmplitude = 15f;
    public float zigzagSegmentLength = 5f;

    [Header("Helix/Spring Parameters")]
    public float helixRadius = 10f;
    public float helixPitch = 5f;

    [Header("Lissajous Parameters")]
    public float lissajousA = 1f;
    public float lissajousB = 2f;
    public float lissajousAmplitudeX = 10f;
    public float lissajousAmplitudeY = 10f;
    public float lissajousDelta = Mathf.PI / 2;

    [Header("Gate Customization")]
    public bool rotateGates = true;
    public Vector3 gateRotationOffset = Vector3.zero;

    [Header("Gate Orientation Axis")]
    [Tooltip("Jaká LOKÁLNÍ osa gate prefabu představuje směr \"skrz kruh\".\n" +
             "Nejčastěji Vector3.forward (Z), někdy Vector3.up (Y).")]
    public Vector3 gateHoleAxisLocal = Vector3.forward;

    [Header("Oscillation")]
    public bool oscillateGates = false;
    public OscillationType oscillationType = OscillationType.None;
    public float oscillationAmplitude = 2f;
    public float oscillationSpeed = 1f;

    [Header("Per-Gate Variation")]
    public bool varyOscillationPhase = true;
    public float phaseOffsetPerGate = 0.5f;
    public bool varyOscillationSpeed = false;
    public float speedVariationAmount = 0.2f;

    [Header("Textures")]
    public Material[] gateTextures;
    public bool randomizeTextures = false;

    [Header("UI / Countdown")]
    [Tooltip("Když je vyplněné, závod začne až na GO. Když není, StartRace se zavolá hned po spawnu.")]
    public RaceCountdown countdown;

    [Header("Background")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.2f);
    public bool applyBackgroundColor = true;

    // Runtime instances
    private GameObject trackContainer;
    private GameObject playerInstance;
    private GameObject cameraInstance;

    public GameObject PlayerInstance => playerInstance;
    public GameObject CameraInstance => cameraInstance;

    public enum TrackType
    {
        Linear,
        Sine,
        Zigzag,
        Helix,
        Lissajous,
        Random
    }

    public enum OscillationType
    {
        None,
        VerticalSine,
        HorizontalSine,
        Circular,
        Random
    }

    void Awake()
    {
        // V Play módu smaž staré runtime věci (GeneratedTrack/Player/Camera),
        // aby ti po reloadu/stop-play nezůstával bordel.
        if (Application.isPlaying && clearOnPlayStart)
            ClearTrack();
    }

    void Start()
    {
        // DŮLEŽITÉ: už negeneruj vždycky — jen když to chceš.
        if (generateOnStart)
            GenerateTrack();
    }

    // === PUBLIC API ===

    [ContextMenu("Generate Track")]
    public void GenerateTrack()
    {
        GenerateTrackWithCurrentSettings();
    }

    // Plně parametrická verze pomocí TrackConfig
    public void GenerateTrack(TrackConfig config)
    {
        ApplyConfig(config);
        GenerateTrackWithCurrentSettings();
    }

    // Zjednodušená verze
    public void GenerateTrackSimple(TrackType type, int count, float spacing)
    {
        trackType = type;
        gateCount = count;
        gateSpacing = spacing;
        GenerateTrackWithCurrentSettings();
    }

    [ContextMenu("Clear Track")]
    public void ClearTrack()
    {
        DestroySafe(trackContainer);
        trackContainer = null;

        // fallback – když container neexistuje, zkus najít podle jména
        if (Application.isPlaying)
        {
            var foundTrack = GameObject.Find("GeneratedTrack");
            if (foundTrack != null) DestroySafe(foundTrack);
        }

        DestroySafe(playerInstance);
        playerInstance = null;
        if (Application.isPlaying)
        {
            var foundPlayer = GameObject.Find("Player");
            if (foundPlayer != null) DestroySafe(foundPlayer);
        }

        DestroySafe(cameraInstance);
        cameraInstance = null;
        if (Application.isPlaying)
        {
            var foundCam = GameObject.Find("Main Camera");
            if (foundCam != null) DestroySafe(foundCam);
        }
    }

    // === GENERATION CORE ===

    private void GenerateTrackWithCurrentSettings()
    {
        if (gatePrefab == null)
        {
            Debug.LogError("TrackGenerator: gatePrefab není přiřazený!");
            return;
        }

        ClearTrack();

        trackContainer = new GameObject("GeneratedTrack");
        trackContainer.transform.SetParent(transform, false);

        for (int i = 0; i < gateCount; i++)
        {
            Vector3 position = GetGatePositionByType(i);
            Quaternion rotation = GetGateRotation(i, position);

            GameObject gate = Instantiate(gatePrefab, position, rotation, trackContainer.transform);
            gate.name = $"Gate_{i:00}";

            SetupCheckpointDetector(gate, i);

            if (gateTextures != null && gateTextures.Length > 0)
                ApplyTexture(gate, i);

            if (oscillateGates && oscillationType != OscillationType.None)
                AddOscillation(gate, i, position);
        }

        SpawnPlayer();
        SpawnCamera();
        ApplyCameraBackground();

        // Start závodu:
        // - když máš countdown, start je až na GO
        // - když nemáš countdown, startni hned
        if (countdown == null)
        {
            // auto-find (když jsi zapomněl přiřadit)
            countdown = FindObjectOfType<RaceCountdown>(true);
        }

        if (countdown != null && playerInstance != null)
        {
            countdown.trackGenerator = this;
            countdown.Begin(playerInstance);
        }
        else
        {
            // bez countdownu -> start hned
            ScoreManager.Instance?.StartRace(gateCount, timeLimitSeconds);
        }
    }

    private void SetupCheckpointDetector(GameObject gate, int index)
    {
        // Doporučeno: detektor sedí na TriggerZone (child), protože root má mesh collider apod.
        Transform trigger = gate.transform.Find("TriggerZone");

        GameObject host = (trigger != null) ? trigger.gameObject : gate;

        if (trigger == null)
            Debug.LogWarning($"{gate.name} nemá child 'TriggerZone' -> dávám CheckpointDetector na root.");

        var detector = host.GetComponent<CheckpointDetector>();
        if (detector == null) detector = host.AddComponent<CheckpointDetector>();
        detector.checkpointIndex = index;

        // Bezpečnost: když je tam collider, ať je trigger
        var col = host.GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void AddOscillation(GameObject gate, int index, Vector3 originalPos)
    {
        GateOscillator oscillator = gate.AddComponent<GateOscillator>();
        oscillator.oscillationType = oscillationType;
        oscillator.amplitude = oscillationAmplitude;

        if (varyOscillationSpeed)
        {
            float speedVar = Random.Range(-speedVariationAmount, speedVariationAmount);
            oscillator.speed = Mathf.Max(0.01f, oscillationSpeed + speedVar);
        }
        else
        {
            oscillator.speed = oscillationSpeed;
        }

        if (varyOscillationPhase)
            oscillator.phaseOffset = index * phaseOffsetPerGate;

        oscillator.originalPosition = originalPos;
    }

    private void ApplyCameraBackground()
    {
        if (!applyBackgroundColor || cameraInstance == null) return;

        Camera cam = cameraInstance.GetComponent<Camera>();
        if (cam != null)
            cam.backgroundColor = backgroundColor;
    }

    // === POSITIONS / ROTATIONS ===

    Vector3 GetGatePositionByType(int index)
    {
        float t = index * gateSpacing;
        float z = startOffset + t;

        switch (trackType)
        {
            case TrackType.Linear:
                // lineární směr (když chceš, můžeš použít linearDirection)
                return new Vector3(0, 0, z);

            case TrackType.Sine:
                float y = sineAmplitude * Mathf.Sin(sineFrequency * z);
                float x = sineHorizontalOffset * Mathf.Sin(sineFrequency * z + Mathf.PI / 2);
                return new Vector3(x, y, z);

            case TrackType.Zigzag:
                float zigzagX = (index % 2 == 0) ? -zigzagAmplitude : zigzagAmplitude;
                return new Vector3(zigzagX, 0, z);

            case TrackType.Helix:
                float angle = (t / helixPitch) * 2f * Mathf.PI;
                float helixX = helixRadius * Mathf.Cos(angle);
                float helixY = helixRadius * Mathf.Sin(angle);
                return new Vector3(helixX, helixY, z);

            case TrackType.Lissajous:
                float lissX = lissajousAmplitudeX * Mathf.Sin(lissajousA * t + lissajousDelta);
                float lissY = lissajousAmplitudeY * Mathf.Sin(lissajousB * t);
                return new Vector3(lissX, lissY, z);

            case TrackType.Random:
                float randX = Random.Range(-15f, 15f);
                float randY = Random.Range(-15f, 15f);
                return new Vector3(randX, randY, z);

            default:
                return new Vector3(0, 0, z);
        }
    }

    Quaternion GetGateRotation(int index, Vector3 position)
    {
        if (!rotateGates)
            return Quaternion.Euler(gateRotationOffset);

        Vector3 dir;

        if (gateCount <= 1)
            dir = Vector3.forward;
        else if (index < gateCount - 1)
            dir = GetGatePositionByType(index + 1) - position;
        else
            dir = position - GetGatePositionByType(index - 1);

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.forward;

        dir.Normalize();

        // 1) align hole axis to dir
        Quaternion rot = Quaternion.FromToRotation(gateHoleAxisLocal.normalized, dir);

        // 2) stabilize twist (keep "up" close to world up)
        Vector3 upRef = Vector3.ProjectOnPlane(Vector3.up, dir);
        if (upRef.sqrMagnitude > 0.0001f)
        {
            Vector3 curUp = Vector3.ProjectOnPlane(rot * Vector3.up, dir);
            if (curUp.sqrMagnitude > 0.0001f)
            {
                Quaternion twist = Quaternion.FromToRotation(curUp.normalized, upRef.normalized);
                rot = twist * rot;
            }
        }

        // 3) manual offset (for prefab corrections)
        rot *= Quaternion.Euler(gateRotationOffset);

        return rot;
    }

    // === VISUALS ===

    void ApplyTexture(GameObject gate, int index)
    {
        if (gateTextures == null || gateTextures.Length == 0) return;

        Material mat = randomizeTextures
            ? gateTextures[Random.Range(0, gateTextures.Length)]
            : gateTextures[index % gateTextures.Length];

        Renderer[] renderers = gate.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.material = mat;
    }

    // === SPAWNS ===

    void SpawnPlayer()
    {
        if (playerPrefab == null) return;

        Vector3 playerPos = GetGatePositionByType(0) - Vector3.forward * 20f;
        playerInstance = Instantiate(playerPrefab, playerPos, Quaternion.identity);
        playerInstance.name = "Player";
        playerInstance.tag = "Player"; // hodně důležité pro CheckpointDetector
    }

    void SpawnCamera()
    {
        if (cameraPrefab == null) return;

        Vector3 cameraPos = (playerInstance != null)
            ? playerInstance.transform.position + cameraOffsetFromPlayer
            : GetGatePositionByType(0) + cameraOffsetFromPlayer - Vector3.forward * 20f;

        cameraInstance = Instantiate(cameraPrefab, cameraPos, Quaternion.identity);
        cameraInstance.name = "Main Camera";

        if (setCameraAsMainCamera)
        {
            Camera cam = cameraInstance.GetComponent<Camera>();
            if (cam != null) cam.tag = "MainCamera";
        }

        if (playerInstance != null)
        {
            AircraftChaseCamera chaseCamera = cameraInstance.GetComponent<AircraftChaseCamera>();
            if (chaseCamera != null)
                chaseCamera.target = playerInstance.transform;
        }
    }

    // === CONFIG ===

    private void ApplyConfig(TrackConfig config)
    {
        trackType = config.trackType;
        gateCount = config.gateCount;
        gateSpacing = config.gateSpacing;
        startOffset = config.startOffset;
        timeLimitSeconds = config.timeLimitSeconds;

        // Sine
        sineAmplitude = config.sineAmplitude;
        sineFrequency = config.sineFrequency;
        sineHorizontalOffset = config.sineHorizontalOffset;

        // Zigzag
        zigzagAmplitude = config.zigzagAmplitude;

        // Helix
        helixRadius = config.helixRadius;
        helixPitch = config.helixPitch;

        // Lissajous
        lissajousA = config.lissajousA;
        lissajousB = config.lissajousB;
        lissajousAmplitudeX = config.lissajousAmplitudeX;
        lissajousAmplitudeY = config.lissajousAmplitudeY;
        lissajousDelta = config.lissajousDelta;

        // Gates
        rotateGates = config.rotateGates;
        gateRotationOffset = config.gateRotationOffset;
        oscillateGates = config.oscillateGates;
        oscillationType = config.oscillationType;
        oscillationAmplitude = config.oscillationAmplitude;
        oscillationSpeed = config.oscillationSpeed;

        varyOscillationPhase = config.varyOscillationPhase;
        phaseOffsetPerGate = config.phaseOffsetPerGate;
        varyOscillationSpeed = config.varyOscillationSpeed;
        speedVariationAmount = config.speedVariationAmount;

        // Background
        backgroundColor = config.backgroundColor;
        applyBackgroundColor = config.applyBackgroundColor;
    }

    // === HELPERS ===

    private void DestroySafe(GameObject go)
    {
        if (go == null) return;
        if (Application.isPlaying) Destroy(go);
        else DestroyImmediate(go);
    }
}

// === TRACK CONFIG ===
[System.Serializable]
public class TrackConfig
{
    // Basic
    public float timeLimitSeconds = 60f;

    public TrackGenerator.TrackType trackType = TrackGenerator.TrackType.Sine;
    public int gateCount = 20;
    public float gateSpacing = 50f;
    public float startOffset = -40f;

    // Sine
    public float sineAmplitude = 10f;
    public float sineFrequency = 0.1f;
    public float sineHorizontalOffset = 5f;

    // Zigzag
    public float zigzagAmplitude = 15f;

    // Helix
    public float helixRadius = 10f;
    public float helixPitch = 5f;

    // Lissajous
    public float lissajousA = 1f;
    public float lissajousB = 2f;
    public float lissajousAmplitudeX = 10f;
    public float lissajousAmplitudeY = 10f;
    public float lissajousDelta = Mathf.PI / 2;

    // Gates
    public bool rotateGates = true;
    public Vector3 gateRotationOffset = Vector3.zero;
    public bool oscillateGates = false;
    public TrackGenerator.OscillationType oscillationType = TrackGenerator.OscillationType.None;
    public float oscillationAmplitude = 2f;
    public float oscillationSpeed = 1f;

    // Variation
    public bool varyOscillationPhase = true;
    public float phaseOffsetPerGate = 0.5f;
    public bool varyOscillationSpeed = false;
    public float speedVariationAmount = 0.2f;

    // Visual
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.2f);
    public bool applyBackgroundColor = true;

    // Ctor
    public TrackConfig(TrackGenerator.TrackType type, int count, float spacing)
    {
        trackType = type;
        gateCount = count;
        gateSpacing = spacing;
    }
}

// === GATE OSCILLATOR ===
public class GateOscillator : MonoBehaviour
{
    public TrackGenerator.OscillationType oscillationType;
    public float amplitude = 2f;
    public float speed = 1f;
    public float phaseOffset = 0f;
    public Vector3 originalPosition;

    private float time;

    void Update()
    {
        time += Time.deltaTime * speed;
        float t = time + phaseOffset;

        Vector3 offset = Vector3.zero;

        switch (oscillationType)
        {
            case TrackGenerator.OscillationType.VerticalSine:
                offset = Vector3.up * Mathf.Sin(t) * amplitude;
                break;

            case TrackGenerator.OscillationType.HorizontalSine:
                offset = Vector3.right * Mathf.Sin(t) * amplitude;
                break;

            case TrackGenerator.OscillationType.Circular:
                offset = new Vector3(
                    Mathf.Cos(t) * amplitude,
                    Mathf.Sin(t) * amplitude,
                    0
                );
                break;

            case TrackGenerator.OscillationType.Random:
                offset = new Vector3(
                    Mathf.PerlinNoise(t, 0) * amplitude * 2 - amplitude,
                    Mathf.PerlinNoise(0, t) * amplitude * 2 - amplitude,
                    0
                );
                break;
        }

        transform.position = originalPosition + offset;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TrackGenerator))]
public class TrackGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TrackGenerator generator = (TrackGenerator)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Track", GUILayout.Height(30)))
        {
            generator.GenerateTrack();
        }

        if (GUILayout.Button("Clear Track", GUILayout.Height(30)))
        {
            generator.ClearTrack();
        }
    }
}
#endif
