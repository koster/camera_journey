using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraZone : ZoneGizmo
{
    public int priority;
    
    [SerializeReference, SubclassSelector]
    public List<CameraZoneComponent> cameraSettings = new List<CameraZoneComponent>();

    void Start()
    {
        CameraJourney.i.RegisterZone(this);
    }

    public bool ContainsPoint(Vector2 controllerPosition)
    {
        var component = GetComponent<Collider2D>();
        if (component.OverlapPoint(controllerPosition))
            return true;
        return false;
    }

    protected override void ExtaGizmos()
    {
        base.ExtaGizmos();
        
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position, $"Priority: {priority}");
        for (var i = 0; i < cameraSettings.Count; i++)
            UnityEditor.Handles.Label(transform.position + (1+ i) * Vector3.down*2f, $"{cameraSettings[i].GetType()}");
#endif
    }
}

[Serializable]
public abstract class CameraZoneComponent
{
    public virtual void Apply(CameraZone cameraZoneComponent, CameraSettings settings)
    {
    }
}

[Serializable]
public class CameraZoneUseRail : CameraZoneComponent
{
    public CinemachineDollyCart dollyCart;

    public override void Apply(CameraZone cameraZoneComponent, CameraSettings settings)
    {
    }
}

[Serializable]
public class CameraZoneApplyConfiner : CameraZoneComponent
{
    public Collider2D confiner;
    
    public override void Apply(CameraZone cameraZoneComponent, CameraSettings settings)
    {
        settings.confine = confiner;
    }
}

[Serializable]
public class CameraZoneAdjustScale : CameraZoneComponent
{
    public float orthoScale = 10f;
    
    public override void Apply(CameraZone cameraZoneComponent, CameraSettings settings)
    {
        settings.targetOrthoScale = orthoScale;
    }
}

[Serializable]
public class CameraZoneAdjustOffset : CameraZoneComponent
{
    public float offsetX = 0f;
    public float offsetY = 0f;
    
    public override void Apply(CameraZone cameraZoneComponent, CameraSettings settings)
    {
        settings.targetOffset.x = offsetX;
        settings.targetOffset.y = offsetY;
    }
}

[Serializable]
public class CameraZoneFixedY : CameraZoneComponent
{
    public float value;
    public bool offsetZone;
    
    public override void Apply(CameraZone cameraZoneComponent, CameraSettings settings)
    {
        if (offsetZone)
            settings.targetY = cameraZoneComponent.transform.position.y + value;
        else
            settings.targetY = value;
        
        settings.lockAxisY = true;
    }
}

[Serializable]
public class CameraZoneFixedX : CameraZoneComponent
{
    public float value;
    public bool offsetZone;

    public override void Apply(CameraZone cameraZoneComponent, CameraSettings settings)
    {
        if (offsetZone)
            settings.targetX = cameraZoneComponent.transform.position.x + value;
        else
            settings.targetX = value;
        
        settings.lockAxisX = true;
    }
}

[Serializable]
public class CameraZoneVista : CameraZoneComponent
{
    public GameObject target;
    public float parallax = 0.9f;

    public override void Apply(CameraZone cameraZoneComponent, CameraSettings settings)
    {
        settings.vista = target;
        settings.parallax = parallax;
    }
}

[Serializable]
public class CameraZoneInterestPoint : CameraZoneComponent
{
    public float weight = 0.5f;
    public GameObject target;

    public override void Apply(CameraZone cameraZoneComponent, CameraSettings settings)
    {
        settings.weight = weight;
        settings.interestPoint = target;
    }
}