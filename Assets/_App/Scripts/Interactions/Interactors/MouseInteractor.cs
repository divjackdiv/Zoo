using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class MouseInteractor : InteractorBase
{
    public Transform m_floorPlane;
    public List<MouseControlMapping> m_controls;
    public enum MouseAction { leftMouse = 0, rightMouse = 1}

    public override bool IsDoingInteractAction(Action action)
    {
        for(int i = 0; i < m_controls.Count; i++)
        {
            if(m_controls[i].action == action)
                return IsDoingMouseAction(m_controls[i].control);
        }
        return false;
    }

    public override bool IsDoingStopInteractAction(Action action)
    {
        return IsDoingInteractAction(action) == false;
    }

    private bool IsDoingMouseAction(MouseAction action)
    {
        switch (action)
        {
            case MouseAction.leftMouse:
                return Input.GetMouseButtonDown(0) || Input.GetMouseButton(0);

            case MouseAction.rightMouse:
                return Input.GetMouseButtonDown(1) || Input.GetMouseButton(1);
        }
        return false;
    }

    protected override void AttemptInteracting(Action action)
    {
        if (m_interactableObject)
        {
            Interact(m_interactableObject, action);
        }
        else
        {
            RaycastHit hit;
            Ray ray = UtilityMono.MainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 10f, m_includeLayers))
            {
                InteractableObject interactable = hit.collider.gameObject.GetComponent<InteractableObject>();
                if (interactable)
                {
                    Interact(interactable, action);
                }
            }
        }
    }
    
    public void LateUpdate()
    {
        transform.position = MousePosOnPlane(m_floorPlane);
    }

    Vector3 MousePosOnPlane(Transform plane)
    {
        Vector3 planeNormal = new Vector3(0f, 1f, 0f);
        Vector3 planeCenter = plane.position;

        Ray cameraRay = UtilityMono.MainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 lineOrigin = cameraRay.origin;
        Vector3 lineDirection = cameraRay.direction;

        Vector3 difference = planeCenter - lineOrigin;
        float denominator = Vector3.Dot(lineDirection, planeNormal);
        float t = Vector3.Dot(difference, planeNormal) / denominator;

        Vector3 planeIntersection = lineOrigin + (lineDirection * t);
        planeIntersection.y += 0.05f;
        return planeIntersection;
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
            Gizmos.DrawWireSphere(MousePosOnPlane(m_floorPlane), 0.1f);
    }

}

[Serializable]
public struct MouseControlMapping
{
    public InteractorBase.Action action;
    public MouseInteractor.MouseAction control;
}
