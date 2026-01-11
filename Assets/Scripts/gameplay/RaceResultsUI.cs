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
    public TMP_Text bestTimeText; // volitelné (když nechceš, nech null)

    [Header("Buttons")]
    public Button retryButton;
    public Button nextButton;
    public Button menuButton;

    [Header("Scenes")]
    public string menuSceneName = "Menu";
    public string playSceneName = "PlayScene";

    [Header("Behavior")]
    public bool pauseGameOnResults = true;   // dá Time.timeScale = 0 po dojezdu
    public bool disablePlayerInputOnResults = true;

    private bool resultsShown;

    void Awake()
    {
        // Results nesmí být vidět při startu
        if (resultsPanel != null) resultsPanel.SetActive(false);

        // tlačítka – napojíme programově, ať to nemusíš klikat v Inspectoru
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

        // vyplň texty
        float elapsed = ScoreManager.Instance != null ? ScoreManager.Instance.GetElapsed() : 0f;
        int passed = ScoreManager.Instance != null ? ScoreManager.Instance.GetPassed() : 0;
        int total = ScoreManager.Instance != null ? ScoreManager.Instance.GetTotal() : 0;

        if (titleText != null)
            titleText.text = (state == ScoreManager.RaceState.Finished) ? "FINISH!" : "FAILED";

        if (timeText != null)
            timeText.text = $"TIME  {FormatTime(elapsed)}";

        if (checkpointsText != null)
            checkpointsText.text = $"CHECKPOINTS  {passed} / {total}";

        // best time (jen když finish)
        if (bestTimeText != null)
        {
            if (state == ScoreManager.RaceState.Finished)
            {
                float best = LoadBestTime();
                if (elapsed < best)
                {
                    best = elapsed;
                    SaveBestTime(best);
                }
                bestTimeText.text = $"BEST  {FormatTime(best)}";
            }
            else
            {
                bestTimeText.text = "";
            }
        }

        // Next button – jen pokud existuje další level
        if (nextButton != null)
        {
            int next = GameSession.SelectedLevelIndex + 1;
            nextButton.interactable = (next < LevelsCatalog.Count);
        }

        // kurzor (pohodlí)
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

        GameSession.SelectedLevelIndex = next;
        Time.timeScale = 1f;
        SceneManager.LoadScene(playSceneName);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    private float LoadBestTime()
    {
        int idx = GameSession.SelectedLevelIndex;
        string key = $"best_time_{idx}";
        return PlayerPrefs.GetFloat(key, float.PositiveInfinity);
    }

    private void SaveBestTime(float t)
    {
        int idx = GameSession.SelectedLevelIndex;
        string key = $"best_time_{idx}";
        PlayerPrefs.SetFloat(key, t);
        PlayerPrefs.Save();
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
