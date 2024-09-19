using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Actor : MonoBehaviour
{
    private Vector2 speed;
    private Vector2 acceleration;
    private float mass;
    private bool desireToEat;
    [SerializeField]
    private float fullness;

    private float energyUsage;
    
    private Grid currentGrid;
    private Vector2Int gridPosition;
    private List<Grid> surroundingGrids;

    private Environment env;

    private SpriteRenderer spriteRenderer;
    
    private Dictionary<string, float> genes = new Dictionary<string, float>()
    {
        {"Attraction", 50f},
        {"Runaway", 50f},
        {"Dampening", 0.5f},
        {"Wander", .5f},
        {"HungerThreshold", .5f}
    };

    private Dictionary<string, bool> bGenes = new Dictionary<string, bool>()
    {
        {"Herbivore", true}
    };

    public bool test;

    private void Start()
    {
        fullness = 1f;
        
        if (!test) bGenes["Herbivore"] = false;
        
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

                if (bGenes["Herbivore"] == false && desireToEat && actor.bGenes["Herbivore"] == true)
                {
                    // attraction force
                    Vector2 attractionForce = direction * genes["Attraction"] / distance.magnitude; // should be sqrMagnitude but whatever
                    Vector2 attractionAcceleration = attractionForce / mass;
                    acceleration += attractionAcceleration;
                }
                
                if (bGenes["Herbivore"] == true && actor.bGenes["Herbivore"] == false)
                {
                    // runaway force
                    Vector2 runawayForce = -direction * genes["Runaway"] / distance.magnitude; // should be sqrMagnitude but whatever
                    Vector2 runawayAcceleration = runawayForce / mass;
                    acceleration += runawayAcceleration;
                }
                
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
        
        // apply movement
        Movement();

        energyUsage = acceleration.magnitude;
        
        // apply hunger
        Hunger();
    }

    private void Hunger()
    {
        if (fullness > 0.2) fullness -= Time.deltaTime * energyUsage;
        if (fullness > 0 && fullness <= 0.2)
        {
            float fatBurn = Time.deltaTime * energyUsage;
            AdjustMass(-fatBurn);
            fullness += fatBurn;
        }
        
        if (fullness <= 0) Die();

        if (fullness <= genes["HungerThreshold"]) desireToEat = true;
        else desireToEat = false;
    }
    
    private void Movement()
    {
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
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        // collision handling
        Actor otherActor = other.gameObject.GetComponent<Actor>();

        if (otherActor.bGenes["Herbivore"] == false)
        {
            // collider is carnivore
            
            if (bGenes["Herbivore"] == true)
            {
                // eaten
                otherActor.Eat(this);
            }
            else
            {
                // bigger mass eats
                if (mass > otherActor.mass)
                {
                    Eat(otherActor);
                } else if (mass < otherActor.mass)
                {
                    otherActor.Eat(this);
                }
            }
        }
        else
        {
            // collider is herbivore
            if (bGenes["Herbivore"] == true)
            {
                // herb - herb collision
                // TODO: reproduction
                return;
            }
            else
            {
                // eats
                Eat(otherActor);
            }
        }
    }

    public void AdjustMass(float adjustment)
    {
        mass += adjustment;
    }


    public void Eat(Actor otherActor)
    {
        if (!desireToEat) return;

        float energyGained = otherActor.mass * 0.5f;
        fullness += energyGained;

        float extraEnergy = fullness - 1f;
        if (extraEnergy > 0)
        {
            fullness -= extraEnergy;
            AdjustMass(extraEnergy * 0.4f); 
            // dont add all extra energy to mass
            // this way it is more energy efficient to eat only when hungry
        }
        
        otherActor.Die();
    }
    
    public void Die()
    {
        env.RemoveFromGrid(this, gridPosition);
        Destroy(gameObject);
    }
}
