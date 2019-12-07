using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPainter : MonoBehaviour
{
    public int m_textureWidth;
    public int m_textureHeight;
    public int m_minDrawingRadius;
    public int m_maxDrawingRadius;
    public Material m_terrainMat;
    public float m_scaleMultiplier = 1f;
    public string m_textureName = "_DensityTex";
    public float m_minDistForGrass = 0.01f;
    public Transform m_drawerVisual;

    private Texture2D m_terrainPaint;
    private Vector2 m_lastGrassPos;
    private bool m_lastFrameDrawing;
    private int m_drawingRadius;

    void Start()
    {
        m_terrainPaint = new Texture2D(m_textureWidth, m_textureHeight);
        Color[] colors = new Color[m_textureWidth * m_textureHeight];
        m_drawingRadius = m_minDrawingRadius;
        m_drawerVisual.localScale = m_drawingRadius * Vector3.one;
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.black;
        }
        m_terrainPaint.SetPixels(0, 0, m_textureWidth, m_textureHeight, colors);
    }
    public float dist;
    public float inter;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.G))
        {
            m_drawerVisual.gameObject.SetActive(true);
            if (TerrainManager.Instance.IsDoingPrimaryInteractionOnTerrain())
            {
                Vector2 texPos = TerrainManager.Instance.GetPosOnTerrainTexSpace(m_textureWidth, m_textureHeight);
                dist = Vector2.Distance(m_lastGrassPos, texPos);
                float interpolationsF = inter = m_lastFrameDrawing ? Vector2.Distance(m_lastGrassPos, texPos) / m_minDistForGrass : 1;
                int interpolations = Mathf.RoundToInt(Mathf.Clamp(interpolationsF, 1, 50f));

                for (int i = 0; i < interpolations; i++)
                {
                    Vector2 pos = Vector2.Lerp(texPos, m_lastGrassPos, (i * 1f) / interpolations);
                    PaintTex((int)pos.x, (int)pos.y, m_drawingRadius, Color.white);
                }
                m_terrainMat.SetTexture(m_textureName, m_terrainPaint);
                m_lastFrameDrawing = true;
                m_lastGrassPos = texPos;
            }
            else if (TerrainManager.Instance.IsDoingSecondaryInteractionOnTerrain())
            {
                Vector2 texPos = TerrainManager.Instance.GetPosOnTerrainTexSpace(m_textureWidth, m_textureHeight);
                float interpolationsF = m_lastFrameDrawing ? Vector2.Distance(m_lastGrassPos, texPos) / m_minDistForGrass : 1;
                int interpolations = Mathf.RoundToInt(Mathf.Clamp(interpolationsF, 1, 50f));
                for (int i = 0; i < interpolations; i++)
                {
                    Vector2 pos = Vector2.Lerp(texPos, m_lastGrassPos, (i * 1f) / interpolations);
                    PaintTex((int)pos.x, (int)pos.y, m_drawingRadius, Color.black);
                }
                m_terrainMat.SetTexture(m_textureName, m_terrainPaint);
                m_lastFrameDrawing = true;
                m_lastGrassPos = texPos;
            }
            else
            {               
                m_lastFrameDrawing = false;
            }
            if (Input.mouseScrollDelta.y != 0)
            {
                m_drawingRadius += Mathf.RoundToInt(Input.mouseScrollDelta.y * m_scaleMultiplier * -1f);
                m_drawerVisual.localScale = m_drawingRadius * Vector3.one;
            }
        }
        else
        {
            m_drawerVisual.gameObject.SetActive(false);
            m_lastFrameDrawing = false;
        }
    }


    void PaintTex(int x, int y, int radius, Color col)
    {
        int r = radius / 2;
        Color[] colors = new Color[radius * radius];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = col;
        }
        int posX = Mathf.Clamp(x - r, 0, m_textureWidth);
        int posY = Mathf.Clamp(y - r, 0, m_textureHeight);
        int rXClamped = Mathf.Clamp(radius, 1, (m_textureWidth - posX));
        int rYClamped = Mathf.Clamp(radius, 1, (m_textureHeight - posY));
        m_terrainPaint.SetPixels(posX, posY, rXClamped, rYClamped, colors);
        m_terrainPaint.Apply();
    }

}
