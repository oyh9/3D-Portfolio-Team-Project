using UnityEngine;

public class ReturnToStart : MonoBehaviour
{
    public GameObject player;
    public Transform startPosition;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            player.transform.position = startPosition.position;
        }
    }
}
