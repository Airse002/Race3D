using UnityEngine;
using UnityEngine.InputSystem; // NOVÝ INPUT SYSTEM


public class LevelMenuManager : MonoBehaviour
{
    public TrackGenerator trackGenerator;

    // Předpřipravené levely
    private TrackConfig[] levelConfigs;

    void Awake()
    {
        Debug.Log("LevelMenuManager: Awake called");
        InitializeLevelConfigs();
        Debug.Log($"LevelMenuManager: {levelConfigs.Length} levels initialized");
    }

    void Start()
    {
        if (trackGenerator == null)
        {
            Debug.LogError("LevelMenuManager: TrackGenerator is NOT assigned! Please assign it in the Inspector.");
        }
        else
        {
            Debug.Log("LevelMenuManager: TrackGenerator is assigned correctly. Press 1-6 to load levels.");
        }
    }

    void InitializeLevelConfigs()
    {
        levelConfigs = new TrackConfig[]
        {
            // Level 1 - Easy Tutorial
            new TrackConfig(TrackGenerator.TrackType.Linear, 10, 60f)
            {
                timeLimitSeconds = 150f,
                startOffset = -40f,
                rotateGates = false,
                oscillateGates = false,
                backgroundColor = new Color(0.2f, 0.3f, 0.5f)
            },

            // Level 2 - Gentle Sine
            new TrackConfig(TrackGenerator.TrackType.Sine, 15, 50f)
            {
                timeLimitSeconds = 150f,
                sineAmplitude = 8f,
                sineFrequency = 0.08f,
                sineHorizontalOffset = 5f,
                rotateGates = true,
                oscillateGates = false,
                backgroundColor = new Color(0.1f, 0.2f, 0.4f)
            },

            // Level 3 - Zigzag Challenge
            new TrackConfig(TrackGenerator.TrackType.Zigzag, 20, 45f)
            {
                timeLimitSeconds = 150f,
                zigzagAmplitude = 12f,
                rotateGates = true,
                oscillateGates = true,
                oscillationType = TrackGenerator.OscillationType.VerticalSine,
                oscillationAmplitude = 2f,
                oscillationSpeed = 1f,
                varyOscillationPhase = true,
                phaseOffsetPerGate = 0.5f,
                backgroundColor = new Color(0.15f, 0.15f, 0.3f)
            },

            // Level 4 - Helix Spiral
            new TrackConfig(TrackGenerator.TrackType.Helix, 25, 40f)
            {
                timeLimitSeconds = 150f,
                helixRadius = 12f,
                helixPitch = 8f,
                rotateGates = true,
                oscillateGates = true,
                oscillationType = TrackGenerator.OscillationType.Circular,
                oscillationAmplitude = 1.5f,
                oscillationSpeed = 1.2f,
                varyOscillationPhase = true,
                phaseOffsetPerGate = 0.3f,
                backgroundColor = new Color(0.1f, 0.1f, 0.25f)
            },

            // Level 5 - Lissajous Madness
            new TrackConfig(TrackGenerator.TrackType.Lissajous, 30, 35f)
            {
                timeLimitSeconds = 150f,
                lissajousA = 3f,
                lissajousB = 2f,
                lissajousAmplitudeX = 12f,
                lissajousAmplitudeY = 10f,
                lissajousDelta = Mathf.PI / 4,
                rotateGates = true,
                oscillateGates = true,
                oscillationType = TrackGenerator.OscillationType.Random,
                oscillationAmplitude = 3f,
                oscillationSpeed = 1.5f,
                varyOscillationPhase = true,
                phaseOffsetPerGate = 0.4f,
                varyOscillationSpeed = true,
                speedVariationAmount = 0.3f,
                backgroundColor = new Color(0.05f, 0.05f, 0.2f)
            },

            // Level 6 - Extreme Random
            new TrackConfig(TrackGenerator.TrackType.Random, 40, 30f)
            {
                timeLimitSeconds = 150f,
                rotateGates = true,
                oscillateGates = true,
                oscillationType = TrackGenerator.OscillationType.Circular,
                oscillationAmplitude = 4f,
                oscillationSpeed = 2f,
                varyOscillationPhase = true,
                phaseOffsetPerGate = 0.2f,
                varyOscillationSpeed = true,
                speedVariationAmount = 0.5f,
                backgroundColor = Color.black
            }
        };
    }

    // Volání z UI menu
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelConfigs.Length)
        {
            Debug.LogError($"Invalid level index: {levelIndex}");
            return;
        }

        if (trackGenerator == null)
        {
            Debug.LogError("TrackGenerator not assigned!");
            return;
        }

        Debug.Log($"Loading Level {levelIndex + 1}");
        trackGenerator.GenerateTrack(levelConfigs[levelIndex]);
    }

    // Helper metody pro UI
    public int GetLevelCount()
    {
        return levelConfigs.Length;
    }

    public string GetLevelName(int index)
    {
        string[] names = { "Tutorial", "Gentle Sine", "Zigzag", "Helix Spiral", "Lissajous", "Extreme Chaos" };
        return index < names.Length ? names[index] : $"Level {index + 1}";
    }

    // Příklad použití v kódu - NOVÝ INPUT SYSTEM
    void Update()
    {
        // Pro testování - stiskni číslo 1-6 pro načtení levelu
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            Debug.Log("Key 1 pressed - Loading Level 1");
            LoadLevel(0);
        }
        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            Debug.Log("Key 2 pressed - Loading Level 2");
            LoadLevel(1);
        }
        if (keyboard.digit3Key.wasPressedThisFrame)
        {
            Debug.Log("Key 3 pressed - Loading Level 3");
            LoadLevel(2);
        }
        if (keyboard.digit4Key.wasPressedThisFrame)
        {
            Debug.Log("Key 4 pressed - Loading Level 4");
            LoadLevel(3);
        }
        if (keyboard.digit5Key.wasPressedThisFrame)
        {
            Debug.Log("Key 5 pressed - Loading Level 5");
            LoadLevel(4);
        }
        if (keyboard.digit6Key.wasPressedThisFrame)
        {
            Debug.Log("Key 6 pressed - Loading Level 6");
            LoadLevel(5);
        }

        // Pro numerickou klávesnici
        if (keyboard.numpad1Key.wasPressedThisFrame) LoadLevel(0);
        if (keyboard.numpad2Key.wasPressedThisFrame) LoadLevel(1);
        if (keyboard.numpad3Key.wasPressedThisFrame) LoadLevel(2);
        if (keyboard.numpad4Key.wasPressedThisFrame) LoadLevel(3);
        if (keyboard.numpad5Key.wasPressedThisFrame) LoadLevel(4);
        if (keyboard.numpad6Key.wasPressedThisFrame) LoadLevel(5);
    }
}