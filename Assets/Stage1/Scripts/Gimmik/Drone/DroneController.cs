using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    public float rotationSpeed = 120f;
    public float maxSpeed = 10f;
    public float verticalSpeed = 5f;

    private Rigidbody _rigidbody;
    private Vector3 _rotation;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameManager.canPlayerMove=!GameManager.canPlayerMove;
        }
        
        if (GameManager.canPlayerMove)
        {
            return;
        }
        
        HandleRotation();
        HandleVerticalMovement();
        HandleHorizontalMovement();
    }

    void HandleRotation()
    {
        if (Input.GetKey(KeyCode.A))
        {
            _rotation.y -= rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            _rotation.y += rotationSpeed * Time.deltaTime;
        }

        transform.rotation = Quaternion.Euler(0f, _rotation.y, 0f);
    }

    void HandleHorizontalMovement()
    {
        float input = 0f;
        if (Input.GetKey(KeyCode.W)) input = 1f;
        else if (Input.GetKey(KeyCode.S)) input = -1f;

        Vector3 moveDirection = transform.forward * input;
        Vector3 velocity = moveDirection.normalized * maxSpeed;

        // 수직 속도 보존
        velocity.y = _rigidbody.linearVelocity.y;

        // 입력이 없으면 수평 속도 제거
        if (input == 0f)
        {
            velocity.x = 0f;
            velocity.z = 0f;
        }

        _rigidbody.linearVelocity = velocity;
    }

    void HandleVerticalMovement()
    {
        float verticalInput = 0f;
        if (Input.GetKey(KeyCode.Space))
            verticalInput += 1f;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            verticalInput -= 1f;

        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.y = verticalInput * verticalSpeed;
        _rigidbody.linearVelocity = velocity;
    }
}
