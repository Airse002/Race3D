using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    [Header("Track Settings")]
    public GameObject gatePrefab;
    public int gateCount = 20;
    public float gateSpacing = 50f;

    [Header("Sine Wave Parameters")]
    public float amplitude = 10f;      // Výška vlny
    public float frequency = 0.1f;     // Frekvence vlny
    public float horizontalOffset = 5f; // Boční odchylka
    public float verticalOffset = -40f; // mezera od startu

    void Start()
    {
        GenerateTrack();
    }

    void GenerateTrack()
    {
        for (int i = 0; i < gateCount; i++)
        {
            float z = verticalOffset + i * gateSpacing;

            // Sinusová dráha pro Y osu
            float y = amplitude * Mathf.Sin(frequency * z);

            // Volitelně: sinusová dráha pro X osu (fázově posunutá)
            float x = horizontalOffset * Mathf.Sin(frequency * z + Mathf.PI / 2);

            Vector3 position = new Vector3(x, y, z);

            // Vytvoř obruč s rotací kolmou ke směru letu
            Quaternion rotation = Quaternion.Euler(90, 0, 0); // Postav obruč kolmo na osu Z
            GameObject gate = Instantiate(gatePrefab, position, rotation, transform);
            gate.name = $"Gate_{i}";

            // Volitelně: orientuj směrem k další obruči
            if (i < gateCount - 1)
            {
                Vector3 nextPos = GetGatePosition(i + 1);
                Vector3 direction = (nextPos - position).normalized;
                gate.transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    Vector3 GetGatePosition(int index)
    {
        float z = index * gateSpacing;
        float y = amplitude * Mathf.Sin(frequency * z);
        float x = horizontalOffset * Mathf.Sin(frequency * z + Mathf.PI / 2);
        return new Vector3(x, y, z);
    }
}