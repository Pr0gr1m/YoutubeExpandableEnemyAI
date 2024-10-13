using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface EnemyAI
{
    //Current states enum
    States currentState 
    { get; set; }

    //Maximum health
    int MaxHealth
    { get; set; }

    //Actual health
    int Health
    { get; set; }
    
    //Health neeeded for fleeing
    int HealthForFleeing
    { get; set; }

    //How much damage does enemy deal
    int attackDamage
    { get; set; }

    //Range in order to attack
    float attackRange 
    { get; set; }
    //Time since investigating started
    float attackTimer
    { get; set; }

    //Time beetwen the attacks
    float attackTime
    { get; set; }

    //Range where enemy detects player
    float sightRange
    { get; set; }

    //How much enemy moves
    float moveSpeed 
    { get; set; }
    //How far should enemy flee to heal
    float fleeingDistance
    { get; set; }

    //How much time does it take for enemy to heal
    float healingTime
    { get; set; }

    //Has the enemy seen the player?
    bool hasSeenPlayer 
    { get; set; }

    //Is the enemy currently investigating (States.Investigating is the state of enemy even if it isnt at seenPlayerPosition yet)
    bool isInvestigating
    { get; set; }
    //Is the enemy currently healing (Fixes healing coroutines to be runned every frame)
    bool isHealing
    { get; set; }

    //Time since investigating started
    float InvestigatingTimer
    { get; set; }
    
    //Time needed to end investigation
    float InvestingTime
    { get; set; }

    //Player position, updated each tick
    Vector3 playerPosition
    { get; set; }

    //Last position where player was seen by enemy
    Vector3 seenPlayerPosition
    { get; set; }

    //Target position where the navmeshagent will try to go
    Vector3 targetPosition
    { get; set; }

    //Reference to navmeshagent
    NavMeshAgent navMeshAgent
    { get; set; }

    //Events declaration
    public event Action<OnEnemySawPlayerArgs> OnEnemySawPlayer;
    public event Action<OnEnemyStoppedSeingPlayerArgs> OnEnemyStoppedSeingPlayer;
    public event Action<OnEnemyTakeDamageArgs> OnEnemyTakeDamage;
    public event Action<OnEnemyTryAttackArgs> OnEnemyTryAttack;

    //Enable, disable functions where functions are assigned / removed from events
    private void Enable() { }
    private void Disable() { }
}

public struct OnEnemyTryAttackArgs
{
    public States currentState;
    public Vector3 playerPosition;
    public Player playerScript;
    public Vector3 enemyPosition;
    public float AttackRange;
    public int AttackDamage;
}

public struct OnEnemySawPlayerArgs
{
    public States currentState;
    public Vector3 playerPositon;
    public float distanceToPlayer;
}

public struct OnEnemyTakeDamageArgs
{
    public States currentEnemyState;
    public Vector3 playerPositon;
    public Vector3 enemyPosition;
    public int damageDealt;
    public int oldHealth;
}

public struct OnEnemyStoppedSeingPlayerArgs
{
    public States currentEnemyState;
    public Vector3 lastSeenPosition;
}

public enum States
{
    Idle, Patrol, Investigate, Chase, Flee
}