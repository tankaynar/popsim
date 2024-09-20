using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private GameObject actorObj;
    
    private void Start()
    {
        actorObj = Resources.Load<GameObject>("Actor");
        
        var size = Environment.Instance.EnvironmentSize;

        for (int i = 0; i < 50; i++)
        {
            GameObject obj = Instantiate(actorObj, new Vector3(Random.Range(0f, size.x), Random.Range(0f, size.y), 0f), Quaternion.identity, Environment.Instance.ActorsParent);
            if (Random.value >= 0.5f)
            {
                
            }
            obj.GetComponent<Actor>().AdjustMass(Random.Range(20f, 50f));
        }
    }

    public void CreateActor(Vector2 position, float mass, Dictionary<string, float> genes, Dictionary<string, bool> bGenes)
    {
        GameObject obj = Instantiate(actorObj, position, Quaternion.identity, Environment.Instance.ActorsParent);
        Actor actor = obj.GetComponent<Actor>();
        actor.InitializeGenes(genes, bGenes);
        actor.AdjustMass(mass);
    }

    public void CreateActorFromParents(Actor[] parents, Vector2 position)
    {
        float childMass = 0f;
        
        float contribution1 = parents[0].genes["ChildMass"] * parents[0].GetMass();
        float contribution2 = parents[1].genes["ChildMass"] * parents[1].GetMass();
        
        parents[0].AdjustMass(-contribution1);
        parents[1].AdjustMass(-contribution2);
        
        childMass = contribution1 + contribution2;

        Dictionary<string, float> childGenes = new Dictionary<string, float>();
        Dictionary<string, bool> childBGenes = new Dictionary<string, bool>();

        foreach (var genePair in parents[0].genes)
        {
            float newValue = (genePair.Value + parents[1].genes[genePair.Key]) / 2;
            newValue += Random.Range(-0.1f, 0.1f);
            childGenes[genePair.Key] = newValue;
        }

        foreach (var genePair in parents[0].bGenes)
        {
            if (Random.value > 0.5f) childBGenes[genePair.Key] = genePair.Value;
            else childBGenes[genePair.Key] = parents[1].bGenes[genePair.Key];
        }
        
        CreateActor(position, childMass, childGenes, childBGenes);
    }
}
