using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InteractorBase;

public class InteractableSpawner : MonoBehaviour
{
    public GameObject m_spawnable;

    protected InteractableObject m_interactable;
    protected Vector3 m_originalPos;
    protected Quaternion m_originalRot;

    private void Awake()
    {
        m_originalPos = transform.position;
        m_originalRot = transform.rotation;
    }
    void Start()
    {
        m_interactable = GetComponent<InteractableObject>();
        m_interactable.OnStartInteract += Interact;
        m_interactable.OnStopInteract += StopInteracting;
    }

    protected virtual void Interact(InteractorBase interactor, Action action)
    {
    }

    protected virtual void StopInteracting(InteractorBase interactor, Action action)
    {
        if (action == Action.Grab)
        {
            GameObject spawned = Spawn(transform.position, transform.rotation);
            transform.position = m_originalPos;
            transform.rotation = m_originalRot;
        }
    }

    public GameObject Spawn(Vector3 spawnPoint, Quaternion spawnRot)
    {
        GameObject spawned1 = Instantiate(m_spawnable);
        spawned1.transform.position = spawnPoint;
        spawned1.transform.rotation = spawnRot;
        return spawned1;
    }
}
