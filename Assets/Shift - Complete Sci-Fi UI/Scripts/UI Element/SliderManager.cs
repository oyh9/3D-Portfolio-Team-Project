using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace Michsky.UI.Shift
{
    public class SliderManager : MonoBehaviour
    {
        [Header("Resources")]
        public TextMeshProUGUI valueText;

        [Header("Saving")]
        public bool enableSaving = false;
        public string sliderTag = "Tag Text";
        public float defaultValue = 1;

        [Header("Settings")]
        public bool usePercent = false;
        public bool showValue = true;
        public bool useRoundValue = false;

        [Header("External Callback")]
        public UnityEvent<float> onValueChangedExternally;
        
        Slider mainSlider;
        float saveValue;

        void Start()
        {
            mainSlider = gameObject.GetComponent<Slider>();

            if (showValue == false)
                valueText.enabled = false;

            if (enableSaving)
            {
                saveValue = PlayerPrefs.GetFloat(sliderTag + "SliderValue", defaultValue);
                mainSlider.value = saveValue;
            }
            
            mainSlider.onValueChanged.AddListener(value =>
            {
                if (enableSaving)
                {
                    PlayerPrefs.SetFloat(sliderTag + "SliderValue", value);
                }
                
                if (sliderTag == "Master Volume" && ImprovedSoundManager.Instance != null)
                    ImprovedSoundManager.Instance.SetMasterVolume(value);
                else if (sliderTag == "BGM Volume" && ImprovedSoundManager.Instance != null)
                    ImprovedSoundManager.Instance.SetBGMVolume(value);
                else if (sliderTag == "SFX Volume" && ImprovedSoundManager.Instance != null)
                    ImprovedSoundManager.Instance.SetSFXVolume(value);

                // 유니티 이벤트 호출도 병행
                onValueChangedExternally?.Invoke(value);
            });

        }

        void Update()
        {
            if (useRoundValue == true)
            {
                if (usePercent == true)
                    valueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString() + "%";
                else
                    valueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString();
            }

            else
            {
                if (usePercent == true)
                    valueText.text = mainSlider.value.ToString("F1") + "%";
                else
                    valueText.text = mainSlider.value.ToString("F1");
            }
        }
    }
}