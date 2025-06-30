#define VISUALISE
#define DEBUG

using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;
using DelaunatorSharp;
using DelaunatorSharp.Unity;
using UnityEngine.Experimental.GlobalIllumination;
using Unity.Jobs;
using UnityEditor.Experimental.GraphView;
using Unity.Cinemachine;



public class Room
{
    public int id, x, y, width, height, area; // grid coords (not pixels)

    public Room(int id, int x, int y, int width, int height)
    {
        this.id = id;
        this.x = x; this.y = y;
        this.width = width; this.height = height;
        this.area = this.width * this.height;
    }

    public Vector2 GetVector()
    {
        //Returns a vector of x and y position of the TOP LEFT

        return new Vector2(x, y);
    }

    //public Vector2 GetCentreVector()
    //{
    //    //Returns a vector of x and y position of the TOP RIGHT
    //    return new Vector2(x + width/2, y-height/2);
    //}

    //public void UpdateXY(Vector2 xy)//, bool centre = true)
    //{
    //    DIDNT WORK, not 100% sure why. Converison problem.
    //    this.x = (int)xy.x;
    //    this.x = (int)xy.y;

    //    //if (centre == false)
    //    //{
    //    //    this.x = (int)xy.x;
    //    //    this.x = (int)xy.y;
    //    //}
    //    //else
    //    //{
    //    //    this.x = (int)xy.x - height/2;
    //    //    this.x = (int)xy.y + width/2;
    //    //}
    //}
        

    public void OutputDetails()
    {
#if DEBUG
        UnityEngine.Debug.Log($"Room {id}: x:{x} y: {y}, width:{width}, height: {height}, area: {area}");
#endif
    }

    //public bool Overlaps(Room other)
    //{
    //    return !(x + width <= other.x || other.x + other.width <= x ||
    //             y + height <= other.y || other.y + other.height <= y);
    //}
}


public class RoomManager : MonoBehaviour
{
    

    //public Vector2 topLeft = new Vector2(-8f, 4f);
    public Vector2 topLeft = new Vector2(0f, 0f);

    private int prefabSize = 100;

    public int gridWidth =  100;
    public int gridHeight = 100;

    private int worldWidth = 1000;//8 - (-8);
    private int worldHeight = 1000;//4 - (-4);

    public int roomCount = 10;
    //public int minRoomSize = 5;
    //public int maxRoomSize = 15;
    public float extraEdgePercentage;

    public float keepRoomPercentage;

    public GameObject platformPrefab; // assign this in the Inspector
    public GameObject playerPrefab;
    public GameObject cameraThirdPerson;

    int centrePointX;
    int centrePointY;

    float cellWidth;
    float cellHeight;

    public List<Room> rooms = new List<Room>();

    // Store references to the rooms by name
    private Dictionary<string, GameObject> roomObjects = new Dictionary<string, GameObject>();

    void Start()
    {

#if VISUALISE

        cellWidth = prefabSize;// worldWidth / (float)gridWidth;
        cellHeight = prefabSize;//  worldHeight / (float)gridHeight; //100 is the size of octagon prefab

        worldWidth = gridWidth *  prefabSize;
        worldHeight = gridHeight * prefabSize;

        GameObject lineManager = new GameObject("lineManager");
#endif

        centrePointX = gridWidth / 2;
        centrePointY = gridHeight / 2;

        StartCoroutine(GenerateDungeon());

    }

    void Update()
    {
//        // Detect spacebar press
//        if (Input.GetKeyDown(KeyCode.Space))
//        {

//            //CreateLine("TestLine", GridToWorld(rooms[0].GetVector()), GridToWorld(rooms[1].GetVector()), Color.black);

//            //CreateLine("TestLine", GridToWorld(new Vector2(0,0)), GridToWorld(new Vector2(gridWidth, gridHeight)), Color.black);
//#if DEBUG
//            UnityEngine.Debug.Log("Spacebar Pressed 1");
//#endif
//        }
    }

 
    
