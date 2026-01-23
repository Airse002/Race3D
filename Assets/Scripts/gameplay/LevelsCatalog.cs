using UnityEngine;

public static class LevelsCatalog
{
    public struct LevelDef
    {
        public string name;
        public TrackConfig config;

        public LevelDef(string name, TrackConfig config)
        {
            this.name = name;
            this.config = config;
        }
    }

    private static LevelDef[] levels;

    public static int Count
    {
        get { Ensure(); return levels.Length; }
    }

    public static string GetName(int index)
    {
        Ensure();
        index = Mathf.Clamp(index, 0, levels.Length - 1);
        return levels[index].name;
    }

    public static TrackConfig GetConfig(int index)
    {
        Ensure();
        index = Mathf.Clamp(index, 0, levels.Length - 1);
        return levels[index].config;
    }

    private static void Ensure()
    {
        if (levels != null) return;

        levels = new LevelDef[]
        {
            // Level 1 - Tutorial (musíš proletět všechny obruče)
            new LevelDef("Tutorial",
                new TrackConfig(TrackGenerator.TrackType.Linear, 5, 60f)
                {
                    requiredPercentage = 0.2f,  // stačí proletět jednu
                    startOffset = -40f,
                    rotateGates = true,
                    oscillateGates = false,
                    backgroundColor = new Color(0.3f, 0.5f, 0.8f),
                    timeLimitSeconds = 60f
                }
            ),

            // Level 2 - Gentle Sine
            new LevelDef("Gentle Sine",
                new TrackConfig(TrackGenerator.TrackType.Sine, 8, 50f)
                {
                    requiredPercentage = 0.5f,  // 50%
                    sineAmplitude = 8f,
                    sineFrequency = 0.08f,
                    sineHorizontalOffset = 5f,
                    rotateGates = true,
                    oscillateGates = false,
                    backgroundColor = new Color(0.8f, 0.4f, 0.2f),
                    timeLimitSeconds = 80f
                }
            ),

            // Level 3 - Zigzag
            new LevelDef("Zigzag",
                new TrackConfig(TrackGenerator.TrackType.Zigzag, 10, 45f)
                {
                    requiredPercentage = 0.7f,  // 70%
                    zigzagAmplitude = 12f,
                    rotateGates = true,
                    oscillateGates = true,
                    oscillationType = TrackGenerator.OscillationType.VerticalSine,
                    oscillationAmplitude = 2f,
                    oscillationSpeed = 1f,
                    varyOscillationPhase = true,
                    phaseOffsetPerGate = 0.5f,
                    backgroundColor = new Color(0.6f, 0.2f, 0.6f),
                    timeLimitSeconds = 90f
                }
            ),

            // Level 4 - Helix Spiral
            new LevelDef("Helix Spiral",
                new TrackConfig(TrackGenerator.TrackType.Helix, 15, 40f)
                {
                    requiredPercentage = 0.8f,  // 80%
                    helixRadius = 12f,
                    helixPitch = 8f,
                    rotateGates = true,
                    oscillateGates = true,
                    oscillationType = TrackGenerator.OscillationType.Circular,
                    oscillationAmplitude = 1.5f,
                    oscillationSpeed = 1.2f,
                    varyOscillationPhase = true,
                    phaseOffsetPerGate = 0.3f,
                    backgroundColor = new Color(0.2f, 0.6f, 0.4f),
                    timeLimitSeconds = 100f
                }
            ),

            // Level 5 - Lissajous
            new LevelDef("Lissajous",
                new TrackConfig(TrackGenerator.TrackType.Lissajous, 18, 35f)
                {
                    requiredPercentage = 0.50f,  // 50%
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
                    backgroundColor = new Color(0.8f, 0.2f, 0.3f),
                    timeLimitSeconds = 120f
                }
            ),

            // Level 6 - Extreme Chaos
            new LevelDef("Extreme Chaos",
                new TrackConfig(TrackGenerator.TrackType.Random, 25, 30f)
                {
                    requiredPercentage = 0.8f,  // 80%
                    rotateGates = true,
                    oscillateGates = true,
                    oscillationType = TrackGenerator.OscillationType.Circular,
                    oscillationAmplitude = 4f,
                    oscillationSpeed = 2f,
                    varyOscillationPhase = true,
                    phaseOffsetPerGate = 0.2f,
                    varyOscillationSpeed = true,
                    speedVariationAmount = 0.5f,
                    backgroundColor = new Color(0.1f, 0.1f, 0.3f),
                    timeLimitSeconds = 150f
                }
            ),
        };
    }
}
