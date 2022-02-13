using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


/* 
 * This script will manage all of the enemy AI.
 * AI will not compute its own pathfinding, this is to save valuable performance.
 * Jobs will be queued to handle each enemies pathfinding once the enemy is within a specified range.
*/

public class AIManager : MonoBehaviour
{
    [SerializeField] private bool showGrid = false;

    [SerializeField] private Tilemap walkableMap;
    private BoundsInt mapBounds;

    private List<Node> nodes = new List<Node>();

    private List<Node> openNodes = new List<Node>();
    private List<Node> closedNodes = new List<Node>();

    [SerializeField] private List<EnemyAI> enemies = new List<EnemyAI>();
    private List<Vector3> pathVecs = new List<Vector3>();

    private Transform playerTrans;


    private void Awake()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        enemies = UpdateEnemyList();
        mapBounds = GetTileMapBounds();
        GenerateNodes();
    }

    private void Update()
    {
        for(int i = 0; i < enemies.Count; i++)
        {
            if(enemies[i].state == EnemyAI.AIState.Searching || enemies[i].state == EnemyAI.AIState.MovingAndSearching)
            {
                if(!enemies[i].hasPath)
                {
                    enemies[i].path.Clear();
                    enemies[i].SetPath(FindPath(enemies[i].GetTransform(), playerTrans));
                }
            }
        }
    }

    private List<Node> path = new List<Node>(); // For showing the grid when debugging
    private void OnDrawGizmos()
    {
        if (showGrid) 
        {
            Gizmos.color = Color.white;
            foreach (Node node in nodes)
            {
                Gizmos.DrawCube(new Vector3(node.gridX - .5f, node.gridY - .5f, 9), new Vector3(.5f, .5f, .25f));
            }
        }
    }

    // Gets a list of all of the currently spawned enemies in the level and makes them children
    private List<EnemyAI> UpdateEnemyList()
    {
        List<EnemyAI> enemyList = new List<EnemyAI>();
        
        foreach (GameObject e in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemyList.Add(e.GetComponent<EnemyAI>());
        }

        enemyList.Reverse();

        return enemyList;
    }


    public void RefreshEnemyList()
    {
        enemies = UpdateEnemyList();
    }


    private List<Vector3> FindPath(Transform start, Transform target)
    {
        Node startNode = WorldPosToGrid(start);
        Node targetNode = WorldPosToGrid(target);
        
        openNodes.Clear(); // Nodes that may need checked
        closedNodes.Clear(); // Nodes that have already been checked
        openNodes.Add(startNode);

        // While there are still nodes that need to be checked
        while (openNodes.Count > 0)
        {
            Node currentNode = openNodes[0];
            for (int i = 1; i < openNodes.Count; i++) // index starts at 1 since we already know the first node is the start
            {
                // If the total cost is less or the heuritical distance is shorter then we check that node
                if (openNodes[i].fCost < currentNode.fCost || openNodes[i].fCost == currentNode.fCost && openNodes[i].hCost < currentNode.hCost)
                {
                    currentNode = openNodes[i];
                }
            }

            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return pathVecs;
            }

            List<Node> neighbors = new List<Node>();
            if (currentNode != null)
            { 
                neighbors = FindNeighbors(currentNode);
            }

            if (neighbors.Count > 0)
            {
                foreach (Node neighbor in neighbors)
                {
                    if (closedNodes.Contains(neighbor) || neighbor == null)
                    {
                        continue;
                    }

                    int newMovementCostToNeighbor = currentNode.gCost + NodeDistance(currentNode, neighbor);
                    if (newMovementCostToNeighbor < neighbor.gCost || !openNodes.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = NodeDistance(neighbor, targetNode);
                        neighbor.parentNode = currentNode;

                        if (!openNodes.Contains(neighbor))
                        {
                            openNodes.Add(neighbor);
                        }
                    }
                }
            }
        }

        return pathVecs;
    }

    private void RetracePath(Node start, Node target)
    {
        pathVecs.Clear();

        List<Node> _path = new List<Node>();
        Node currentNode = target;

        while(currentNode != start)
        {
            _path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }

        _path.Reverse();
        //path = _path;
        foreach(Node n in _path)
        {
            pathVecs.Add(new Vector3(n.gridX - .5f, n.gridY - .5f, 0));
        }
    }

    private int NodeDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
        {
            return 14*dstY + 10*(dstX - dstY);
        }
        return 14*dstX + 10*(dstY - dstX);
    }

    private List<Node> FindNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        if (node != null)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    var nodeToAdd = WorldPosToGrid(node.gridX + x, node.gridY + y);
                    if (nodeToAdd != null)
                        neighbors.Add(nodeToAdd);
                    else
                        continue;
                }
            }
        }
        return neighbors;
    }

    private void GenerateNodes()
    {
        var offset = 1;

        nodes.Clear(); // Clears existing list if this is a refresh

        print("Generating Nodes");

        for(int x = 0; x < mapBounds.size.x; x++)
        {
            for(int y = 0; y < mapBounds.size.y; y++)
            {
                if(walkableMap.GetTile(new Vector3Int(x - Mathf.Abs(mapBounds.position.x), y - Mathf.Abs(mapBounds.position.y), 0)) != null)
                {
                    nodes.Add(new Node(x - Mathf.Abs(mapBounds.position.x) + offset, y - Mathf.Abs(mapBounds.position.y) + offset));
                }
            }
        }

        print("Finished Generation --- Walkable area: " + nodes.Count);
    }

    // This function is very intensive. Need to be optimized for use with many units.
    private Node WorldPosToGrid(float x, float y)
    {
        foreach(Node n in nodes)
        {
            if(n.gridX == Mathf.RoundToInt(x) && n.gridY == Mathf.RoundToInt(y))
            {
                if (n != null)
                    return n;
                else
                    continue;
            }
        }

        return null;
    }

    private Node WorldPosToGrid(Transform trans)
    {
        foreach (Node n in nodes)
        {
            if (n.gridX == Mathf.RoundToInt(trans.position.x) && n.gridY == Mathf.RoundToInt(trans.position.y))
            {
                if (n != null)
                    return n;
                else
                    continue;
            }
        }

        return null;
    }

    private BoundsInt GetTileMapBounds()
    {
        walkableMap.CompressBounds(); // Makes sure the bounds aren't bigger than the map itself

        BoundsInt bounds = walkableMap.cellBounds;
        print(bounds);

        return bounds;
    }
}

public class Node
{
    public int gridX; // Grid coords
    public int gridY;

    public int gCost; // Distance from start
    public int hCost; // Distance from finish

    public Node parentNode; // Node this one came from

    public Node(int _x, int _y)
    {
        this.gridX = _x;
        this.gridY = _y;
    }

    public int fCost
    {
        get { return gCost + hCost; }
    }
}