    IEnumerator GenerateDungeon()
    {
        // Step 1: Generating rooms in a circle in the centre of the space.
        yield return StartCoroutine(GenerateRooms());

        // Step 2: Seperating the rooms evenly.
        // This is done by generating a physics object for each one, and letting them slide apart, and then updating the values based on the simulation.
        yield return StartCoroutine(WaitForPhysicsToSettleAndUpdateRooms());

        // Step 3: Selecting main rooms.
        // Pick the top X% top rooms where X is keepRoomPercentage.
        yield return StartCoroutine(RoomFilter()); 

        // Step 4: Triangulation

        yield return StartCoroutine(FindCorridors());

        // Step 5: Populate Corridors

        // Step 5: Populate Rooms

        yield return StartCoroutine(SpawnRooms());

        yield return StartCoroutine(SpawnPlayer());
    }

    //STEP 1 FUNCITONS

    //int[,] RoomSizes = { { 4, 4 }, { 6, 6 }, { 10, 10 } };//, { 4, 8 }, { 8, 4 }, { 10, 10 } };
    int[,] RoomSizes = { { 1, 1 }, { 2, 2 }};//, { 4, 8 }, { 8, 4 }, { 10, 10 } };

    Room GenerateRoom()
    {
        int roomSizeIndex = Random.Range(0, RoomSizes.GetLength(0));
        int w = RoomSizes[roomSizeIndex,0];
        int h = RoomSizes[roomSizeIndex, 1];

        int[] coord = GetRandomPointInCircle(gridWidth / 10);

        int x = centrePointX + coord[0];
        int y = centrePointY + coord[1];

        return new Room(rooms.Count, x, y, w, h);
    }

    IEnumerator GenerateRooms()
    {

        while (rooms.Count < roomCount)// && attempts < maxAttempts)
        {
            Room newRoom = GenerateRoom();

            rooms.Add(newRoom);
#if DEBUG
            UnityEngine.Debug.Log($"Added Room: ({newRoom.x},{newRoom.y}) size {newRoom.width}x{newRoom.height} area {newRoom.area}");
#endif
        }

        yield break;
    }

    static int[] GetRandomPointInCircle(float radius)
    {
        float t = 2 * Mathf.PI * Random.value; // Random angle
        float u = Random.value + Random.value;
        float r = (u > 1) ? 2 - u : u;

        float x = radius * r * Mathf.Cos(t);
        float y = radius * r * Mathf.Sin(t);

        int[] return_val = { (int)x, (int)y };

        return return_val;
    }
    IEnumerator WaitOneSecond()
    {
        yield return new WaitForSeconds(1f);
#if DEBUG
        UnityEngine.Debug.Log("Waited 1 second!");
#endif
    }

    //void SetTopLeftAndBottomRightFromCamera()
    //{
    //    Camera cam = Camera.main;
    //    float camHeight = 2f * cam.orthographicSize;
    //    float camWidth = camHeight * cam.aspect;

    //    Vector2 camPos = cam.transform.position;

    //    topLeft = new Vector2(camPos.x - camWidth / 2f, camPos.y + camHeight / 2f);
    //    bottomRight = new Vector2(camPos.x + camWidth / 2f, camPos.y - camHeight / 2f);
    //}

    //STEP 2 FUNCTIONS

    GameObject CreateRoomPhysicsObject(Room room)
    {
        GameObject roomObj = new GameObject($"Room_{room.id}");

#if VISUALISE
        // SpriteRenderer just visualizes the actual room dimensions (unaffected by padding)
        SpriteRenderer renderer = roomObj.AddComponent<SpriteRenderer>();
        renderer.sprite = GenerateSprite(room.width, room.height);
        renderer.color = new Color(1.0f, 0.5f, 0.0f);
        renderer.drawMode = SpriteDrawMode.Sliced; // If you're using a 9-slice or scalable sprite
        renderer.size = new Vector2(room.width * cellWidth, room.height * cellHeight);
#endif

        // Set position
        Vector2 grid_coords = GridToWorld(room.GetVector());
        roomObj.transform.position = new Vector2(grid_coords.x, grid_coords.y);

        // Add and configure BoxCollider2D – make it larger than room size
        BoxCollider2D collider = roomObj.AddComponent<BoxCollider2D>();

        // Use padded size: add 2 units in both directions (converted to world units)
        float paddedWidth = (room.width + 2) * cellWidth;
        float paddedHeight = (room.height + 2) * cellHeight;
        collider.size = new Vector2(paddedWidth, paddedHeight);

        // Do not use object scale to affect collider size
        roomObj.transform.localScale = Vector3.one;

        // Add physics
        Rigidbody2D rb = roomObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        return roomObj;
    }


