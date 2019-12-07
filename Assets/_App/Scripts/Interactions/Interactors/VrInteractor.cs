using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OVRInput;

public class VrInteractor : InteractorBase
{

    public Controller m_controller;
    public enum ControllerAction { Trigger = 0, Grip = 1}

    private Collider m_collider;
    private InteractableObject m_collidingInteractable;

    private void Awake()
    {
        m_collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        InteractableObject interactable = other.GetComponent<InteractableObject>();
        if (interactable)
        {
            m_collidingInteractable = interactable;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        InteractableObject interactable = other.GetComponent<InteractableObject>();
        if (interactable != null && interactable == m_collidingInteractable)
        {
            m_collidingInteractable = null;
        }
    }

    public override bool IsDoingInteractAction(Action action)
    {
        return IsDoingControllerAction((ControllerAction)((int)action));
    }

    public override bool IsDoingStopInteractAction(Action action)
    {
        return IsDoingInteractAction(action) == false;
    }

    private bool IsDoingControllerAction(ControllerAction action)
    {
        switch (action)
        {
            case ControllerAction.Trigger:
                return Get(Button.PrimaryIndexTrigger, m_controller);

            case ControllerAction.Grip:
                return Get(Button.PrimaryHandTrigger, m_controller);
        }
        return false;
    }

}
