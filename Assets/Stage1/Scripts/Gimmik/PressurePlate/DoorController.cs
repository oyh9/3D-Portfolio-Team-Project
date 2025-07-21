using UnityEngine;

public class DoorController : MonoBehaviour, IStageGimmick
{
    [Header("문 움직임")]
    public Vector3 openOffset = new Vector3(0, 13f, 0); // 열릴 위치(기준 위치 + 이만큼 이동)
    public float moveSpeed = 2f;

    private Vector3 closedPosition;
    private Vector3 targetPosition;

    [SerializeField] private string gimmickID = "DoorController_A";
    public GimmickType GetGimmickType() => GimmickType.DoorController;
    public string GetGimmickID() => gimmickID;
    
    void Start()
    {
        closedPosition = transform.position;
        targetPosition = closedPosition;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }

    public void Activate()
    {
        targetPosition = closedPosition + openOffset;
    }

    public void Deactivate()
    {
        targetPosition = closedPosition;
    }
}
