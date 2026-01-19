using TMPro;
using UnityEngine;

public class RaceHUD : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text checkpointText;
    public TMP_Text timeText;
    public TMP_Text stateText;
    public TMP_Text levelInfoText;  // NOVÉ: zobrazí info o levelu (název, cíl)
    public TMP_Text bestScoreText;  // NOVÉ: nejlepší skóre pro aktuální level

    [Header("Smooth")]
    public float timeSmoothSpeed = 12f;

    [Header("Level Info Display")]
    public float levelInfoDisplayTime = 4f;  // Jak dlouho se zobrazí info o levelu

    float shownTime;
    private bool levelInfoShown = false;

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
            HandleCheckpoint(ScoreManager.Instance.GetPassed(), ScoreManager.Instance.GetRequired());
            HandleTime(ScoreManager.Instance.GetRemaining(), 1f);
            HandleState(ScoreManager.Instance.GetState());
        }

        // Zobraz info o levelu při startu
        ShowLevelInfo();

        // Zobraz best score
        ShowBestScore();
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

    void HandleCheckpoint(int passed, int required)
    {
        if (checkpointText != null)
        {
            int total = ScoreManager.Instance.GetTotal();
            checkpointText.text = $"CHECKPOINT  {passed} / {required} ({total} total)";
        }
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
            case ScoreManager.RaceState.Idle:
                stateText.text = "READY";
                break;

            case ScoreManager.RaceState.Running:
                stateText.text = "";
                // Schovej level info po startu
                if (levelInfoShown && levelInfoText != null)
                {
                    levelInfoText.gameObject.SetActive(false);
                }
                break;

            case ScoreManager.RaceState.Finished:
                stateText.text = "FINISH!";
                break;

            case ScoreManager.RaceState.Failed:
                stateText.text = "FAILED";
                break;
        }
    }

    /// <summary>
    /// Zobrazí informace o levelu (název, cíl) na pár sekund
    /// </summary>
    void ShowLevelInfo()
    {
        if (levelInfoText == null) return;

        int levelIndex = GameSession.SelectedLevelIndex;
        string levelName = LevelsCatalog.GetName(levelIndex);
        var config = LevelsCatalog.GetConfig(levelIndex);

        int required = config.GetRequiredGateCount();
        int total = config.gateCount;
        float percentage = config.requiredPercentage * 100f;

        levelInfoText.text = $"<size=150%><b>{levelName}</b></size>\n" +
                            $"Proletět alespoň <b>{required}/{total}</b> obručí ({percentage:F0}%)";

        levelInfoText.gameObject.SetActive(true);
        levelInfoShown = true;

        // Schovej po určité době (lze udělat i přes coroutine)
        Invoke(nameof(HideLevelInfo), levelInfoDisplayTime);
    }

    void HideLevelInfo()
    {
        if (levelInfoText != null)
            levelInfoText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Zobrazí nejlepší skóre pro aktuální level
    /// </summary>
    void ShowBestScore()
    {
        if (bestScoreText == null) return;
        if (ScoreManager.Instance == null) return;

        int levelIndex = GameSession.SelectedLevelIndex;
        int bestScore = ScoreManager.Instance.GetBestScore(levelIndex);

        if (bestScore > 0)
        {
            int total = LevelsCatalog.GetConfig(levelIndex).gateCount;
            bestScoreText.text = $"Best: {bestScore}/{total}";
        }
        else
        {
            bestScoreText.text = "Best: ---";
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
