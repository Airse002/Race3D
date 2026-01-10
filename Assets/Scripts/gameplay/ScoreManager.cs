using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public enum RaceState { Idle, Running, Finished, Failed }

    [Header("Race Settings")]
    [Tooltip("Když true = odpočítává do nuly. Když false = stopky (elapsed) + limit pro fail.")]
    public bool countdownMode = true;

    [Header("Read Only (runtime)")]
    [SerializeField] private RaceState state = RaceState.Idle;
    [SerializeField] private int totalCheckpoints = 0;
    [SerializeField] private int checkpointsPassed = 0;
    [SerializeField] private int lastCheckpointIndex = -1;

    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private float elapsedTime = 0f;
    [SerializeField] private float timeRemaining = 60f;

    // Events (pro UI)
    public event Action<int, int> OnCheckpointChanged; // passed, total
    public event Action<float, float> OnTimeChanged;    // remaining, limit
    public event Action<RaceState> OnStateChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // klidně můžeš vypnout, pokud chceš per-scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (state != RaceState.Running) return;

        elapsedTime += Time.deltaTime;

        if (countdownMode)
            timeRemaining = Mathf.Max(0f, timeLimit - elapsedTime);
        else
            timeRemaining = Mathf.Max(0f, timeLimit - elapsedTime); // pořád držíme remaining kvůli failu/HUD

        OnTimeChanged?.Invoke(timeRemaining, timeLimit);

        if (timeRemaining <= 0f)
            FailRace();
    }

    // === PUBLIC API ===

    public void StartRace(int total, float limitSeconds)
    {
        totalCheckpoints = Mathf.Max(0, total);
        timeLimit = Mathf.Max(1f, limitSeconds);

        checkpointsPassed = 0;
        lastCheckpointIndex = -1;
        elapsedTime = 0f;
        timeRemaining = timeLimit;

        SetState(RaceState.Running);

        OnCheckpointChanged?.Invoke(checkpointsPassed, totalCheckpoints);
        OnTimeChanged?.Invoke(timeRemaining, timeLimit);

        Debug.Log($"Race started. Total checkpoints: {totalCheckpoints}, Time limit: {timeLimit}s");
    }

    public void AddCheckpoint(int checkpointIndex)
    {
        if (state != RaceState.Running) return;

        // musí být po řadě
        if (checkpointIndex == lastCheckpointIndex + 1)
        {
            checkpointsPassed++;
            lastCheckpointIndex = checkpointIndex;

            OnCheckpointChanged?.Invoke(checkpointsPassed, totalCheckpoints);

            Debug.Log($"Checkpoint {checkpointIndex} passed! {checkpointsPassed}/{totalCheckpoints}");

            if (checkpointsPassed >= totalCheckpoints)
                FinishRace();
        }
        else
        {
            Debug.Log($"Skipped checkpoint! Expected {lastCheckpointIndex + 1}, got {checkpointIndex}");
        }
    }

    public void FinishRace()
    {
        if (state != RaceState.Running) return;
        SetState(RaceState.Finished);
        Debug.Log($"Race finished! Time: {elapsedTime:0.00}s");
    }

    public void FailRace()
    {
        if (state != RaceState.Running) return;
        SetState(RaceState.Failed);
        Debug.Log("Race failed! Time ran out.");
    }

    // Gettery (pro HUD bez eventů)
    public int GetPassed() => checkpointsPassed;
    public int GetTotal() => totalCheckpoints;
    public float GetElapsed() => elapsedTime;
    public float GetRemaining() => timeRemaining;
    public RaceState GetState() => state;

    private void SetState(RaceState newState)
    {
        state = newState;
        OnStateChanged?.Invoke(state);
    }
}
