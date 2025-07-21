using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class NamedLiquidPrefab
{
    public string Name;
    public GameObject prefab;
}

public class LiquidCombinationManager : MonoBehaviour
{
    public static LiquidCombinationManager Instance;

    private Dictionary<string, LiquidCombination> combinationDict = new Dictionary<string, LiquidCombination>();

    public TextAsset csvFile; // Unity �ν����Ϳ��� ����

    [Header("���� �̸� ������ ����")]
    public List<NamedLiquidPrefab> resultPrefabs;

    private void Awake()
    {
        Instance = this;
        LoadCombinations();
    }

    void LoadCombinations()
    {
        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // 0���� ����ϱ� ��ŵ
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');

            if (values.Length < 6) continue;

            var combo = new LiquidCombination
            {
                Liquid1 = values[0].Trim(),
                Liquid2 = values[1].Trim(),
                Name = values[2].Trim(),
                Color = values[3].Trim(),
                Danger = int.Parse(values[4]),
                ph = float.Parse(values[5])
            };

            // ������ �����ؼ� �����ϰ� ����� (A+B != B+A)
            string key = $"{combo.Liquid1}_{combo.Liquid2}";

            if (!combinationDict.ContainsKey(key))
            {
                combinationDict.Add(key, combo);
            }
            else
            {
                Debug.LogWarning($"[�ߺ� ���� ���õ�] {key}");
            }
        }

        Debug.Log($"[���� �ε�] {combinationDict.Count}�� ���� �ε� �Ϸ�");
    }


    private string GetComboKey(string a, string b)
    {
        return string.Compare(a, b) < 0 ? $"{a}_{b}" : $"{b}_{a}";
    }

    public LiquidCombination GetCombination(string a, string b)
    {
        string key = GetComboKey(a, b);
        if (combinationDict.TryGetValue(key, out var combo))
        {
            return combo;
        }
        return null;
    }

    public GameObject GetCombinationPrefab(string name)
    {
        foreach (var entry in resultPrefabs)
        {
            if (entry.Name == name)
                return entry.prefab;
        }
        Debug.LogWarning($"[���� ������ ����] �̸�: {name}");
        return null;
    }
    public LiquidCombination GetCombinationByName(string name)
    {
        return combinationDict.Values.FirstOrDefault(c => c.Name == name);
    }
}