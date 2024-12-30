using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//　ゲーム内のBGMおよびSEの再生・管理を行うクラス。
//　シーン遷移に応じてBGMを自動的に変更する機能を持つ。
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource bgmAudioSource;  // BGM用のAudioSource
    [SerializeField] private AudioSource seAudioSource;   // SE用のAudioSource

    [SerializeField] private List<BGMSoundData> bgmSoundDatas;  // BGMリスト
    [SerializeField] private List<SESoundData> seSoundDatas;    // SEリスト
    
    [Header("Sliders")]
    [SerializeField] private Slider bgmSlider;  // BGM音量調整スライダー
    [SerializeField] private Slider seSlider;   // SE音量調整スライダー


    public AudioSource BgmAudioSource => bgmAudioSource;
    public AudioSource SeAudioSource => seAudioSource;

    public float masterVolume = 1;      
    public float bgmMasterVolume = 0.5f;   
    public float seMasterVolume = 0.5f;    

    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  
        }
        else
        {
            Destroy(gameObject);  
        }
        
        LoadVolumeSettings(); 
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    //　シーンがロードされた際に呼び出される処理。
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Title":
                PlayBGM(BGMSoundData.BGM.Title, 1.0f).Forget();
                break;
            case "Main":
                PlayBGM(BGMSoundData.BGM.Main, 0.5f).Forget();
                break;
            case "Result":
                PlayBGM(BGMSoundData.BGM.Result, 1.0f).Forget();
                break;
            default:
                Debug.LogWarning($"BGMが設定されていないシーン: {scene.name}");
                break;
        }
    }
    
    private void Start()
    {
        if (bgmSlider != null)
        {
            bgmSlider.value = bgmMasterVolume;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (seSlider != null)
        {
            seSlider.value = seMasterVolume;
            seSlider.onValueChanged.AddListener(SetSEVolume);
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmMasterVolume = volume;
        bgmAudioSource.volume = bgmMasterVolume * masterVolume;
        SaveVolumeSettings();
    }

    public void SetSEVolume(float volume)
    {
        seMasterVolume = volume;
        SaveVolumeSettings();
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("BGMVolume", bgmMasterVolume);
        PlayerPrefs.SetFloat("SEVolume", seMasterVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        // 保存された音量をロード（存在しない場合はデフォルト値を使用）
        bgmMasterVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        seMasterVolume = PlayerPrefs.GetFloat("SEVolume", 0.5f);

        // ロードした音量をAudioSourceに適用
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = bgmMasterVolume * masterVolume;
        }
    }
    
    // 指定されたBGMをフェードアウト後、フェードインして再生。
    public async UniTask PlayBGM(BGMSoundData.BGM newBgm, float fadeDuration = 1.0f)
    {
        if (bgmAudioSource.isPlaying && bgmAudioSource.clip == bgmSoundDatas.Find(data => data.bgm == newBgm)?.audioClip)
        {
            return;  // 現在再生中のBGMと同じ場合は処理をスキップ
        }

        await FadeOutBGM(fadeDuration);

        BGMSoundData data = bgmSoundDatas.Find(d => d.bgm == newBgm);
        if (data != null && data.audioClip != null)
        {
            bgmAudioSource.clip = data.audioClip;
            bgmAudioSource.volume = 0;
            bgmAudioSource.Play();
            await FadeInBGM(fadeDuration, data.volume * bgmMasterVolume * masterVolume);
        }
        else
        {
            Debug.LogWarning($"BGMデータが見つかりません: {newBgm}");
        }
    }
    
    // BGMのフェードアウト処理。
    private async UniTask FadeOutBGM(float duration)
    {
        float startVolume = bgmAudioSource.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmAudioSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            await UniTask.Yield();
        }
        bgmAudioSource.Stop();
        bgmAudioSource.volume = startVolume;
    }
    
    // BGMのフェードイン処理。
    private async UniTask FadeInBGM(float duration, float targetVolume)
    {
        bgmAudioSource.volume = 0;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmAudioSource.volume = Mathf.Lerp(0, targetVolume, t / duration);
            await UniTask.Yield();
        }
        bgmAudioSource.volume = targetVolume;
    }
    
    //　指定されたSEを再生。
    public void PlaySE(SESoundData.SE se)
    {
        SESoundData data = seSoundDatas.Find(d => d.se == se);
        if (data != null && data.audioClip != null)
        {
            seAudioSource.volume = data.volume * seMasterVolume * masterVolume;
            seAudioSource.PlayOneShot(data.audioClip);
        }
        else
        {
            Debug.LogWarning($"SEデータが見つかりません: {se}");
        }
    }
}

[System.Serializable]
public class BGMSoundData
{
    public enum BGM
    {
        Title,
        Main,
        Result
    }

    public BGM bgm;
    public AudioClip audioClip;
    [Range(0, 1)]
    public float volume = 1;
}

[System.Serializable]
public class SESoundData
{
    public enum SE
    {
        Fubuki,
        Select,
        StageClear,
        Asioto,
        Damage,
        Get,
        Move
    }

    public SE se;
    public AudioClip audioClip;
    [Range(0, 1)]
    public float volume = 1;
}
