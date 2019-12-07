using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPathPainter : MonoBehaviour
{
    public Material m_pathMat;
    public float m_minDistForWaypoint = 0.02f;
    public int m_maxSmoothness = 20;
    public float m_pathWidth = 0.1f;
    public float m_pathForwardDist = 0.05f;
    public Mesh m_path;

    GameObject m_pathGO;
    MeshFilter m_pathMeshFilter;
    MeshRenderer m_pathMeshRend;
    Mesh m_pathMesh;

    List<PathWaypoint> m_pathWaypoints;
    PathSection m_lastSection;
    float m_lastUV;
    int m_triangleCount;
    bool m_firstRowOfVerts = true;
    bool m_shouldFlipTris = false;

    GameObject m_pathPreviewGO;
    MeshFilter m_pathPreviewMeshFilter;
    MeshRenderer m_pathPreviewMeshRend;
    Mesh m_pathPreview;

    public int m_vertexCount;
    public int m_trisCount;
    public int m_uvsCount;
    public int m_waypointsCount;
    // Start is called before the first frame update
    void Start()
    {
        m_pathWaypoints = new List<PathWaypoint>();
        PrepareNewPath();
    }
    public Transform debugPos0;
    public Transform debugPos1;
    public Transform debugPos2;
    public Vector3 debugNormal;

    void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKey(KeyCode.P) != true)
                return;

        }
        if (TerrainManager.Instance.IsDoingPrimaryInteractionOnTerrain())
        {
            Vector3 pos = TerrainManager.Instance.GetPosOnTerrain();
            ConfirmPathSection(pos);
        }
        else
        {
            Vector3 pos = TerrainManager.Instance.GetPosOnTerrain();
            DrawPreviewPathSection(pos);
        }
       /* else// if (TerrainManager.Instance.IsDoingSecondaryInteractionOnTerrain())
        {
            m_pathGO = null;
        }*/
    }

    private void DrawPreviewPathSection(Vector3 pos)
    {
          CopyMesh();
        //m_pathPreview = m_pathMesh;
        DrawPathSection(pos, m_lastSection, m_pathPreview);
        m_pathMeshFilter.mesh = m_pathPreview;
    }

    private void ConfirmPathSection(Vector3 pos)
    {
      //  m_pathGO.SetActive(true);
      // m_pathPreviewGO.SetActive(false);
        m_pathMeshFilter.mesh = m_pathMesh;
        PathSection section = DrawPathSection(pos, m_lastSection, m_pathMesh);
        if (section != null)
        {
            m_pathWaypoints.AddRange(section.waypoints);
            m_lastSection = section;
            UpdateCounts();
        }
    }

    void PrepareNewPath()
    {
        m_pathGO = new GameObject();
        m_pathGO.name = "Path";
        m_pathGO.transform.parent = transform;
        m_pathMeshFilter = m_pathGO.AddComponent<MeshFilter>();
        m_pathMeshRend = m_pathGO.AddComponent<MeshRenderer>();
        m_pathMesh = new Mesh();
        m_pathMeshFilter.mesh = m_pathMesh;
        m_pathMeshRend.material = m_pathMat;

        m_pathPreviewGO = new GameObject();
        m_pathPreviewGO.name = "PathPreview";
        m_pathPreviewGO.transform.parent = transform;
        m_pathPreviewMeshFilter = m_pathPreviewGO.AddComponent<MeshFilter>();
        m_pathPreviewMeshRend = m_pathPreviewGO.AddComponent<MeshRenderer>();
        m_pathPreview = new Mesh();
        m_pathPreviewMeshFilter.mesh = m_pathPreview;
        m_pathPreviewMeshRend.material = m_pathMat;

        m_uvsCount = 0;
        m_vertexCount = 0;
        m_trisCount = 0;
    }

    private PathSection DrawPathSection(Vector3 sectionEndPos, PathSection lastSection, Mesh path)
    {
        for (int i = 0; i < m_pathWaypoints.Count; i++)
        {
            if (Vector3.Distance(sectionEndPos, m_pathWaypoints[i].position) < m_pathWidth * 2)
            {
                return null;
            }
        }
        //Create a new Section
        PathSection pathSection = new PathSection();
        pathSection.UpdateStartFields(path.vertexCount, path.triangles.Length);

        PathWaypoint lastWaypoint = null;
        Vector3 lastSectionEndPos = sectionEndPos;
        Vector3 lastSectionForward = sectionEndPos;

        if (lastSection != null) //Update the previous path section to get a smooth 
        {
            lastWaypoint = lastSection.waypoints[lastSection.waypoints.Count - 1];
            lastSectionEndPos = lastSection.rawEnd;
            lastSectionForward = lastWaypoint.forward;
            if (lastSectionForward == Vector3.zero)
                lastSectionForward = sectionEndPos - lastSectionEndPos;
        }

        int smoothness =  1; //If this is the start of a path we shouldn't try to smooth it
        float angle = Vector3.SignedAngle(lastSectionForward, sectionEndPos - lastSectionEndPos, Vector3.up);
        
        if (path.vertexCount > 0)
        {
            smoothness = Mathf.Max(Mathf.CeilToInt(angle / 5f),10);
        }

        Vector3 perpendicularV3 = (angle < 0 ? 1 : -1 ) *  GetPerpendicular(Vector3.zero, lastSectionForward) * m_pathWidth;
        for (int i = 1; i < smoothness; i++)
        {
            float t = i / (smoothness * 1f);
            
            float angleLerped = Mathf.Lerp(0, angle, t);
            Vector3 angled = (Quaternion.Euler(0, angleLerped, 0) * perpendicularV3) - perpendicularV3;
            Vector3 waypointPos = lastSectionEndPos + angled;
            if (lastWaypoint == null || waypointPos != lastWaypoint.position)
            {
                PathWaypoint waypoint = DrawPathWaypoint(waypointPos, lastWaypoint, m_path, path);
                pathSection.waypoints.Add(waypoint);
                lastWaypoint = waypoint;
            }
        }
        PathWaypoint finalWaypoint = DrawPathWaypoint(sectionEndPos, lastWaypoint, m_path, path);
        pathSection.waypoints.Add(finalWaypoint);


        pathSection.UpdateEndFields(path.vertices.Length, path.triangles.Length);
        pathSection.rawStart = pathSection.waypoints[0].position;
        pathSection.rawEnd = sectionEndPos;
        return pathSection;
        
    }

    

    PathWaypoint DrawPathWaypoint(Vector3 waypointPos, PathWaypoint lastWaypoint, Mesh originalMesh, Mesh pathMesh)
    {
        int existingVertexCount =  pathMesh.vertexCount;
        Vector3 lastWaypointPos = lastWaypoint == null ? waypointPos : lastWaypoint.position;
        int vertLen = originalMesh.vertices.Length / 2;
        Vector3[] vertices = new Vector3[vertLen];
        Vector2[] uvs = new Vector2[vertLen];
        Vector3[] normals = new Vector3[vertLen];
        List<int> tris = new List<int>();
        Dictionary<int, int> vertPlace = new Dictionary<int, int>();
        //Set Vertices positions
        Vector3 perpendicular  = GetPerpendicular(waypointPos, lastWaypointPos) * m_pathWidth;
        float angle = Vector3.SignedAngle(Vector3.left, perpendicular, Vector3.up);
        float dist = Vector3.Distance(waypointPos, lastWaypointPos);
        int vertCount = 0;
        int ignoredVertsCount = 0;
        for (int i = 0; i < originalMesh.vertices.Length; i++)
        {
            Vector3 localVertPos = originalMesh.vertices[i];
            float uvY = m_lastUV;
            if ((m_firstRowOfVerts && localVertPos.z < 0) || (m_firstRowOfVerts == false && localVertPos.z > 0))
            {
                localVertPos.z = 0;
                vertices[vertCount] = (Quaternion.Euler(0, angle, 0) * localVertPos) + waypointPos;
                uvY = m_lastUV + dist;
                uvs[vertCount] = new Vector2(originalMesh.vertices[i].x, uvY);
                normals[vertCount] = originalMesh.normals[i];
                vertPlace.Add(i, vertCount++);
            }
            else
            {
                vertPlace.Add(i, ignoredVertsCount++);
            }
        }
        m_lastUV = m_lastUV + dist;

        pathMesh.vertices = ArrayAddRangeResize(pathMesh.vertices, vertices);
         //print("pathMesh.vertices  " + pathMesh.vertices.Length + " vertLEn " + vertLen);
        pathMesh.normals = ArrayAddRangeResize(pathMesh.normals, normals, pathMesh.vertices.Length);
        pathMesh.uv = ArrayAddRangeResize(pathMesh.uv, uvs, pathMesh.vertices.Length);
        m_firstRowOfVerts = !m_firstRowOfVerts;

        if (existingVertexCount > 0)
        {
            for (int i = 0; i < originalMesh.triangles.Length / 3; i++)
            {
                int[] triangle = new int[3];
                for (int j = 0; j < 3; j++)
                {
                    int tri = originalMesh.triangles[i * 3 + j];
                    if (originalMesh.vertices[tri].z > 0)
                    {
                        tri = vertPlace[tri];
                    }
                    else
                    {
                        tri = vertPlace[tri] - vertCount;
                    }
                    tri += existingVertexCount;
                    triangle[j] = tri;                
                }

                Vector3 pos1 = originalMesh.vertices[originalMesh.triangles[i * 3]];
                Vector3 pos2 = originalMesh.vertices[originalMesh.triangles[i * 3 + 1]];
                Vector3 pos3 = originalMesh.vertices[originalMesh.triangles[i * 3 + 2]];
                if (ShouldFlipTri(pos1, pos2, pos3, pathMesh.vertices[triangle[0]], pathMesh.vertices[triangle[1]], pathMesh.vertices[triangle[2]]))
                {
                    triangle = FlipTri(triangle);
                }

                tris.AddRange(triangle);
            }
            pathMesh.triangles = ArrayAddRangeResize(pathMesh.triangles, tris.ToArray());
            m_shouldFlipTris = !m_shouldFlipTris;
        }
        Vector3 forward = waypointPos - lastWaypointPos;
        PathWaypoint waypoint = new PathWaypoint(waypointPos, forward);

        if (lastWaypoint != null)
        {
            waypoint.connectedWaypoints.Add(lastWaypoint);
            lastWaypoint.connectedWaypoints.Add(waypoint);
        }
       // print("DrawPathWaypoint " + m_waypointsCount + " finished, vertCount " + m_vertexCount + " tris " + m_trisCount);
        return waypoint;
    }

    //only fliips on Y for now due to global/local pos issues
    bool ShouldFlipTri(Vector3 originalPos1, Vector3 originalPos2, Vector3 originalPos3, Vector3 newPos1, Vector3 newPos2, Vector3 newPos3)
    {
        Vector3 original1 = Vector3.zero;
        Vector3 original2 = originalPos2 - originalPos1;
        Vector3 original3 = originalPos3 - originalPos1;

        Vector3 new1 = Vector3.zero;
        Vector3 new2 = newPos2 - newPos1;
        Vector3 new3 = newPos3 - newPos1;

        Vector3 originalNormal = GetTriNormal(original1, original2, original3);
        Vector3 newNormal = GetTriNormal(new1, new2, new3);
        if (Mathf.Sign(newNormal.y) != Mathf.Sign(originalNormal.y))
        {
            return true;
        }
        return false; // angle > 90 ;
    }

    int[] FlipTri(int[] tri)
    {
        int[] flipped = new int[3];
        flipped[0] = tri[1];
        flipped[1] = tri[0];
        flipped[2] = tri[2];
        return flipped;
    }

    void UpdateCounts()
    {

        m_uvsCount = m_pathMesh.uv.Length;
        m_vertexCount = m_pathMesh.vertices.Length;
        m_trisCount = m_pathMesh.triangles.Length;
        m_waypointsCount = m_pathWaypoints.Count;
    }


    int[] ConnectVertsWithTris(int vertCoord0, int vertCoord1, int vertCoord2, int vertCoord3)
    {
        int[] tris = new int[6];
        tris[0] = vertCoord0;
        tris[1] = vertCoord3;
        tris[2] = vertCoord1;

        tris[3] = vertCoord0;
        tris[4] = vertCoord2;
        tris[5] = vertCoord3;
        return tris;
    }

    T[] ArrayAddRangeResize<T>(T[] arr, T[] toAdd, int arraySizeOverride = -1)
    {
        int arrSize = arraySizeOverride > -1 ? arraySizeOverride : arr.Length + toAdd.Length;
        T[] newArr = new T[arrSize];
        arr.CopyTo(newArr, 0);
        toAdd.CopyTo(newArr, arrSize - toAdd.Length);
        return newArr;
    }

    public Vector3 GetPerpendicular(Vector3 a, Vector3 b)
    {
        Vector3 lastToCenter = a - b;
        Vector2 perpendicular = Vector2.Perpendicular(new Vector2(lastToCenter.x, lastToCenter.z));
        Vector3 perpendicularV3 = (new Vector3(perpendicular.x, 0, perpendicular.y).normalized);
        return perpendicularV3;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(debugPos0.position, debugPos1.position);
        Gizmos.DrawLine(debugPos0.position, debugPos2.position);
        Gizmos.DrawLine(debugPos2.position, debugPos1.position);
        /*if (m_pathWaypoints != null)
        {
            for(int i = 0; i < m_pathWaypoints.Count; i++)
            {
                Gizmos.DrawLine(m_pathWaypoints[i].position, m_pathWaypoints[i].position + Vector3.up);
            }
        }*/
    }

    Vector3 GetTriNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        // Find vectors corresponding to two of the sides of the triangle.
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;

        // Cross the vectors to get a perpendicular vector, then normalize it.
        return Vector3.Cross(side1, side2).normalized;
    }

   /* void CopyMesh(out Mesh copy, Mesh mesh)
    {
        copy = mesh;
        copy.vertices = mesh.vertices;
        copy.triangles = mesh.triangles;
        copy.uv = mesh.uv;
        copy.normals = mesh.normals;
        copy.colors = mesh.colors;
        copy.tangents = mesh.tangents;
    }
    */void CopyMesh()
    {
        m_pathPreview = m_pathMesh;
        m_pathPreview.vertices = m_pathMesh.vertices;
        m_pathPreview.triangles = m_pathMesh.triangles;
        m_pathPreview.uv = m_pathMesh.uv;
        m_pathPreview.normals = m_pathMesh.normals;
        m_pathPreview.colors = m_pathMesh.colors;
        m_pathPreview.tangents = m_pathMesh.tangents;
    }
}