    IEnumerator WaitForPhysicsToSettleAndUpdateRooms()
    {
        float settleThreshold = 0.01f;
        float waitTime = 0.1f;
        float stillTime = 0f;
        float requiredStillTime = 3.0f; //TODO: Come up with a better way to do this.

        // First pass: create and disable
        foreach (var room in rooms)
        {
            GameObject roomObj = CreateRoomPhysicsObject(room);
            roomObj.SetActive(false);
            roomObjects.Add($"Room_{room.id}", roomObj);
#if DEBUG
            UnityEngine.Debug.Log($"Printed Room at ({room.x},{room.y}) size {room.width}x{room.height}");
#endif
        }

        // Second pass: activate all at once
        foreach (var roomObj in roomObjects.Values)
        {
            roomObj.SetActive(true);
        }

        // Wait until all rigidbodies stop moving for at least 1 full second
        while (stillTime < requiredStillTime)
        {
            bool allStopped = true;

            foreach (var kvp in roomObjects)
            {
                Rigidbody2D rb = kvp.Value.GetComponent<Rigidbody2D>();
                if (rb != null && rb.linearVelocity.magnitude > settleThreshold)
                {
                    allStopped = false;
                    break;
                }
            }

            if (allStopped)
            {
                stillTime += waitTime;
            }
            else
            {
                stillTime = 0f; // Reset the still time if any are moving
            }

            yield return new WaitForSeconds(waitTime);
        }

        // Update room coordinates from object positions
        foreach (var room in rooms)
        {
            if (roomObjects.TryGetValue($"Room_{room.id}", out GameObject obj))
            {
                Vector2 worldPos = obj.transform.position;
                Vector2Int gridPos = WorldToGrid(worldPos);

                room.x = gridPos.x;
                room.y = gridPos.y;
            }
#if DEBUG
            UnityEngine.Debug.Log($"Updated Room {room.id}: ({room.x},{room.y}) size {room.width}x{room.height}");
#endif
        }
    }

    //STEP 3

    IEnumerator RoomFilter()
    {
        List<int> removedIds;
#if DEBUG
        UnityEngine.Debug.Log("### Begin room filter ###");
#endif

        FilterRoomsRandomly(ref rooms, keepRoomPercentage, out removedIds);

        foreach (int id in removedIds)
        {
            string objName = $"Room_{id}";
            GameObject roomObj = GameObject.Find(objName);
            if (roomObj != null)
            {
                GameObject.Destroy(roomObj);
            }
        }

        yield break;
    }

    public static void FilterRoomsRandomly(ref List<Room> rooms, float keepRoomPercentage, out List<int> removedRoomIds)
    {
        // Shuffle the list randomly
        rooms = rooms.OrderBy(r => UnityEngine.Random.value).ToList();

        // Calculate how many to keep
        int numToKeep = Mathf.RoundToInt(rooms.Count * keepRoomPercentage);
        numToKeep = Mathf.Clamp(numToKeep, 1, rooms.Count);

        // Split the list
        var keptRooms = rooms.Take(numToKeep).ToList();
        var removedRooms = rooms.Skip(numToKeep).ToList();

        // Record removed room IDs
        removedRoomIds = removedRooms.Select(r => r.id).ToList();

        // Update the original list
        rooms = keptRooms;
    }


    //public static void FilterRoomsByArea(ref List<Room> rooms, float keepRoomPercentage, out List<int> removedRoomIds)
    //{
    //    // Sort rooms in descending order by area
    //    var sortedRooms = rooms.OrderByDescending(r => r.area).ToList();

