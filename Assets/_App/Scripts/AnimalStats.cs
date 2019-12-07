using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Food;

[CreateAssetMenu(fileName = "AnimalStats", menuName = "Mobs/Stat", order = 1)]
public class AnimalStats : ScriptableObject
{
    public enum GroupMovementType { Seperation, Alignement, Cohesion }
    public string speciesName;
    public float maxHealth;

    #region Movement
    public float walkSpeed;
    public float minDistToRun;
    public float runSpeed;
    public float goalPriority;
    public float alignmentFactor;
    public float seperationFactor;
    public float cohesionFactor;
    public float roamingRange;
    #endregion

    #region Eating
    public float maxHunger;
    public float minHungerToEat;
    public float hungerIncrementPerHour;
    public float timeToEat = 2f;
    public FoodType diet;
    #endregion

    #region Combat
    public float chanceOfBeingAggressiveToPreys;
    public float chanceOfBeingAggressiveToSameSpecies;
    public float damage = 1f;
    public float durationBetweenAttacks;
    public float attackRange = 0.1f;
    public float visionRange = 2f;
    #endregion
}


