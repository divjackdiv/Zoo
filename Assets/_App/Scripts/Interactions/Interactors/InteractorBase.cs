using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractorBase : MonoBehaviour
{

    public enum Action { Interact = 0, Grab = 1}
    public LayerMask m_includeLayers;
    public InteractableObject m_interactableObject;
    public List<Action> m_possibleActions;
    private Dictionary<Action, bool> m_interacting;

    private void Awake()
    {
        m_interacting = new Dictionary<Action, bool>();
        for(int i = 0; i < m_possibleActions.Count; i++)
        {
            m_interacting.Add(m_possibleActions[i], false);
        }
    }
    public void Update()
    {
        for (int i = 0; i < m_possibleActions.Count; i++)
        {
            Action action = m_possibleActions[i];
            if (IsDoingInteractAction(action))
            {
                AttemptInteracting(action);
                break;
            }
            else if (IsDoingStopInteractAction(action))
            {
                if (m_interacting[action])
                {
                    StopInteracting(action);
                    break;
                }
            }
        }
    }

    public virtual bool IsDoingInteractAction(Action action)
    {
        return false;
    }

    public virtual bool IsDoingStopInteractAction(Action action)
    {
        return false;
    }

    protected virtual void AttemptInteracting(Action action)
    {
    }

    protected void StopInteracting(Action action)
    {
        m_interacting[action] = false;
        m_interactableObject.StopInteracting(action);
        m_interactableObject = null;
    }

    protected void Interact(InteractableObject interactable, Action action)
    {
        m_interacting[action] = interactable.Interact(this, action);
        if(m_interacting[action])
            m_interactableObject = interactable;
    }
}
