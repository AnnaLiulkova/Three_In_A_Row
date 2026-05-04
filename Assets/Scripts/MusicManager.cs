using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))] 
public class MusicManager : MonoBehaviour
{
    [Header("UI Елементи")]
    public Toggle musicToggle;

    private AudioSource _audioSource;
    private const string MusicPrefsKey = "MusicEnabled";

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = true; 
        _audioSource.playOnAwake = false; 
    }

    private void Start()
    {
        bool isMusicOn = PlayerPrefs.GetInt(MusicPrefsKey, 1) == 1;

        if (musicToggle != null)
        {
            musicToggle.isOn = isMusicOn; 
            musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        }

        ApplyMusicState(isMusicOn);
    }

    private void OnMusicToggleChanged(bool isOn)
    {
        ApplyMusicState(isOn);

        PlayerPrefs.SetInt(MusicPrefsKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyMusicState(bool isOn)
    {
        if (isOn)
        {
            if (!_audioSource.isPlaying) _audioSource.Play();
            _audioSource.mute = false;
        }
        else
        {
            _audioSource.mute = true;
        }
    }
}