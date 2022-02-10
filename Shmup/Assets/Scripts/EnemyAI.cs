using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum AIState
    {
        Idle,
        Moving,
        Attacking,
        Searching
    }
    public AIState state; // Default state is Idle

    private const float SPEED = 10f;
    
    public bool invuln = false;
    public int health = 100;

    public List<Vector3> path = new List<Vector3>();
    [SerializeField] public int currNode = 0;

    private int distToPlayer;
    [SerializeField] private int maxRangeSearch = 5; // Default is 10
    private int minRangeSearch = 2;
    private int rangeToAttack;

    private bool inRange = false;
    private bool hasPath = false;

    private Transform playerTrans;
    private Transform trans;
    private Rigidbody2D rb;


    private void Awake()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        trans = GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();
    }


    void FixedUpdate()
    {
        // 4 States for AI - Idle, Searching for path, Moving, Attacking
        // I want the SearchForPlayer to be put into a job system since many enemies will be pathfinding async 
        if (health > 0)
        {
            if(state == AIState.Idle)
            {
                rb.velocity = new Vector2(0, 0);

                if(CheckPlayerDistance())
                {
                    state = AIState.Searching;
                }
            }
            else if(state == AIState.Searching) // When state is set to Searching the AIManager will auto pathfind for the unit
            {
                if(path.Count > 1)
                {
                    hasPath = true;
                    currNode = 0;
                    state = AIState.Moving;
                }
                else
                {
                    state = AIState.Idle;
                }
            }
            else if(state == AIState.Moving)
            {
                MoveTowardPlayer();
            }
            else if(state == AIState.Attacking)
            {
                print("Attacking!");
            }
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
        if(path.Count < 1) state = AIState.Idle;

        if(inRange)
        {
            state = AIState.Attacking;
        }
        else if(hasPath)
        {
            print("Enemy has path and is moving");
            
            float dist = Mathf.Abs(Vector3.Distance(trans.position, path[currNode]));
            if (dist <= .5f)
            {
                if(path.Count - 1 > currNode)
                {
                    currNode++;
                }
                else if(currNode == path.Count - 1) // Reached destination
                {
                    path.Clear();
                    hasPath = false;
                    currNode = 0;
                    rb.velocity = new Vector3(0, 0, 0);
                    state = AIState.Idle;
                }
            }
            else if(path.Count > 0)
            {
                rb.velocity = Vector3.Normalize(new Vector2(path[currNode].x - trans.position.x, path[currNode].y - trans.position.y)) * SPEED * 10f * Time.deltaTime;
            }
        }
    }

    private void Attack()
    {

    }


    private bool CheckPlayerDistance()
    {
        int distToPlayer = Mathf.Abs(Mathf.RoundToInt(Vector3.Distance(trans.position, playerTrans.position)));
        //print(distToPlayer);
        if (distToPlayer < maxRangeSearch && distToPlayer > minRangeSearch)
        {
            return true;
        }
        return false;
    }
}
