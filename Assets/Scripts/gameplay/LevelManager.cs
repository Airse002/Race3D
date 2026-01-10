using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrackGenerator : MonoBehaviour
{
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
    public float zigzagSegmentLength = 5;

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
    public Vector3 gateHoleAxisLocal = Vector3.forward; 
    // jaká lokální osa gate prefab představuje směr "skrz kruh"
    // nejčastěji forward (Z), někdy up (Y)


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

    [Header("Background")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.2f);
    public bool applyBackgroundColor = true;

    private GameObject trackContainer;
    private GameObject playerInstance;
    private GameObject cameraInstance;

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
        ClearTrack();
    }

    void Start()
    {
        GenerateTrack();
    }

    [ContextMenu("Generate Track")]
    void ContextGenerateTrack()
    {
        GenerateTrack();
    }

    [ContextMenu("Clear Track")]
    void ContextClearTrack()
    {
        ClearTrack();
    }

    void Reset()
    {
        ClearTrack();
    }

    // === PUBLIC API ===

    public void GenerateTrack()
    {
        GenerateTrackWithCurrentSettings();
    }

    // Plně parametrická verze pomocí TrackConfig struktury
    public void GenerateTrack(TrackConfig config)
    {
        ApplyConfig(config);
        GenerateTrackWithCurrentSettings();
    }

    // Zjednodušená verze pro rychlé použití
    public void GenerateTrackSimple(TrackType type, int count, float spacing)
    {
        trackType = type;
        gateCount = count;
        gateSpacing = spacing;
        GenerateTrackWithCurrentSettings();
    }

    // === INTERNÍ GENEROVÁNÍ ===

    private void GenerateTrackWithCurrentSettings()
    {
        ClearTrack();

        trackContainer = new GameObject("GeneratedTrack");
        trackContainer.transform.parent = transform;

        for (int i = 0; i < gateCount; i++)
        {
            Vector3 position = GetGatePositionByType(i);
            Quaternion rotation = GetGateRotation(i, position);

            GameObject gate = Instantiate(gatePrefab, position, rotation, trackContainer.transform);
            gate.name = $"Gate_{i}";

            // Checkpoint detector + index -> na TriggerZone
            Transform trigger = gate.transform.Find("TriggerZone");
            if (trigger != null)
            {
                var detector = trigger.GetComponent<CheckpointDetector>();
                if (detector == null) detector = trigger.gameObject.AddComponent<CheckpointDetector>();
                detector.checkpointIndex = i;
            }
            else
            {
            Debug.LogWarning($"Gate_{i} nemá child 'TriggerZone'! Dávám detektor na root.");
            var detector = gate.GetComponent<CheckpointDetector>();
            if (detector == null) detector = gate.AddComponent<CheckpointDetector>();
            detector.checkpointIndex = i;
            }




            // Aplikuj texturu
            if (gateTextures != null && gateTextures.Length > 0)
            {
                ApplyTexture(gate, i);
            }

            // Přidej oscilaci s variací per-gate
            if (oscillateGates && oscillationType != OscillationType.None)
            {
                GateOscillator oscillator = gate.AddComponent<GateOscillator>();
                oscillator.oscillationType = oscillationType;
                oscillator.amplitude = oscillationAmplitude;

                // Variace rychlosti
                if (varyOscillationSpeed)
                {
                    float speedVar = Random.Range(-speedVariationAmount, speedVariationAmount);
                    oscillator.speed = oscillationSpeed + speedVar;
                }
                else
                {
                    oscillator.speed = oscillationSpeed;
                }

                // Fázový offset pro každou obruč
                if (varyOscillationPhase)
                {
                    oscillator.phaseOffset = i * phaseOffsetPerGate;
                }

                oscillator.originalPosition = position;
            }
        }

        SpawnPlayer();
        

        SpawnCamera();

        if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.StartRace(gateCount, timeLimitSeconds);
            }


        if (applyBackgroundColor && cameraInstance != null)
        {
            Camera cam = cameraInstance.GetComponent<Camera>();
            if (cam != null)
            {
                cam.backgroundColor = backgroundColor;
            }
        }
    }

    Vector3 GetGatePositionByType(int index)
    {
        float t = index * gateSpacing;
        float z = startOffset + t;

        switch (trackType)
        {
            case TrackType.Linear:
                return new Vector3(0, 0, z);

            case TrackType.Sine:
                float y = sineAmplitude * Mathf.Sin(sineFrequency * z);
                float x = sineHorizontalOffset * Mathf.Sin(sineFrequency * z + Mathf.PI / 2);
                return new Vector3(x, y, z);

            case TrackType.Zigzag:
                float zigzagX = (index % 2 == 0) ? -zigzagAmplitude : zigzagAmplitude;
                return new Vector3(zigzagX, 0, z);

            case TrackType.Helix:
                float angle = (t / helixPitch) * 2 * Mathf.PI;
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

    if (index < gateCount - 1)
        dir = GetGatePositionByType(index + 1) - position;
    else
        dir = position - GetGatePositionByType(index - 1);

    if (dir.sqrMagnitude < 0.0001f)
        dir = Vector3.forward;

    dir.Normalize();

    // 1) Otoč tak, aby osa "díry" mířila do směru trati
    Quaternion rot = Quaternion.FromToRotation(gateHoleAxisLocal.normalized, dir);

    // 2) Stabilizace twistu: zkus udržet "nahoru" podobně jako světový up
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

    // 3) ruční offset
    rot *= Quaternion.Euler(gateRotationOffset);

    return rot;
}



    void ApplyTexture(GameObject gate, int index)
    {
        Material mat;

        if (randomizeTextures)
        {
            mat = gateTextures[Random.Range(0, gateTextures.Length)];
        }
        else
        {
            mat = gateTextures[index % gateTextures.Length];
        }

        Renderer[] renderers = gate.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.material = mat;
        }
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null) return;

        Vector3 playerPos = GetGatePositionByType(0) - Vector3.forward * 20f;
        playerInstance = Instantiate(playerPrefab, playerPos, Quaternion.identity);
        playerInstance.name = "Player";
    }

    void SpawnCamera()
    {
        if (cameraPrefab == null) return;

        Vector3 cameraPos;
        if (playerInstance != null)
        {
            cameraPos = playerInstance.transform.position + cameraOffsetFromPlayer;
        }
        else
        {
            cameraPos = GetGatePositionByType(0) + cameraOffsetFromPlayer - Vector3.forward * 20f;
        }

        cameraInstance = Instantiate(cameraPrefab, cameraPos, Quaternion.identity);
        cameraInstance.name = "Main Camera";

        if (setCameraAsMainCamera)
        {
            Camera cam = cameraInstance.GetComponent<Camera>();
            if (cam != null)
            {
                cam.tag = "MainCamera";
            }
        }

        if (playerInstance != null)
        {
            AircraftChaseCamera chaseCamera = cameraInstance.GetComponent<AircraftChaseCamera>();
            if (chaseCamera != null)
            {
                chaseCamera.target = playerInstance.transform;
            }
        }
    }

    public void ClearTrack()
    {
        if (trackContainer != null)
        {
            if (Application.isPlaying)
                Destroy(trackContainer);
            else
                DestroyImmediate(trackContainer);
            trackContainer = null;
        }
        else
        {
            GameObject foundTrack = GameObject.Find("GeneratedTrack");
            if (foundTrack != null)
            {
                if (Application.isPlaying)
                    Destroy(foundTrack);
                else
                    DestroyImmediate(foundTrack);
            }
        }

        if (playerInstance != null)
        {
            if (Application.isPlaying)
                Destroy(playerInstance);
            else
                DestroyImmediate(playerInstance);
            playerInstance = null;
        }
        else
        {
            GameObject foundPlayer = GameObject.Find("Player");
            if (foundPlayer != null)
            {
                if (Application.isPlaying)
                    Destroy(foundPlayer);
                else
                    DestroyImmediate(foundPlayer);
            }
        }

        if (cameraInstance != null)
        {
            if (Application.isPlaying)
                Destroy(cameraInstance);
            else
                DestroyImmediate(cameraInstance);
            cameraInstance = null;
        }
        else
        {
            GameObject foundCamera = GameObject.Find("Main Camera");
            if (foundCamera != null)
            {
                if (Application.isPlaying)
                    Destroy(foundCamera);
                else
                    DestroyImmediate(foundCamera);
            }
        }
    }

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

        // Gate customization
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
}

// ===TRACK CONFIG STRUCTURE ===
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

    // Konstruktor pro snadné vytváření
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