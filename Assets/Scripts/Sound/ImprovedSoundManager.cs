using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 사운드 타입 열거형
public enum SoundType
{
    BGM,      // 2D 사운드 (배경음악)
    SFX_2D,   // 2D 효과음 (UI 등)
    SFX_3D,   // 3D 효과음 (월드 내 사운드)
    Voice_2D, // 2D 음성
    Voice_3D  // 3D 음성
}

// 사운드 클립 정보를 담는 클래스
[System.Serializable]
public class SoundProperty
{
    public string name;
    public AudioClip clip;
    public SoundType soundType;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;
    
    [Header("3D 사운드 설정")]
    public float minDistance = 1f;
    public float maxDistance = 50f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
}

// 3D 오디오 소스 풀링을 위한 클래스
public class PooledAudioSource
{
    public AudioSource audioSource;
    public bool isPlaying;
    public GameObject gameObject;
    public string soundName;  // 속한 사운드 그룹 이름
}

// 사운드 그룹 관리를 위한 클래스
public class SoundGroup
{
    public string soundName;
    public GameObject groupObject;
    public List<PooledAudioSource> pooledSources;
    public int initialPoolSize = 3; // 그룹별 초기 풀 크기

    public SoundGroup(string name, Transform parent, int initialSize = 3)
    {
        soundName = name;
        groupObject = new GameObject($"SoundGroup_{name}");
        groupObject.transform.SetParent(parent);
        pooledSources = new List<PooledAudioSource>();
        initialPoolSize = initialSize;
    }
}