public class PathSection
{
    public Vector3 rawStart;//Updating a section will change the waypoints but this will not change
    public Vector3 rawEnd;//Updating a section will change the waypoints but this will not change

    public List<PathWaypoint> waypoints;
    public int vertIndexStart;
    public int vertIndexEnd;

    public int trisIndexStart;
    public int trisIndexEnd;



    public PathSection()
    {
        waypoints = new List<PathWaypoint>();
        vertIndexStart = int.MaxValue;
        vertIndexEnd = -1;

        trisIndexStart = int.MaxValue;
        trisIndexEnd = -1;
    }

    public void UpdateStartFields(int vertsLen, int trisLen)
    {
        vertIndexStart = Mathf.Min(vertIndexStart, vertsLen);
        trisIndexStart = Mathf.Min(trisIndexStart, trisLen);
    }

    public void UpdateEndFields(int vertsLen, int trisLen)
    {
        vertIndexEnd = Mathf.Max(vertIndexEnd, vertsLen);
        trisIndexEnd = Mathf.Max(trisIndexEnd, trisLen);
    }
}
public class PathWaypoint
{
    public Vector3 position;
    public Vector3 forward;
    public List<PathWaypoint> connectedWaypoints;
    
    public PathWaypoint(Vector3 position, Vector3 forward)
    {
        this.position = position;
        this.forward = forward;
        connectedWaypoints = new List<PathWaypoint>();
    }
}




