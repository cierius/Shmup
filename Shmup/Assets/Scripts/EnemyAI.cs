using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public bool showPath = false;
    public bool showAttackRadius = false;

    public enum AIState
    {
        Idle,
        Moving,
        MovingAndSearching,
        Attacking,
        Searching
    }
    public AIState state; // Default state is Idle

    [SerializeField] private float SPEED = 10f;
    
    public bool invuln = false;
    public int health = 100;
    public int damage = 2;

    public List<Vector3> path = new List<Vector3>();
    private int currNode = 0;
    private float nodeTimeOut = 2.0f; // Time until AI re-pathfinds; may be stuck. THIS IS A QUICK FIX.
    private float nodeTimer = 0;

    private int distToPlayer = 0;
    [SerializeField] private int maxRangeSearch = 5; // Default is 10
    private int minRangeSearch = 2;
    private float attackRange = 3f;
    private Vector2 playerAttackOriginPos = Vector2.zero;
    private Vector2 playerOrigin = Vector2.zero;
    private LayerMask playerMask;

    private bool inRange = false;
    public bool hasPath = false;

    private Transform playerTrans;
    private CharController playerScript;
    private Transform trans;
    private Rigidbody2D rb;
    private LineRenderer laser;


    private void Awake()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        trans = GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();
        laser = GetComponent<LineRenderer>();
        Physics2D.IgnoreLayerCollision(6, 6, true); //Enemies will ignore other enemies collision - Kinda looks weird but works for now
        playerMask = LayerMask.GetMask("Player");
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<CharController>();
    }


    private void OnDestroy()
    {
        GameObject.Find("AIManager").GetComponent<AIManager>().RefreshEnemyList();
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 0, .5f);
        if (path.Count > 0 && showPath)
        {
            foreach (Vector3 node in path)
            {
                Gizmos.DrawCube(new Vector3(node.x, node.y, 0), new Vector3(.25f, .25f, .5f));
            }
        }

        if (showAttackRadius)
        {
            Gizmos.color = new Color(0, 0, 1, .5f);
            Gizmos.DrawWireSphere(transform.position, maxRangeSearch);
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
                    hasPath = false;
                }
            }
            else if(state == AIState.Moving || state == AIState.MovingAndSearching && !hasPath)
            {
                MoveTowardPlayer();
            }
            else if(state == AIState.MovingAndSearching && hasPath)
            {
                state = AIState.Moving;
            }
            else if(state == AIState.Attacking)
            {
                Attack(playerAttackOriginPos);
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

        if(CheckInAttackRange(playerTrans))
        {
            state = AIState.Attacking;
            hasPath = false;
            currNode = 0;
            path.Clear();
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
                if (nodeTimer >= nodeTimeOut)
                {
                    path.Clear();
                    hasPath = false;
                    currNode = 0;
                    state = AIState.Idle;
                    nodeTimer = 0;
                }
                else
                    trans.position = Vector3.MoveTowards(trans.position, path[currNode], SPEED / 10f * Time.deltaTime);
            }


            if (playerOrigin == Vector2.zero) // If playerOrigin has been set yet
            {
                playerOrigin = playerTrans.position;
            }
            else if (PlayerDeltaOriginalPosition(playerOrigin) > 3) // Checks to see if the player has moved 3 tiles since the last path has been generated
            {
                state = AIState.MovingAndSearching;
                currNode = 0;
                hasPath = false;
                playerOrigin = Vector2.zero;
            }
        }
    }


    private bool CheckInAttackRange(Transform targetPos)
    {
        float dist = Mathf.Abs(Vector3.Distance(new Vector3(trans.position.x, trans.position.y, 0), new Vector3(targetPos.position.x, targetPos.position.y, 0)));
        playerAttackOriginPos = targetPos.position;
        if (dist <= attackRange)
        {
            hasPath = false;
            return true;
        }


        return false;
    }


    float windUpTimer = 0;
    float attackDurTimer = 0;
    float attackDur = .25f;
    private void Attack(Vector2 target)
    {
        windUpTimer += Time.deltaTime;

        if (windUpTimer > 0.5f)
        {
            laser.SetPosition(0, new Vector3(trans.position.x, trans.position.y + .35f, trans.position.z - 1));
            laser.SetPosition(1, new Vector3(trans.position.x, trans.position.y + .25f, trans.position.z - 1));
            laser.enabled = true;


            if (attackDurTimer <= attackDur)
            {
                attackDurTimer += Time.deltaTime;

                Vector2 laserLerp = Vector2.Lerp(target, playerTrans.position, .65f);
                laser.SetPosition(1, laserLerp);

                RaycastHit2D hit = Physics2D.Linecast(trans.position, laserLerp, playerMask);
                if (hit.collider != null)
                {
                    playerScript.ReceiveDamage(damage);
                }
            }
            else 
            {
                windUpTimer = 0;
                attackDurTimer = 0;

                laser.enabled = false;

                if (!CheckInAttackRange(playerTrans))
                    state = AIState.Idle;
            }
        }
    }


    private int PlayerDeltaOriginalPosition(Vector2 origin)
    {
        int delta = Mathf.Abs(Mathf.RoundToInt(Vector3.Distance(origin, playerTrans.position)));

        return delta;
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
