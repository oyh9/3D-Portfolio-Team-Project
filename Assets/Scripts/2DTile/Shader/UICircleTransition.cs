using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UICircleTransition : MonoBehaviour
{

    public static UICircleTransition Instance {get; set;}

    [Header("참조")]
    [SerializeField] private Image circleImage; // UI 이미지
    [SerializeField] private Material circleMaterial; // 원형 마스크 셰이더가 적용된 재질
    [SerializeField] private Camera mainCamera;
    
    
    [Header("설정")]
    [SerializeField] private float fadeDuration = 2f; // 페이드 시간 (초)
    [SerializeField] private Color fadeColor = Color.black; // 페이드 색상
    [SerializeField] private float edgeSmoothing = 0.01f; // 원형 경계 부드러움
    [SerializeField] private float maxRadius = 1.5f; // 넉넉한 최대 반지름 설정
    
    // 셰이더 프로퍼티 ID (성능 최적화)
    private readonly int _radiusId = Shader.PropertyToID("_Radius");
    private readonly int _centerXId = Shader.PropertyToID("_CenterX");
    private readonly int _centerYId = Shader.PropertyToID("_CenterY");
    private readonly int _smoothEdgeId = Shader.PropertyToID("_SmoothEdge");
    private readonly int _aspectRatioId = Shader.PropertyToID("_AspectRatio");
    
    // 현재 트윈 애니메이션 (취소 가능)
    private Tween _currentTween;
    private Transform playerTransform;
    
    private void Awake()
    {
        Instance = this;
        // 참조가 지정되지 않은 경우 자동으로 찾기
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        if (circleImage == null)
            circleImage = GetComponent<Image>();
            
        if (circleMaterial == null && circleImage != null)
            circleMaterial = new Material(circleImage.material); // 인스턴스 생성
            circleImage.material = circleMaterial; // 새 인스턴스 할당
            
        // 로그 출력
        Debug.Log("UICircleTransition 초기화됨");
    }
    

    private void Start()
    {
        // 초기 설정
        if (circleMaterial != null)
        {
            // 화면 비율 계산
            float aspectRatio = (float)Screen.width / Screen.height;
            circleMaterial.SetFloat(_aspectRatioId, aspectRatio);
            
            // 전체 화면이 보이도록 반지름을 최대값으로 설정
            circleMaterial.SetFloat(_radiusId, maxRadius);
            circleMaterial.SetFloat(_smoothEdgeId, edgeSmoothing);
            circleMaterial.SetFloat(_centerXId, 0.5f);
            circleMaterial.SetFloat(_centerYId, 0.5f);
            
            // 시작할 때는 투명하게
            Color initialColor = fadeColor;
            initialColor.a = 0f;
            circleImage.color = initialColor;
            
            // 디버그 로그
            Debug.Log($"원형 셰이더 초기화 완료 - 화면 비율: {aspectRatio}, 최대 반지름: {maxRadius}");
        }
        else
        {
            Debug.LogError("원형 셰이더 재질이 할당되지 않았습니다!");
        }
    }
    
    private void Update()
    {
        
    }
    
    // 플레이어 위치에 따라 원형 중심점 업데이트
    private void UpdateCircleCenter()
    {
        if(playerTransform == null) return;
        // 플레이어 월드 위치를 뷰포트 좌표로 변환 (0~1 범위)
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(playerTransform.position);
        
        // 셰이더의 중심점 설정
        circleMaterial.SetFloat(_centerXId, viewportPos.x);
        circleMaterial.SetFloat(_centerYId, viewportPos.y);
    }
    
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }
    
    // 화면이 원형으로 줄어듦 (죽을 때)
    public void CircleFadeOut()
    {
        Debug.Log("원형 페이드 아웃 시작");
    
        // 이전 트윈이 있으면 중지
        if (_currentTween != null && _currentTween.IsActive())
            _currentTween.Kill();
        
        // 원의 중심점을 플레이어 위치로 설정
        UpdateCircleCenter();
    
        // 이미지를 불투명하게 설정
        Color targetColor = fadeColor;
        targetColor.a = 1f;
        circleImage.color = targetColor;
    
        // 반지름 값을 maxRadius에서 0으로 애니메이션 (원이 점점 작아져 투명 영역이 줄어듦)
        _currentTween = DOTween.To(() => circleMaterial.GetFloat(_radiusId),
                value => circleMaterial.SetFloat(_radiusId, value),
                0.0f, fadeDuration)
            .SetEase(Ease.InCubic);
    }

    // 화면이 원형으로 커짐 (리스폰될 때)
    public void CircleFadeIn()
    {
        Debug.Log("원형 페이드 인 시작");
    
        // 이전 트윈이 있으면 중지
        if (_currentTween != null && _currentTween.IsActive())
            _currentTween.Kill();
        
        // 원의 중심점을 플레이어 위치로 설정
        UpdateCircleCenter();
    
        // 셰이더의 반지름 값을 0으로 설정 (시작 상태는 전체 화면이 검은색)
        circleMaterial.SetFloat(_radiusId, 0.0f);
    
        // 이미지를 불투명하게 설정
        Color targetColor = fadeColor;
        targetColor.a = 1f;
        circleImage.color = targetColor;
    
        // 반지름 값을 0에서 maxRadius로 애니메이션 (원이 점점 커져 투명 영역이 늘어남)
        _currentTween = DOTween.To(() => circleMaterial.GetFloat(_radiusId),
                value => circleMaterial.SetFloat(_radiusId, value),
                maxRadius, fadeDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => {
                // 페이드 인이 완료되면 이미지를 투명하게 만듦 (화면이 완전히 보이게)
                Color finalColor = fadeColor;
                finalColor.a = 0f;
                circleImage.color = finalColor;
            });
    }
    
    // 화면 크기가 변경될 때 호출 (화면 회전 등)
    private void OnRectTransformDimensionsChange()
    {
        if (circleMaterial != null)
        {
            // 화면 비율 다시 계산
            float aspectRatio = (float)Screen.width / Screen.height;
            circleMaterial.SetFloat(_aspectRatioId, aspectRatio);
        }
    }
}