/*Legacy
 
    PathSection UpdateExistingWaypointVertices(Vector3 nextWaypointPos, PathSection section)
    {
        Vector3 sectionStart = section.rawStart;
        Vector3 sectionEnd = section.rawEnd;
        if (sectionEnd == sectionStart)
            sectionEnd = nextWaypointPos;
        int smoothness = 50;// (section.vertIndexEnd - section.vertIndexStart + 1) / 2;
        Vector3[] vertices = m_pathMesh.vertices;

        Vector3 lastWaypointPos = sectionStart + (sectionStart - sectionEnd);
        for (int i = 0; i < smoothness; i++)
        {
            float t = ((i  + 1) / (smoothness * 1f)) * 0.5f;
            Vector3 waypointPos = TerrainManager.Instance.GetBezierPosition(sectionStart, sectionEnd, nextWaypointPos, t);
            Vector3 perpendicularV3 = GetPerpendicular(waypointPos, lastWaypointPos) * m_pathWidth;

            print("i " + i + " t " + t + " lastWaypointPos " + lastWaypointPos + " waypointPos " + waypointPos+  "  changing " + ((i * 2) + section.vertIndexStart) + " and " +((i * 2) + 1 + section.vertIndexStart));
            vertices[(i * 2) + section.vertIndexStart] = waypointPos + perpendicularV3;
            vertices[(i * 2) + 1 + section.vertIndexStart] = waypointPos - perpendicularV3;
            lastWaypointPos = waypointPos;
            section.waypoints[i].position = waypointPos;
        }
        m_pathMesh.vertices = vertices;
        return section;
    }

 PathWaypoint DrawPathWaypoint(Vector3 waypointPos, PathWaypoint lastWaypoint)
    {
        Vector3 lastWaypointPos = lastWaypoint == null ? waypointPos : lastWaypoint.position;
        Vector3[] vertices = new Vector3[2];
        Vector2[] uvs = new Vector2[2];
        Vector3[] normals = new Vector3[2];
        

        //Set Vertices positions
        Vector3 perpendicularV3 = GetPerpendicular(waypointPos, lastWaypointPos) * m_pathWidth;
        Vector3 posLeft = waypointPos + perpendicularV3;
        Vector3 posRight = waypointPos - perpendicularV3;

        vertices[0] = posLeft;
        vertices[1] = posRight;
        
        //Set Uvs
        float dist = Vector3.Distance(waypointPos, lastWaypointPos);
        float uv = m_vertexCount > 0 ? m_lastUV + dist : 0;
        uvs[0] = new Vector2(0, uv);
        uvs[1] = new Vector2(1, uv);
        m_lastUV = uv;

        //Set Normals
        normals[0] = Vector3.up;
        normals[1] = Vector3.up;


        m_pathMesh.vertices = ArrayAddRangeResize(m_pathMesh.vertices, vertices);
        m_pathMesh.normals = ArrayAddRangeResize(m_pathMesh.normals, normals, m_pathMesh.vertices.Length);
        m_pathMesh.uv = ArrayAddRangeResize(m_pathMesh.uv, uvs, m_pathMesh.vertices.Length);

        //Get triangles
        if (m_vertexCount > 0)
        {
            int[] newTris = ConnectVertsWithTris(m_vertexCount-2, m_vertexCount-1, m_vertexCount, m_vertexCount + 1);
            m_pathMesh.triangles = ArrayAddRangeResize(m_pathMesh.triangles, newTris);
        }

        m_uvsCount = m_pathMesh.uv.Length;
        m_vertexCount = m_pathMesh.vertices.Length;
        m_trisCount = m_pathMesh.triangles.Length;
        m_waypointsCount = m_pathWaypoints.Count;

        Vector3 forward = waypointPos - lastWaypointPos;
        PathWaypoint waypoint = new PathWaypoint(waypointPos, forward);
        if (lastWaypoint != null)
        {
            waypoint.connectedWaypoints.Add(lastWaypoint);
            lastWaypoint.connectedWaypoints.Add(waypoint);
        }
        return waypoint;
    }
     */
