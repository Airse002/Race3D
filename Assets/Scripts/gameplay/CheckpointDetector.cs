using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointDetector : MonoBehaviour
{
    private bool hasPassed = false;
    public int checkpointIndex;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasPassed)
        {
            hasPassed = true;
            ScoreManager.Instance.AddCheckpoint(checkpointIndex);

            // Vizuální feedback (změň barvu, efekt, zvuk...)
            GetComponent<Renderer>()?.material.SetColor("_Color", Color.green);
        }
    }
}
