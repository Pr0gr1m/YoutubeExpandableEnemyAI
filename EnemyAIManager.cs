using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAIManager : MonoBehaviour
{
    public GameObject PlayerGameObject;

    public GameObject[] EnemiesToSimulate;

    private void Update()
    {
        ExecuteEnemyTurn();
    }

    /// <summary>
    /// Get every EnemyAI from <c>EnemiesToSimulate</c> and update their playerPosition
    /// </summary>
    public void ExecuteEnemyTurn()
    {
        foreach(GameObject enemyGameObject in EnemiesToSimulate)
        {
            bool isPlayerDead = PlayerGameObject.GetComponent<Player>().IsDead;
            EnemyAI enemyAI = enemyGameObject.GetComponent<EnemyAI>();
            enemyAI.playerPosition = isPlayerDead ? new Vector3(0, -100, 0) : PlayerGameObject.transform.position;
            if (isPlayerDead) enemyAI.currentState = States.Patrol;
        }
    }
}
