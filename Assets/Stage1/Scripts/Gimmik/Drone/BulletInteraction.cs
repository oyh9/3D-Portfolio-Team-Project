using UnityEngine;

public class BulletInteraction : MonoBehaviour
{
    public void OnHit()
    {
        Debug.Log($"{gameObject.name}��(��) �Ѿ˿� �¾ҽ��ϴ�!");
        Destroy(gameObject);
    }
}
