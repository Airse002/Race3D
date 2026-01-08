using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrackGenerator : MonoBehaviour
{
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
    public float helixPitch = 5f; // Výška na jeden závit

    [Header("Lissajous Parameters")]
    public float lissajousA = 1f;
    public float lissajousB = 2f;
    public float lissajousAmplitudeX = 10f;
    public float lissajousAmplitudeY = 10f;
    public float lissajousDelta = Mathf.PI / 2;

    [Header("Gate Customization")]
    public bool rotateGates = true;
    public Vector3 gateRotationOffset = Vector3.zero;
    public bool oscillateGates = false;
    public OscillationType oscillationType = OscillationType.None;
    public float oscillationAmplitude = 2f;
    public float oscillationSpeed = 1f;
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

    void Start()
    {
        GenerateTrack();
    }

    // Context menu pro pravé tlačítko v Inspectoru
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

    // Reset button support
    void Reset()
    {
        ClearTrack();
    }

    public void GenerateTrack()
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

            // Aplikuj texturu
            if (gateTextures != null && gateTextures.Length > 0)
            {
                ApplyTexture(gate, i);
            }

            // Přidej oscilaci
            if (oscillateGates && oscillationType != OscillationType.None)
            {
                GateOscillator oscillator = gate.AddComponent<GateOscillator>();
                oscillator.oscillationType = oscillationType;
                oscillator.amplitude = oscillationAmplitude;
                oscillator.speed = oscillationSpeed;
                oscillator.originalPosition = position;
            }
        }

        // Spawn hráče
        SpawnPlayer();

        // Spawn kamery
        SpawnCamera();

        // Nastav pozadí
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
        Quaternion baseRotation = Quaternion.Euler(90, 0, 0);

        if (rotateGates && index < gateCount - 1)
        {
            Vector3 nextPos = GetGatePositionByType(index + 1);
            Vector3 direction = (nextPos - position).normalized;

            if (direction != Vector3.zero)
            {
                baseRotation = Quaternion.LookRotation(direction);
            }
        }

        // Přidej offset rotace
        baseRotation *= Quaternion.Euler(gateRotationOffset);

        return baseRotation;
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

        // Pozice mírně před první obručí
        Vector3 playerPos = GetGatePositionByType(0) - Vector3.forward * 20f;

        playerInstance = Instantiate(playerPrefab, playerPos, Quaternion.identity);
        playerInstance.name = "Player";
    }

    void SpawnCamera()
    {
        if (cameraPrefab == null) return;

        // Umísti kameru za hráče
        Vector3 cameraPos;
        if (playerInstance != null)
        {
            cameraPos = playerInstance.transform.position + cameraOffsetFromPlayer;
        }
        else
        {
            // Fallback pokud není hráč
            cameraPos = GetGatePositionByType(0) + cameraOffsetFromPlayer - Vector3.forward * 20f;
        }

        cameraInstance = Instantiate(cameraPrefab, cameraPos, Quaternion.identity);
        cameraInstance.name = "Main Camera";

        // Nastav jako main camera
        if (setCameraAsMainCamera)
        {
            Camera cam = cameraInstance.GetComponent<Camera>();
            if (cam != null)
            {
                cam.tag = "MainCamera";
            }
        }

        // Nastav target pro AircraftChaseCamera script
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
        // Smaž starý track (i pokud reference chybí, najdi ho podle jména)
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
            // Fallback: najdi track podle jména
            GameObject foundTrack = GameObject.Find("GeneratedTrack");
            if (foundTrack != null)
            {
                if (Application.isPlaying)
                    Destroy(foundTrack);
                else
                    DestroyImmediate(foundTrack);
            }
        }

        // Smaž starého hráče (i pokud reference chybí, najdi ho podle jména)
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
            // Fallback: najdi hráče podle jména
            GameObject foundPlayer = GameObject.Find("Player");
            if (foundPlayer != null)
            {
                if (Application.isPlaying)
                    Destroy(foundPlayer);
                else
                    DestroyImmediate(foundPlayer);
            }
        }

        // Smaž starou kameru (i pokud reference chybí)
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
            // Fallback: najdi kameru podle jména
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
}

// Komponenta pro oscilaci obruče
public class GateOscillator : MonoBehaviour
{
    public TrackGenerator.OscillationType oscillationType;
    public float amplitude = 2f;
    public float speed = 1f;
    public Vector3 originalPosition;

    private float time;

    void Update()
    {
        time += Time.deltaTime * speed;

        Vector3 offset = Vector3.zero;

        switch (oscillationType)
        {
            case TrackGenerator.OscillationType.VerticalSine:
                offset = Vector3.up * Mathf.Sin(time) * amplitude;
                break;

            case TrackGenerator.OscillationType.HorizontalSine:
                offset = Vector3.right * Mathf.Sin(time) * amplitude;
                break;

            case TrackGenerator.OscillationType.Circular:
                offset = new Vector3(
                    Mathf.Cos(time) * amplitude,
                    Mathf.Sin(time) * amplitude,
                    0
                );
                break;

            case TrackGenerator.OscillationType.Random:
                offset = new Vector3(
                    Mathf.PerlinNoise(time, 0) * amplitude,
                    Mathf.PerlinNoise(0, time) * amplitude,
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