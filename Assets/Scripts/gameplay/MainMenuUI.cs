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

    void Start()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMusic();
        BuildLevelButtons();
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
            if (tmp != null)
                tmp.text = $"{idx + 1}. {LevelsCatalog.GetName(idx)}";

            btn.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayButtonClick();
                GameSession.SelectedLevelIndex = idx;
                SceneManager.LoadScene(playSceneName);
            });
        }
    }
}
