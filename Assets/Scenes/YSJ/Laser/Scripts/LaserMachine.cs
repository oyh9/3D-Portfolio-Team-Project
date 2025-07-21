using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

namespace Lightbug.LaserMachine
{
    public class LaserMachine : MonoBehaviour
    {
        [System.Serializable]
        struct LaserElement
        {
            public Transform transform;
            public List<LineRenderer> lineRenderers;
            public GameObject sparks;
        }

        private HashSet<ILaserTriggerable> currentHitReceivers = new();
        private HashSet<ILaserTriggerable> previousHitReceivers = new();

        private HashSet<RotatableMirror> currentHitMirrors = new();
        private HashSet<RotatableMirror> previousHitMirrors = new();

        List<LaserElement> elementsList = new List<LaserElement>();

        [Header("External Data")]
        [SerializeField] LaserData m_data;

        [SerializeField] bool m_overrideExternalProperties = true;
        [SerializeField] LaserProperties m_inspectorProperties = new LaserProperties();

        LaserProperties m_currentProperties;

        float m_time = 0f;
        bool m_active = false; // 초기엔 비활성화 상태로 시작
        bool m_assignLaserMaterial;
        bool m_assignSparks;

        [Header("UI Interaction")]
        [SerializeField] TMP_Text sceneLoadText;
        [SerializeField] Image holdGauge;
        [SerializeField] float requiredHoldTime = 1f;

        bool playerInRange = false;
        float holdTime = 0f;
        [SerializeField] AudioSource laserLoopAudioSource;

        void OnEnable()
        {
            m_currentProperties = m_overrideExternalProperties ? m_inspectorProperties : m_data.m_properties;
            m_currentProperties.m_initialTimingPhase = Mathf.Clamp01(m_currentProperties.m_initialTimingPhase);
            m_time = m_currentProperties.m_initialTimingPhase * m_currentProperties.m_intervalTime;

            float angleStep = m_currentProperties.m_angularRange / m_currentProperties.m_raysNumber;
            m_assignSparks = m_data.m_laserSparks != null;
            m_assignLaserMaterial = m_data.m_laserMaterial != null;

            for (int i = 0; i < m_currentProperties.m_raysNumber; i++)
            {
                LaserElement element = new LaserElement
                {
                    transform = new GameObject("LaserEmitter_" + i).transform,
                    lineRenderers = new List<LineRenderer>()
                };

                element.transform.SetParent(transform);
                element.transform.position = transform.position;
                element.transform.rotation = transform.rotation;
                element.transform.Rotate(Vector3.up, i * angleStep);
                element.transform.position += element.transform.forward * m_currentProperties.m_minRadialDistance;

                if (m_assignSparks)
                {
                    element.sparks = Instantiate(m_data.m_laserSparks);
                    element.sparks.transform.SetParent(element.transform);
                    element.sparks.SetActive(false);
                }

                elementsList.Add(element);
            }

            if (sceneLoadText != null)
                sceneLoadText.gameObject.SetActive(false);

            if (holdGauge != null)
            {
                holdGauge.fillAmount = 0f;
                holdGauge.gameObject.SetActive(false);
            }
            if (laserLoopAudioSource != null)
            {
                laserLoopAudioSource.Stop();
            }
                
        }

