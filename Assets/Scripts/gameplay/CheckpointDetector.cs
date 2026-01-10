using UnityEngine;

public class CheckpointDetector : MonoBehaviour
{
    private bool hasPassed = false;
    public int checkpointIndex;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger hit by: {other.name}, tag: {other.tag}");

        if (!hasPassed && other.CompareTag("Player"))
        {
            hasPassed = true;
            ScoreManager.Instance.AddCheckpoint(checkpointIndex);

            var r = GetComponent<Renderer>();
            if (r != null && r.material != null)
                r.material.color = Color.green;
        }
    }
}
