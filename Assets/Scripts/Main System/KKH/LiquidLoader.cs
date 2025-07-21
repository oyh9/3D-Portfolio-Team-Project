using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LiquidLoader : MonoBehaviour
{
    public TextAsset liquidDataFile;
    public List<LiquidData> allLiquids;     // 전체 CSV 파싱 결과
    public List<LiquidData> usableLiquids;  // Danger 5 이하

    public LiquidDisplay3D liquidDisplay3D;

    private void Awake()
    {
        allLiquids = ParseCsv(liquidDataFile.text);

        // Danger ≤ 5만 usable 리스트로 따로 저장
        usableLiquids = allLiquids.Where(l => l.Danger <= 5).ToList();

        LiquidData noneData = new LiquidData
        {
            Name = "None",
            Color = "None",
            Danger = 0,
            ph = 0.0f
        };

        liquidDisplay3D.SetLiquidInfo(noneData);
    }

    List<LiquidData> ParseCsv(string text)
    {
        var lines = liquidDataFile.text.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
        List<LiquidData> list = new List<LiquidData>();

        for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더
        {
            string[] parts = lines[i].Split(',');
            if (parts.Length < 4) continue;

            LiquidData data = new LiquidData
            {
                Name = parts[0].Trim(),
                Color = parts[1].Trim(),
                Danger = int.Parse(parts[2].Trim()),
                ph = float.Parse(parts[3].Trim())
            };

            list.Add(data);
            Debug.Log($"[CSV 파싱] 줄 {i + 1}: {data.Name}, Danger={data.Danger}");
        }

        return list;
    }
}
