using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManagerTest : MonoBehaviour
{
    public static AudioManagerTest instance;

    [Header("AudioMixer 설정")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string bgmMixerParameter = "BGM";

    [Header("오디오 클립 설정")]
    [SerializeField] private AudioClip bgmClip;

    [Header("UI 슬라이더")]
    [SerializeField] private Slider bgmSlider;

    private AudioSource bgmSource;

    private void Awake()
    {
        // 싱글톤 설정
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitAudioSource();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitSlider();
    }

    private void InitAudioSource()
    {
        // AudioSource 자동 추가
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.clip = bgmClip;

        // AudioMixer 그룹 연결
        AudioMixerGroup[] groups = audioMixer.FindMatchingGroups("BGM");
        if (groups.Length > 0)
        {
            bgmSource.outputAudioMixerGroup = groups[0];
        }

        bgmSource.Play();
    }

    private void InitSlider()
    {
        if (bgmSlider != null)
        {
            // AudioMixer에서 현재 볼륨을 가져와 슬라이더 초기값 설정
            float currentVolume;
            if (audioMixer.GetFloat(bgmMixerParameter, out currentVolume))
            {
                bgmSlider.value = Mathf.Pow(10, currentVolume / 20f); // dB → 0~1
            }

            // 슬라이더 이벤트 연결
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }
        else
        {
            Debug.LogWarning("슬라이더가 연결되지 않았습니다.");
        }
    }

    public void SetBGMVolume(float volume)
    {
        float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(bgmMixerParameter, dB);
    }
}
