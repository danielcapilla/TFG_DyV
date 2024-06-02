using UnityEngine;
using UnityEngine.Localization.Settings;

public class SettingsMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ChangeLanguage(PlayerPrefs.GetInt("Language"));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeLanguage(int language)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[language];
        PlayerPrefs.SetInt("Language", language);
    }
}