    //    int numToKeep = Mathf.RoundToInt(sortedRooms.Count * keepRoomPercentage);
    //    numToKeep = Mathf.Clamp(numToKeep, 1, sortedRooms.Count);

    //    // Keep top rooms, identify removed ones
    //    var keptRooms = sortedRooms.Take(numToKeep).ToList();
    //    var removedRooms = sortedRooms.Skip(numToKeep).ToList();

    //    // Extract IDs of removed rooms
    //    removedRoomIds = removedRooms.Select(r => r.id).ToList();

    //    // Update the original list with the filtered one
    //    rooms = keptRooms;
    //}

    IEnumerator FindCorridors()
    {
        Delaunator del;

        GenerateDelaunayMesh(out del);
        CreateMinimumSpanningTree(del);

        yield break;
    }

    bool GenerateDelaunayMesh(out Delaunator pointsDel)
    {
        // Step 1: Collect your points (e.g., from room centers)
        List<Vector2> unityPoints = rooms.Select(r => new Vector2(r.x, r.y)).ToList();

        // Step 2: Convert to IPoint[]
        IPoint[] points = unityPoints
            .Select(p => (IPoint)new Point(p.x, p.y))
            .ToArray();

        // Step 3: Generate triangulation

        if (points.Length <= 3)
        {
#if DEBUG
            UnityEngine.Debug.LogWarning($"Not enough points to perform triangulation ({points.Length}).");
#endif
            pointsDel = null;
            return false;
        }

        pointsDel = new Delaunator(points);

#if VISUALISE
#if DEBUG
        Debug.LogWarning("Drawing Triangles.");
#endif
        int count = 0;

        // Step 4: Loop over triangles and optionally visualize them
        foreach (var triangle in pointsDel.GetTriangles())
        {
#if DEBUG
            Debug.LogWarning($"Drawing Triangle {count}.");
#endif
            var pts = triangle.Points.ToArray();  // Converts IEnumerable<IPoint> to an array
            Vector2 a = GridToWorld(new Vector2((float)pts[0].X, (float)pts[0].Y));
            Vector2 b = GridToWorld(new Vector2((float)pts[1].X, (float)pts[1].Y));
            Vector2 c = GridToWorld(new Vector2((float)pts[2].X, (float)pts[2].Y));

            //CreateLine($"TestLine_AB_{count}", a, b, Color.red);
            //CreateLine($"TestLine_BC_{count}", b, c, Color.red);
            //CreateLine($"TestLine_CA_{count}", c, a, Color.red);
            count++;
        }
#endif

        return true;
    }
    public List<(int, int)> CreateMinimumSpanningTree(Delaunator delaunator)
    {
        // Step 1: Build all edges from triangulation (undirected, so avoid duplicates)
        var edges = new HashSet<(int, int, float)>();
        var finalEdges = new List<(int, int)>();

        foreach (var triangle in delaunator.GetTriangles())
        {
            var pts = triangle.Points.ToArray();
            AddEdge(edges, pts[0], pts[1]);
            AddEdge(edges, pts[1], pts[2]);
            AddEdge(edges, pts[2], pts[0]);
        }

        // Step 2: Sort edges by weight (distance)
        var sortedEdges = edges.OrderBy(e => e.Item3).ToList();

        // Step 3: Kruskal's algorithm - union-find
        var parent = new Dictionary<int, int>();
        foreach (var r in rooms)
            parent[r.id] = r.id;

        int Find(int x)
        {
            if (parent[x] != x) parent[x] = Find(parent[x]);
            return parent[x];
        }

        void Union(int a, int b) => parent[Find(a)] = Find(b);

        // Tracks which edges are part of the MST
        var mstEdges = new HashSet<(int, int)>();

        // Step 4: Build minimum spanning tree
        foreach (var (id1, id2, _) in sortedEdges)
        {
            if (Find(id1) != Find(id2))
            {
                Union(id1, id2);
                var edge = NormalizeEdge(id1, id2);
                mstEdges.Add(edge); // Keep track of MST edges
                finalEdges.Add(edge);

#if VISUALISE
                Vector2 p1 = GridToWorld(rooms.First(r => r.id == id1).GetVector());
                Vector2 p2 = GridToWorld(rooms.First(r => r.id == id2).GetVector());
                CreateLine($"triLin_{id1}-{id2}", p1, p2, Color.green);
#endif
            }
        }

        // Step 5: Add extra edges randomly from unused ones
        var nonMstEdges = sortedEdges
            .Where(e => !mstEdges.Contains(NormalizeEdge(e.Item1, e.Item2)))
            .ToList();

        int extrasToAdd = Mathf.RoundToInt(nonMstEdges.Count * Mathf.Clamp01(extraEdgePercentage));

        var rng = new System.Random();
        var selectedExtras = nonMstEdges.OrderBy(_ => rng.Next()).Take(extrasToAdd);


        foreach (var (id1, id2, _) in selectedExtras)
        {
            var edge = NormalizeEdge(id1, id2);
            finalEdges.Add(edge);
#if VISUALISE
            Vector2 p1 = GridToWorld(rooms.First(r => r.id == id1).GetVector());
            Vector2 p2 = GridToWorld(rooms.First(r => r.id == id2).GetVector());
            CreateLine($"extraLin_{id1}-{id2}", p1, p2, Color.yellow); // Yellow to show these are extra
#endif
        }

        // Local function for consistent edge representation
        (int, int) NormalizeEdge(int a, int b) => a < b ? (a, b) : (b, a);

        return finalEdges;
    }

