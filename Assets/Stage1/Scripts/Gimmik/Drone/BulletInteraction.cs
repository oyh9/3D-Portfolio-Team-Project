using UnityEngine;

public class BulletInteraction : MonoBehaviour
{
    public void OnHit()
    {
        Debug.Log($"{gameObject.name}이(가) 총알에 맞았습니다!");
        Destroy(gameObject);
    }
}
