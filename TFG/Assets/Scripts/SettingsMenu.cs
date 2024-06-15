using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{

    [SerializeField] AudioMixer Mixer;

    [SerializeField] Slider MasterVolumeSlider;
    [SerializeField] TextMeshProUGUI MasterVolumeText;

    [SerializeField] Slider MusicVolumeSlider;
    [SerializeField] TextMeshProUGUI MusicVolumeText;

    [SerializeField] Slider SFXVolumeSlider;
    [SerializeField] TextMeshProUGUI SFXVolumeText;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        ChangeLanguage(PlayerPrefs.GetInt("Language"));
        MasterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        MusicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        SFXVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        ChangeMasterVolume();
        ChangeMusicVolume();
        ChangeSFXVolume();
    }

    public void ChangeLanguage(int language)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[language];
        PlayerPrefs.SetInt("Language", language);
    }

    public void ChangeMasterVolume()
    {
        float value = MasterVolumeSlider.value;
        MasterVolumeText.text = (100f * value).ToString("0");
        Mixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);

        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void ChangeMusicVolume()
    {
        float value = MusicVolumeSlider.value;
        MusicVolumeText.text = (100f * value).ToString("0");
        Mixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);

        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void ChangeSFXVolume()
    {
        float value = SFXVolumeSlider.value;
        SFXVolumeText.text = (100f * value).ToString("0");
        Mixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);

        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}
