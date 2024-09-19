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

    private void Start()
    {
        GameObject actorObj = Resources.Load<GameObject>("Actor");
        var size = Environment.Instance.EnvironmentSize;

        for (int i = 0; i < 20; i++)
        {
            GameObject obj = Instantiate(actorObj, new Vector3(Random.Range(0f, size.x), Random.Range(0f, size.y), 0f), Quaternion.identity, Environment.Instance.transform);
            if (Random.value >= 0.5f)
            {
                obj.GetComponent<Actor>().test = true;
            }
        }
    }
}
