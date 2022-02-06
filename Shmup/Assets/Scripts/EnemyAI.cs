using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public bool invuln = false;
    public int health = 100;

    public enum AIState
    {
        Idle,
        Moving,
        Attacking,
        Searching
    }
    public AIState state = AIState.Idle;

    private bool inRange = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        // 3 States for AI - if ai should move, if in range: attack, if neither then find way to player and then start over.
        // I want the SearchForPlayer to be put into a job system since many enemies will be pathfinding async 
        if (health > 0)
        {
            if (state == AIState.Moving) MoveTowardPlayer();
            else if (state == AIState.Attacking) Attack();
            else if (state == AIState.Searching) SearchForPlayer();
        }
        else
        { 
            if (!invuln)
            {
                Destroy(this.gameObject);
            }
        }

    }

    private void MoveTowardPlayer()
    {
        if(inRange)
        {
            state = AIState.Attacking;
        }
        else 
        { 
            
        }
    }

    private void Attack()
    {

    }

    private void SearchForPlayer()
    {

    }
}
