using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Ovládá Motion Blur efekt během boost režimu.
/// Automaticky najde Volume v scéně.
/// Přidej tento skript na Player GameObject (ten s AircraftRocketController).
/// </summary>
public class BoostMotionBlurSimple : MonoBehaviour
{
    [Header("Motion Blur Settings")]
    [Range(0f, 1f)]
    [Tooltip("Maximální intenzita motion blur při plném boostu")]
    public float maxBlurIntensity = 0.6f;

    [Range(1f, 20f)]
    [Tooltip("Rychlost přechodu blur efektu")]
    public float blurTransitionSpeed = 5f;

    [Header("Vignette Effect")]
    [Tooltip("Zapnout ztmavení okrajů při boostu?")]
    public bool enableVignette = true;

    [Range(0f, 1f)]
    [Tooltip("Intenzita ztmavení okrajů")]
    public float vignetteIntensity = 0.4f;

    [Header("Chromatic Aberration")]
    [Tooltip("Zapnout barevné rozostření při boostu?")]
    public bool enableChromaticAberration = true;

    [Range(0f, 1f)]
    [Tooltip("Intenzita barevného rozostření")]
    public float chromaticIntensity = 0.3f;

    // Private
    private AircraftRocketController controller;
    private Volume volume;
    private MotionBlur motionBlur;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;

    private float currentBlurIntensity = 0f;
    private float currentVignetteIntensity = 0f;
    private float currentChromaticIntensity = 0f;

    void Start()
    {
        // Najdi controller na tomto GameObject
        controller = GetComponent<AircraftRocketController>();
        if (controller == null)
        {
            Debug.LogError("[BoostMotionBlur] AircraftRocketController nebyl nalezen! Přidej tento skript na Player GameObject.");
            enabled = false;
            return;
        }

        // Automaticky najdi Volume v scéně
        volume = FindFirstObjectByType<Volume>();
        if (volume == null)
        {
            Debug.LogWarning("[BoostMotionBlur] Žádný Volume GameObject nebyl nalezen ve scéně!");
            enabled = false;
            return;
        }

        // Získej komponenty z Volume Profile
        if (volume.profile == null)
        {
            Debug.LogWarning("[BoostMotionBlur] Volume nemá přiřazený Profile!");
            enabled = false;
            return;
        }

        // Motion Blur
        if (!volume.profile.TryGet(out motionBlur))
        {
            Debug.LogWarning("[BoostMotionBlur] Motion Blur komponenta nebyla nalezena v Volume Profile!");
        }
        else
        {
            motionBlur.intensity.value = 0f;
            Debug.Log("[BoostMotionBlur] Motion Blur připojen!");
        }

        // Vignette
        if (enableVignette && volume.profile.TryGet(out vignette))
        {
            vignette.intensity.value = 0f;
            Debug.Log("[BoostMotionBlur] Vignette připojen!");
        }

        // Chromatic Aberration
        if (enableChromaticAberration && volume.profile.TryGet(out chromaticAberration))
        {
            chromaticAberration.intensity.value = 0f;
            Debug.Log("[BoostMotionBlur] Chromatic Aberration připojen!");
        }

        Debug.Log("[BoostMotionBlur] Inicializace dokončena!");
    }

    void Update()
    {
        if (controller == null) return;

        // Zjisti jestli se boostuje
        bool isBoosting = controller.IsBoosting();

        // Target hodnoty
        float targetBlur = isBoosting ? maxBlurIntensity : 0f;
        float targetVignette = isBoosting ? vignetteIntensity : 0f;
        float targetChromatic = isBoosting ? chromaticIntensity : 0f;

        // Smooth přechod
        float deltaSpeed = blurTransitionSpeed * Time.deltaTime;
        currentBlurIntensity = Mathf.Lerp(currentBlurIntensity, targetBlur, deltaSpeed);
        currentVignetteIntensity = Mathf.Lerp(currentVignetteIntensity, targetVignette, deltaSpeed);
        currentChromaticIntensity = Mathf.Lerp(currentChromaticIntensity, targetChromatic, deltaSpeed);

        // Aplikuj efekty
        if (motionBlur != null)
        {
            motionBlur.intensity.value = currentBlurIntensity;
        }

        if (enableVignette && vignette != null)
        {
            vignette.intensity.value = currentVignetteIntensity;
        }

        if (enableChromaticAberration && chromaticAberration != null)
        {
            chromaticAberration.intensity.value = currentChromaticIntensity;
        }
    }

    void OnDisable()
    {
        // Reset všech efektů
        if (motionBlur != null)
            motionBlur.intensity.value = 0f;

        if (vignette != null)
            vignette.intensity.value = 0f;

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = 0f;
    }
}