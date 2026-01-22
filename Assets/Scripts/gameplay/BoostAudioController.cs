using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Ovládá audio efekty při boostu - ducking (ztlumení) a distortion (praskání)
/// Přidej tento skript na Player GameObject (s AircraftRocketController)
/// </summary>
public class BoostAudioController : MonoBehaviour
{
    [Header("Audio Mixer")]
    [Tooltip("Reference na hlavní Audio Mixer")]
    public AudioMixerGroup musicGroup;

    [Tooltip("Reference na SFX Audio Mixer Group")]
    public AudioMixerGroup sfxGroup;

    [Tooltip("Reference na celý Audio Mixer pro kontrolu parametrů")]
    public AudioMixer audioMixer;

    [Header("Ducking (Ztlumení)")]
    [Tooltip("O kolik decibelů ztlumit hudbu při boostu")]
    [Range(-80f, 0f)]
    public float musicDuckingAmount = -10f;  // -10 dB = cca 30% hlasitost

    [Tooltip("O kolik decibelů ztlumit SFX při boostu")]
    [Range(-80f, 0f)]
    public float sfxDuckingAmount = -5f;     // -5 dB = cca 56% hlasitost

    [Tooltip("Rychlost přechodu ducking efektu")]
    [Range(1f, 20f)]
    public float duckingSpeed = 8f;

    [Header("Distortion (Praskání)")]
    [Tooltip("Zapnout distortion efekt při boostu?")]
    public bool enableDistortion = true;

    [Tooltip("Síla distortion efektu (0 = čistý zvuk, 1 = maximální zkreslení)")]
    [Range(0f, 1f)]
    public float distortionAmount = 0.3f;

    [Tooltip("Rychlost zapínání/vypínání distortion")]
    [Range(1f, 20f)]
    public float distortionSpeed = 5f;

    [Header("Boost Sound")]
    [Tooltip("Zvuk motoru/boostu který se přehraje")]
    public AudioClip boostSound;

    [Tooltip("Zapnout loop boost zvuku?")]
    public bool loopBoostSound = true;

    [Range(0f, 1f)]
    public float boostSoundVolume = 0.7f;

    // Private
    private AircraftRocketController controller;
    private AudioSource boostAudioSource;
    private float currentMusicVolume = 0f;
    private float currentSFXVolume = 0f;
    private float currentDistortion = 0f;
    private bool wasBoostingLastFrame = false;

    void Start()
    {
        controller = GetComponent<AircraftRocketController>();
        if (controller == null)
        {
            Debug.LogError("[BoostAudioController] AircraftRocketController nebyl nalezen!");
            enabled = false;
            return;
        }

        // Vytvoř AudioSource pro boost zvuk
        if (boostSound != null)
        {
            boostAudioSource = gameObject.AddComponent<AudioSource>();
            boostAudioSource.clip = boostSound;
            boostAudioSource.loop = loopBoostSound;
            boostAudioSource.volume = boostSoundVolume;
            boostAudioSource.playOnAwake = false;

            // Přiřaď do Boost mixer group pokud existuje
            if (audioMixer != null)
            {
                var boostGroup = audioMixer.FindMatchingGroups("Boost");
                if (boostGroup.Length > 0)
                    boostAudioSource.outputAudioMixerGroup = boostGroup[0];
            }
        }

        // Inicializuj mixer parametry
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", 0f);
            audioMixer.SetFloat("SFXVolume", 0f);
            audioMixer.SetFloat("MusicDistortion", 0f);
        }

        Debug.Log("[BoostAudioController] Inicializace dokončena!");
    }

    void Update()
    {
        if (controller == null || audioMixer == null) return;

        bool isBoosting = controller.IsBoosting();

        // === DUCKING (Ztlumení hudby a SFX) ===
        float targetMusicVolume = isBoosting ? musicDuckingAmount : 0f;
        float targetSFXVolume = isBoosting ? sfxDuckingAmount : 0f;

        currentMusicVolume = Mathf.Lerp(currentMusicVolume, targetMusicVolume, duckingSpeed * Time.deltaTime);
        currentSFXVolume = Mathf.Lerp(currentSFXVolume, targetSFXVolume, duckingSpeed * Time.deltaTime);

        audioMixer.SetFloat("MusicVolume", currentMusicVolume);
        audioMixer.SetFloat("SFXVolume", currentSFXVolume);

        // === DISTORTION (Praskání) ===
        if (enableDistortion)
        {
            float targetDistortion = isBoosting ? distortionAmount : 0f;
            currentDistortion = Mathf.Lerp(currentDistortion, targetDistortion, distortionSpeed * Time.deltaTime);

            audioMixer.SetFloat("MusicDistortion", currentDistortion);
        }

        // === BOOST SOUND ===
        if (boostAudioSource != null && boostSound != null)
        {
            if (isBoosting && !wasBoostingLastFrame)
            {
                // Začal boost
                boostAudioSource.Play();
            }
            else if (!isBoosting && wasBoostingLastFrame)
            {
                // Skončil boost
                if (loopBoostSound)
                    boostAudioSource.Stop();
            }
        }

        wasBoostingLastFrame = isBoosting;
    }

    void OnDisable()
    {
        // Reset všech efektů
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", 0f);
            audioMixer.SetFloat("SFXVolume", 0f);
            audioMixer.SetFloat("MusicDistortion", 0f);
        }

        if (boostAudioSource != null && boostAudioSource.isPlaying)
        {
            boostAudioSource.Stop();
        }
    }
}