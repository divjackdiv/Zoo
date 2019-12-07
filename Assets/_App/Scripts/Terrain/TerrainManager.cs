using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance { get; private set; }
    
    public InteractorBase m_interactor;
    public Renderer m_terrain;
    public float m_maxDistForInteraction = 0.05f;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public Vector3 GetPosOnTerrain()
    {
        Vector3 interactPos = m_interactor.transform.position;
        Vector3 pointOnTerrain = m_terrain.bounds.ClosestPoint(interactPos);
        return pointOnTerrain;
    }

   public Vector2 GetPosOnTerrainTexSpace(float textureWidth, float textureHeight) { 
        Vector3 pointOnTerrainLoc = (GetPosOnTerrain() - m_terrain.bounds.min);
        Vector2 texPos = new Vector2((pointOnTerrainLoc.x / (m_terrain.bounds.max.x - m_terrain.bounds.min.x)),
            (pointOnTerrainLoc.z / (m_terrain.bounds.max.z - m_terrain.bounds.min.z)));
        texPos.x *= textureWidth;
        texPos.y *= textureHeight;
        return texPos;
    }

    public bool IsDoingPrimaryInteractionOnTerrain()
    {
        float dist = Vector3.Distance(GetPosOnTerrain(), m_interactor.transform.position);
        return dist < m_maxDistForInteraction && m_interactor.IsDoingInteractAction(InteractorBase.Action.Grab);
    } 
    
    public bool IsDoingSecondaryInteractionOnTerrain()
    {
        float dist = Vector3.Distance(GetPosOnTerrain(), m_interactor.transform.position);
        return dist < m_maxDistForInteraction && m_interactor.IsDoingInteractAction(InteractorBase.Action.Interact);
    }
    

    public Vector3 GetBezierPosition(Vector3 pos1, Vector3 pos2, Vector3 pos3, float t, AnimationCurve curve = null) 
    {
        Vector3 ab = Vector3.Lerp(pos1, pos2, t);
        Vector3 bc = Vector3.Lerp(pos2, pos3, t);
        float animT = t;
        if (curve != null)
        {
            animT = curve.Evaluate(t) * t;
        }
        return Vector3.Lerp(ab, bc, animT);
    }

    public Vector3 GetBezierPosition(Vector3 pos1, Vector3 pos2, Vector3 pos4,Vector3 pos5, float t, AnimationCurve curve = null)
    {
        Vector3 ab = Vector3.Lerp(pos1, pos2, t);
       // Vector3 bc = Vector3.Lerp(pos2, pos3, t);
        Vector3 de = Vector3.Lerp(pos4, pos5, t);
        float animT = t;
        if (curve != null)
        {
            animT = curve.Evaluate(t) * t;
        }
        return Vector3.Lerp(ab, de, animT);
    }
}
