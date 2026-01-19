using System.Collections;
using UnityEngine;

public class CheckpointDetector : MonoBehaviour
{
    [HideInInspector] public int checkpointIndex;

    [Header("Visual")]
    public Color neonGreen = new Color(0.2f, 1.0f, 0.2f, 1f);
    [Tooltip("Jak silně bude svítit emission (vyšší = víc neon).")]
    public float emissionIntensity = 6f;

    [Header("Disappear")]
    [Tooltip("Zpoždění před zmizením obruče po průletu (v sekundách).")]
    public float disappearDelay = 1.5f;

    [Tooltip("Rychlost fade-out efektu.")]
    public float fadeOutDuration = 0.5f;

    private bool hasPassed;

    void OnTriggerEnter(Collider other)
    {
        if (hasPassed) return;
        if (!other.CompareTag("Player")) return;

        hasPassed = true;

        // Změň barvu Gate (parent) na neon zelenou
        SetGateNeonGreen();

        Debug.Log($"PASSED {transform.parent.name} idx={checkpointIndex}");

        // Přidej checkpoint do ScoreManageru
        ScoreManager.Instance.AddCheckpoint(checkpointIndex);

        // Spusť mizení obruče
        StartCoroutine(DisappearAfterDelay());
    }

    private IEnumerator DisappearAfterDelay()
    {
        // Počkej zadanou dobu
        yield return new WaitForSeconds(disappearDelay);

        // Získej root objektu (Gate)
        Transform gateRoot = transform.parent != null ? transform.parent : transform.root;

        // Fade-out efekt (pokud chceš)
        if (fadeOutDuration > 0f)
        {
            var renderers = gateRoot.GetComponentsInChildren<Renderer>(true);
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / fadeOutDuration);

                foreach (var r in renderers)
                {
                    if (r.material == null) continue;

                    // Fade barvy
                    Color c = r.material.color;
                    c.a = alpha;
                    r.material.color = c;

                    // Fade emission
                    if (r.material.HasProperty("_EmissionColor"))
                    {
                        Color emColor = r.material.GetColor("_EmissionColor");
                        emColor.a = alpha;
                        r.material.SetColor("_EmissionColor", emColor);
                    }
                }

                yield return null;
            }
        }

        // Deaktivuj celou obruč
        gateRoot.gameObject.SetActive(false);
    }

    private void SetGateNeonGreen()
    {
        // Gate je parent TriggerZone; když je struktura jiná, zkus root
        Transform gateRoot = transform.parent != null ? transform.parent : transform.root;

        // vezmi všechny renderery v gate (i děti)
        var renderers = gateRoot.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0) return;

        // emission HDR barva
        Color emissive = neonGreen * emissionIntensity;

        foreach (var r in renderers)
        {
            // Pozor: r.material vytvoří instanci materiálu (správně pro runtime změnu)
            var mat = r.material;
            if (mat == null) continue;

            // Base color (funguje ve většině shaderů)
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", neonGreen);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", neonGreen);

            // Emission (neon)
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissive);
            }
        }
    }
}
