using UnityEngine;
using System.Collections;

public class LiquidSlot : MonoBehaviour
{
    public enum SlotType { Slot1, Slot2 }
    public SlotType slotType;
    public GameObject spawnEffectPrefab; // 이펙트 프리팹

    public GameObject hintBoard1;
    public GameObject hintBoard2;
    public GameObject hintBoard3;

    public GameObject currentLiquidObject;

    public LiquidLoader loader; // Liquid 데이터를 참조하기 위해 연결

    public string currentLiquidName;

    public Transform spawnPoint;

    private void Start()
    {
        if (hintBoard1 != null && hintBoard2 != null && hintBoard3 != null)
        {
            hintBoard1.SetActive(false);
            hintBoard2.SetActive(false);
            hintBoard3.SetActive(false);
        }
        else
        {
            Debug.LogWarning("힌트보드가 할당이 안됨");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Liquid")) return;

        Liquid liquid = other.GetComponent<Liquid>();
        if (liquid != null)
        {
            string nameToUse;

            // liquidIndex 유효하면 loader에서 이름 가져오고,
            // 아니면 liquid.liquidName 직접 사용 (조합된 프리팹 등)
            if (liquid.liquidIndex >= 0 && liquid.liquidIndex < loader.allLiquids.Count)
            {
                LiquidData data = loader.allLiquids[liquid.liquidIndex];
                nameToUse = data.Name;
            }
            else
            {
                nameToUse = liquid.liquidName;
            }

            currentLiquidName = nameToUse;
            currentLiquidObject = other.gameObject;


            Debug.Log($"[{slotType}] 용액 들어옴: {currentLiquidName}");

            TryCombine();
        }
    }


    private void TryCombine()
    {
        string otherName = null;
        LiquidSlot otherSlot = null;

        foreach (LiquidSlot slot in FindObjectsOfType<LiquidSlot>())
        {
            if (slot != this && !string.IsNullOrEmpty(slot.currentLiquidName))
            {
                otherName = slot.currentLiquidName;
                otherSlot = slot;
                break;
            }
        }

        if (!string.IsNullOrEmpty(currentLiquidName) && !string.IsNullOrEmpty(otherName))
        {
            Debug.Log($"[조합 시도] {currentLiquidName} + {otherName}");

            var combo = LiquidCombinationManager.Instance.GetCombination(currentLiquidName, otherName);

            if (combo != null)
            {
                if (slotType == SlotType.Slot1)
                {
                    if (spawnPoint == null)
                    {
                        Debug.LogWarning("[오류] spawnPoint가 설정되지 않았습니다.");
                        return;
                    }

                    GameObject prefab = LiquidCombinationManager.Instance.GetCombinationPrefab(combo.Name);
                    if (prefab == null)
                    {
                        Debug.LogWarning($"[오류] 프리팹이 존재하지 않습니다: {combo.Name}");
                        return;
                    }

                    GameObject obj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

                    // 이펙트 프리팹 생성
                    if (spawnEffectPrefab != null)
                    {
                        Instantiate(spawnEffectPrefab, spawnPoint.position, Quaternion.identity);
                    }

                    Liquid comboLiquid = obj.GetComponent<Liquid>();
                    if (comboLiquid != null)
                    {
                        comboLiquid.liquidName = combo.Name;
                        comboLiquid.liquidIndex = -1;
                    }

                    if (comboLiquid.liquidName == "Middle1")
                    {
                        hintBoard1.SetActive(true);
                    }

                    if (comboLiquid.liquidName == "Middle2")
                    {
                        hintBoard2.SetActive(true);
                    }

                    if (comboLiquid.liquidName == "Middle3")
                    {
                        hintBoard3.SetActive(true);
                    }

                    if (comboLiquid.liquidName == "Last Liquid")
                    {
                        DialogueManager.Instance.TutorialDialogue(3);
                    }

                    // 기존 용액 비활성화 및 슬롯 정보 제거
                    if (currentLiquidObject != null)
                    {
                        currentLiquidObject.SetActive(false);
                        currentLiquidObject = null;
                        currentLiquidName = null;
                    }

                    if (otherSlot != null && otherSlot.currentLiquidObject != null)
                    {
                        otherSlot.currentLiquidObject.SetActive(false);
                        otherSlot.currentLiquidObject = null;
                        otherSlot.currentLiquidName = null;
                    }


                    Debug.Log($"[프리팹 생성] {combo.Name} 프리팹 생성됨");
                }

            }
            else
            {
                Debug.LogWarning($"[조합 실패] {currentLiquidName} + {otherName} 는 유효하지 않음");
            }
        }
    }

    private IEnumerator SmoothRotate(Transform target, Vector3 targetEulerAngles, float duration)
    {
        Quaternion startRotation = target.rotation;
        Quaternion endRotation = Quaternion.Euler(targetEulerAngles);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            target.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.rotation = endRotation;
    }
}