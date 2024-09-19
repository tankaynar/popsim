using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Actor : MonoBehaviour
{
    [SerializeField]
    private Vector2 speed;
    private Vector2 acceleration;
    private float mass;

    private Grid currentGrid;
    private Vector2Int gridPosition;
    private List<Grid> surroundingGrids;

    private Environment env;

    private SpriteRenderer spriteRenderer;
    
    private Dictionary<string, float> genes = new Dictionary<string, float>()
    {
        {"Attraction", 50f},
        {"Dampening", 0.5f},
        {"Wander", .5f}
    };

    public bool test;

    private void Start()
    {
        if (test) genes["Attraction"] = -50f;
        
        mass = 10f;
        
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (!test) spriteRenderer.color = Color.red;
        
        env = Environment.Instance;
        
        gridPosition = Environment.GetGridPosition(transform.position);
        currentGrid = env.AddToGrid(this, gridPosition);
        surroundingGrids = env.GetSurroundingGrids(gridPosition);
    }

    private void CalculateGeneForces()
    {
        for (var i = 0; i < surroundingGrids.Count; i++)
        {
            Grid grid = surroundingGrids[i];

            for (var j = 0; j < grid.Count; j++)
            {
                Actor actor = grid[j];
                if (actor == this) continue;

                Vector2 distance = actor.transform.position - transform.position;
                Vector2 direction = distance.normalized;

                // attraction force
                Vector2 attractionForce = direction * genes["Attraction"] / distance.magnitude; // should be sqrMagnitude but whatever
                Vector2 attractionAcceleration = attractionForce / mass;
                acceleration += attractionAcceleration;
            }
        }
        
        // wander force
        if (acceleration == Vector2.zero)
        {
            if (wanderTarget == Vector2.zero)
                wanderTarget = new Vector2(Random.Range(0f, env.EnvironmentSize.x), Random.Range(0f, env.EnvironmentSize.y));

            while ((wanderTarget - (Vector2)transform.position).magnitude < 2f)
            {
                wanderTarget = new Vector2(Random.Range(0f, env.EnvironmentSize.x), Random.Range(0f, env.EnvironmentSize.y));
            }

            Vector2 direction = (wanderTarget - (Vector2)transform.position).normalized;
            Vector2 wanderForce = direction * genes["Wander"];
            Vector2 wanderAcceleration = wanderForce / mass;

            acceleration += wanderAcceleration;
        }
    }

    private Vector2 wanderTarget;

    private void UpdateGrid()
    {
        Vector2Int _gridPosition = Environment.GetGridPosition(transform.position);
        if (_gridPosition != gridPosition)
        {
            env.RemoveFromGrid(this, gridPosition);
            currentGrid = env.AddToGrid(this, _gridPosition);
            
            gridPosition = _gridPosition;
            surroundingGrids = env.GetSurroundingGrids(gridPosition);
        }
    }

    private void Render()
    {
        transform.localScale = Vector3.one * mass / 5f;
    }
    
    private void Update()
    {
        // update grid
        // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
        UpdateGrid();
     
        // update visuals
        Render();
        
        acceleration = new Vector2();
        
        // calculate all acting forces
        CalculateGeneForces();
        
        // apply dampening
        speed *= 1f - genes["Dampening"] * Time.deltaTime;
        
        // apply acceleration
        speed += acceleration;
        
        // clamp speed
        if (speed.magnitude > Environment.MaxSpeed) 
            speed = speed.normalized * Environment.MaxSpeed;
        
        // edge detection
        if (transform.position.x + speed.x*Time.deltaTime <= 0 || transform.position.x + speed.x*Time.deltaTime >= env.EnvironmentSize.x) speed.x = -speed.x;
        if (transform.position.y + speed.y*Time.deltaTime <= 0 || transform.position.y + speed.y*Time.deltaTime >= env.EnvironmentSize.y) speed.y = -speed.y;
        
        // apply speed
        transform.Translate(speed * Time.deltaTime);
    }
}
