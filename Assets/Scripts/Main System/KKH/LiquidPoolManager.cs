using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class LiquidPoolManager : MonoBehaviour
{
    public static LiquidPoolManager Instance;

    public LiquidLoader liquidLoader;

    public GameObject[] usablePrefabs; // 5개 (플레이어용)
    public GameObject[] dangerPrefabs; // 5개 (잠깐 보여줄 용)

    public int poolSize = 30;
    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Start()
    {
        DialogueManager.Instance.TutorialDialogue(0);

        Instance = this;

        Debug.LogWarning($"[POOL INIT] allLiquids.Count = {liquidLoader.allLiquids.Count}");


        if (liquidLoader == null)
        {
            Debug.LogWarning("[LiquidPoolManager] liquidLoader가 연결되어 있지 않습니다.");
            return;
        }

        for (int i = 0; i < liquidLoader.allLiquids.Count; i++)
        {
            GameObject prefabToUse;

            if (liquidLoader.allLiquids[i].Danger <= 5)
            {
                // Danger 낮은 것 = usable
                int usableIndex = i % usablePrefabs.Length;
                prefabToUse = usablePrefabs[usableIndex];
                Debug.LogWarning($"[POOL] Usable prefab index {usableIndex} → {prefabToUse.name}");
            }
            else
            {
                // Danger 높은 것 = danger
                int dangerIndex = i % dangerPrefabs.Length;
                prefabToUse = dangerPrefabs[dangerIndex];
                Debug.LogWarning($"[POOL] Danger prefab index {dangerIndex} → {prefabToUse.name}");
            }

            if (prefabToUse == null)
            {
                Debug.LogError($"[POOL ERROR] prefabToUse가 null입니다. i={i}");
                continue;
            }

            GameObject obj = Instantiate(prefabToUse);
            obj.SetActive(false);

            Liquid liquidComponent = obj.GetComponent<Liquid>();
            if (liquidComponent != null)
            {
                liquidComponent.liquidIndex = i;
            }

            pool.Enqueue(obj);
        }

        /*
        int countPerType = poolSize / 3;
        
        for (int i = 0; i < countPerType; i++)
        {
            GameObject obj = Instantiate(liquidPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
        
        for (int i = 0; i < countPerType; i++)
        {
            GameObject obj = Instantiate(liquidPrefab1);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        for (int i = 0; i < countPerType; i++)
        {
            GameObject obj = Instantiate(liquidPrefab2);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
        */
    }

    public GameObject GetLiquid()
    {
        if(pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            Debug.LogWarning("pool 부족하여 새로 생성");

            // 예외적으로 하나 생성하되, usablePrefabs에서 선택
            if (usablePrefabs != null && usablePrefabs.Length > 0)
            {
                return Instantiate(usablePrefabs[0]);
            }
            else
            {
                Debug.LogError("usablePrefabs가 비어있습니다.");
                return null;
            }
        }
    }

    public void ReturnLiquid(GameObject obj)
    {
        // obj.SetActive(true);
        pool.Enqueue(obj);
    }
}