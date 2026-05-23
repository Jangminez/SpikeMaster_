using System.Collections.Generic;
using UnityEngine;
using JangLib;
using System.Collections;
using DG.Tweening;
using UnityEngine.Audio;

public enum SoundType { BGM, SFX }

public class SoundManager : SingletonWithMono<SoundManager>, IBaseManager
{
    private const string BGM_KEY = "Setting_BGMVolume";
    private const string SFX_KEY = "Setting_SFXVolume";

    public bool IsInitialized { private set; get; }
    private int _initialPoolSize = 10;

    private AudioMixer _mainMixer;

    private AudioSource _bgmSource;

    private Queue<AudioSource> _sfxPool = new Queue<AudioSource>();
    private List<AudioSource> _activeSfx = new List<AudioSource>();

    private float _bgmVolume;
    private float _sfxVolume;
    public (float bgm, float sfx) GetVolumes() => (_bgmVolume, _sfxVolume);

    #region <<  INITILIZE  >>
    public void Init()
    {
        InitBgmSource();
        GeneratePool();

        _mainMixer = Resources.Load<AudioMixer>("Main_AuidoMixer");
        LoadVolume();

        IsInitialized = true;
    }

    private void InitBgmSource()
    {
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
    }
    #endregion

    #region <<  OBJECT POOLING  >>
    private void GeneratePool()
    {
        for (int i = 0; i < _initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }
    }

    private AudioSource CreateNewAudioSource()
    {
        GameObject obj = new GameObject("SFX_Source");
        obj.transform.SetParent(this.transform);
        AudioSource source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        obj.SetActive(false);
        _sfxPool.Enqueue(source);
        return source;
    }
    #endregion

    #region << MIXER VOLUME CONTROL >>
    /// <summary>
    /// 믹서 볼륨 설정 
    /// </summary>
    /// <param name="parameterName">BGMVolume 또는 SFXVolume</param>
    /// <param name="normalizedValue">0.0 (음소거) ~ 1.0 (최대)</param>
    public void SetVolume(SoundType type, float normalizedValue)
    {
        if (_mainMixer == null) return;

        if (type == SoundType.BGM) _bgmVolume = normalizedValue;
        else if (type == SoundType.SFX) _sfxVolume = normalizedValue;

        float dB = Mathf.Log10(Mathf.Max(0.0001f, normalizedValue)) * 20f;
        _mainMixer.SetFloat(GetNameByType(type), dB);
    }
    #endregion

    #region << BGM CONTROL >>
    public void PlayBGM(string path, float fadeTime = 1f)
    {
        var data = ResourceManager.Instance.GetSound(path);

        if (_bgmSource.outputAudioMixerGroup == null)
            _bgmSource.outputAudioMixerGroup = data.output;

        if (_bgmSource.isPlaying)
        {
            _bgmSource.DOFade(0f, fadeTime).OnComplete(() =>
            {
                ApplyBgmSettings(data);
                _bgmSource.DOFade(data.volume, fadeTime);
            });
        }
        else
        {
            ApplyBgmSettings(data);
            _bgmSource.volume = 0f;
            _bgmSource.DOFade(data.volume, fadeTime);
        }
    }

    private void ApplyBgmSettings(SoundDataSO data)
    {
        _bgmSource.clip = data.clip;
        _bgmSource.Play();
    }

    public void StopBGM(float fadeTime = 1f)
    {
        _bgmSource.DOFade(0f, fadeTime).OnComplete(() =>
        {
            _bgmSource.Stop();
        });
    }
    #endregion

    #region << SFX CONTROL >>
    public void PlaySFX(string path)
    {
        var data = ResourceManager.Instance.GetSound(path);

        if (data == null || data.clip == null) return;

        AudioSource source = GetSourceFromPool();

        // 데이터 적용
        source.clip = data.clip;
        source.outputAudioMixerGroup = data.output;
        source.volume = data.volume;
        source.loop = data.loop;
        source.pitch = data.useRandomPitch ? Random.Range(data.minPitch, data.maxPitch) : data.pitch;

        source.gameObject.SetActive(true);
        source.Play();

        if (!data.loop)
        {
            StartCoroutine(ReturnToPoolAfterPlay(source));
        }
    }

    private AudioSource GetSourceFromPool()
    {
        if (_sfxPool.Count == 0) CreateNewAudioSource();
        AudioSource source = _sfxPool.Dequeue();
        _activeSfx.Add(source);
        return source;
    }

    private IEnumerator ReturnToPoolAfterPlay(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);

        source.Stop();
        source.gameObject.SetActive(false);
        _activeSfx.Remove(source);
        _sfxPool.Enqueue(source);
    }

    #endregion

    #region <<  SAVE & LOAD  >>
    private void LoadVolume()
    {
        float bgm = PlayerPrefs.GetFloat(BGM_KEY, 1.0f);
        float sfx = PlayerPrefs.GetFloat(SFX_KEY, 1.0f);

        SetVolume(SoundType.BGM, bgm);
        SetVolume(SoundType.SFX, sfx);
    }

    public void SaveVolume()
    {
        PlayerPrefs.SetFloat(BGM_KEY, _bgmVolume);
        PlayerPrefs.SetFloat(SFX_KEY, _sfxVolume);
        PlayerPrefs.Save();
    }
    #endregion

    private string GetNameByType(SoundType type)
    {
        return type switch
        {
            SoundType.BGM => "BGMVolume",
            SoundType.SFX => "SFXVolume",
            _ => "BGMVolume"
        };
    }
}