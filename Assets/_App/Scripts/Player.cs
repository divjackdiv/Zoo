using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float m_Speed = 3f;
    public Transform m_Head;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MovementManagement();
    }

    void MovementManagement()
    {
        float verticalAxis = Input.GetAxis("Vertical");
        float horizontalAxis = Input.GetAxis("Horizontal");

        Vector3 forward = m_Head.forward;
        forward.y = 0;
        Vector3 right = m_Head.right;
        right.y = 0;
        transform.position += verticalAxis * Time.deltaTime * m_Speed * forward;
        transform.position += horizontalAxis * Time.deltaTime * m_Speed * right;
    }
}
