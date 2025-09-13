using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource effectsAudioSource;
    [SerializeField] private AudioSource musicAudioSource;
    private float currentMusicVolume;

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

    private void Start()
    {
        currentMusicVolume = musicAudioSource.volume;
    }

    public void PlayHoverSound()
    {
        PlayEffect(uiNavigationSound, UI_VOLUME_SCALE);
    }

    public void PlayNavigateSound()
    {
        PlayEffect(uiNavigationSound, UI_VOLUME_SCALE);
    }

    public void PlayClickSound() => PlayEffect(uiClickSound, UI_VOLUME_SCALE);
    public void PlayDieSound() => PlayEffect(dieSound, UI_VOLUME_SCALE);
    public void PlayShootSound() => PlayEffect(shootSound, SHOOT_VOLUME_SCALE, 0.5f, 1.2f);

    public void PlayDashSound() => PlayEffect(dashSound, DASH_VOLUME_SCALE);
    public void PlayDamageSound() => PlayEffect(damageSound, DAMAGE_VOLUME_SCALE, 0.5f, 0.8f);

    private void PlayEffect(AudioClip clip, float volumeScale, float minPitch = 0.8f, float maxPitch = 1.2f)
    {
        if (clip != null && effectsAudioSource != null)
        {
            effectsAudioSource.pitch = Random.Range(minPitch, maxPitch);
            effectsAudioSource.PlayOneShot(clip, volumeScale * effectsVolume);
        }
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
}