public class ImprovedSoundManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static ImprovedSoundManager Instance { get; private set; }

    [Header("2D 오디오 소스")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource[] sfx2DSources;
    private int sfx2DSourceIndex = 0;

    [Header("3D 오디오 소스 풀")]
    [SerializeField] private int globalPoolSize = 10;
    private List<PooledAudioSource> globalAudioSourcePool = new List<PooledAudioSource>();

    [Header("사운드 리스트")]
    [SerializeField] private List<SoundProperty> sounds = new List<SoundProperty>();

    // 사운드 딕셔너리 (빠른 검색을 위함)
    private Dictionary<string, SoundProperty> soundDictionary;

    // 사운드 그룹 딕셔너리
    private Dictionary<string, SoundGroup> soundGroups;

    [Header("볼륨 설정")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    // 3D 사운드 풀 관리용 부모 오브젝트
    private GameObject soundPoolParent;
    private GameObject soundGroupParent;
    
    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (GameManager.Instance?.currentSceneName == "Stage3")
        {
            Debug.Log("BGM Play");
            PlaySound2D("2DModeBGM");
        }
        
        masterVolume = PlayerPrefs.GetFloat("MasterVolumeSliderValue", 1f);
        bgmVolume = PlayerPrefs.GetFloat("BGMVolumeSliderValue", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolumeSliderValue", 1f);

        UpdateAllVolumes();
    }

    private void InitializeSoundManager()
    {
        // 2D 오디오 소스 초기화
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f; // 2D 사운드
        }

        // 2D SFX 소스 초기화
        if (sfx2DSources == null || sfx2DSources.Length == 0)
        {
            sfx2DSources = new AudioSource[5];
            for (int i = 0; i < sfx2DSources.Length; i++)
            {
                sfx2DSources[i] = gameObject.AddComponent<AudioSource>();
                sfx2DSources[i].spatialBlend = 0f; // 2D 사운드
            }
        }

        // 부모 오브젝트 생성
        soundPoolParent = new GameObject("Global_3D_Sound_Pool");
        soundPoolParent.transform.SetParent(transform);
        
        soundGroupParent = new GameObject("Sound_Groups");
        soundGroupParent.transform.SetParent(transform);

        // 3D 오디오 소스 풀 초기화
        InitializeGlobalAudioSourcePool();

        // 사운드 딕셔너리 초기화
        soundDictionary = new Dictionary<string, SoundProperty>();
        foreach (SoundProperty sound in sounds)
        {
            if (!soundDictionary.ContainsKey(sound.name))
            {
                soundDictionary.Add(sound.name, sound);
            }
        }

        // 사운드 그룹 딕셔너리 초기화
        soundGroups = new Dictionary<string, SoundGroup>();
    }

    private void InitializeGlobalAudioSourcePool()
    {
        for (int i = 0; i < globalPoolSize; i++)
        {
            CreatePooledAudioSource($"Global_PooledAudioSource_{i}", soundPoolParent.transform);
        }
    }

    private PooledAudioSource CreatePooledAudioSource(string name, Transform parent, string groupName = "")
    {
        GameObject audioObject = new GameObject(name);
        audioObject.transform.SetParent(parent);
        
        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f; // 3D 사운드
        source.playOnAwake = false;
        
        PooledAudioSource pooledSource = new PooledAudioSource
        {
            audioSource = source,
            isPlaying = false,
            gameObject = audioObject,
            soundName = groupName
        };
        
        if (string.IsNullOrEmpty(groupName))
        {
            globalAudioSourcePool.Add(pooledSource);
        }
        
        return pooledSource;
    }

    // 2D 사운드 재생
    public void PlaySound2D(string soundName)
    {
        // 블록된 사운드인지 확인
        if (blockedSounds.Contains(soundName))
        {
            Debug.Log($"'{soundName}' 사운드가 일시적으로 블록되어 있습니다.");
            return;
        }

        if (soundDictionary.TryGetValue(soundName, out SoundProperty sound))
        {
            PlaySound2D(sound);
        }
        else
        {
            Debug.LogWarning($"사운드 '{soundName}'을 찾을 수 없습니다.");
        }
    }

    // 3D 사운드 재생
    public void PlaySound3D(string soundName, Vector3 position)
    {
        // 블록된 사운드인지 확인
        if (blockedSounds.Contains(soundName))
        {
            Debug.Log($"'{soundName}' 사운드가 일시적으로 블록되어 있습니다.");
            return;
        }

        if (soundDictionary.TryGetValue(soundName, out SoundProperty sound))
        {
            PlaySound3D(sound, position);
        }
        else
        {
            Debug.LogWarning($"사운드 '{soundName}'을 찾을 수 없습니다.");
        }
    }

    private void PlaySound2D(SoundProperty sound)
    {
        // 블록 체크는 이미 PlaySound2D(string) 메서드에서 했으므로 여기서는 하지 않음
        switch (sound.soundType)
        {
            case SoundType.BGM:
                PlayBGM(sound);
                break;
            case SoundType.SFX_2D:
            case SoundType.Voice_2D:
                PlaySFX2D(sound);
                break;
            default:
                Debug.LogWarning($"사운드 '{sound.name}'은 2D 타입이 아닙니다.");
                break;
        }
    }

    private void PlaySound3D(SoundProperty sound, Vector3 position)
    {
        if (sound.soundType != SoundType.SFX_3D && sound.soundType != SoundType.Voice_3D)
        {
            Debug.LogWarning($"사운드 '{sound.name}'은 3D 타입이 아닙니다.");
            return;
        }

        PooledAudioSource pooledSource = GetAvailableAudioSource(sound.name);
        if (pooledSource != null)
        {
            pooledSource.gameObject.transform.position = position;
            SetupAndPlay3DSound(pooledSource.audioSource, sound);
            
            // 사운드 재생이 끝나면 비활성화
            StartCoroutine(WaitForSoundToFinish(pooledSource));
        }
    }

    private PooledAudioSource GetAvailableAudioSource(string soundName)
    {
        // 사운드 그룹이 없으면 생성
        if (!soundGroups.ContainsKey(soundName))
        {
            CreateSoundGroup(soundName);
        }

        SoundGroup group = soundGroups[soundName];
        
        // 1. 먼저 그룹 내에서 사용 가능한 오디오 소스 찾기
        foreach (var pooledSource in group.pooledSources)
        {
            if (!pooledSource.isPlaying && !pooledSource.audioSource.isPlaying)
            {
                pooledSource.isPlaying = true;
                return pooledSource;
            }
        }

        // 2. 그룹에 사용 가능한 소스가 없으면 글로벌 풀에서 가져오기
        PooledAudioSource availableSource = null;
        for (int i = 0; i < globalAudioSourcePool.Count; i++)
        {
            if (!globalAudioSourcePool[i].isPlaying && !globalAudioSourcePool[i].audioSource.isPlaying)
            {
                availableSource = globalAudioSourcePool[i];
                globalAudioSourcePool.RemoveAt(i);
                break;
            }
        }

        // 3. 글로벌 풀에도 없으면 새로 생성
        if (availableSource == null)
        {
            availableSource = CreatePooledAudioSource($"Dynamic_PooledAudioSource_{GetTotalAudioSources()}", soundPoolParent.transform);
        }

        // 4. 그룹으로 이동
        availableSource.gameObject.transform.SetParent(group.groupObject.transform);
        availableSource.soundName = soundName;
        availableSource.isPlaying = true;
        group.pooledSources.Add(availableSource);
        
        return availableSource;
    }

    private void CreateSoundGroup(string soundName)
    {
        SoundGroup newGroup = new SoundGroup(soundName, soundGroupParent.transform);
        soundGroups[soundName] = newGroup;
        
        // 그룹별 초기 풀 생성
        for (int i = 0; i < newGroup.initialPoolSize; i++)
        {
            PooledAudioSource pooledSource = CreatePooledAudioSource(
                $"Group_{soundName}_AudioSource_{i}", 
                newGroup.groupObject.transform, 
                soundName
            );
            newGroup.pooledSources.Add(pooledSource);
        }
    }

    private int GetTotalAudioSources()
    {
        int total = globalAudioSourcePool.Count;
        foreach (var group in soundGroups.Values)
        {
            total += group.pooledSources.Count;
        }
        return total;
    }

    private IEnumerator WaitForSoundToFinish(PooledAudioSource pooledSource)
    {
        yield return new WaitUntil(() => !pooledSource.audioSource.isPlaying);
        pooledSource.isPlaying = false;
    }

    // 사운드 재생을 막는 블록 리스트
    private HashSet<string> blockedSounds = new HashSet<string>();

    // 특정 사운드 그룹의 모든 사운드 정지
    public void StopSoundGroup(string soundName)
    {
        if (!soundGroups.ContainsKey(soundName))
        {
            Debug.LogWarning($"사운드 그룹 '{soundName}'을 찾을 수 없습니다.");
            return;
        }

        SoundGroup group = soundGroups[soundName];
        int stoppedCount = 0;

        // 그룹 내 모든 재생 중인 사운드 정지
        foreach (var pooledSource in group.pooledSources)
        {
            if (pooledSource.isPlaying && pooledSource.audioSource.isPlaying)
            {
                pooledSource.audioSource.Stop();
                pooledSource.isPlaying = false;
                stoppedCount++;
            }
        }

        Debug.Log($"'{soundName}' 그룹의 {stoppedCount}개 사운드를 정지했습니다.");
    }

    // 특정 사운드 정지 + 한 프레임 재생 블록
    public void StopSoundWithFrameBlock(string soundName)
    {
        // 이 사운드의 재생을 잠시 막음
        blockedSounds.Add(soundName);
        
        // 3D 사운드인 경우 그룹 정지
        StopSoundGroup(soundName);
        
        // 2D 사운드인 경우 정지
        if (soundDictionary.TryGetValue(soundName, out SoundProperty sound))
        {
            if (sound.soundType == SoundType.BGM && bgmSource.isPlaying && bgmSource.clip == sound.clip)
            {
                bgmSource.Stop();
            }
            else if (sound.soundType == SoundType.SFX_2D || sound.soundType == SoundType.Voice_2D)
            {
                foreach (var source in sfx2DSources)
                {
                    if (source.isPlaying && source.clip == sound.clip)
                    {
                        source.Stop();
                    }
                }
            }
        }
        
        // 다음 프레임에 블록 해제
        StartCoroutine(UnblockSoundNextFrame(soundName));
    }
    
    // 특정 사운드 정지 + 지정된 시간 동안 재생 블록
    public void StopSoundWithBlock(string soundName, float blockDuration = 0.1f)
    {
        // 이 사운드의 재생을 잠시 막음
        blockedSounds.Add(soundName);
        
        // 3D 사운드인 경우 그룹 정지
        StopSoundGroup(soundName);
        
        // 2D 사운드인 경우 정지
        if (soundDictionary.TryGetValue(soundName, out SoundProperty sound))
        {
            if (sound.soundType == SoundType.BGM && bgmSource.isPlaying && bgmSource.clip == sound.clip)
            {
                bgmSource.Stop();
            }
            else if (sound.soundType == SoundType.SFX_2D || sound.soundType == SoundType.Voice_2D)
            {
                foreach (var source in sfx2DSources)
                {
                    if (source.isPlaying && source.clip == sound.clip)
                    {
                        source.Stop();
                    }
                }
            }
        }
        
        // 일정 시간 후 블록 해제
        StartCoroutine(UnblockSoundAfterDelay(soundName, blockDuration));
    }

    private IEnumerator UnblockSoundNextFrame(string soundName)
    {
        yield return new WaitForSeconds(0.1f);
        blockedSounds.Remove(soundName);
    }

    // 특정 사운드 그룹의 모든 소스 강제 정지 (재생 여부 무관)
    public void ForceStopAllInGroup(string soundName)
    {
        if (!soundGroups.ContainsKey(soundName))
        {
            Debug.LogWarning($"사운드 그룹 '{soundName}'을 찾을 수 없습니다.");
            return;
        }

        SoundGroup group = soundGroups[soundName];
        
        // 모든 오디오 소스를 강제로 정지 (재생 여부와 관계없이)
        foreach (var pooledSource in group.pooledSources)
        {
            pooledSource.audioSource.Stop();
            pooledSource.audioSource.clip = null; // 클립도 제거
            pooledSource.isPlaying = false;
        }

        Debug.Log($"'{soundName}' 그룹의 모든 오디오 소스를 강제 정지했습니다.");
    }

    // 사운드 재생 블록/해제
    public void BlockSound(string soundName)
    {
        blockedSounds.Add(soundName);
    }

    public void UnblockSound(string soundName)
    {
        blockedSounds.Remove(soundName);
    }

    public bool IsSoundBlocked(string soundName)
    {
        return blockedSounds.Contains(soundName);
    }

    private IEnumerator UnblockSoundAfterDelay(string soundName, float delay)
    {
        yield return new WaitForSeconds(delay);
        blockedSounds.Remove(soundName);
    }

    // 모든 3D 사운드 정지
    public void StopAll3DSounds()
    {
        foreach (var groupName in soundGroups.Keys)
        {
            StopSoundGroup(groupName);
        }
    }

    // 특정 사운드 그룹이 재생 중인지 확인
    public bool IsSoundGroupPlaying(string soundName)
    {
        if (!soundGroups.ContainsKey(soundName))
        {
            return false;
        }

        SoundGroup group = soundGroups[soundName];
        foreach (var pooledSource in group.pooledSources)
        {
            if (pooledSource.isPlaying && pooledSource.audioSource.isPlaying)
            {
                return true;
            }
        }
        return false;
    }

    // 특정 사운드 그룹의 재생 중인 사운드 개수 확인
    public int GetPlayingSoundCount(string soundName)
    {
        if (!soundGroups.ContainsKey(soundName))
        {
            return 0;
        }

        SoundGroup group = soundGroups[soundName];
        int count = 0;
        foreach (var pooledSource in group.pooledSources)
        {
            if (pooledSource.isPlaying && pooledSource.audioSource.isPlaying)
            {
                count++;
            }
        }
        return count;
    }

    // 특정 사운드 그룹의 총 오디오 소스 개수 확인
    public int GetTotalSoundSourceCount(string soundName)
    {
        if (!soundGroups.ContainsKey(soundName))
        {
            return 0;
        }
        
        return soundGroups[soundName].pooledSources.Count;
    }

    // 특정 사운드 그룹의 볼륨 조절
    public void SetSoundGroupVolume(string soundName, float volume)
    {
        if (!soundGroups.ContainsKey(soundName))
        {
            Debug.LogWarning($"사운드 그룹 '{soundName}'을 찾을 수 없습니다.");
            return;
        }

        SoundGroup group = soundGroups[soundName];
        foreach (var pooledSource in group.pooledSources)
        {
            if (pooledSource.isPlaying)
            {
                pooledSource.audioSource.volume = volume * sfxVolume * masterVolume;
            }
        }
    }

    // 사운드 그룹의 초기 풀 크기 설정
    public void SetGroupInitialPoolSize(string soundName, int size)
    {
        if (soundGroups.ContainsKey(soundName))
        {
            soundGroups[soundName].initialPoolSize = size;
        }
    }
    
    private void SetupAndPlay3DSound(AudioSource source, SoundProperty sound)
    {
        source.clip = sound.clip;
        source.volume = sound.volume * sfxVolume * masterVolume;
        source.pitch = sound.pitch;
        source.loop = sound.loop;
        source.minDistance = sound.minDistance;
        source.maxDistance = sound.maxDistance;
        source.rolloffMode = sound.rolloffMode;
        source.spatialBlend = 1f; // 3D 사운드
        source.Play();
    }

    private void PlayBGM(SoundProperty sound)
    {
        bgmSource.clip = sound.clip;
        bgmSource.volume = sound.volume * bgmVolume * masterVolume;
        bgmSource.pitch = sound.pitch;
        bgmSource.loop = sound.loop;
        bgmSource.Play();
    }

    private void PlaySFX2D(SoundProperty sound)
    {
        // null 체크 추가
        if (sfx2DSources == null || sfx2DSources.Length == 0)
        {
            Debug.LogError("sfx2DSources가 초기화되지 않았습니다. sfx2DSources 배열을 초기화합니다.");
        
            // 런타임에 동적으로 생성
            sfx2DSources = new AudioSource[5];
            for (int i = 0; i < sfx2DSources.Length; i++)
            {
                sfx2DSources[i] = gameObject.AddComponent<AudioSource>();
                sfx2DSources[i].spatialBlend = 0f; // 2D 사운드
            }
        
            sfx2DSourceIndex = 0; // 인덱스 초기화
        }
    
        // 인덱스 안전성 확인
        if (sfx2DSourceIndex < 0 || sfx2DSourceIndex >= sfx2DSources.Length)
        {
            sfx2DSourceIndex = 0;
        }
    
        AudioSource source = sfx2DSources[sfx2DSourceIndex];
    
        // 소스가 null인지 확인
        if (source == null)
        {
            Debug.LogWarning($"sfx2DSources[{sfx2DSourceIndex}]가 null입니다. 새 AudioSource를 생성합니다.");
            source = gameObject.AddComponent<AudioSource>();
            source.spatialBlend = 0f; // 2D 사운드
            sfx2DSources[sfx2DSourceIndex] = source;
        }
    
        source.clip = sound.clip;
        source.volume = sound.volume * sfxVolume * masterVolume;
        source.pitch = sound.pitch;
        source.loop = sound.loop;
        source.Play();

        sfx2DSourceIndex = (sfx2DSourceIndex + 1) % sfx2DSources.Length;
    }

    // 특정 오브젝트에 부착된 사운드 재생
    public AudioSource PlaySound3DAttached(string soundName, Transform target)
    {
        if (soundDictionary.TryGetValue(soundName, out SoundProperty sound))
        {
            return PlaySound3DAttached(sound, target);
        }
        else
        {
            Debug.LogWarning($"사운드 '{soundName}'을 찾을 수 없습니다.");
            return null;
        }
    }

    private AudioSource PlaySound3DAttached(SoundProperty sound, Transform target)
    {
        if (sound.soundType != SoundType.SFX_3D && sound.soundType != SoundType.Voice_3D)
        {
            Debug.LogWarning($"사운드 '{sound.name}'은 3D 타입이 아닙니다.");
            return null;
        }

        // 타겟에 부착된 오디오 소스가 있는지 확인
        AudioSource attachedSource = target.GetComponent<AudioSource>();
        if (attachedSource == null)
        {
            attachedSource = target.gameObject.AddComponent<AudioSource>();
            attachedSource.spatialBlend = 1f; // 3D 사운드
        }

        SetupAndPlay3DSound(attachedSource, sound);
        return attachedSource;
    }

    // BGM 제어 메서드들
    public void StopBGM()
    {
        if (bgmSource!=null)
        {
            bgmSource.Stop();
        }
        
    }

    public void PauseBGM()
    {
        if (bgmSource!=null)
        {
            bgmSource.Pause();
        }
        
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    public void FadeBGM(float targetVolume, float duration)
    {
        StartCoroutine(FadeCoroutine(bgmSource, targetVolume, duration));
    }

    private IEnumerator FadeCoroutine(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            source.volume = Mathf.Lerp(startVolume, targetVolume * bgmVolume * masterVolume, t);
            yield return null;
        }

        source.volume = targetVolume * bgmVolume * masterVolume;
    }

    // 볼륨 조절 메서드들
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MasterVolumeSliderValue", masterVolume);
        UpdateAllVolumes();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("BGMVolumeSliderValue", bgmVolume);
        UpdateAllVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolumeSliderValue", sfxVolume);
    }

    private void UpdateAllVolumes()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.volume = bgmSource.volume * bgmVolume * masterVolume;
        }
    }

    // 오디오 리스너 설정 (플레이어 전환 시 사용)
    public void SetAudioListener(GameObject newListener)
    {
        // 기존 오디오 리스너 비활성화
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        foreach (var listener in listeners)
        {
            listener.enabled = false;
        }

        // 새 오디오 리스너 설정
        AudioListener newAudioListener = newListener.GetComponent<AudioListener>();
        if (newAudioListener == null)
        {
            newAudioListener = newListener.AddComponent<AudioListener>();
        }
        newAudioListener.enabled = true;
    }

    // 런타임에 사운드 추가
    public void AddSound(SoundProperty sound)
    {
        if (!soundDictionary.ContainsKey(sound.name))
        {
            sounds.Add(sound);
            soundDictionary.Add(sound.name, sound);
        }
    }

    // 사운드 존재 여부 확인
    public bool HasSound(string soundName)
    {
        return soundDictionary.ContainsKey(soundName);
    }

    // 특정 BGM이 재생 중인지 확인
    public bool IsPlayingBGM()
    {
        return bgmSource.isPlaying;
    }

    // 현재 재생 중인 BGM 이름 가져오기
    public string GetCurrentBGMName()
    {
        if (bgmSource.clip != null)
        {
            foreach (var sound in sounds)
            {
                if (sound.clip == bgmSource.clip)
                {
                    return sound.name;
                }
            }
        }
        return null;
    }
}

