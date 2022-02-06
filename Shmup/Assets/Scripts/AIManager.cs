using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


// This script will manage all of the enemies AI.
// AI will not compute its own pathfinding, this is for us to save valuable performance.

public class AIManager : MonoBehaviour
{

    [SerializeField] private Tilemap walkableMap;
    private BoundsInt mapBounds;

    private List<Node> nodes = new List<Node>();

    private List<Node> openNodes;
    private List<Node> closedNodes;

    private List<EnemyAI> enemies = new List<EnemyAI>();
    public Transform enemyTrans;

    private Transform player;


    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        enemies = UpdateEnemyList();
        mapBounds = GetTileMapBounds();
        GenerateNodes();
    }

    private void Update()
    {
        FindPath(WorldPosToGrid(enemyTrans), WorldPosToGrid(player));
    }

    public List<Node> path = new List<Node>();
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        foreach(Node node in nodes)
        {
            if(path.Contains(node))
            {
                Gizmos.color = Color.black;
            }
            else if(openNodes.Contains(node))
            {
                Gizmos.color = Color.cyan;
            }
            else if (closedNodes.Contains(node))
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.white;
            }
            Gizmos.DrawCube(new Vector3(node.gridX -.5f, node.gridY - .5f, 9), new Vector3(.5f, .5f, .25f));
        }
        
    }

    // Gets a list of all of the currently spawned enemies in the level and makes them children
    private List<EnemyAI> UpdateEnemyList()
    {
        List<EnemyAI> enemyList = new List<EnemyAI>();
        
        foreach (GameObject e in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemies.Add(e.GetComponent<EnemyAI>());
            e.transform.parent = this.transform;
            //print(e.name);
        }

        return enemyList;
    }


    private void FindPath(Node start, Node target)
    {
        openNodes = new List<Node>();
        closedNodes = new List<Node>();
        openNodes.Add(start);


        while(openNodes.Count > 0)
        {
            Node currentNode = openNodes[0];
            for(int i = 1; i < openNodes.Count; i++)
            {
                if(openNodes[i].fCost < currentNode.fCost || openNodes[i].fCost == currentNode.fCost && openNodes[i].hCost < currentNode.hCost)
                {
                    currentNode = openNodes[i];
                }
            }

            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);

            if(currentNode == target)
            {
                RetracePath(start, target);
                return;
            }

            foreach (Node neighbor in FindNeighbors(currentNode))
            {
                if (closedNodes.Contains(neighbor) || neighbor == null)
                {
                    continue;
                }

                int newMovementCostToNeighbor = currentNode.gCost + NodeDistance(currentNode, neighbor);
                if(newMovementCostToNeighbor < neighbor.gCost || !openNodes.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = NodeDistance(neighbor, target);
                    neighbor.parentNode = currentNode;

                    if(!openNodes.Contains(neighbor))
                    {
                        openNodes.Add(neighbor);
                    }
                }

            }

        }
    }

    private void RetracePath(Node start, Node target)
    {
        List<Node> _path = new List<Node>();
        Node currentNode = target;

        while(currentNode != start)
        {
            _path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }

        _path.Reverse();
        path = _path;
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

        for(int x = -1; x < 2; x++)
        {
            for(int y = -1; y < 2; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                neighbors.Add(WorldPosToGrid(node.gridX + x, node.gridY + y));
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
        Node node = null;

        foreach(Node n in nodes)
        {
            if(n.gridX == Mathf.RoundToInt(x) && n.gridY == Mathf.RoundToInt(y))
            {
                //print(n);
                return n;
            }
        }

        return node;
    }

    private Node WorldPosToGrid(Transform trans)
    {
        Node node = null;

        foreach (Node n in nodes)
        {
            if (n.gridX == Mathf.RoundToInt(trans.position.x + .5f) && n.gridY == Mathf.RoundToInt(trans.position.y + .5f))
            {
                //print(n);
                return n;
            }
        }

        return node;
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
