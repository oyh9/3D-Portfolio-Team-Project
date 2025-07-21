using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InvisibleBlock : MonoBehaviour
{
    public float fadeDuration = 0.5f;
    public float inactiveAlpha = 0f;
    public float activeAlpha = 1f;
    public InvisiblePathManager manager;
    
    private Renderer BlockRenderer;
    private Material BlockMaterial;
    
    private bool isStepped = false;

    private void Awake()
    {
        BlockRenderer = GetComponent<Renderer>();
        // Material 인스턴스를 복제해서 다른 오브젝트와 공유되지 않도록 함
        BlockMaterial = BlockRenderer.material;
        SetAlpha(inactiveAlpha);
    }
    
    public void ActivatePlatform()
    {
        StopAllCoroutines();
        StartCoroutine(FadeToAlpha(activeAlpha, fadeDuration));
    }
    
    public void DeactivatePlatform()
    {
        StopAllCoroutines();
        StartCoroutine(FadeToAlpha(inactiveAlpha, fadeDuration));
    }

    IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        float initialAlpha = BlockMaterial.color.a;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(initialAlpha, targetAlpha, elapsed / duration);
            SetAlpha(newAlpha);
            yield return null;
        }
        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        Color c = BlockMaterial.color;
        c.a = alpha;
        BlockMaterial.color = c;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isStepped) return;
        if (!other.CompareTag("Player")) return;

        isStepped = true;
        manager?.OnBlockStepped(this);
    }
    
    public void ResetStepped()
    {
        isStepped = false;
    }
}