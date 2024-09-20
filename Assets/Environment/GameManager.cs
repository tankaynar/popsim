using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    private string[] names;

    private Dictionary<string, float> defaultGenes = new Dictionary<string, float>()
    {
        { "Attraction", 1f },
        { "Runaway", 1f },
        { "Dampening", 0.5f },
        { "Wander", .5f },
        { "HungerThreshold", .5f },
        { "ChildMass", 0.3f },
        { "ChildDesireThreshold", 1f },
        { "MatingCooldown", 1f }
    };

    private Dictionary<string, bool> defaultBGenes = new Dictionary<string, bool>()
    {
        { "Herbivore", true }
    };

    private void Start()
    {
        actorObj = Resources.Load<GameObject>("Actor");
        string namesFull = Resources.Load<TextAsset>("names").text;
        names = namesFull.Split("\n");

        for (int i = 0; i < 50; i++)
        {
            CreateRandomActor();
        }
    }

    public void CreateActor(Vector2 position, float mass, Dictionary<string, float> genes, Dictionary<string, bool> bGenes, string actorName)
    {
        GameObject obj = Instantiate(actorObj, position, Quaternion.identity, Environment.Instance.ActorsParent);
        obj.transform.GetChild(0).name = actorName;
        
        Actor actor = obj.GetComponentInChildren<Actor>();
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

        string actorName = parents[0].gameObject.name.Substring(0, parents[0].gameObject.name.Length / 2) +
                           parents[1].gameObject.name.Substring(parents[1].gameObject.name.Length / 2);
        
        CreateActor(position, childMass, childGenes, childBGenes, actorName);
    }

    private void CreateRandomActor()
    {
        var size = Environment.Instance.EnvironmentSize;
        var pos = new Vector3(Random.Range(0f, size.x), Random.Range(0f, size.y), 0f);
        float mass = Random.Range(5f, 25f);

        Dictionary<string, float> randGenes = new Dictionary<string, float>();
        Dictionary<string, bool> randBGenes = new Dictionary<string, bool>();

        foreach (var genePair in defaultGenes)
        {
            randGenes[genePair.Key] = Random.value;
        }
        
        foreach (var genePair in defaultBGenes)
        {
            randBGenes[genePair.Key] = Random.value > 0.5f;
        }
        
        string actorName = names[Random.Range(0, names.Length)];
        
        CreateActor(pos, mass, randGenes, randBGenes, actorName);
    }
}