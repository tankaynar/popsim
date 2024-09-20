using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public float foodValue;

    public void SetFood(float value)
    {
        foodValue = value;
        transform.localScale = new Vector3(foodValue, foodValue, 1f);
    }

    public void Eaten()
    {
        Destroy(gameObject);
    }
}
