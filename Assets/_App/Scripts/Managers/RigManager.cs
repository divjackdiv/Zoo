using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigManager : MonoBehaviour
{
    public GameObject m_AndroidRig;
    public GameObject m_UnityRig;
    void Awake()
    {
#if UNITY_EDITOR
        m_AndroidRig.SetActive(false);
        m_UnityRig.SetActive(true);
#else
        m_AndroidRig.SetActive(true);
        m_UnityRig.SetActive(false);
#endif

    }

}
