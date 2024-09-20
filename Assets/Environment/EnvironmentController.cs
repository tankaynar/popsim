using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnvironmentController : MonoBehaviour
{
    [SerializeField] private float foodGenerationRate = .2f;
    
    private float foodGenerationTimer = 0f;

    private GameObject foodPrefab;

    private void Start()
    {
        foodPrefab = Resources.Load("Food") as GameObject;
    }

    private void Update()
    {
        foodGenerationTimer -= Time.deltaTime;

        if (foodGenerationTimer <= 0)
        {
            foodGenerationTimer = 1f;
            if (Random.value <= foodGenerationRate) GenerateRandomFood();
        }
    }

    private void GenerateRandomFood()
    {
        Vector2 position = new Vector2(Random.Range(0f, Environment.Instance.EnvironmentSize.x), Random.Range(0f, Environment.Instance.EnvironmentSize.y));
        float foodValue = Random.value * 2.5f;
        GenerateFood(position, foodValue);
    }
    
    private void GenerateFood(Vector2 position, float foodValue)
    {
        Vector3 rot = new Vector3(0f, 0f, Random.Range(0f, 360f));
        
        GameObject food = Instantiate(foodPrefab, position, Quaternion.Euler(rot), Environment.Instance.FoodsParent);
        food.GetComponent<Food>().SetFood(foodValue);
    }
}
