using UnityEngine;

public class MaterialSensor : MonoBehaviour
{
    public Material required;

    public bool CanReact(Material playerMaterial)
    {
        return playerMaterial == required;
    }

    public void ReactToPlayer()
    {
        Debug.Log("��ȣ�ۿ� ����");
        DoInteraction();
    }

    private void DoInteraction()
    {
        //��ȣ�ۿ� �ڵ�
    }
}
