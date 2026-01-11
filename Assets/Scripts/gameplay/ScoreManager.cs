using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public enum RaceState { Idle, Running, Finished, Failed }

    [Header("Singleton")]
    [Tooltip("Když true, ScoreManager zůstane mezi scénami (DontDestroyOnLoad). Doporučuju false, ať nevznikají duplikáty.")]
    public bool persistAcrossScenes = false;

    [Tooltip("Když se objeví druhý ScoreManager, tento ho nahradí (lepší při vývoji).")]
    public bool replaceExistingInstance = true;

    [Header("Race Settings")]
    [Tooltip("Když true = odpočítává do nuly. Když false = můžeš zobrazit elapsed a zároveň mít limit pro fail.")]
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
        if (Instance != null && Instance != this)
        {
            if (replaceExistingInstance)
            {
                Destroy(Instance.gameObject);
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            Instance = this;
        }

        if (persistAcrossScenes)
            DontDestroyOnLoad(gameObject);

        // inicialní event (aby UI vidělo "READY")
        OnStateChanged?.Invoke(state);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (state != RaceState.Running) return;

        elapsedTime += Time.deltaTime;

        timeRemaining = Mathf.Max(0f, timeLimit - elapsedTime);
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

        Debug.Log($"[ScoreManager] Race started. Total={totalCheckpoints}, Limit={timeLimit:0.##}s");
    }

    public void AddCheckpoint(int checkpointIndex)
    {
        if (state != RaceState.Running) return;

        if (checkpointIndex == lastCheckpointIndex + 1)
        {
            checkpointsPassed++;
            lastCheckpointIndex = checkpointIndex;

            OnCheckpointChanged?.Invoke(checkpointsPassed, totalCheckpoints);

            Debug.Log($"[ScoreManager] Checkpoint {checkpointIndex} passed! {checkpointsPassed}/{totalCheckpoints}");

            if (checkpointsPassed >= totalCheckpoints && totalCheckpoints > 0)
                FinishRace();
        }
        else
        {
            Debug.Log($"[ScoreManager] Skipped checkpoint! Expected {lastCheckpointIndex + 1}, got {checkpointIndex}");
        }
    }

    public void FinishRace()
    {
        if (state != RaceState.Running) return;
        SetState(RaceState.Finished);
        Debug.Log($"[ScoreManager] Race finished! Time: {elapsedTime:0.00}s");
    }

    public void FailRace()
    {
        if (state != RaceState.Running) return;
        SetState(RaceState.Failed);
        Debug.Log("[ScoreManager] Race failed! Time ran out.");
    }

    // Gettery
    public int GetPassed() => checkpointsPassed;
    public int GetTotal() => totalCheckpoints;
    public float GetElapsed() => elapsedTime;
    public float GetRemaining() => timeRemaining;
    public RaceState GetState() => state;

    private void SetState(RaceState newState)
    {
        if (state == newState) return;
        state = newState;
        OnStateChanged?.Invoke(state);
    }
}
