using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    public Material ToApply;
    public float duration = 5f;

    public void ApplyToPlayer(PlayerMaterialState playerState)
    {
        if(playerState != null)
        {
            playerState.SetMaterialTemporarily(ToApply, duration);
            Debug.Log("마테리얼 변경");
        }
    }
}
