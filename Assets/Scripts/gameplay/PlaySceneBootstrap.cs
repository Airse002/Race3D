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

        trackGenerator.GenerateTrack(config);
    }
}
