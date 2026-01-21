using System.Collections;
using TMPro;
using UnityEngine;

public class RaceCountdown : MonoBehaviour
{
    [Header("References")]
    public TMP_Text countdownText;
    public TrackGenerator trackGenerator;

    [Header("Timing")]
    [Min(0.05f)] public float stepSeconds = 1f;
    [Min(0.05f)] public float goVisibleSeconds = 0.6f;

    Coroutine routine;

    public void Begin(GameObject playerInstance)
    {
        // když by se Begin zavolal znovu (regen levelu), zruš starý countdown
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        routine = StartCoroutine(Run(playerInstance));
    }

    private IEnumerator Run(GameObject playerInstance)
    {
        // LOCK
        LockPlayer(playerInstance, true);

        // === 3 ===
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "3";
        }

        // Audio: pípnutí
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCountdownBeep();

        yield return new WaitForSeconds(stepSeconds);

        // === 2 ===
        if (countdownText != null)
            countdownText.text = "2";

        // Audio: pípnutí
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCountdownBeep();

        yield return new WaitForSeconds(stepSeconds);

        // === 1 ===
        if (countdownText != null)
            countdownText.text = "1";

        // Audio: pípnutí
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCountdownBeep();

        yield return new WaitForSeconds(stepSeconds);

        // === GO! ===
        if (countdownText != null)
            countdownText.text = "GO!";

        // Audio: "GO!" zvuk
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCountdownGo();

        // START závodu až na GO
        if (ScoreManager.Instance != null && trackGenerator != null)
        {

            var config = LevelsCatalog.GetConfig(GameSession.SelectedLevelIndex);


            ScoreManager.Instance.StartRace(
                config.gateCount,
                config.timeLimitSeconds,
                config.requiredPercentage  // Předej procento k vítězství
            );
        }

        // UNLOCK
        LockPlayer(playerInstance, false);

        yield return new WaitForSeconds(goVisibleSeconds);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        routine = null;
    }

    private void LockPlayer(GameObject player, bool locked)
    {
        if (player == null) return;

        // 1) Zamknout ovládání i pohyb v controlleru
        var ctrl = player.GetComponent<AircraftRocketController>();
        if (ctrl != null)
        {
            ctrl.inputEnabled = !locked;
            ctrl.movementEnabled = !locked;
        }

        // 2) Zastavit fyziku
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = locked;

            if (locked)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}