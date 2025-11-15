using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private List<AudioSource> effectsAudioSources;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip uiNavigationSound;
    [SerializeField] private AudioClip uiClickSound;

    [Header("Gameplay Sounds")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip dieSound;

    [Header("Music Tracks")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameMusic;

    [Header("Volumes")]
    [Range(0f, 1f)] public float effectsVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;

    [Header("Effect Limits")]
    [SerializeField] private List<EffectLimit> effectLimits = new List<EffectLimit>();

    private const float UI_VOLUME_SCALE = 0.02f;
    private const float SHOOT_VOLUME_SCALE = 0.02f;
    private const float DASH_VOLUME_SCALE = 0.03f;
    private const float DAMAGE_VOLUME_SCALE = 0.01f;
    private const float MUSIC_VOLUME_SCALE = 0.03f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void PlayHoverSound()
    {
        PlayEffect(uiNavigationSound, UI_VOLUME_SCALE);
    }

    public void PlayNavigateSound()
    {
        PlayEffect(uiNavigationSound, UI_VOLUME_SCALE);
    }

    public void PlayDieSound() => PlayEffect(dieSound, UI_VOLUME_SCALE);
    public void PlayShootSound() => PlayEffect(shootSound, SHOOT_VOLUME_SCALE, "Shoot", 0.5f, 1.2f);
    public void PlayDashSound() => PlayEffect(dashSound, DASH_VOLUME_SCALE, "Dash");
    public void PlayDamageSound() => PlayEffect(damageSound, DAMAGE_VOLUME_SCALE, "Damage", 0.5f, 0.8f);
    public void PlayClickSound() => PlayEffect(uiClickSound, UI_VOLUME_SCALE, "UI");

    private void PlayEffect(AudioClip clip, float volumeScale, string limitName = null, float minPitch = 0.8f, float maxPitch = 1.2f)
    {
        if (clip == null) return;

        // Check if we have a limit for this effect
        EffectLimit limit = null;
        if (!string.IsNullOrEmpty(limitName))
        {
            limit = effectLimits.Find(e => e.name == limitName);
        }

        if (limit != null && limit.currentPlaying >= limit.maxSimultaneous)
            return;

        // Find a free AudioSource from your pool
        AudioSource freeSource = effectsAudioSources.Find(s => !s.isPlaying);
        if (freeSource == null) return;

        freeSource.pitch = Random.Range(minPitch, maxPitch);
        freeSource.volume = volumeScale * effectsVolume;
        freeSource.PlayOneShot(clip);

        // Track how many are playing
        if (limit != null)
            StartCoroutine(limit.Track(clip.length / freeSource.pitch));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioClip clipToPlay = IsGameplayScene(scene.name) ? gameMusic : mainMenuMusic;
        PlayMusic(clipToPlay);
    }

    private bool IsGameplayScene(string sceneName)
    {
        return sceneName == "Game" || sceneName == "PVP" || sceneName == "Testing" || sceneName == "PVPBattle";
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicAudioSource == null) return;

        if (musicAudioSource.clip == clip && musicAudioSource.isPlaying) return;

        musicAudioSource.clip = clip;
        musicAudioSource.loop = true;
        musicAudioSource.volume = musicVolume * MUSIC_VOLUME_SCALE;
        musicAudioSource.Play();
    }

    public void ChangeMusicVolume(float amount)
    {
        musicAudioSource.volume *= amount;
    }

    [System.Serializable]
    public class EffectLimit
    {
        public string name;
        public int maxSimultaneous;
        public int currentPlaying = 0;

        public IEnumerator Track(float duration)
        {
            currentPlaying++;
            yield return new WaitForSeconds(duration);
            currentPlaying--;
        }
    }
}
