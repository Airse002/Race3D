using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject hudPanel;
    public GameObject resultsPanel;
    public GameObject pausePanel;

    [Header("HUD Elements")]
    public Slider boosterSlider;           // Slider pro vizualizaci boosteru
    public TMP_Text boosterText;           // Text pro % hodnotu (volitelné)
    public Image boosterFillImage;         // Image slider fill pro změnu barvy
    public Color boosterFullColor = Color.green;
    public Color boosterEmptyColor = Color.red;

    [Header("Results UI")]
    public TMP_Text titleText;
    public TMP_Text timeText;
    public TMP_Text checkpointsText;

    [Header("Results Buttons")]
    public Button retryButton;
    public Button nextButton;
    public Button menuButton;

    [Header("Pause Buttons")]
    public Button resumeButton;
    public Button pauseRetryButton;
    public Button pauseMenuButton;

    [Header("Scenes")]
    public string menuSceneName = "Menu";
    public string playSceneName = "PlayScene";

    bool isPaused;
    AircraftRocketController playerController;

    void Awake()
    {
        // bezpečnost: při startu scény vždy odpauzovat
        Time.timeScale = 1f;
        isPaused = false;

        if (hudPanel != null) hudPanel.SetActive(true);
        if (resultsPanel != null) resultsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // Results buttons
        if (retryButton != null) retryButton.onClick.AddListener(Retry);
        if (nextButton != null) nextButton.onClick.AddListener(NextLevel);
        if (menuButton != null) menuButton.onClick.AddListener(GoMenu);

        // Pause buttons
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (pauseRetryButton != null) pauseRetryButton.onClick.AddListener(Retry);
        if (pauseMenuButton != null) pauseMenuButton.onClick.AddListener(GoMenu);

        // Najít player controller
        FindPlayerController();

        // Inicializace booster slideru
        if (boosterSlider != null)
        {
            boosterSlider.minValue = 0f;
            boosterSlider.maxValue = 100f;
            boosterSlider.value = 100f;
        }
    }

    void FindPlayerController()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<AircraftRocketController>();
        }
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

    void Update()
    {
        // ESC (New Input System)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        // Update booster UI
        UpdateBoosterUI();
    }

    void UpdateBoosterUI()
    {
        if (playerController == null)
        {
            FindPlayerController();
            return;
        }

        float boosterPercentage = playerController.GetBoosterPercentage();

        // Update slider
        if (boosterSlider != null)
        {
            boosterSlider.value = boosterPercentage;
        }

        // Update text (pokud existuje)
        if (boosterText != null)
        {
            boosterText.text = $"BOOST: {boosterPercentage:F0}%";
        }

        // Update barva podle hodnoty
        if (boosterFillImage != null)
        {
            boosterFillImage.color = Color.Lerp(boosterEmptyColor, boosterFullColor, boosterPercentage / 100f);
        }
    }

    void TogglePause()
    {
        // během výsledků pauzu nedává smysl otevírat
        var sm = ScoreManager.Instance;
        if (sm != null)
        {
            var st = sm.GetState();
            if (st == ScoreManager.RaceState.Finished || st == ScoreManager.RaceState.Failed)
                return;
        }

        if (!isPaused) Pause();
        else Resume();
    }

    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        // UI
        if (pausePanel != null) pausePanel.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(false);
        // resultsPanel nechme schované
        if (resultsPanel != null) resultsPanel.SetActive(false);

        // vypnout input hráče (ne fyziku)
        SetPlayerInput(false);
    }

    void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);

        SetPlayerInput(true);
    }

    void HandleState(ScoreManager.RaceState state)
    {
        // když hra skončí, vždy odpauzovat a ukázat výsledky
        if (state == ScoreManager.RaceState.Finished || state == ScoreManager.RaceState.Failed)
        {
            isPaused = false;
            Time.timeScale = 1f;

            if (pausePanel != null) pausePanel.SetActive(false);
            ShowResults(state);
        }
        else
        {
            // Idle/Running = výsledky pryč
            if (resultsPanel != null) resultsPanel.SetActive(false);
        }
    }

    void ShowResults(ScoreManager.RaceState state)
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(true);

        int passed = ScoreManager.Instance != null ? ScoreManager.Instance.GetPassed() : 0;
        int total = ScoreManager.Instance != null ? ScoreManager.Instance.GetTotal() : 0;
        float elapsed = ScoreManager.Instance != null ? ScoreManager.Instance.GetElapsed() : 0f;

        if (titleText != null)
            titleText.text = (state == ScoreManager.RaceState.Finished) ? "FINISH!" : "FAILED";

        if (timeText != null)
            timeText.text = $"TIME  {FormatTime(elapsed)}";

        if (checkpointsText != null)
            checkpointsText.text = $"CHECKPOINTS  {passed} / {total}";

        if (nextButton != null)
            nextButton.gameObject.SetActive(state == ScoreManager.RaceState.Finished);

        // při výsledcích vypnout input
        SetPlayerInput(false);
    }

    void SetPlayerInput(bool enabled)
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        var ctrl = player.GetComponent<AircraftRocketController>();
        if (ctrl != null) ctrl.inputEnabled = enabled;
    }

    void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(playSceneName);
    }

    void NextLevel()
    {
        Time.timeScale = 1f;
        GameSession.SelectedLevelIndex = Mathf.Min(GameSession.SelectedLevelIndex + 1, LevelsCatalog.Count - 1);
        SceneManager.LoadScene(playSceneName);
    }

    void GoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    string FormatTime(float t)
    {
        t = Mathf.Max(0f, t);
        int m = (int)(t / 60f);
        float s = t - m * 60f;
        return $"{m:00}:{s:00.00}";
    }
}