// 사용 헬퍼 클래스
public static class SoundHelper
{
    // BGM 재생 예제
    public static void PlayBGM(string bgmName)
    {
        ImprovedSoundManager.Instance?.PlaySound2D(bgmName);
    }

    // 2D SFX 재생 예제
    public static void PlaySFX2D(string sfxName)
    {
        ImprovedSoundManager.Instance?.PlaySound2D(sfxName);
    }

    // 3D SFX 재생 예제
    public static void PlaySFX3D(string sfxName, Vector3 position)
    {
        ImprovedSoundManager.Instance?.PlaySound3D(sfxName, position);
    }

    // UI 사운드 재생 예제
    public static void PlayUISound(string uiSoundName)
    {
        ImprovedSoundManager.Instance?.PlaySound2D(uiSoundName);
    }
    
    // 사운드 그룹 정지 예제
    public static void StopSoundGroup(string soundName)
    {
        ImprovedSoundManager.Instance?.StopSoundGroup(soundName);
    }
    
    // 사운드 그룹 재생 중인지 확인 예제
    public static bool IsSoundGroupPlaying(string soundName)
    {
        return ImprovedSoundManager.Instance?.IsSoundGroupPlaying(soundName) ?? false;
    }
    
    // 사운드 그룹 볼륨 조절 예제
    public static void SetSoundGroupVolume(string soundName, float volume)
    {
        ImprovedSoundManager.Instance?.SetSoundGroupVolume(soundName, volume);
    }
}