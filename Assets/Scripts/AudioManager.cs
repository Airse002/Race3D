using UnityEngine;

/// <summary>
/// Centrální správce zvuků a hudby pro celou hru
/// Singleton pattern - přežije mezi scénami
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music Sources")]
    [Tooltip("AudioSource pro hudbu (loop)")]
    public AudioSource musicSource;

    [Tooltip("AudioSource pro zvukové efekty (one-shot)")]
    public AudioSource sfxSource;

    [Header("Menu Music")]
    public AudioClip menuMusic;

    [Header("Level Music")]
    [Tooltip("Hudba pro jednotlivé levely - index odpovídá číslu levelu")]
    public AudioClip[] levelMusic;

    [Header("Event Sounds")]
    public AudioClip ringPassSound;      // Průlet obručí
    public AudioClip victoryMusic;       // Dokončení levelu
    public AudioClip defeatMusic;        // Prohra

    [Header("UI Sounds")]
    public AudioClip buttonClickSound;   // Kliknutí na tlačítko
    public AudioClip countdownBeep;      // Pípání při odpočtu 3-2-1
    public AudioClip countdownGo;        // Zvuk na "GO!"

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.6f;

    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;

    [Header("Settings")]
    public bool persistBetweenScenes = true;

    private bool isInitialized = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistBetweenScenes)
            DontDestroyOnLoad(gameObject);

        InitializeAudioSources();
        isInitialized = true;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Inicializuje AudioSource komponenty, pokud nejsou přiřazené
    /// </summary>
    private void InitializeAudioSources()
    {
        // Music source
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.priority = 0; // Nejvyšší priorita
        }
        musicSource.volume = musicVolume;

        // SFX source
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.priority = 128; // Střední priorita
        }
        sfxSource.volume = sfxVolume;
    }

    // === MUSIC PLAYBACK ===

    /// <summary>
    /// Přehraje hudbu v menu
    /// </summary>
    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    /// <summary>
    /// Přehraje hudbu pro konkrétní level
    /// </summary>
    public void PlayLevelMusic(int levelIndex)
    {
        if (levelMusic == null || levelMusic.Length == 0)
        {
            Debug.LogWarning("[AudioManager] Žádná levelová hudba není přiřazená.");
            return;
        }

        // Použij index, nebo poslední dostupnou hudbu
        int index = Mathf.Clamp(levelIndex, 0, levelMusic.Length - 1);
        PlayMusic(levelMusic[index]);
    }

    /// <summary>
    /// Přehraje vítěznou hudbu (bez loopu)
    /// </summary>
    public void PlayVictory()
    {
        PlayMusic(victoryMusic, loop: false);
    }

    /// <summary>
    /// Přehraje hudbu pro prohru (bez loopu)
    /// </summary>
    public void PlayDefeat()
    {
        PlayMusic(defeatMusic, loop: false);
    }

    /// <summary>
    /// Interní metoda pro přehrání hudby
    /// </summary>
    private void PlayMusic(AudioClip clip, bool loop = true, float fadeTime = 0.5f)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] Pokus o přehrání null AudioClip.");
            return;
        }

        if (!isInitialized)
            InitializeAudioSources();

        // Jednoduché přepnutí (můžeš později přidat fade)
        musicSource.loop = loop;
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();

        Debug.Log($"[AudioManager] Playing music: {clip.name} (loop: {loop})");
    }

    /// <summary>
    /// Zastaví hudbu
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    // === SOUND EFFECTS ===

    /// <summary>
    /// Přehraje zvuk průletu obručí
    /// </summary>
    public void PlayRingPass()
    {
        PlaySFX(ringPassSound);
    }

    /// <summary>
    /// Přehraje zvuk kliknutí na tlačítko
    /// </summary>
    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    /// <summary>
    /// Přehraje pípnutí při odpočtu
    /// </summary>
    public void PlayCountdownBeep()
    {
        PlaySFX(countdownBeep);
    }

    /// <summary>
    /// Přehraje zvuk "GO!"
    /// </summary>
    public void PlayCountdownGo()
    {
        PlaySFX(countdownGo);
    }

    /// <summary>
    /// Interní metoda pro přehrání zvukového efektu
    /// </summary>
    private void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;

        if (!isInitialized)
            InitializeAudioSources();

        sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
    }

    // === VOLUME CONTROL ===

    /// <summary>
    /// Nastaví hlasitost hudby
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    /// <summary>
    /// Nastaví hlasitost zvukových efektů
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    // === UTILITY ===

    /// <summary>
    /// Vypne/zapne všechny zvuky
    /// </summary>
    public void SetMuted(bool muted)
    {
        if (musicSource != null)
            musicSource.mute = muted;

        if (sfxSource != null)
            sfxSource.mute = muted;
    }
}
