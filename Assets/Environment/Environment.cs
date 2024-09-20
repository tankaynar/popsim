using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Environment : MonoBehaviour
{
    public static Environment Instance;

    public const float MaxSpeed = 100f;
    
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private Transform actorsParent;
    public Transform ActorsParent
    {
        get { return actorsParent; }
    }
    
    [SerializeField] private Transform gridsParent;
    public Transform GridsParent
    {
        get { return gridsParent; }
    }
    
    [SerializeField] private Transform foodsParent;
    public Transform FoodsParent 
    { get { return foodsParent; } }
    
    private static readonly Grid[,] ActorGrid = new Grid[16*2, 9*2];
    public const int GridSize = 25;
    
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
                float grayscale = Random.Range(0f, 0.45f);
                ActorGrid[i, j] = new Grid(new Vector2Int(i, j), new Color(grayscale, grayscale, grayscale));
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
    
    public static Vector2Int GetGridPosition(Vector2 position)
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

    public static Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        Vector2 pos = gridPosition * GridSize;
        return (Vector3)pos;
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
    public Color color;
    public GameObject gameObject;

    public Grid(Vector2Int _gridPosition, Color _color)
    {
        gridPosition = _gridPosition;
        color = _color;
        
        
        GameObject gridObj = Resources.Load<GameObject>("Grid");
        gameObject = GameObject.Instantiate(gridObj,
            Environment.GetWorldPosition(gridPosition) + new Vector3(Environment.GridSize / 2f, Environment.GridSize / 2f, 0f), 
            Quaternion.identity,
            Environment.Instance.GridsParent
            );
        
        gridObj.GetComponent<SpriteRenderer>().color = color;
        gameObject.transform.localScale = new Vector3(Environment.GridSize, Environment.GridSize, 1f);
    }
} 