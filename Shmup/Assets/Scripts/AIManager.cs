using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


// This script will manage all of the enemies AI.
// AI will not compute its own pathfinding, this is for us to save valuable performance.

public class AIManager : MonoBehaviour
{
    public enum NodeType
    {
        Idle,
        Open,
        Closed,
        Start,
        End,
        Unwalkable
    }

    [SerializeField] private Tilemap walkableMap;
    private BoundsInt mapBounds;
    private TileBase[] tiles;
    [SerializeField] private Tilemap unwalkableMap; // IDK if I need this


    private List<Node> nodes = new List<Node>();
    //private List<Node> closedNodes = new List<Node>();

    private List<EnemyAI> enemies = new List<EnemyAI>();

    private GameObject player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        enemies = UpdateEnemyList();
        mapBounds = GetTileMapBounds();
        print(mapBounds.size);
        GenerateNodes();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach(Node node in nodes)
        {
            Gizmos.DrawCube(new Vector3(node.x - .5f, node.y - .5f, 9), new Vector3(1, 1, 1));
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
                    nodes.Add(new Node(x - Mathf.Abs(mapBounds.position.x) + offset, y - Mathf.Abs(mapBounds.position.y) + offset, NodeType.Idle));
                }
            }
        }

        foreach(Node n in nodes)
        {
            print(n.x + " " + n.y + " " + n.nodeType);
        }

        //Trying to figure out how to find nodes
        print(nodes.Contains(new Node(25 - Mathf.Abs(mapBounds.position.x), 5 - Mathf.Abs(mapBounds.position.y), NodeType.Idle))); 

        print("Finished Generation --- Walkable area: " + nodes.Count);
    }

    private BoundsInt GetTileMapBounds()
    {
        walkableMap.CompressBounds(); // Makes sure the bounds aren't bigger than the map itself

        BoundsInt bounds = walkableMap.cellBounds;
        tiles = walkableMap.GetTilesBlock(bounds);
        print(bounds);

        return bounds;
    }
}

public class Node
{
    public AIManager.NodeType nodeType; // Open, Closed, Start, End

    public int x; // Grid coords
    public int y;

    public int g; // Distance from start
    public int h; // Distance from finish
    public int f; // G + H

    public Node parentNode; // Node this one came from

    public Node(int x, int y, AIManager.NodeType nodeType)
    {
        this.x = x;
        this.y = y;
        this.nodeType = nodeType;
    }
}
