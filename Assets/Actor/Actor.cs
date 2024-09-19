using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    private Vector2 speed;
    private Vector2 acceleration;
    
    private Dictionary<string, float> genes;

    private void Start()
    {
        genes = new Dictionary<string, float>()
        {
            {"Attraction", 1f}
        };
    }

    private void CalculateGeneForces()
    {
        
    }
    
    private void Update()
    {
        acceleration = new Vector2();
        
        // calculate all acting forces
        CalculateGeneForces();
    }
}
