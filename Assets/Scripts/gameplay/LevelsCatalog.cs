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
            new LevelDef("Tutorial",
                new TrackConfig(TrackGenerator.TrackType.Linear, 10, 60f)
                {
                    startOffset = -40f,
                    rotateGates = true,
                    oscillateGates = false,
                    backgroundColor = new Color(0.2f, 0.3f, 0.5f),
                    timeLimitSeconds = 180f
                }
            ),

            new LevelDef("Gentle Sine",
                new TrackConfig(TrackGenerator.TrackType.Sine, 15, 50f)
                {
                    sineAmplitude = 8f,
                    sineFrequency = 0.08f,
                    sineHorizontalOffset = 5f,
                    rotateGates = true,
                    oscillateGates = false,
                    backgroundColor = new Color(0.1f, 0.2f, 0.4f),
                    timeLimitSeconds = 180f
                }
            ),

            new LevelDef("Zigzag",
                new TrackConfig(TrackGenerator.TrackType.Zigzag, 20, 45f)
                {
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

            new LevelDef("Helix Spiral",
                new TrackConfig(TrackGenerator.TrackType.Helix, 25, 40f)
                {
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

            new LevelDef("Lissajous",
                new TrackConfig(TrackGenerator.TrackType.Lissajous, 30, 35f)
                {
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

            new LevelDef("Extreme Chaos",
                new TrackConfig(TrackGenerator.TrackType.Random, 40, 30f)
                {
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
