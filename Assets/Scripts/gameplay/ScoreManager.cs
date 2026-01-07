using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private int score = 0;
    private int lastCheckpoint = -1;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddCheckpoint(int checkpointIndex)
    {
        // Kontrola, že hráč prochází checkpointy po řadě
        if (checkpointIndex == lastCheckpoint + 1)
        {
            score++;
            lastCheckpoint = checkpointIndex;
            Debug.Log($"Checkpoint {checkpointIndex} passed! Score: {score}");
        }
        else
        {
            Debug.Log($"Skipped checkpoint! Expected {lastCheckpoint + 1}, got {checkpointIndex}");
        }
    }

    public int GetScore() => score;
}