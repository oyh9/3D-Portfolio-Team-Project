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
        Debug.Log("상호작용 실행");
        DoInteraction();
    }

    private void DoInteraction()
    {
        //상호작용 코드
    }
}
