using TMPro;
using UnityEngine;

public class RaceHUD : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text checkpointText;
    public TMP_Text timeText;
    public TMP_Text stateText;

    [Header("Smooth")]
    public float timeSmoothSpeed = 12f;

    float shownTime;

    void OnEnable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnCheckpointChanged += HandleCheckpoint;
            ScoreManager.Instance.OnTimeChanged += HandleTime;
            ScoreManager.Instance.OnStateChanged += HandleState;
        }
    }

    void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnCheckpointChanged -= HandleCheckpoint;
            ScoreManager.Instance.OnTimeChanged -= HandleTime;
            ScoreManager.Instance.OnStateChanged -= HandleState;
        }
    }

    void Start()
    {
        shownTime = 0f;

        // inicializace UI (kdyby eventy nepřišly hned)
        if (ScoreManager.Instance != null)
        {
            HandleCheckpoint(ScoreManager.Instance.GetPassed(), ScoreManager.Instance.GetTotal());
            HandleTime(ScoreManager.Instance.GetRemaining(), 1f);
            HandleState(ScoreManager.Instance.GetState());
        }
    }

    void Update()
    {
        // smooth time bez cukání
        if (ScoreManager.Instance == null) return;

        float target = ScoreManager.Instance.GetRemaining();
        shownTime = Mathf.Lerp(shownTime, target, 1f - Mathf.Exp(-timeSmoothSpeed * Time.deltaTime));

        if (timeText != null)
            timeText.text = $"TIME  {FormatTime(shownTime)}";
    }

    void HandleCheckpoint(int passed, int total)
    {
        if (checkpointText != null)
            checkpointText.text = $"CHECKPOINT  {passed} / {total}";
    }

    void HandleTime(float remaining, float limit)
    {
        // necháme Update dělat smoothing
        if (shownTime <= 0.001f) shownTime = remaining;
    }

    void HandleState(ScoreManager.RaceState state)
    {
        if (stateText == null) return;

        switch (state)
        {
            case ScoreManager.RaceState.Idle:     stateText.text = "READY"; break;
            case ScoreManager.RaceState.Running:  stateText.text = ""; break;
            case ScoreManager.RaceState.Finished: stateText.text = "FINISH!"; break;
            case ScoreManager.RaceState.Failed:   stateText.text = "FAILED"; break;
        }
    }

    string FormatTime(float t)
    {
        t = Mathf.Max(0f, t);
        int m = (int)(t / 60f);
        float s = t - m * 60f;
        return $"{m:00}:{s:00.00}";
    }
}
