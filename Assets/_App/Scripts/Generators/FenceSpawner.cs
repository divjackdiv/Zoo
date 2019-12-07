
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InteractorBase;

public class FenceSpawner : MonoBehaviour
{
    public float m_tableHeight = 0.852f;
    public GameObject m_fencePolePrefab;
    public GameObject m_fenceLinkPrefab;
    
    public float m_poleLength;
    public float m_linkLength;
    public float m_maxDistToSnap;

    private Vector3 m_lastPolePosition;
    public List<PoleConnection> m_connectedPoles;
    private List<GameObject> m_currentLinks;
    private InteractableObject m_interactable;
    

    void Start()
    {
        transform.position = new Vector3(transform.position.x, m_tableHeight, transform.position.z);
        m_lastPolePosition = transform.position;
        if (m_connectedPoles == null)
            m_connectedPoles = new List<PoleConnection>();
        m_currentLinks = new List<GameObject>();
        m_interactable = GetComponent<InteractableObject>();
        m_interactable.OnStartInteract += Interact;
        m_interactable.OnStayInteract += Interact;
        m_interactable.OnStopInteract += StopInteracting;
    }

    protected void Interact(InteractorBase interactor, Action action)
    {
        switch (action)
        {
            case (Action.Interact):
                Extend(interactor);
                break;
            case (Action.Grab):
                Grabbed(interactor);
                break;
        }
    }

    protected void StopInteracting(InteractorBase interactor, Action action)
    {
        switch (action)
        {
            case (Action.Interact):
                StopExtending(interactor);
                break;
            case (Action.Grab):
                Released(interactor);
                break;
        }

    }

    void Extend(InteractorBase interactor)
    {
        m_lastPolePosition = transform.position;
        Vector3 pos = interactor.transform.position;
        pos.y = m_tableHeight;
        GenerateFenceLink(m_currentLinks, m_lastPolePosition, pos);
    }

    void StopExtending(InteractorBase interactor)
    {
        Vector3 pos = interactor.transform.position;
        pos.y = m_tableHeight;
        GenerateFencePole(pos);
    }

    void Grabbed(InteractorBase interactor)
    {
        transform.parent = interactor.transform;
        for (int i = 0; i < m_connectedPoles.Count; i++)
        {
            GenerateFenceLink(m_connectedPoles[i].links, transform.position, m_connectedPoles[i].pole.transform.position);
            m_connectedPoles[i].pole.UpdateLinks(this, m_connectedPoles[i].links);
        }
    }

    void Released(InteractorBase interactor)
    {
        transform.parent = null;
        transform.position = new Vector3(transform.position.x, m_tableHeight, transform.position.z);
    }


    void GenerateFencePole(Vector3 polePosition)
    {
        Collider[] hitColliders = Physics.OverlapSphere(polePosition, m_maxDistToSnap);
        FenceSpawner nearPole = null;
        for (int i = 0; i < hitColliders.Length; i++)
        {
            FenceSpawner fencePole = hitColliders[i].GetComponent<FenceSpawner>();
            if (fencePole) 
                nearPole = fencePole;
        }
        float distFromFirstPole = nearPole  == null ? m_maxDistToSnap + 1f: Vector3.Distance(nearPole.transform.position, polePosition);
        if (distFromFirstPole < m_maxDistToSnap)
        {
            if (ContainsPole(m_connectedPoles, nearPole.gameObject))
            {
                for (int i = 0; i < m_currentLinks.Count; i++)
                {
                    Destroy(m_currentLinks[i].gameObject);
                    m_currentLinks.RemoveAt(i);
                }
            }
            else
            {
                print("Linking existing poles");
                LinkPoles(m_currentLinks, nearPole.transform.position, m_lastPolePosition);
                m_connectedPoles.Add(new PoleConnection(nearPole, m_currentLinks));
                nearPole.GetComponent<FenceSpawner>().ConnectToPole(this, m_currentLinks);
            }
        }
        else
        {
            print("Linking new pole");
            GameObject pole = Instantiate(m_fencePolePrefab);

            pole.transform.position = polePosition;
            FenceSpawner poleSpawner = pole.GetComponent<FenceSpawner>();

            m_lastPolePosition = polePosition;
            m_connectedPoles.Add(new PoleConnection(poleSpawner, m_currentLinks));
            poleSpawner.ConnectToPole(this, m_currentLinks);
        }
        m_currentLinks.Clear();
    }

    void UpdateLinks(FenceSpawner from, List<GameObject> links)
    {
        for (int i = 0; i < m_connectedPoles.Count; i++)
        {
            if (m_connectedPoles[i].pole == from)
                m_connectedPoles[i].links = links;
        }
    }

   // void GenerateFenceLink(List<GameObject> links, Vector3 fenceStart, FenceSpawner fenceEnd)
    void GenerateFenceLink(List<GameObject> links, Vector3 fenceStart, Vector3 fenceEnd)
    {
        float dist = Vector3.Distance(fenceEnd, fenceStart);
        int idealFenceNb = Mathf.CeilToInt(dist/m_linkLength);
        int actualFenceNb = links.Count;
        for(int i = 0; i < Mathf.Abs(idealFenceNb - actualFenceNb); i++)
        {

            if (idealFenceNb > actualFenceNb)
            {
                GameObject link = Instantiate(m_fenceLinkPrefab);
                links.Add(link);
            }
            else
            {
                Destroy(links[links.Count - 1]);
                links.RemoveAt(links.Count - 1);
            }
        }
        LinkPoles(links, fenceStart, fenceEnd);
    }

    void LinkPoles(List<GameObject> links, Vector3 pole1, Vector3 pole2)
    {        
        float poleDist = Vector3.Distance(pole1, pole2);
        Vector3 vecToPole2 = (pole2 - pole1).normalized;
        Vector3 linkPos = pole1+ (vecToPole2 * m_poleLength);
        for (int i = 0; i < links.Count; i++)
        {
            if (links[i] == null)
            {
                links.RemoveAt(i);
                continue;
            }
            if ((i + 1) == links.Count)
            {
                float distLastLinkToPole2 = Vector3.Distance(linkPos, pole2);
                Vector3 scale = Vector3.one;
                scale.z *= Mathf.Clamp01(distLastLinkToPole2 / m_linkLength);
                links[i].transform.localScale = scale;
            }
            else
            {
                links[i].transform.localScale = Vector3.one;
            }
            links[i].transform.position = linkPos;
            Debug.DrawLine(pole1, pole2);

            links[i].transform.LookAt(pole2);
            linkPos += (vecToPole2 * m_linkLength); 
        }
    }

    public void ConnectToPole(FenceSpawner otherPole, List<GameObject> links)
    {
        if (m_connectedPoles == null)
            m_connectedPoles = new List<PoleConnection>();        
        m_connectedPoles.Add(new PoleConnection(otherPole, links));
    }


    bool ContainsPole(List<PoleConnection> tupleList, GameObject pole)
    {
        for (int i = 0; i < tupleList.Count; i++)
        {
            if (tupleList[i].pole.GetInstanceID() == pole.GetInstanceID())
                return true;
        }
        return false;
    }
}


public class PoleConnection {
    public FenceSpawner pole;
    public List<GameObject> links;

    public PoleConnection(FenceSpawner pole)
    {
        this.pole = pole;
        this.links = new List<GameObject>();
    }


    public PoleConnection(FenceSpawner pole, List<GameObject> links)
    {
        this.pole = pole;
        this.links = new List<GameObject>(links);
    }
}