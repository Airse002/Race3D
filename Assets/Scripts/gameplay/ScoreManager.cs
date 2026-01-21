using System;
using System.Collections.Generic;
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
    [SerializeField] private int requiredCheckpoints = 0;  // NOVÉ: kolik potřebuješ k vítězství
    [SerializeField] private int checkpointsPassed = 0;
    [SerializeField] private float requiredPercentage = 1.0f; // NOVÉ: procento k vítězství

    // ZMĚNA: místo sledování posledního indexu používáme HashSet
    private HashSet<int> passedCheckpointIndices = new HashSet<int>();

    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private float elapsedTime = 0f;
    [SerializeField] private float timeRemaining = 60f;

    // Events (pro UI)
    public event Action<int, int> OnCheckpointChanged; // passed, required (ZMĚNA: required místo total)
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

    /// <summary>
    /// Zahájí závod s procentuálním požadavkem na průlet obručí
    /// </summary>
    /// <param name="total">Celkový počet obručí v levelu</param>
    /// <param name="limitSeconds">Časový limit</param>
    /// <param name="requiredPercent">Procento obručí potřebné k vítězství (0.0 - 1.0)</param>
    public void StartRace(int total, float limitSeconds, float requiredPercent = 1.0f)
    {
        totalCheckpoints = Mathf.Max(0, total);
        timeLimit = Mathf.Max(1f, limitSeconds);
        requiredPercentage = Mathf.Clamp01(requiredPercent);
        requiredCheckpoints = Mathf.CeilToInt(totalCheckpoints * requiredPercentage);

        checkpointsPassed = 0;
        passedCheckpointIndices.Clear();
        elapsedTime = 0f;
        timeRemaining = timeLimit;

        SetState(RaceState.Running);

        OnCheckpointChanged?.Invoke(checkpointsPassed, requiredCheckpoints);
        OnTimeChanged?.Invoke(timeRemaining, timeLimit);

        Debug.Log($"[ScoreManager] Race started. Total={totalCheckpoints}, Required={requiredCheckpoints} ({requiredPercentage * 100:F0}%), Limit={timeLimit:0.##}s");
    }

    /// <summary>
    /// Přidá průlet obručí - funguje BEZ OHLEDU NA POŘADÍ
    /// </summary>
    public void AddCheckpoint(int checkpointIndex)
    {
        if (state != RaceState.Running) return;

        // Zkontroluj, jestli už jsi touto obručí neproletěl
        if (passedCheckpointIndices.Contains(checkpointIndex))
        {
            Debug.Log($"[ScoreManager] Checkpoint {checkpointIndex} již byl proletěn dříve - ignoruji.");
            return;
        }

        // Přidej do seznamu proletěných
        passedCheckpointIndices.Add(checkpointIndex);
        checkpointsPassed++;

        OnCheckpointChanged?.Invoke(checkpointsPassed, requiredCheckpoints);

        Debug.Log($"[ScoreManager] Checkpoint {checkpointIndex} passed! {checkpointsPassed}/{requiredCheckpoints} (total: {totalCheckpoints})");

        // Zavolej audio event pro průlet obručí
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayRingPass();

        // Zkontroluj vítězství
        if (checkpointsPassed >= totalCheckpoints)
            FinishRace();
    }

    public void FinishRace()
    {
        if (state != RaceState.Running) return;
        SetState(RaceState.Finished);

        // Ulož best score pro aktuální level
        SaveBestScore();

        Debug.Log($"[ScoreManager] Race finished! Time: {elapsedTime:0.00}s, Checkpoints: {checkpointsPassed}/{totalCheckpoints}");

        // Zavolej audio pro vítězství
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayVictory();
    }

    public void FailRace()
    {
        if (state != RaceState.Running) return;
        SetState(RaceState.Failed);
        Debug.Log($"[ScoreManager] Race failed! Time ran out. Checkpoints: {checkpointsPassed}/{requiredCheckpoints}");

        // Zavolej audio pro prohru
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDefeat();
    }

    // === SAVE/LOAD ===

    private void SaveBestScore()
    {
        int levelIndex = GameSession.SelectedLevelIndex;
        string key = $"Level_{levelIndex}_BestScore";

        int currentBest = PlayerPrefs.GetInt(key, 0);
        if (checkpointsPassed > currentBest)
        {
            PlayerPrefs.SetInt(key, checkpointsPassed);
            PlayerPrefs.Save();
            Debug.Log($"[ScoreManager] New best score for level {levelIndex}: {checkpointsPassed}");
        }
    }

    public int GetBestScore(int levelIndex)
    {
        string key = $"Level_{levelIndex}_BestScore";
        return PlayerPrefs.GetInt(key, 0);
    }

    // === GETTERY ===

    public int GetPassed() => checkpointsPassed;
    public int GetTotal() => totalCheckpoints;
    public int GetRequired() => requiredCheckpoints;
    public float GetRequiredPercentage() => requiredPercentage;
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