    void AddEdge(HashSet<(int, int, float)> edges, IPoint a, IPoint b)
    {
        Vector2 va = new Vector2((float)a.X, (float)a.Y);
        Vector2 vb = new Vector2((float)b.X, (float)b.Y);
        int idA = FindRoomId(va);
        int idB = FindRoomId(vb);
        if (idA == idB) return;

        // Undirected: always store (min, max)
        int min = Mathf.Min(idA, idB);
        int max = Mathf.Max(idA, idB);
        float dist = Vector2.Distance(va, vb);
        edges.Add((min, max, dist));
    }

    int FindRoomId(Vector2 pos)
    {
        return rooms.First(r => Vector2.Distance(r.GetVector(), pos) < 0.01f).id;
    }

    IEnumerator SpawnRooms()
    {
        foreach (Room room in rooms)
        {
            Vector2 worldPos = GridToWorld(room.GetVector());
            Vector3 position = new Vector3(worldPos.x*3, 0f, worldPos.y*3);
            Quaternion rotation = Quaternion.Euler(-90f,0f, 0f);
            GameObject spawnedRoom = Instantiate(platformPrefab, position, rotation);
            spawnedRoom.layer = LayerMask.NameToLayer("Ground");

            // Get current scale
            Vector3 originalScale = spawnedRoom.transform.localScale;

            float scaleFactor = room.width;// / 4f;
            spawnedRoom.transform.localScale = new Vector3(
                originalScale.x * scaleFactor,
                originalScale.y * scaleFactor,            
                originalScale.z// * scaleFactor
                );

            Vector3 size = spawnedRoom.GetComponent<MeshRenderer>().bounds.size;
            Debug.Log("Room size: " + size);
        }



        yield break;
    }

    IEnumerator SpawnPlayer()
    {

        if (null != playerPrefab)
        {
            Vector2 worldPos = GridToWorld(rooms.First().GetVector());
            Vector3 position = new Vector3(worldPos.x * 3, 10f, worldPos.y * 3);
            //Vector3 position = new Vector3(0, 20, 0);

            GameObject spawnedPlayer = Instantiate(playerPrefab, position, Quaternion.Euler(0F, 0f, 0f));

            if (null != cameraThirdPerson)
            { 
                cameraThirdPerson.GetComponent<CinemachineCamera>().Follow = spawnedPlayer.transform;
            }

        }

        yield break;
    }

#if VISUALISE
    // VISUALISATION FUNCITONS

