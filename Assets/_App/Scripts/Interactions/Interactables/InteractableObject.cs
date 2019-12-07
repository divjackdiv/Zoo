using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using static InteractorBase;

public class InteractableObject : MonoBehaviour
{
    public CommonDelegates.InteractableDelegate OnStartInteract;
    public CommonDelegates.InteractableDelegate OnStayInteract;
    public CommonDelegates.InteractableDelegate OnStopInteract;

    protected InteractorBase m_interactor;
    protected bool m_isInteracting;

    public virtual bool Interact(InteractorBase interactor, Action action)
    {
        if (m_isInteracting)
        {
            if (OnStayInteract != null)
                OnStayInteract(interactor, action);
        }
        else
        {
            if (OnStartInteract != null)
                OnStartInteract(interactor, action);
            m_isInteracting = true;
        }
        m_interactor = interactor;
        return true;
    }
    
    public virtual bool StopInteracting(Action action)
    {
        if (m_isInteracting)
        {
            if (OnStopInteract != null)
                OnStopInteract(m_interactor, action);
            m_isInteracting = false;
            m_interactor = null;
            return true;
        }
        return false;
    }
}
