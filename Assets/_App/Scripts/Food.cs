using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public enum FoodType { Herbivorous, Carnivorous, Omnivorous}
    public float m_foodValue;

    public float GetEaten()
    {
        Destroy(gameObject);
        return m_foodValue;
    }
}
