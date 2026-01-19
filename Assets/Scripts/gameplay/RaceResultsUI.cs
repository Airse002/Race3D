using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RaceResultsUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject hudPanel;
    public GameObject resultsPanel;

    [Header("Results UI")]
    public TMP_Text titleText;
    public TMP_Text timeText;
    public TMP_Text checkpointsText;
    public TMP_Text requiredText;        // NOVÉ: kolik bylo potřeba
    public TMP_Text percentageText;      // NOVÉ: dosažené procento
    public TMP_Text bestScoreText;       // NOVÉ: nejlepší skóre
    public TMP_Text newRecordText;       // NOVÉ: oznámení o novém rekordu

    [Header("Buttons")]
    public Button retryButton;
    public Button nextButton;
    public Button menuButton;

    [Header("Scenes")]
    public string menuSceneName = "Menu";
    public string playSceneName = "PlayScene";

    [Header("Behavior")]
    public bool pauseGameOnResults = true;
    public bool disablePlayerInputOnResults = true;

    private bool resultsShown;

    void Awake()
    {
        // Results nesmí být vidět při startu
        if (resultsPanel != null) resultsPanel.SetActive(false);

        // tlačítka – napojíme programově
        if (retryButton != null) retryButton.onClick.AddListener(Retry);
        if (nextButton != null) nextButton.onClick.AddListener(NextLevel);
        if (menuButton != null) menuButton.onClick.AddListener(BackToMenu);
    }

    void OnEnable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnStateChanged += HandleState;
    }

    void OnDisable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnStateChanged -= HandleState;
    }

    private void HandleState(ScoreManager.RaceState state)
    {
        if (state == ScoreManager.RaceState.Finished || state == ScoreManager.RaceState.Failed)
            ShowResults(state);
    }

    private void ShowResults(ScoreManager.RaceState state)
    {
        if (resultsShown) return;
        resultsShown = true;

        if (pauseGameOnResults) Time.timeScale = 0f;

        if (disablePlayerInputOnResults)
            DisablePlayer();

        if (hudPanel != null) hudPanel.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(true);

        // Získej data
        float elapsed = ScoreManager.Instance != null ? ScoreManager.Instance.GetElapsed() : 0f;
        int passed = ScoreManager.Instance != null ? ScoreManager.Instance.GetPassed() : 0;
        int required = ScoreManager.Instance != null ? ScoreManager.Instance.GetRequired() : 0;
        int total = ScoreManager.Instance != null ? ScoreManager.Instance.GetTotal() : 0;
        float percentage = total > 0 ? (passed / (float)total) * 100f : 0f;

        // Získej best score
        int levelIndex = GameSession.SelectedLevelIndex;
        int oldBestScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetBestScore(levelIndex) : 0;
        bool isNewRecord = passed > oldBestScore;

        // === VYPLŇ TEXTY ===

        // Titulek
        if (titleText != null)
        {
            if (state == ScoreManager.RaceState.Finished)
            {
                titleText.text = "<color=#00FF00><size=200%>VÍTĚZSTVÍ!</size></color>";
            }
            else
            {
                titleText.text = "<color=#FF0000><size=200%>PROHRA</size></color>";
            }
        }

        // Čas
        if (timeText != null)
            timeText.text = $"ČAS: <b>{FormatTime(elapsed)}</b>";

        // Checkpointy - hlavní
        if (checkpointsText != null)
        {
            string color = state == ScoreManager.RaceState.Finished ? "#00FF00" : "#FFAA00";
            checkpointsText.text = $"<color={color}><size=150%><b>{passed} / {total}</b></size></color> obručí proletěno";
        }

        // Požadované checkpointy
        if (requiredText != null)
        {
            if (state == ScoreManager.RaceState.Finished)
            {
                requiredText.text = $"<color=#00FF00>✓</color> Požadováno: <b>{required}</b> ({(required / (float)total * 100f):F0}%)";
            }
            else
            {
                requiredText.text = $"<color=#FF0000>✗</color> Požadováno: <b>{required}</b> ({(required / (float)total * 100f):F0}%)";
            }
        }

        // Dosažené procento
        if (percentageText != null)
        {
            percentageText.text = $"Úspěšnost: <b>{percentage:F1}%</b>";
        }

        // Best score
        if (bestScoreText != null)
        {
            int displayBestScore = Mathf.Max(oldBestScore, passed);
            bestScoreText.text = $"Nejlepší: <b>{displayBestScore}/{total}</b>";
        }

        // Nový rekord
        if (newRecordText != null)
        {
            if (isNewRecord && state == ScoreManager.RaceState.Finished)
            {
                newRecordText.text = "<color=#FFD700>★ NOVÝ REKORD! ★</color>";
                newRecordText.gameObject.SetActive(true);
            }
            else
            {
                newRecordText.gameObject.SetActive(false);
            }
        }

        // Next button – jen pokud existuje další level
        if (nextButton != null)
        {
            int next = GameSession.SelectedLevelIndex + 1;
            bool hasNext = next < LevelsCatalog.Count;
            nextButton.gameObject.SetActive(hasNext && state == ScoreManager.RaceState.Finished);
        }

        // Kurzor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void DisablePlayer()
    {
        var player = GameObject.FindWithTag("Player");
        if (!player) return;

        var ctrl = player.GetComponent<AircraftRocketController>();
        if (ctrl != null)
            ctrl.inputEnabled = false;
    }

    public void Retry()
    {
        Time.timeScale = 1f;

        // Přehraj kliknutí
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        int next = GameSession.SelectedLevelIndex + 1;
        if (next >= LevelsCatalog.Count)
        {
            BackToMenu();
            return;
        }

        Time.timeScale = 1f;

        // Přehraj kliknutí
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        GameSession.SelectedLevelIndex = next;
        SceneManager.LoadScene(playSceneName);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;

        // Přehraj kliknutí
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        SceneManager.LoadScene(menuSceneName);
    }

    private string FormatTime(float t)
    {
        if (float.IsInfinity(t)) return "--:--.--";
        t = Mathf.Max(0f, t);
        int m = (int)(t / 60f);
        float s = t - m * 60f;
        return $"{m:00}:{s:00.00}";
    }
}
