using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicEnemy : MonoBehaviour, EnemyAI
{
    //Starting state (IDLE, PATROL)
    [SerializeField] private States startingState = States.Idle;
    
    //Current state (made it to debug current state easly)
    [SerializeField] private States current = States.Idle;

    //Current health (made it for easier debugging)
    [SerializeField] private int health = 100;

    //Interface implementations
    public States currentState { get; set; } = States.Idle;
    public int MaxHealth { get; set; } = 100;
    public int Health { get; set; } = 100;
    public int HealthForFleeing { get; set; } = 50;
    public float attackRange { get; set; } = 5f;
    public int attackDamage { get; set; } = 25;
    public float attackTime { get; set; } = 1f;
    public float attackTimer { get; set; } = 0;
    public float sightRange { get; set; } = 10f;
    public float moveSpeed { get; set; } = 2f;
    public float fleeingDistance { get; set; } = 3;
    public float healingTime { get; set; } = 7.5f;
    public float InvestingTime { get; set; } = 5f;
    public bool hasSeenPlayer { get; set; } = false;
    public NavMeshAgent navMeshAgent { get; set; }
    public Vector3 playerPosition { get; set; }
    public bool isInvestigating { get; set; }
    public bool isHealing { get; set; }
    public float InvestigatingTimer { get; set; }
    public Vector3 seenPlayerPosition { get; set; }
    public Vector3 targetPosition { get; set; }

    public event Action<OnEnemySawPlayerArgs> OnEnemySawPlayer;
    public event Action<OnEnemyTakeDamageArgs> OnEnemyTakeDamage;
    public event Action<OnEnemyStoppedSeingPlayerArgs> OnEnemyStoppedSeingPlayer;
    public event Action<OnEnemyTryAttackArgs> OnEnemyTryAttack;

    //Array of vector3s for enemy to patrol
    [SerializeField] private Vector3[] patrolArea;

    //Index for patrolling's array
    private int patrolIndex;

    //Original speed, used to store previous speeds when performing investigating / healing
    private float originalSpeed;

    private void Awake()
    {
        if(!navMeshAgent) navMeshAgent = GetComponent<NavMeshAgent>();
        currentState = startingState;
        Enable();
    }

    private void Update()
    {
        //Update current field to make it easier to debug
        current = currentState;

        //Update health field to make it easier to debug
        health = Health;

        //Call OnDie if Health is less or equal to 0 (should happen firstly)
        if (Health <= 0)
        {
            OnDie();
        }

        //If the enemy sees the player then invoke OnEnemySawPlayer
        if (CanSeePoint(playerPosition))
        {
            OnEnemySawPlayer?.Invoke(new OnEnemySawPlayerArgs { currentState = currentState, distanceToPlayer = Vector3.Distance(transform.position, playerPosition), 
                playerPositon = playerPosition });
        }
        else
        {
            //If the enemy stoppedSeeingPlayer, but it has seen it invoke OnEnemyStoppedSeingPlayer
            if (hasSeenPlayer) OnEnemyStoppedSeingPlayer?.Invoke(new OnEnemyStoppedSeingPlayerArgs { currentEnemyState = currentState, lastSeenPosition = playerPosition });
        }

        //Handle the investigation timer
        if(currentState == States.Investigate && isInvestigating)
        {
            InvestigatingTimer += Time.deltaTime;

            if(InvestigatingTimer > InvestingTime)
            {
                moveSpeed = originalSpeed;
                isInvestigating = false;
                InvestigatingTimer = 0;
                currentState = startingState;
            }
        }

        //Check if XZ distance is less than stoppingDistance
        float stoppingDistance = .5f;
        Vector2 a = new Vector2(transform.position.x, transform.position.z);
        Vector2 b = new Vector2(targetPosition.x, targetPosition.z);

        if(Vector3.Distance(a, b) <= stoppingDistance)
        {
            //Call the method if it is
            HandleEnemyCloseToTarget();
        }

        //Handle patrolling
        if (currentState == States.Patrol)
        {
            targetPosition = patrolArea[patrolIndex];
        }

        //Set the navmeshagents destination to targetPosition (making the raycast so the destination won't be in any object / high in air)
        if(Physics.Raycast(new Vector3(targetPosition.x, 1000, targetPosition.z), -transform.up, out RaycastHit hit,float.MaxValue))
        {
            navMeshAgent?.SetDestination(hit.point);
        } else
        {
            //Logs warning if we couldn't find destination
            Debug.LogWarning("Raycast couldn't find place for destination: " + new Vector3(targetPosition.x, 0, targetPosition.z));
        }
    }

    /// <summary>
    /// Enables the script (assigns functions to events)
    /// </summary>
    private void Enable()
    {
        OnEnemySawPlayer += EnemySawPlayer;
        OnEnemyStoppedSeingPlayer += EnemyStoppedSeeingPlayer;
        OnEnemyTakeDamage += EnemyTakeDamageFromPlayer;
        OnEnemyTryAttack += EnemyTryToAttack;
    }

    /// <summary>
    /// Disables the script (dissasigns functions from events)
    /// </summary>
    private void Disable()
    {
        OnEnemySawPlayer -= EnemySawPlayer;
        OnEnemyStoppedSeingPlayer -= EnemyStoppedSeeingPlayer;
        OnEnemyTakeDamage -= EnemyTakeDamageFromPlayer;
        OnEnemyTryAttack -= EnemyTryToAttack;
    }

    /// <summary>
    /// Called when enemy is close to targetPosition, handles based on <c>currentState</c>
    /// </summary>
    private void HandleEnemyCloseToTarget()
    {
        if (currentState == States.Flee) EnemyTryToHeal();
        if (currentState == States.Patrol) patrolIndex = patrolArea.Length - 1 == patrolIndex ? 0 : patrolIndex + 1;
        if (currentState == States.Investigate && !isInvestigating) { isInvestigating = true; InvestigatingTimer = 0; originalSpeed = moveSpeed; moveSpeed = 0; }
        if (currentState == States.Chase) OnEnemyTryAttack?.Invoke(new OnEnemyTryAttackArgs
        {
            currentState = currentState,
            AttackDamage = attackDamage,
            AttackRange = attackRange,
            playerPosition = playerPosition,
            playerScript = FindAnyObjectByType<Player>(),
            enemyPosition = transform.position
        });
    }

    /// <summary>
    /// Called when <c>Health</c> is 0
    /// </summary>
    private void OnDie()
    {
        Disable();
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Called in event when enemy sees player (every tick)
    /// </summary>
    /// <param name="args">Event arguments</param>
    private void EnemySawPlayer(OnEnemySawPlayerArgs args)
    {
        hasSeenPlayer = true;
        seenPlayerPosition = args.playerPositon;

        States lastState = args.currentState;

        if (lastState != States.Flee)
        {
            targetPosition = playerPosition;
            currentState = States.Chase;
        }
    }

    /// <summary>
    ///  Called in event when enemy stops seeing player
    /// </summary>
    /// <param name="args">Event arguments</param>
    private void EnemyStoppedSeeingPlayer(OnEnemyStoppedSeingPlayerArgs args)
    {
        hasSeenPlayer = false;
        seenPlayerPosition = playerPosition;

        States lastState = args.currentEnemyState;

        if (lastState != States.Flee)
        {
            targetPosition = playerPosition;
            currentState = States.Investigate;
        }
    }

    /// <summary>
    /// Called in event when enemy tries to attack
    /// </summary>
    /// <param name="args">Event arguments</param>
    private void EnemyTryToAttack(OnEnemyTryAttackArgs args)
    {
        attackTimer += Time.deltaTime;

        if(attackTimer >= attackTime)
        {
            args.playerScript?.DamagePlayer(args);
            attackTimer = 0;
        }
    }

    /// <summary>
    /// Called in event when enemy takes damage from player
    /// </summary>
    /// <param name="args">Event arguments</param>
    private void EnemyTakeDamageFromPlayer(OnEnemyTakeDamageArgs args)
    {
        int newHP = Health - args.damageDealt;
        Health = newHP;

        if (newHP <= HealthForFleeing)
        {
            currentState = States.Flee;
            Vector3 dir = args.playerPositon - args.enemyPosition;
            targetPosition = playerPosition.normalized - (dir * fleeingDistance);
        }
    }

    /// <summary>
    /// Called when <c>currentState</c> is fleeing and enemy reaches <c>targetPosition</c>
    /// </summary>
    private void EnemyTryToHeal()
    {
        StartCoroutine(StartHealingProcess());
    }

    /// <summary>
    /// Coroutine that stops enemy for <c>healingTime</c> and then heals it to <c>MaxHealth</c>
    /// </summary>
    private IEnumerator StartHealingProcess()
    {
        if (!isHealing)
        {
            isHealing = true;
            originalSpeed = moveSpeed;
            moveSpeed = 0;

            yield return new WaitForSeconds(healingTime);
            
            isHealing = false;
            currentState = startingState;
            Health = MaxHealth;
            moveSpeed = originalSpeed;
        }
    }

    /// <summary>
    /// Called from Player script when it damages the enemy (inacessibility due to interfaces limitations)
    /// </summary>
    /// <param name="args">event arguments</param>
    public void TryTakeDamageFromPlayer(OnEnemyTakeDamageArgs args)
    {
        OnEnemyTakeDamage?.Invoke(args);
    }

    /// <summary>
    /// Can the enemy see the point?
    /// </summary>
    /// <param name="point">The point to check</param>
    /// <returns>Returns true when Raycasting does NOT hit anything</returns>
    private bool CanSeePoint(Vector3 point)
    {
        return !Physics.Raycast(transform.position, point, sightRange);
    }

    //Drawing debug lines
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = CanSeePoint(playerPosition) ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, playerPosition);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, targetPosition);
    }
}
