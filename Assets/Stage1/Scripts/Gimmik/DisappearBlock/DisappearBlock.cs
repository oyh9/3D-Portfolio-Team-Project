using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DisappearBlock : MonoBehaviour
{
    [Header("재출현 대기 시간")]
    public float delayBeforeBlink = 1.0f;
    public float respawnDelay = 3f;
    
    [Header("블링크 효과 설정")]
    public float blinkDuration = 1f;
    public float blinkInterval = 0.2f;
    
    private Collider blockCollider;
    private Renderer blockRenderer;
    private bool isDisappeared = false;

    void Start()
    {
        blockCollider = GetComponent<Collider>();
        blockRenderer = GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isDisappeared && other.CompareTag("Player"))
        {
            StartCoroutine(BlinkAndDisappear());
        }
    }

    IEnumerator BlinkAndDisappear()
    {
        isDisappeared = true;
        
        yield return new WaitForSeconds(delayBeforeBlink);
        
        float elapsed = 0f;
        while (elapsed < blinkDuration)
        {
            blockRenderer.enabled = !blockRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }
        
        blockRenderer.enabled = false;
        blockCollider.enabled = false;
        
        yield return new WaitForSeconds(respawnDelay);
        
        blockRenderer.enabled = true;
        blockCollider.enabled = true;

        isDisappeared = false;
    }
}