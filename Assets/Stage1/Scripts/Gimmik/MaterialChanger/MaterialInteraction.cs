using UnityEngine;

public class MaterialInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(interactKey))
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, interactionDistance))
            {
                var playerState = GetComponent<PlayerMaterialState>();
                if(playerState == null) return;

                var target = hit.collider.GetComponent<MaterialSensor>();
                if(target != null)
                {
                    if(target.CanReact(playerState.current))
                    {
                        target.ReactToPlayer();
                    }
                    else
                    {
                        Debug.Log("마테리얼 불일치");
                    }
                    return;
                }

                var materialItem = hit.collider.GetComponent<MaterialChanger>();
                if(materialItem != null && playerState != null)
                {
                    materialItem.ApplyToPlayer(playerState);
                    return;
                }
            }
        }
    }
}
