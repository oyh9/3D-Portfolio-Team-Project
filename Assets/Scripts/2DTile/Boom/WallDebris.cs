using UnityEngine;

public class WallDebris : MonoBehaviour
{
    private float destroyTime;
    private float timer = 0f;
    
    public void InitializeDebris(float time)
    {
        destroyTime = time;
    }
    
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= destroyTime)
        {
            Destroy(gameObject);
        }
    }
}
