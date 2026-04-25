using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";

    private const string MasterParams = "MasterVol";
    private const string MusicParams = "MusicVol";

    private void Start()
    {
        float savedMaster = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        float savedMusic = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);

        _masterSlider.value = savedMaster;
        _musicSlider.value = savedMusic;

        SetMasterVolume(savedMaster);
        SetMusicVolume(savedMusic);

        _masterSlider.onValueChanged.AddListener(SetMasterVolume);
        _musicSlider.onValueChanged.AddListener(SetMusicVolume);
    }

    public void SetMasterVolume(float value)
    {
        float dbValue = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        _mixer.SetFloat(MasterParams, dbValue);
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
    }

    public void SetMusicVolume(float value)
    {
        float dbValue = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        _mixer.SetFloat(MusicParams, dbValue);
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
    }

    private void OnDisable()
    {
        PlayerPrefs.Save();
    }
}