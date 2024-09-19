using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public static Environment Instance;

    private void Awake()
    {
        Instance = this;
    }

    private Actor[,] actorGrid = new Actor[16, 9];
    private const int gridSize = 10;

    private void CreateSimArea()
    {
        GameObject simAreaObj = Instantiate(Resources.Load<GameObject>("SimArea"));
        simAreaObj.transform.localScale = new Vector3(gridSize*(actorGrid.GetUpperBound(0)+1), gridSize*(actorGrid.GetUpperBound(1)+1), 1f);
        simAreaObj.transform.SetParent(transform);
        simAreaObj.transform.localPosition = Vector3.zero;
    }
    
    private void Start()
    {
        CreateSimArea();
    }
}