        void Update()
        {


            // 이전 프레임에는 맞았지만 이번 프레임에는 안 맞은 애들한테 false 보내기
            foreach (var prev in previousHitReceivers)
            {
                if (!currentHitReceivers.Contains(prev))
                {
                    prev.SetHit(false);
                    // Debug.Log("Receiver Lost: " + prev);
                }
            }
            // current → previous로 스왑
            var temp = previousHitReceivers;
            previousHitReceivers = currentHitReceivers;
            currentHitReceivers = temp;
            currentHitReceivers.Clear(); // 스왑 후 비우기

            // 이전 프레임에서 맞았지만, 이번 프레임에는 안 맞은 mirror는 false 처리
            foreach (var prevMirror in previousHitMirrors)
            {
                if (!currentHitMirrors.Contains(prevMirror))
                {
                    prevMirror.SetLaserHit(false);
                }
            }
            var tempMirror = previousHitMirrors;
            previousHitMirrors = currentHitMirrors;
            currentHitMirrors = tempMirror;
            currentHitMirrors.Clear();
            
            if (m_currentProperties.m_intermittent && m_active)
            {
                m_time += Time.deltaTime;
                if (m_time >= m_currentProperties.m_intervalTime)
                {
                    m_active = !m_active;
                    m_time = 0f;
                    return;
                }
            }

            foreach (LaserElement element in elementsList)
            {
                if (m_currentProperties.m_rotate)
                {
                    float rotation = Time.deltaTime * m_currentProperties.m_rotationSpeed;
                    if (!m_currentProperties.m_rotateClockwise)
                        rotation *= -1;
                    element.transform.RotateAround(transform.position, transform.up, rotation);
                }

                if (m_active)
                {
                    SimulateLaserReflection3D(element);
                }
                else
                {
                    foreach (var lr in element.lineRenderers)
                        lr.enabled = false;

                    if (m_assignSparks)
                        element.sparks.SetActive(false);
                }
            }

            // ▶ E 키로 활성화/비활성화 토글
            if (playerInRange)
            {
                if (Input.GetKey(KeyCode.E))
                {
                    holdTime += Time.deltaTime;

                    if (holdGauge != null)
                    {
                        holdGauge.gameObject.SetActive(true);
                        holdGauge.fillAmount = holdTime / requiredHoldTime;
                    }

                    if (holdTime >= requiredHoldTime)
                    {
                        m_active = !m_active;
                        holdTime = 0f;

                        // ▶ 사운드 상태 동기화
                        if (laserLoopAudioSource != null)
                        {
                            if (m_active)
                                laserLoopAudioSource.Play();
                            else
                                laserLoopAudioSource.Stop();
                        }

                        if (holdGauge != null)
                        {
                            holdGauge.fillAmount = 0f;
                            holdGauge.gameObject.SetActive(false);
                        }

                        if (sceneLoadText != null)
                        {
                            sceneLoadText.text = m_active ? "E키를 눌러 비활성화" : "E키를 눌러 활성화";
                        }
                    }
                }
                else
                {
                    holdTime = 0f;
                    if (holdGauge != null)
                    {
                        holdGauge.fillAmount = 0f;
                        holdGauge.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                holdTime = 0f;
                if (holdGauge != null)
                {
                    holdGauge.fillAmount = 0f;
                    holdGauge.gameObject.SetActive(false);
                }
            }
        }

        void SimulateLaserReflection3D(LaserElement element)
        {
            Vector3 startPosition = element.transform.position;
            Vector3 direction = element.transform.forward;

            int maxReflections = m_currentProperties.m_maxReflections;
            int currentReflections = 0;

            foreach (var lr in element.lineRenderers)
                lr.enabled = false;

            while (element.lineRenderers.Count <= maxReflections)
            {
                LineRenderer lr = new GameObject("LineRenderer").AddComponent<LineRenderer>();
                lr.transform.SetParent(element.transform);
                lr.material = m_assignLaserMaterial ? m_data.m_laserMaterial : null;
                lr.receiveShadows = false;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.startWidth = m_currentProperties.m_rayWidth;
                lr.useWorldSpace = true;
                element.lineRenderers.Add(lr);
            }

            for (int i = 0; i <= maxReflections; i++)
            {
                LineRenderer lr = element.lineRenderers[i];
                lr.enabled = true;
                lr.SetPosition(0, startPosition);

                if (Physics.Raycast(startPosition, direction, out RaycastHit hit, m_currentProperties.m_maxRadialDistance, m_currentProperties.m_layerMask))
                {
                    lr.SetPosition(1, hit.point);

                    var receiver = hit.collider.GetComponent<ILaserTriggerable>();
                    if (receiver != null)
                    {
                        if (receiver is LaserReceiver laserReceiver)
                        {
                        laserReceiver.SetHit(true, m_currentProperties.m_laserColor);
                        currentHitReceivers.Add(receiver);
                        }
                    }

                    // ✅ RotatableMirror 처리
                    var mirror = hit.collider.GetComponentInParent<RotatableMirror>();
                    if (mirror != null)
                    {  
                        FinalAreaManager finalArea = FindObjectOfType<FinalAreaManager>();
                        if (finalArea != null)
                        {
                            finalArea.RegisterMirrorHit(mirror);
                        }
                        mirror.SetLaserHit(true);
                        currentHitMirrors.Add(mirror);
                    }
                      
                   
                    if (m_assignSparks)
                    {
                        element.sparks.transform.position = hit.point;
                        element.sparks.transform.rotation = Quaternion.LookRotation(hit.normal);
                        element.sparks.SetActive(true);
                    }

                    // Wall, receiver에 닿으면 반사하지 않고 종료
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Receiver"))
                    {
                        break;
                    }

                    startPosition = hit.point;
                    direction = Vector3.Reflect(direction, hit.normal);
                    currentReflections++;
                }
                else
                {
                    lr.SetPosition(1, startPosition + direction * m_currentProperties.m_maxRadialDistance);
                    if (m_assignSparks)
                        element.sparks.SetActive(false);
                    break;
                }
            }

            for (int i = currentReflections + 1; i < element.lineRenderers.Count; i++)
                element.lineRenderers[i].enabled = false;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;

                if (sceneLoadText != null)
                {
                    sceneLoadText.text = m_active ? "E키를 눌러 비활성화" : "E키를 눌러 활성화";
                    sceneLoadText.gameObject.SetActive(true);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;

                if (sceneLoadText != null)
                    sceneLoadText.gameObject.SetActive(false);
            }
        }
    }
}
