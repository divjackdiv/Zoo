using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionRelayer : MonoBehaviour
{
    public delegate void CollisionEvent(Collision other);
    public CollisionEvent OnColliderEntered;
    public CollisionEvent OnColliderStayed;
    public CollisionEvent OnColliderExited;

    
    public delegate void TriggerEvent(Collider other);
    public TriggerEvent OnTriggerEntered;
    public TriggerEvent OnTriggerStayed;
    public TriggerEvent OnTriggerExited;

    private void OnTriggerEnter(Collider other)
    {
        if (OnTriggerEntered != null)
            OnTriggerEntered(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (OnTriggerStayed != null)
            OnTriggerStayed(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (OnTriggerExited != null)
            OnTriggerExited(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (OnColliderEntered != null)
            OnColliderEntered(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (OnColliderStayed != null)
            OnColliderStayed(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (OnColliderExited != null)
            OnColliderExited(collision);
    }
}
