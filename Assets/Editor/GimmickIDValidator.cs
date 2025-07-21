#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class GimmickIDValidator
{
    [MenuItem("Tools/Gimmick/Validate Gimmick IDs")]
    public static void ValidateGimmickIDs()
    {
        var allGimmicks = Object.FindObjectsOfType<MonoBehaviour>(true)
            .OfType<IStageGimmick>()
            .ToList();

        Dictionary<string, List<GameObject>> idMap = new();

        foreach (var gimmick in allGimmicks)
        {
            string id = gimmick.GetGimmickID();
            if (string.IsNullOrEmpty(id)) continue;

            if (!idMap.ContainsKey(id))
                idMap[id] = new List<GameObject>();

            idMap[id].Add(((MonoBehaviour)gimmick).gameObject);
        }

        bool hasDuplicate = false;

        foreach (var pair in idMap)
        {
            if (pair.Value.Count > 1)
            {
                hasDuplicate = true;
                Debug.LogError($"중복된 gimmickID: '{pair.Key}' - {pair.Value.Count}개");
                foreach (var go in pair.Value)
                {
                    Debug.Log($"GameObject: {go.name}", go);
                }
            }
        }

        if (!hasDuplicate)
            Debug.Log("모든 gimmickID가 유일합니다.");
    }
}
#endif

