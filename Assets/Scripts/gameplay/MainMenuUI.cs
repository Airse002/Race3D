using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI")]
    public Transform levelsParent;      // parent pro tlačítka (Vertical Layout Group)
    public Button levelButtonPrefab;    // prefab buttonu s TMP textem
    public string playSceneName = "PlayScene";

    [Header("Buttons")]
    public Button quitButton;           // NOVÉ: tlačítko pro ukončení hry

    [Header("Level Info Display")]
    public TMP_Text levelInfoText;      // zobrazí info o vybraném levelu

    void Start()
    {
        // Přehraj menu hudbu
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMusic();

        BuildLevelButtons();

        // Připoj Quit button
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    void BuildLevelButtons()
    {
        // vyčisti parent (kdyby tam něco bylo)
        for (int i = levelsParent.childCount - 1; i >= 0; i--)
            Destroy(levelsParent.GetChild(i).gameObject);

        for (int i = 0; i < LevelsCatalog.Count; i++)
        {
            int idx = i;

            var btn = Instantiate(levelButtonPrefab, levelsParent);
            var tmp = btn.GetComponentInChildren<TMP_Text>();

            // Získej info o levelu
            var config = LevelsCatalog.GetConfig(idx);
            int required = config.GetRequiredGateCount();
            int total = config.gateCount;

            if (tmp != null)
            {
                tmp.text = $"{idx + 1}. {LevelsCatalog.GetName(idx)}\n" +
                          $"<size=70%>{required}/{total} gates ({config.requiredPercentage * 100:F0}%)</size>";
            }

            // Přidej listener s audio efektem
            btn.onClick.AddListener(() =>
            {
                // Přehraj zvuk kliknutí
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayButtonClick();

                GameSession.SelectedLevelIndex = idx;
                SceneManager.LoadScene(playSceneName);
            });

            // NOVÉ: Zobraz info při najetí myší (volitelné)
            var eventTrigger = btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) => { ShowLevelInfo(idx); });
            eventTrigger.triggers.Add(pointerEnter);
        }
    }

    /// <summary>
    /// Zobrazí detailní info o levelu (volitelné)
    /// </summary>
    void ShowLevelInfo(int levelIndex)
    {
        if (levelInfoText == null) return;

        var config = LevelsCatalog.GetConfig(levelIndex);
        string levelName = LevelsCatalog.GetName(levelIndex);

        int required = config.GetRequiredGateCount();
        int total = config.gateCount;
        float timeLimit = config.timeLimitSeconds;

        levelInfoText.text = $"<b>{levelName}</b>\n" +
                            $"Track type: {config.trackType}\n" +
                            $"Gate number: {total}\n" +
                            $"Gates for win: {required} ({config.requiredPercentage * 100:F0}%)\n" +
                            $"Time limit: {FormatTime(timeLimit)}";
    }

    string FormatTime(float t)
    {
        int m = (int)(t / 60f);
        float s = t - m * 60f;
        return $"{m}:{s:00}";
    }

    /// <summary>
    /// Ukončí hru (funguje v buildu i editoru)
    /// </summary>
    void QuitGame()
    {
        // Přehraj zvuk kliknutí
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Debug.Log("[MainMenuUI] Ukončuji hru...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}