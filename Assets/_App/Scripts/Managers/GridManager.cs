using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int m_gridWidth = 200;
    public int m_gridLength = 200;

    private GridCell[,] m_grid;

    void Awake()
    {
        m_grid = new GridCell[m_gridWidth, m_gridLength];
    }
    
    public bool ClaimCell(int x, int y)
    {
        if (CheckCell(x, y))
            return false;
        m_grid[x, y].isTaken = true;
        return true;
    }

    public bool ReleaseCell(int x, int y)
    {
        if (CheckCell(x, y) == false)
            return false;
        m_grid[x, y].isTaken = false;
        return true;
    }

    public bool CheckCell(int x, int y)
    {
        return m_grid[x, y].isTaken;
    }

    public List<GridCell> GetPath(Vector2Int from, Vector2Int to)
    {
        List<GridCell> path = new List<GridCell>();
        return path;
    }
}


public struct GridCell
{
    public bool isTaken;
}