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
    [SerializeField]
    private float mass;
    private bool desireToEat;
    private bool desireToReproduce;
    private float fullness;

    [SerializeField]
    private float matingCooldown;

    private float energyUsage;
    
    private Grid currentGrid;
    private Vector2Int gridPosition;
    private List<Grid> surroundingGrids;

    private Environment env;

    private SpriteRenderer spriteRenderer;
    
    public Dictionary<string, float> genes = new Dictionary<string, float>()
    {
        {"Attraction", 50f},
        {"Runaway", 50f},
        {"Dampening", 0.5f},
        {"Wander", .5f},
        {"HungerThreshold", .5f},
        {"ChildMass", 0.3f},
        {"ChildDesireThreshold", 20f},
        {"MatingCooldown", 60f}
    };

    public Dictionary<string, bool> bGenes = new Dictionary<string, bool>()
    {
        {"Herbivore", true}
    };

    [SerializeField] private EdgeCollider2D leftCollider;
    [SerializeField] private EdgeCollider2D centerCollider;
    [SerializeField] private EdgeCollider2D rightCollider;

    private void Start()
    {
        fullness = 1f;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        env = Environment.Instance;
        
        gridPosition = Environment.GetGridPosition(transform.position);
        currentGrid = env.AddToGrid(this, gridPosition);
        surroundingGrids = env.GetSurroundingGrids(gridPosition);
    }

    public void InitializeGenes(Dictionary<string, float> _genes, Dictionary<string, bool> _bGenes)
    {
        genes = _genes;
        bGenes = _bGenes;
  
        if (!bGenes["Herbivore"]) spriteRenderer.color = Color.red;
        
        matingCooldown = genes["MatingCooldown"];
    }

    public float GetMass()
    {
        return mass;
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

                bool herbivoreEatingAttraction =
                    bGenes["Herbivore"] == false && desireToEat && actor.bGenes["Herbivore"] == true;

                bool matingAttraction =
                    bGenes["Herbivore"] && actor.bGenes["Herbivore"] && desireToReproduce && actor.desireToReproduce;
                
                
                if (herbivoreEatingAttraction || matingAttraction)
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
        
        // food attraction
        if (foodDetection != Vector3Int.zero)
        {
            Vector3 leftVector = Quaternion.Euler(0, 0, 45) * transform.right;
            Vector3 rightVector = Quaternion.Euler(0, 0, -45) * transform.right;
            Vector3 centerVector = transform.right;
            
            Vector3 direction = leftVector*foodDetection.x + centerVector*foodDetection.y + rightVector*foodDetection.z;
            Vector2 foodForce = direction * 10f;
            Vector2 foodAcceleration = foodForce / mass;

            acceleration += foodAcceleration;
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

        // apply rotation
        HandleRotation();
        
        energyUsage = acceleration.magnitude;
        
        // apply hunger
        Hunger();
        
        // children handling
        Children();
    }

    private void HandleRotation()
    {
        Vector2 rotationVector = speed.normalized;
        transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, rotationVector));
    }

    private void Children()
    {
        if (matingCooldown > 0f) matingCooldown -= Time.deltaTime;
        if (matingCooldown < 0f) matingCooldown = 0f;
        
        desireToReproduce = (mass >= genes["ChildDesireThreshold"]) && matingCooldown == 0f;
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
        transform.position = transform.position + (Vector3)(speed * Time.deltaTime);
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
                if (desireToReproduce && otherActor.desireToReproduce)
                {
                    // fun times
                    matingCooldown = genes["MatingCooldown"];
                    otherActor.matingCooldown = otherActor.genes["MatingCooldown"];
                    
                    desireToReproduce = false;
                    otherActor.desireToReproduce = false;
                    GameManager.Instance.CreateActorFromParents(new []{ this, otherActor }, transform.position);
                }
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

    private Vector3Int foodDetection;
    
    private void OnTriggerStay2D(Collider2D other)
    {
        Vector3Int touching = new Vector3Int();
        if (leftCollider.IsTouching(other))
        {
            touching.x = 1;
        }
        if (centerCollider.IsTouching(other))
        {
            touching.y = 1;
        }
        if (rightCollider.IsTouching(other))
        {
            touching.z = 1;
        }

        if (other.gameObject.CompareTag("Food"))
        {
            // food detected!
            foodDetection = touching;
        }
    }
}
