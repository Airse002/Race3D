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
                new TrackConfig(TrackGenerator.TrackType.Linear, 10, 60f)
                {
                    requiredPercentage = 1.0f,  // 100% - musíš proletět všech 10
                    startOffset = -40f,
                    rotateGates = true,
                    oscillateGates = false,
                    backgroundColor = new Color(0.2f, 0.3f, 0.5f),
                    timeLimitSeconds = 180f
                }
            ),

            // Level 2 - Gentle Sine (stačí 90%)
            new LevelDef("Gentle Sine",
                new TrackConfig(TrackGenerator.TrackType.Sine, 15, 50f)
                {
                    requiredPercentage = 0.9f,  // 90% - stačí 14 z 15
                    sineAmplitude = 8f,
                    sineFrequency = 0.08f,
                    sineHorizontalOffset = 5f,
                    rotateGates = true,
                    oscillateGates = false,
                    backgroundColor = new Color(0.1f, 0.2f, 0.4f),
                    timeLimitSeconds = 180f
                }
            ),

            // Level 3 - Zigzag (stačí 85%)
            new LevelDef("Zigzag",
                new TrackConfig(TrackGenerator.TrackType.Zigzag, 20, 45f)
                {
                    requiredPercentage = 0.85f,  // 85% - stačí 17 z 20
                    zigzagAmplitude = 12f,
                    rotateGates = true,
                    oscillateGates = true,
                    oscillationType = TrackGenerator.OscillationType.VerticalSine,
                    oscillationAmplitude = 2f,
                    oscillationSpeed = 1f,
                    varyOscillationPhase = true,
                    phaseOffsetPerGate = 0.5f,
                    backgroundColor = new Color(0.15f, 0.15f, 0.3f),
                    timeLimitSeconds = 180f
                }
            ),

            // Level 4 - Helix Spiral (stačí 80%)
            new LevelDef("Helix Spiral",
                new TrackConfig(TrackGenerator.TrackType.Helix, 25, 40f)
                {
                    requiredPercentage = 0.8f,  // 80% - stačí 20 z 25
                    helixRadius = 12f,
                    helixPitch = 8f,
                    rotateGates = true,
                    oscillateGates = true,
                    oscillationType = TrackGenerator.OscillationType.Circular,
                    oscillationAmplitude = 1.5f,
                    oscillationSpeed = 1.2f,
                    varyOscillationPhase = true,
                    phaseOffsetPerGate = 0.3f,
                    backgroundColor = new Color(0.1f, 0.1f, 0.25f),
                    timeLimitSeconds = 180f
                }
            ),

            // Level 5 - Lissajous (stačí 75%)
            new LevelDef("Lissajous",
                new TrackConfig(TrackGenerator.TrackType.Lissajous, 30, 35f)
                {
                    requiredPercentage = 0.75f,  // 75% - stačí 23 z 30
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
                    backgroundColor = new Color(0.05f, 0.05f, 0.2f),
                    timeLimitSeconds = 180f
                }
            ),

            // Level 6 - Extreme Chaos (stačí 70%)
            new LevelDef("Extreme Chaos",
                new TrackConfig(TrackGenerator.TrackType.Random, 40, 30f)
                {
                    requiredPercentage = 0.7f,  // 70% - stačí 28 z 40
                    rotateGates = true,
                    oscillateGates = true,
                    oscillationType = TrackGenerator.OscillationType.Circular,
                    oscillationAmplitude = 4f,
                    oscillationSpeed = 2f,
                    varyOscillationPhase = true,
                    phaseOffsetPerGate = 0.2f,
                    varyOscillationSpeed = true,
                    speedVariationAmount = 0.5f,
                    backgroundColor = Color.black,
                    timeLimitSeconds = 180f
                }
            ),
        };
    }
}
