using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;

    [Header("Audio Clips SFX")]
    public AudioClip clickSFX;
    public AudioClip dealCardSFX;
    public AudioClip drawCardSFX; 
    public AudioClip wrongMoveSFX; 
    public AudioClip playFoundationPlaceSFX;

    [Header("Audio Clips BGM")]
    public AudioClip mainMenuBGM;
    public AudioClip gamePlayBGM;
    public AudioClip gameOverBGM;
    public AudioClip winGameBGM;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("UI Sliders (Optional)")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    public bool isMuted { get; private set; } = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("BGMVolume"))
        {
            LoadVolume();
        }
        else
        {
            bgmSlider.value = 1f;
            sfxSlider.value = 1f;

            SetBGMVolume();
            SetSFXVolume();
        }
    }

    public void SetBGMVolume()
    {
        float volume = bgmSlider.value;
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("BGMVolume", volume);
    }

    public void SetSFXVolume()
    {
        float volume = sfxSlider.value;
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    private void LoadVolume()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        SetBGMVolume();
        SetSFXVolume();
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    private void PlayBGM(AudioClip clip)
    {
        if (clip != null && bgmSource != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlayClickSFX() => PlaySFX(clickSFX);
    public void PlayDealCardSFX() => PlaySFX(dealCardSFX);
    public void PlayDrawCardSFX() => PlaySFX(drawCardSFX);
    public void PlayWrongMoveSFX() => PlaySFX(wrongMoveSFX);
    public void PlayFoundationPlaceSFX() => PlaySFX(playFoundationPlaceSFX);

    public void PlayMainMenuBGM() => PlayBGM(mainMenuBGM);
    public void PlayGameBGM() => PlayBGM(gamePlayBGM);
    public void PlayGameOverBGM() => PlayBGM(gameOverBGM);
    public void PlayWinGameBGM() => PlayBGM(winGameBGM);
}
