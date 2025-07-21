using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    // 결합할 큐브들을 이 배열에 할당
    public GameObject[] cubesToCombine;
    
    void Start()
    {
        CombineMeshes();
    }
    
    void CombineMeshes()
    {
        // 결합할 메시들을 저장할 배열
        CombineInstance[] combine = new CombineInstance[cubesToCombine.Length];
        
        for (int i = 0; i < cubesToCombine.Length; i++)
        {
            // 각 큐브의 메시 정보 가져오기
            combine[i].mesh = cubesToCombine[i].GetComponent<MeshFilter>().sharedMesh;
            
            // 로컬 좌표를 월드 좌표로 변환
            combine[i].transform = cubesToCombine[i].transform.localToWorldMatrix;
            
            // 원본 큐브 숨기기
            cubesToCombine[i].SetActive(false);
        }
        
        // 새 메시 생성
        Mesh newMesh = new Mesh();
        newMesh.CombineMeshes(combine);
        
        // 이 게임오브젝트에 결합된 메시 적용
        gameObject.AddComponent<MeshFilter>().mesh = newMesh;
        gameObject.AddComponent<MeshRenderer>().material = cubesToCombine[0].GetComponent<MeshRenderer>().material;
        
        // 필요하면 콜라이더 추가
        gameObject.AddComponent<MeshCollider>().sharedMesh = newMesh;
    }
}