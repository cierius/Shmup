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
    private int currNode = 0;
    private float nodeTimeOut = 2.0f; // Time until AI re-pathfinds; may be stuck. THIS IS A QUICK FIX.
    private float nodeTimer = 0;

    private int distToPlayer;
    [SerializeField] private int maxRangeSearch = 5; // Default is 10
    private int minRangeSearch = 2;
    private int rangeToAttack;

    private bool inRange = false;
    public bool hasPath = false;

    private Transform playerTrans;
    private Transform trans;
    private Rigidbody2D rb;


    private void Awake()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        trans = GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();
        Physics2D.IgnoreLayerCollision(6, 6, true); //Enemies will ignore other enemies collision - Kinda looks weird but works for now
    }


    private void OnDestroy()
    {
        trans.parent.GetComponent<AIManager>().RefreshEnemyList();
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 0, .5f);
        if (path.Count > 0)
        {
            foreach(Vector3 node in path)
            {
                Gizmos.DrawCube(new Vector3(node.x, node.y, 0), new Vector3(.25f, .25f, .5f));
            }
        }
    }


    void FixedUpdate()
    {
        // 4 States for AI - Idle, Searching for path, Moving, Attacking
        // I want the pathfinding to be put into a job system since many enemies will be pathfinding async 
        if (health > 0)
        {
            if(state == AIState.Idle)
            {
                if(CheckPlayerDistance())
                {
                    state = AIState.Searching;
                }
            }
            else if(state == AIState.Searching) // When state is set to Searching the AIManager will auto pathfind for the unit
            {
                if(path.Count > 1)
                {
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

        if(rb.velocity != Vector2.zero) rb.velocity = Vector2.zero;
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
            nodeTimer += Time.deltaTime;

            float dist = Mathf.Abs(Vector3.Distance(trans.position, path[currNode]));
            if (dist <= .5f)
            {
                if(path.Count - 1 > currNode)
                {
                    currNode++;
                    nodeTimer = 0;
                }
                else if(currNode == path.Count - 1) // Reached destination
                {
                    path.Clear();
                    hasPath = false;
                    currNode = 0;
                    state = AIState.Idle;
                }
            }
            else if(path.Count > 0)
            {
                if(nodeTimer >= nodeTimeOut)
                {
                    path.Clear();
                    hasPath = false;
                    currNode = 0;
                    state = AIState.Idle;
                    nodeTimer = 0;
                }
                else
                    trans.position = Vector3.MoveTowards(trans.position, path[currNode], SPEED/10f * Time.deltaTime);
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


    public void SetPath(List<Vector3> pathToSet)
    {
        path.AddRange(pathToSet);
        hasPath = true;
    }


    public Transform GetTransform()
    {
        return trans;
    }
}
