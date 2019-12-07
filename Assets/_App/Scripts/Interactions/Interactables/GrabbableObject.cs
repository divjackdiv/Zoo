using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using static InteractorBase;

public class GrabbableObject : InteractableObject
{
    private Transform m_originalParent;

    private void Awake()
    {
        m_originalParent = transform.parent;
    }

    public override bool Interact(InteractorBase interactor, Action action)
    {
        if (action == Action.Grab)
        {
            if (base.Interact(interactor, Action.Grab))
            {
                transform.parent = interactor.transform;
                return true;
            }
        }
        return false;
    }

    public override bool StopInteracting(Action action)
    {
        if (action == Action.Grab)
        {
            if (base.StopInteracting(Action.Grab))
            {
                transform.parent = m_originalParent;
                return true;
            }
        }
        return false;
    }
}
