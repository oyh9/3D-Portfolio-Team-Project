using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class PressurePlate : MonoBehaviour
{
    [Header("눌리는 판 설정")] 
    public Transform plate; // 눌리는 판
    public Vector3 pressedOffset = new Vector3(0, -0.1f, 0); // 눌렸을 때 위치 오프셋(버튼이 얼마만큼 움직일 지)
    public float moveSpeed = 5f;
    
    [Header("대상 오브젝트 태그")]
    public string targetTag = "Player";
    
    [Header("코드 연결 대상")]
    public GameObject connectedObject;
    public GameObject stageManager;
    
    [Header("유지형 설정")]
    public bool stayPressed = false;

    [Header("Raycast 설정")]
    public float raycastDistance = 1f;
    public LayerMask raycastLayer;
 
    [Header("Dialogue")]
    [SerializeField]private int dialogueNumber;
        
    private DoorController door;
    private Dissolve dissolve;
    private Vector3 initialPosition;
    private bool isPressed = false;
    
    private ConditionalShaderEffect effect;
    //private int objectCount = 0;

    
    
    
    private void Start()
    {
        dissolve = connectedObject.GetComponent<Dissolve>();
        effect= stageManager.GetComponent<ConditionalShaderEffect>();
        
        if (plate == null)
        {
            return;
        }

        initialPosition = plate.localPosition;
    }

    private void TriggerPressed()
    {
        if (GameManager.Instance.currentSceneName == "Tutorial")
        {
            ImprovedSoundManager.Instance.PlaySound3D("PressButton",transform.position);
        }
        dissolve?.ApplyDissolveToAllTargets();
        effect?.TriggerEffect();
        if (dialogueNumber!=0)
        {
            DialogueManager.Instance.TutorialDialogue(dialogueNumber);
        }
            
    }

    private void TriggerReleased()
    {
        if (!stayPressed)
        {
            door?.Deactivate();
        }
    }
    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            objectCount++;

            if (!isPressed)
            {
                isPressed = true;
                TriggerPressed();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            objectCount = Mathf.Max(0, objectCount - 1);

            if (!stayPressed && objectCount == 0)
            {
                isPressed = false;
                TriggerReleased();
            }
        }
    }*/

    private void Update()
    {
        Ray ray = new Ray(transform.position, Vector3.up);
        if(Physics.Raycast(ray, out RaycastHit hit, raycastDistance, raycastLayer))
        {
            if (hit.collider.CompareTag(targetTag))
            {
                if(!isPressed)
                {
                    isPressed = true;
                    TriggerPressed();
                }
            }
        }
        else
        {
            if (isPressed && !stayPressed)
            {
                isPressed = false;
                TriggerReleased();
            }
        }

        Vector3 targetPos = isPressed ? initialPosition + pressedOffset : initialPosition;

        plate.localPosition = Vector3.Lerp(plate.localPosition, targetPos, Time.deltaTime * moveSpeed);
    }
}