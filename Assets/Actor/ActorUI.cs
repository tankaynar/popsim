using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ActorUI : MonoBehaviour
{
    [SerializeField] TMP_Text mainText;

    public bool mateDesire;
    public float fullness;
    public bool desireToEat;
    public float energyUsage;
    public float mass;
    
    private Actor actor;
    
    public void Initialize(Actor _actor)
    {
        actor = _actor;
    }

    private void Update()
    {
        mainText.text = "wants to mate: " + (mateDesire ? "yes" : "no") + "\n" +
                        "wants to eat: " + (desireToEat ? "yes" : "no") + "\n" +
                        "fullness: " + fullness + "\n" +
                        "energy usage: " + energyUsage + "\n" +
                        "mass: " + mass + "\n" +
                        actor.gameObject.name;
    }

    public void Press()
    {
        mainText.gameObject.SetActive(!mainText.gameObject.activeSelf);
    }
}