    Sprite GenerateSprite(int width, int height)
    {
        // Texture size in pixels, 100x100 works for visual clarity regardless of in-world units
        int pixelWidth = 100;
        int pixelHeight = 100;

        Texture2D texture = new Texture2D(pixelWidth, pixelHeight);
        Color[] pixels = new Color[pixelWidth * pixelHeight];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;

        texture.SetPixels(pixels);
        texture.Apply();

        // Create sprite from the texture
        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), // pivot in the center
            100f                     // Pixels per unit
        );
    }

    void CreateLine(string name, Vector2 start, Vector2 end, Color col)
    {
#if DEBUG
        Debug.Log($"Drawing Line {name}: ({start.x},{start.y}) to ({end.x},{end.y})");
#endif

        GameObject lineSegment = new GameObject(name);
        lineSegment.transform.parent = GameObject.Find("lineManager").transform;

        LineRenderer lr = lineSegment.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        float lineThickness = 0.1f * (100f / gridWidth);
        lr.startWidth = lineThickness;// 0.1f;
        lr.endWidth = lineThickness;// 0.1f;

        // Add material if needed
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = col;
        lr.endColor = col;
    }

    void DeleteLine(int line_id)
    {

        GameObject lineObj = GameObject.Find("lineManager");
        string objName = "myLineRenderer_1";
        //string objName = $"myLineRenderer_{lr_id}";

        if (lineObj != null)
        {

            Transform child = lineObj.transform.Find(objName);

            if (child != null)
            {
                Destroy(child.gameObject);
#if DEBUG
                UnityEngine.Debug.Log($"Destroying {objName}");
#endif
            }
            else
            {
#if DEBUG
                UnityEngine.Debug.LogWarning($"Line {objName} not found.");
#endif
            }
        }
        else
        {
#if DEBUG
            UnityEngine.Debug.LogWarning("Object lineManager not found.");
#endif
        }

        return;
    }

    void ClearLines()
    {
        GameObject lineObj = GameObject.Find("lineManager");

        foreach (Transform child in lineObj.transform)
        {
            Destroy(child.gameObject);
        }
    }
#endif

    Vector2 GridToWorld(Vector2 gridPos)
    {
        float worldX = topLeft.x + (cellWidth * 0.5f) + (gridPos.x * cellWidth);
        float worldY = topLeft.y - (cellHeight * 0.5f) - (gridPos.y * cellHeight);
        return new Vector2(worldX, worldY);
    }

    Vector2Int WorldToGrid(Vector2 worldPos)
    {
        int gridX = Mathf.RoundToInt((worldPos.x - topLeft.x - (cellWidth * 0.5f)) / cellWidth);
        int gridY = Mathf.RoundToInt((topLeft.y - worldPos.y - (cellHeight * 0.5f)) / cellHeight);
        return new Vector2Int(gridX, gridY);
    }


}




// Unused code:

//foreach (var room in rooms)
//{
//    GameObject room_cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
//    Vector2 grid_coords = GridToWorld(room.x, room.y);
//    room_cube.transform.position = new Vector2(grid_coords.x, grid_coords.y, 0);
//    room_cube.transform.localScale = new Vector2(room.width* cellWidth, room.height* cellHeight, 1);
//    room_cube.GetComponent<Renderer>().material.color = Color.blue;
//    UnityEngine.Debug.Log($"Printed Room at ({grid_coords.x},{grid_coords.y}) size {room.width * cellWidth}x{room.height * cellHeight}");
//    //StartCoroutine(WaitOneSecond());
//}

//

////LineRenderer myLineRenderer_1 = lineManager.AddComponent<LineRenderer>(); ;

////myLineRenderer_1.positionCount = 2;

////myLineRenderer_1.startWidth = 0.1f;
////myLineRenderer_1.endWidth = 0.1f;

////myLineRenderer_1.SetPosition(0, new Vector2(5,0, -2));
////myLineRenderer_1.SetPosition(1, new Vector2(0, 5, -2));

//CreateLine("myLineRenderer_1", new Vector2(5, 0), new Vector2(0, 5));