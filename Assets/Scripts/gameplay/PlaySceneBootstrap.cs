using UnityEngine;

public class PlaySceneBootstrap : MonoBehaviour
{
    [SerializeField] private TrackGenerator trackGenerator;

    private void Start()
    {
        if (trackGenerator == null)
        {
            Debug.LogError("PlaySceneBootstrap: trackGenerator není přiřazený.");
            return;
        }

        int idx = GameSession.SelectedLevelIndex;
        var config = LevelsCatalog.GetConfig(idx);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLevelMusic(idx);
            Debug.Log($"[PlaySceneBootstrap] Playing music for level {idx}");
        }
        else
        {
            Debug.LogWarning("[PlaySceneBootstrap] AudioManager not found!");
        }

        trackGenerator.GenerateTrack(config);
    }
}