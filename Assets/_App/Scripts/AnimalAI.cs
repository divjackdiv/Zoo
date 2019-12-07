using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimalAI : MonoBehaviour
{
    #region Public fields
    [SerializeField] private AnimalStats m_stats;
    public Vector3 m_debugRoamOrigin;
    public float m_AlignementFactor;
    public float m_CohesionFactor;
    public float m_SeperationFactor;
    #endregion
    #region Animation
    private Animator m_animator;
    enum AnimStates { Idle, Walk, Run, Eat, Attack, Die, Special1, Special2 }
    /*
     Animation States :
     0 - Idle
     1 - Walk
     2 - Run
     3 - Eat
     4 - Attack
     5 - Die
     6 - Special1
     7 - Special2
     */
    #endregion
   
    #region Navigation
    private NavMeshAgent m_navAgent;
    private Vector3 m_currentTarget; //where the ai is walking to currently
    private Vector3 m_roamingTarget;
    private bool m_roamingTargetDefined;

    #endregion

    #region Current Stats
    private float m_currentHealth;
    private float m_currentHunger;
    #endregion

    #region Logic
    private GrabbableObject m_grabbable;
    private Rigidbody m_rigidbody;
    private CollisionRelayer m_visionCollider;
    private bool m_dead = false;
    private bool m_isGrabbed;
    #endregion
   
    #region Detected Scene Objects
    public List<AnimalAI> m_sameSpeciesDetected;
    private List<Food> m_foodDetected;
    #endregion
    #region Timers
    private float m_eatTimer; //Timer since we started eating an item
    private float m_attackTimer; //Timer since we started an attack
    private float m_waitTimer;
    #endregion


    void Start()
    {
        m_currentHealth = m_stats.maxHealth;
        m_currentHunger = 0;

        m_rigidbody = GetComponent<Rigidbody>();
        m_animator = GetComponentInChildren<Animator>();
        m_sameSpeciesDetected = new List<AnimalAI>();
        m_foodDetected = new List<Food>();
        m_navAgent = GetComponent<NavMeshAgent>();
        m_grabbable = GetComponentInParent<GrabbableObject>();
        m_visionCollider = GetComponentInChildren<CollisionRelayer>();
        m_visionCollider.OnTriggerEntered += TriggerEntered;
        m_visionCollider.OnTriggerExited += TriggerExited;
        if(TimeManager.Instance != null)
            TimeManager.Instance.OnHourPassed += HourPassed;

        m_attackTimer = 0;
        m_eatTimer = 0;
        m_waitTimer = 0;
        m_navAgent.enabled = true;
        if (m_visionCollider)
            m_visionCollider.GetComponent<SphereCollider>().radius = m_stats.visionRange;

        if (m_grabbable)
        {
            m_grabbable.OnStartInteract += OnGrab;
            m_grabbable.OnStopInteract += OnRelease;
        }

    }
    void Update()
    {
      //  Roam();
        return;
        if (m_dead || m_isGrabbed)
            return;
        if (m_currentHealth <= 0)
        {
            Die();
            return;
        }

        DetectIfThingsAreStillThere();
        if (m_currentHunger > m_stats.minHungerToEat)
            FindFood();
        else if (m_waitTimer > 0)
            WaitAround();
        else
            Roam();
    }
    

    private void FindFood()
    {
        print("Find food");
        if (m_foodDetected.Count > 0)
        {
            m_roamingTargetDefined = false;
            if (m_currentHunger > m_stats.maxHunger)
            {   //Too hungry to run anymore
                if (WalkTo(m_foodDetected[0].transform.position))
                    Eat(m_foodDetected[0]);
            }
            else
            {
                if (RunTo(m_foodDetected[0].transform.position))
                    Eat(m_foodDetected[0]);
            }
        }
        else
            Roam();
    }
    private void Roam()
    {
        bool arrivedAtTarget = false;
        if(m_roamingTargetDefined)
            arrivedAtTarget = WalkTo(m_roamingTarget, false);
        if (m_roamingTargetDefined == false || arrivedAtTarget)
        {
            // if (m_roamingTargetDefined)
            //    m_waitTimer = UnityEngine.Random.Range(1f, 5f);
            float x = UnityEngine.Random.Range(-m_stats.roamingRange, m_stats.roamingRange);
            float z = UnityEngine.Random.Range(-m_stats.roamingRange, m_stats.roamingRange);
            m_roamingTarget = new Vector3(x, transform.position.y, z);
            //m_roamingTarget = transform.position + new Vector3(x, 0, z);
            //m_roamingTarget = GetClosestPointOnNavmesh(m_roamingTarget);
           // m_roamingTarget = goingForward ? transform.position - new Vector3(0, 0, 1) : (transform.position + new Vector3(0, 0, 1));
           // goingForward = !goingForward;
            m_roamingTargetDefined = true;
        }
    }


    #region Detecting things
    void TriggerEntered(Collider other)
    {
        AnimalAI animalAI = other.gameObject.GetComponent<AnimalAI>();
        Food food = other.gameObject.GetComponent<Food>();
        if (animalAI && animalAI.GetSpeciesName() == m_stats.speciesName && m_sameSpeciesDetected.Contains(animalAI) == false)
            m_sameSpeciesDetected.Add(animalAI);
        if (food != null && m_foodDetected.Contains(food) == false)
            m_foodDetected.Add(food);
    }

    void TriggerExited(Collider other)
    {
        AnimalAI animalAI = other.gameObject.GetComponent<AnimalAI>();
        Food food = other.gameObject.GetComponent<Food>();
        if (animalAI && m_sameSpeciesDetected.Contains(animalAI))
            m_sameSpeciesDetected.Remove(animalAI);
        if (food != null && m_foodDetected.Contains(food))
            m_foodDetected.Remove(food);
    }

    void DetectIfThingsAreStillThere()
    {
        for (int i = 0; i < m_sameSpeciesDetected.Count; i++)
        {
            if (m_sameSpeciesDetected[i] == null)
            {
                m_sameSpeciesDetected.RemoveAt(i);
                i--;
                continue;
            }
        }
        for (int i = 0; i < m_foodDetected.Count; i++)
        {
            if (m_foodDetected[i] == null)
            {
                m_foodDetected.RemoveAt(i);
                i--;
                continue;
            }
        }
    }
    #endregion

    #region Actions
    void Attack(AnimalAI enemy)
    {
        if (Vector3.Distance(enemy.transform.position, transform.position) <= m_stats.attackRange)
        {
            m_navAgent.isStopped = true;
            m_animator.SetInteger("State", (int)AnimStates.Attack);
            m_attackTimer -= Time.deltaTime;
            if (m_attackTimer <= 0)
            {
                m_attackTimer = m_stats.durationBetweenAttacks;
                enemy.TakeDamage(m_stats.damage);
            }
        }
        else
        {
            RunTo(enemy.transform.position);
        }
    }

    void WaitAround()
    {
        m_waitTimer -= Time.deltaTime;
        m_roamingTargetDefined = false;
        m_animator.SetInteger("State", (int)AnimStates.Idle);
        m_navAgent.isStopped = true;
    }
    public float m_speedOverride = 1f;
    public float m_rotSpeedOverride = 1f;

    bool WalkTo(Vector3 target, bool goStraightToGoal = true)
    {

        //Vector3 goal = target;
        // if (goStraightToGoal == false)
        //   goal = AdjustPositionWithMovementType(target, m_stats.goalPriority);
        target.y = transform.position.y;
        Vector3 goal = target;
        Vector3 alignment = AdjustPositionWithMovementType(goal, m_AlignementFactor, AnimalStats.GroupMovementType.Alignement);
        Vector3 cohesion = AdjustPositionWithMovementType(goal, m_CohesionFactor, AnimalStats.GroupMovementType.Cohesion);
        Vector3 seperation = AdjustPositionWithMovementType(goal, m_SeperationFactor, AnimalStats.GroupMovementType.Seperation);
        goal = (alignment + cohesion + seperation) / 3;


        if (Vector3.Distance(target, transform.position) <= m_navAgent.stoppingDistance)
            return true;
        m_animator.SetInteger("State", (int) AnimStates.Walk);
        m_navAgent.isStopped = false;
        m_currentTarget = target;

        var targetRotation = Quaternion.LookRotation((goal - transform.position));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_rotSpeedOverride * Time.deltaTime);
        
        //transform.Translate(Vector3.forward * m_stats.walkSpeed * Time.deltaTime * m_speedOverride);
        m_navAgent.SetDestination(goal);
        m_navAgent.speed = m_stats.walkSpeed;
        return false;
    }
   
    bool RunTo(Vector3 target)
    {
        if (Vector3.Distance(target, transform.position) < m_stats.minDistToRun)
            return WalkTo(target);

        Vector3 goal = (target - transform.position).normalized  * m_stats.runSpeed * Time.deltaTime;
        //goal = AdjustPositionWithMovementType(target, m_stats.goalPriority);

        Vector3 alignment = AdjustPositionWithMovementType(goal, m_AlignementFactor, AnimalStats.GroupMovementType.Alignement);
        Vector3 cohesion = AdjustPositionWithMovementType(goal, m_CohesionFactor, AnimalStats.GroupMovementType.Cohesion);
        Vector3 seperation = AdjustPositionWithMovementType(goal, m_SeperationFactor, AnimalStats.GroupMovementType.Seperation);
        goal = (alignment + cohesion + seperation) / 3;
        if (Vector3.Distance(goal, transform.position) <= m_navAgent.stoppingDistance)
            return true;
        m_animator.SetInteger("State", (int)AnimStates.Run);
        m_navAgent.isStopped = false;
        m_currentTarget = goal;
        m_navAgent.SetDestination(goal);
        m_navAgent.speed = m_stats.runSpeed;
        return false;
    }

    private void Eat(Food food)
    {
        m_animator.SetInteger("State", (int)AnimStates.Eat);
        m_navAgent.isStopped = true;
        m_eatTimer += Time.deltaTime;
        if (m_eatTimer >= m_stats.timeToEat)
        {
            if (m_foodDetected.Contains(food))
                m_foodDetected.Remove(food);
            float foodValue = food.GetEaten();
            m_currentHunger = Mathf.Clamp(m_currentHunger - foodValue, 0, m_stats.maxHealth);
            m_eatTimer = 0;
        }

    }

    void Die()
    {
        if (m_dead == false)
        {
            m_dead = true;
            m_navAgent.isStopped = true;
            m_animator.SetInteger("State", (int)AnimStates.Die);
        }
    }

    IEnumerator MobReleased()
    {
        m_rigidbody.isKinematic = false;
        while (IsOnNavMesh() == false)
        {
            if (transform.position.y <= 0)//floor is in 0
            {
                Destroy(gameObject);
            }
            yield return null;
        }
        m_navAgent.enabled = true;
        m_navAgent.isStopped = false;
        m_rigidbody.isKinematic = true;
        m_isGrabbed = false;
    }
    #endregion

    #region Info

    private void HourPassed()
    {
        m_currentHunger = Mathf.Clamp(m_currentHunger - m_stats.hungerIncrementPerHour, 0, m_stats.maxHunger);
        if (m_currentHunger <= 0)
            TakeDamage(1f);
    }

    bool IsOnNavMesh()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 0.5f, NavMesh.AllAreas))
        {
            return true;
        }
        return false;
    }

    Vector3 GetClosestPointOnNavmesh(Vector3 target)
    {
        NavMeshHit hit;
        bool pointFound = NavMesh.SamplePosition(target, out hit, m_stats.roamingRange,1);
        if (pointFound == false)
            return transform.position;
        return hit.position;
    }

    private Vector3 steer;
    private Vector3 AdjustPositionWithMovementType(Vector3 goal, float goalPriority, AnimalStats.GroupMovementType movementType)
    {
        Vector3 movementTypeSteer = goal;
        if (m_sameSpeciesDetected.Count > 0)
        {
            switch (movementType)
            {
                case (AnimalStats.GroupMovementType.Alignement):
                    Vector3 targetSum = Vector3.zero;
                    for (int i = 0; i < m_sameSpeciesDetected.Count; i++)
                    {
                        targetSum += m_sameSpeciesDetected[i].transform.forward;
                    }
                    movementTypeSteer = targetSum / m_sameSpeciesDetected.Count;
                    break;
                case (AnimalStats.GroupMovementType.Cohesion):
                    Vector3 positionSum = Vector3.zero;
                    for (int i = 0; i < m_sameSpeciesDetected.Count; i++)
                    {
                        positionSum += m_sameSpeciesDetected[i].transform.position;
                    }
                    movementTypeSteer = positionSum / m_sameSpeciesDetected.Count;
                    break;
                case (AnimalStats.GroupMovementType.Seperation):
                    Vector3 positionSum2 = Vector3.zero;
                    for (int i = 0; i < m_sameSpeciesDetected.Count; i++)
                    {
                        positionSum2 += m_sameSpeciesDetected[i].transform.position;
                    }
                    Vector3 awaySum = (positionSum2 / m_sameSpeciesDetected.Count);
                    awaySum = (transform.position - awaySum).normalized;
                    movementTypeSteer = transform.position + awaySum;
                    break;
            }
        }
        steer = movementTypeSteer;
        return Vector3.Lerp(goal, movementTypeSteer, goalPriority);//* (1/(m_sameSpeciesDetected.Count + 1))
    }
    #endregion

    #region Publicfunctions
    public void OnGrab(InteractorBase g, InteractorBase.Action action)
    {
        if (action == InteractorBase.Action.Grab)
        {
            m_isGrabbed = true;
            m_navAgent.enabled = false;
            m_animator.SetInteger("State", (int)AnimStates.Idle);
        }
    }


    public void OnRelease(InteractorBase g, InteractorBase.Action action)
    {
        if (action == InteractorBase.Action.Grab)
        {
            StartCoroutine(MobReleased());
        }
    }

    public void TakeDamage(float damage)
    {
        m_currentHealth -= damage;
        if (m_currentHealth <= 0)
        {
            Die();
        }
    }


    public string GetSpeciesName()
    {
        return m_stats.speciesName;
    }

    public Vector3 GetCurrentTarget()
    {
        Vector3 target = m_currentTarget;
        if(m_navAgent == null || m_navAgent.enabled == false || m_waitTimer > 0)
            target = transform.position;
        return target;
    }

    #endregion

    public bool m_showGiwzmos;
    private void OnDrawGizmosSelected()
    {
        if (m_showGiwzmos)
        {
            //Gizmos.DrawWireSphere(transform.position, m_stats.visionRange);
            Debug.DrawLine(transform.position, m_currentTarget, Color.white);
            Debug.DrawLine(transform.position, steer, Color.red);
        }
    }
}
