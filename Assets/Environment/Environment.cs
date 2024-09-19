using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public static Environment Instance;

    public const float MaxSpeed = 100f;
    
    private void Awake()
    {
        Instance = this;
    }

    private static readonly Grid[,] ActorGrid = new Grid[16, 9];
    private const int GridSize = 10;
    
    public readonly Vector2 EnvironmentSize = new Vector2(GridSize*ActorGrid.GetLength(0), GridSize*ActorGrid.GetLength(1));

    private void CreateSimArea()
    {
        GameObject simAreaObj = Instantiate(Resources.Load<GameObject>("SimArea"), transform, true);
        simAreaObj.transform.localScale = new Vector3(GridSize*(ActorGrid.GetUpperBound(0)+1), GridSize*(ActorGrid.GetUpperBound(1)+1), 1f);
        simAreaObj.transform.localPosition = new Vector3(GridSize*(ActorGrid.GetUpperBound(0)+1)/2f, GridSize*(ActorGrid.GetUpperBound(1)+1)/2f, 0);
    }

    private void InitializeGrid()
    {
        for (int i = 0; i < ActorGrid.GetLength(0); i++)
        {
            for (int j = 0; j < ActorGrid.GetLength(1); j++)
            {
                ActorGrid[i, j] = new Grid {gridPosition = new Vector2Int(i, j)};
            }
        }
    }

    public Grid GetGrid(Vector2Int gridPosition)
    {
        return ActorGrid[gridPosition.x, gridPosition.y];
    }
    
    public Grid AddToGrid(Actor actor, Vector2Int gridPosition)
    {
        Grid grid = ActorGrid[gridPosition.x, gridPosition.y]; 
        grid.Add(actor);
        return grid;
    }

    public void RemoveFromGrid(Actor actor, Vector2Int gridPosition)
    {
        ActorGrid[gridPosition.x, gridPosition.y].Remove(actor);
    }
    
    public Vector2Int GetGridPosition(Vector2 position)
    {
        Vector2Int gridPosition = new Vector2Int
        {
            x = (int)(position.x / GridSize),
            y = (int)(position.y / GridSize)
        };


        if (gridPosition.x >= 0 && gridPosition.y >= 0) return gridPosition;
        
        // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
        Debug.LogError("Grid position is out of range " + gridPosition);
        return Vector2Int.zero;
    }

    public List<Grid> GetSurroundingGrids(Vector2Int gridPosition)
    {
        List<Grid> grids = new List<Grid>();

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int x = i + gridPosition.x;
                int y = j + gridPosition.y;

                if (x >= 0 && y >= 0 && x < ActorGrid.GetLength(0) && y < ActorGrid.GetLength(1))
                    grids.Add(GetGrid(new Vector2Int(x, y)));
            }
        }

        return grids;
    }
    
    private void Start()
    {
        CreateSimArea();
        InitializeGrid();
    }

    private void Update()
    {
        // Debug.Log(ActorGrid[0,0].ToSeparatedString(","));
    }
}

public class Grid : List<Actor>
{
    public Vector2Int gridPosition;    
}

