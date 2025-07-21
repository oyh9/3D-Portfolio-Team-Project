using UnityEngine;

public class Liquid : MonoBehaviour
{
    public int liquidIndex;
    public string liquidName;

    public void Drop(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
    }

    public void Recycle()
    {
        LiquidPoolManager.Instance.ReturnLiquid(gameObject);
    }
}
