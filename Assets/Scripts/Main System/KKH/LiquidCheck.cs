using UnityEngine;

public class LiquidCheck : MonoBehaviour
{
    public LiquidLoader loader;
    public LiquidDisplay3D display3D;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Liquid")) return;

        Liquid liquid = other.GetComponent<Liquid>();
        if (liquid == null) return;

        LiquidData data = null;

        // 1. 기본 용액 데이터에서 찾기
        if (liquid.liquidIndex >= 0 && liquid.liquidIndex < loader.allLiquids.Count)
        {
            data = loader.allLiquids[liquid.liquidIndex];
        }
        else
        {
            // 2. liquid 이름으로 조합 정보에서 찾기
            var combo = LiquidCombinationManager.Instance.GetCombinationByName(liquid.liquidName);
            if (combo != null)
            {
                data = new LiquidData
                {
                    Name = combo.Name,
                    Color = combo.Color,
                    Danger = combo.Danger,
                    ph = combo.ph
                };
            }
        }

        if (data != null)
        {
            display3D.SetLiquidInfo(data);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Liquid"))
        {
            LiquidData noneData = new LiquidData
            {
                Name = "None",
                Color = "None",
                Danger = 0,
                ph = 0.0f
            };

            display3D.SetLiquidInfo(noneData);
        }
    }
}
