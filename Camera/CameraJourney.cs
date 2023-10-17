using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraJourney : MonoBehaviour
{
    public static CameraJourney i;
    
    public bool showCameraTarget;

    public CameraSettings settings = new CameraSettings();
    public CinemachineVirtualCamera virtualCamera;

    public float defaultOrtho = 19f;
    public float speedToScaleRatio = 0.25f;
    public float speedToScaleEdge = 2f;
    public Vector2 baseOffset = Vector2.zero;
    
    List<CameraZone> allZones = new();

    GameObject cameraTarget;
    Vector2 targetPos;
    CinemachineConfiner2D _cinemachineConfiner2D;

    Vector2 interestPointOffset;
    
    void Awake()
    {
        i = this;

        cameraTarget = Instantiate(Resources.Load<GameObject>("camera_target"));
        virtualCamera.Follow = cameraTarget.transform;

        _cinemachineConfiner2D = virtualCamera.AddComponent<CinemachineConfiner2D>();
        virtualCamera.AddExtension(_cinemachineConfiner2D);

        SetDefaultCameraSettings();
    }

    public void Snap()
    {
        cameraTarget.transform.position = Main.player.controller.position;
        for (var i = 0; i < 100; i++)
        {
            UpdateCameraSettings(0.1f);
        }
    }

    public void SetDefaultCameraSettings()
    {
        settings.lockAxisX = false;
        settings.lockAxisY = false;
        settings.targetX = 0;
        settings.targetY = 0;
        settings.lookAhead = true;
        settings.targetOffset = Vector2.zero;
        settings.targetOrthoScale = defaultOrtho;
        settings.confine = null;
        settings.vista = null;
        settings.parallax = 1f;
        settings.interestPoint = null;
        settings.weight = 0f;
    }

    public void RegisterZone(CameraZone cameraZone)
    {
        allZones.Add(cameraZone);
        allZones.Sort((a, b) => b.priority - a.priority);
    }

    void FixedUpdate()
    {
        cameraTarget.GetComponent<SpriteRenderer>().enabled = showCameraTarget;

        FigureOutMyZone();
        UpdateCameraSettings(Time.fixedDeltaTime);
    }

    void FigureOutMyZone()
    {
        foreach (var z in allZones)
        {
            if (z.ContainsPoint(Main.player.controller.position))
            {
                ApplyZone(z);
                return;
            }
        }

        ApplyZone(null);
    }

    void UpdateCameraSettings(float dt)
    {
        const float vistaSpeed = 1f;
        const float scaleSpeed = 2f;
        const float trackOffsetSpeed = 10f;
        const float adjustSpeed = 0.1f; 
        const float adjustSpeedIP = 0.01f;

        var playerVelocity = Main.player.controller.measuredVelocity.magnitude;
        playerVelocity -= speedToScaleEdge;
        if (playerVelocity < 0)
            playerVelocity = 0;
        var speedScale = playerVelocity * speedToScaleRatio;
        var settingsTargetOrthoScale = settings.targetOrthoScale + speedScale;

        if (!Main.player.controller.isGrounded && virtualCamera.m_Lens.OrthographicSize > settingsTargetOrthoScale)
            settingsTargetOrthoScale = virtualCamera.m_Lens.OrthographicSize;
        
        virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, settingsTargetOrthoScale, scaleSpeed * dt);

        var frameTrans = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        var offsetLook = Vector3.right * (3.5f * Main.player.controller.facing);
        if (!settings.lookAhead)
            offsetLook = Vector3.zero;

        frameTrans.m_LookaheadTime = 0.65f;
        frameTrans.m_LookaheadSmoothing = 16;

        frameTrans.m_TrackedObjectOffset += (settings.targetOffset + offsetLook - frameTrans.m_TrackedObjectOffset) * (trackOffsetSpeed * dt);

        var controllerPosition = Main.player.controller.position;
        
        if (settings.interestPoint != null)
        {
            Vector2 ipPos = settings.interestPoint.transform.position;
            Vector2 targetInterestPointPos = Vector3.Lerp(controllerPosition, ipPos, settings.weight);
            interestPointOffset = Vector3.Lerp(interestPointOffset, targetInterestPointPos - controllerPosition, adjustSpeedIP);
        }
        else
        {
            interestPointOffset = Vector3.Lerp(interestPointOffset, Vector3.zero, adjustSpeedIP);
        }
        
        if (settings.lockAxisX)
            targetPos.x = Mathf.Lerp(targetPos.x, settings.targetX, adjustSpeed);
        else
            targetPos.x = Mathf.Lerp(targetPos.x, controllerPosition.x, adjustSpeed);

        if (settings.lockAxisY)
            targetPos.y = Mathf.Lerp(targetPos.y, settings.targetY, adjustSpeed);
        else
            targetPos.y = Mathf.Lerp(targetPos.y, controllerPosition.y, adjustSpeed);

        if (settings.vista != null)
        {
            settings.vista.SetActive(true);
            settings.vista.transform.position = Vector3.Lerp(settings.vista.transform.position, targetPos * settings.parallax, vistaSpeed * dt);
        }
        
        _cinemachineConfiner2D.m_BoundingShape2D = settings.confine;
        cameraTarget.transform.position = targetPos + interestPointOffset + baseOffset;
    }


    void ApplyZone(CameraZone p0)
    {
        SetDefaultCameraSettings();

        if (p0 == null)
            return;

        foreach (var com in p0.cameraSettings)
            com.Apply(p0, settings);
    }
}

public class CameraSettings
{
    public float targetOrthoScale;

    public Vector3 targetOffset;

    public Collider2D confine;

    public bool lookAhead;

    public bool lockAxisX;
    public float targetX;

    public bool lockAxisY;
    public float targetY;
    
    public GameObject vista;
    public float parallax;

    public GameObject interestPoint;
    public float weight;
}