using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EyeBlinkEffect : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 0.5f;
    public float delayBeforeOpen = 1.0f;

    public void TriggerBlink()
    {
        StartCoroutine(EyeBlinkRoutine());
    }

    private IEnumerator EyeBlinkRoutine()
    {
        // 1. 초기 딜레이 (로딩 직전 느낌)
        yield return new WaitForSeconds(0.5f);

        // 2. 천천히 살짝 눈 뜨기
        yield return FadeTo(0f, 1.5f);
        
        // 3. 중간정도 뜬 상태 유지 (스캔 느낌)
        yield return new WaitForSeconds(0.8f);

        // 4. 다시 감기
        yield return FadeTo(1f, 1.0f);

        // 5. 시스템 점검 시간
        yield return new WaitForSeconds(1.2f);

        // 6. 진짜로 완전히 눈 뜨기
        yield return FadeTo(0f, 1.8f);
    }


    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = fadeImage.color.a;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, targetAlpha);
    }
}
