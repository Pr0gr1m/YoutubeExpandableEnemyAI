using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int Health;
    public bool IsDead { get; private set; } = false;

    public BasicEnemy EnemyAI;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.J))
        {
            EnemyAI.TryTakeDamageFromPlayer(new OnEnemyTakeDamageArgs { damageDealt = 25, enemyPosition = EnemyAI.transform.position, playerPositon = transform.position });
        }
    }

    public void DamagePlayer(OnEnemyTryAttackArgs args)
    {
        Health -= args.AttackDamage;

        if (Health <= 0) IsDead = true;
    }
}
