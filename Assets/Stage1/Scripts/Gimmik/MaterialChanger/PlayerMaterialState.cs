using UnityEngine;
using System.Collections;

public class PlayerMaterialState : MonoBehaviour
{
    public Material current;
    private Material origin;
    private Coroutine revertCorutine;
    private new Renderer renderer;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        if (renderer == null)
            renderer = GetComponentInChildren<Renderer>();

        origin = renderer.material;
        current = origin;
    }

    public void SetMaterial(Material newMaterial)
    {
        current = newMaterial;
        origin = newMaterial;
        renderer.material = newMaterial;
    }

    public void SetMaterialTemporarily(Material newMaterial, float duration)
    {
        if (revertCorutine != null)
            StopCoroutine(revertCorutine);

        current = newMaterial;
        renderer.material = newMaterial;
        revertCorutine = StartCoroutine(RevertAfterTime(duration));
    }

    private IEnumerator RevertAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        current = origin;
        renderer.material = origin;
        revertCorutine = null;
    }
}
