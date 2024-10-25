using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

// https://www.youtube.com/watch?v=yOgIncKp0BE&list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9&index=2
//https://www.youtube.com/watch?v=v7yyZZjF1z4&list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9&index=1
public class CaveGeneration : MonoBehaviour
{
    [Header("Cave Options")]
    public int width;
    public int height;
    public int passes;

    public string seed; // store a  seed 
    public bool useRandomSeed;

    public GameObject dirtBlock;
    public GameObject bedrock;
    public GameObject waterBlock;

    [Range(0, 100)]//percentage 
    public int randomFillPercent; // how much is going to be filled with wall compared to space 

    [Header("Water Options")]
    [SerializeField] private int minDepth = 3; 
    [SerializeField] private int maxDepth = 10;
    [SerializeField] private int minWidth = 5;
    [SerializeField] private int maxWidth = 10;

    [SerializeField] private int minVolume = 20;
    [SerializeField] private int maxVolume = 40;

    private GameObject container;
    private GameObject waterContainer;

    private int[,] map; //container 
    private List<Vector3> spawnPositions = new();
    private List<WaterVolume> waterPools = new();

    void Start()
    {
        container = new GameObject();
        waterContainer = new GameObject("Water Container");
        GenerateMap(); // calling the function as soon as this script is loaded
        DrawElements();
        SpawnWater();

        int index = UnityEngine.Random.Range(0, spawnPositions.Count);
        GameManager.Instance.SpawnPlayer(spawnPositions[index]);

    }
    
    struct coord
    {
    public int tilex; // X-coordinate of the tile
    public int tiley; // Y-coordinate of the tile

    // Constructor to initialize the coord with given x and y values
        public coord(int x, int y)
        {
        tilex = x;
        tiley = y;
        }
    }  

    // Function to get all connected tiles of the same type startingfrom (startx, starty)
    List<coord> getregiontiles(int startx, int starty)
    {
        List<coord> tiles = new List<coord>(); // List to store all connected tiles
        int[,] mapflags = new int[widthg, height]; // Flags to mark checked tiles
        int tiletype = map[startx, starty]; // The type of tile we are searching for (e.g., wall or floor)

        Queue<coord> queue = new Queue<coord>(); // Queue for breadth-first search (BFS)
        queue.Enqueue(new coord(startx, starty)); // Start search at the given coordinates
        mapflags[startx, starty] = 1; // Mark the starting tile as checked

        // Perform BFS to find all connected tiles of the same type
        while (queue.Count > 0)
        {
            coord tile = queue.Dequeue(); // Get the current tile
            tiles.Add(tile); // Add it to the list of connected tiles

            // Loop through the neighbors of the current tile (left, right, up, down)
            for (int x = tile.tilex - 1; x <= tile.tilex + 1; x++)
            {
                for (int y = tile.tiley - 1; y <= tile.tiley + 1; y++)
            {
                    // Check if the neighbor is within the map bounds and in the same row or column
                    if (isinmaprange(x, y) && (y == tile.tiley || x == tile.tilex)) 
                    {
                        // If the neighbor is the same tile type and hasn't been checked yet
                        if (mapflags[x, y] == 0 && map[x, y] == tiletype)
                        {
                            mapflags[x, y] = 1; // Mark it as checked
                            queue.Enqueue(new coord(x, y)); // Add it to the queue for further exploration
                        }
                    }
                }
            }
        }    

        return tiles; // Return the list of all connected tiles
    }

// Function to check if the given coordinates are within the bounds of the map
bool isinmaprange(int x, int y)
{
    return x >= 0 && x < width && y >= 0 && y < height; // Returns true if (x, y) is within the map range
}

// Room class to represent a region of connected tiles in the cave (like a room or a corridor)


    

    // Function to find and return regions of tiles of a specific type (e.g., wall, floor)
    // tiletype: the type of tile (0 for floor, 1 for wall, etc.)
    List<List<coord>> getregions(int tiletype) 
    {
        List<List<coord>> regions = new List<List<coord>>(); // Stores all regions of the specified tile type
        int[,] mapflags = new int[width, height]; // Flags to keep track of which tiles have been checked

        // Loop through every tile in the map
        for (int x = 0; x < width; x++) 
        {
            for (int y = 0; y < height; y++) 
            {
                // If the tile hasn't been checked and is of the specified type
                if (mapflags[x, y] == 0 && map[x, y] == tiletype) 
                {
                    List<coord> newregion = getregiontiles(x, y); // Get all tiles connected to this tile
                    regions.Add(newregion); // Add the found region to the list of regions

                    // Mark all tiles in this region as checked
                    foreach (coord tile in newregion) 
                    {
                        mapflags[tile.tilex, tile.tiley] = 1;
                    }
                }
            }
        }    

        return regions; // Return the list of regions
    }
    bool isinmaprange(int x, int y)
{
    return x >= 0 && x < width && y >= 0 && y < height; // Returns true if (x, y) is within the map range
}


class room
{
    public List<coord> tiles; // List of all tiles in the room
    public List<coord> edgetiles; // List of tiles on the edge of the room (for pathfinding or connecting rooms)
    public List<room> connectedrooms; // List of rooms connected to this room
    public int roomsize; // Size of the room (number of tiles)

    // Constructor that takes the room's tiles and the map
    public room(List<coord> roomtiles, int[,] map) 
    {
        tiles = roomtiles; // Initialize the tiles of the room
        roomsize = tiles.Count; // Set the size of the room
        connectedrooms = new List<room>(); // Initialize the list of connected rooms

        edgetiles = new List<coord>(); // Initialize the list of edge tiles
        foreach (coord tile in roomtiles) 
        {
            // Logic to determine if a tile is an edge tile goes here
        }
    }
}


    void processmap() 
    {
        // Get all regions made up of wall tiles (tiletype 1)
        List<List<coord>> wallregions = getregions(1); 

        int wallthreshholdsize = 50; // Minimum size for a valid wall region

        // Loop through each wall region
        foreach(List<coord> wallregion in wallregions)  
        {
            // If the region is smaller than the threshold size, remove it (i.e., turn it into floor)
            if (wallregion.Count < wallthreshholdsize)
        {
            foreach (coord tile in wallregion)
            {
                map[tile.tilex, tile.tiley] = 0; // Set tile to floor (0)
            }
        }
    }

    



    //void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))// when mouse is pressed call function
    //    {
    //        GenerateMap();
    //    }
    //}

    void GenerateMap()
    {
        Debug.Log("This is starting generation");
        map = new int[width, height]; // seting the parameters for how big we want the map
        RandomFillMap();

        for (int i = 0; i < passes; i++) //set number of passes 
        {
            SmoothMap();
        }
    }

    void RandomFillMap()
    {
        Debug.Log("This is generating the map");
        if (useRandomSeed || string.IsNullOrWhiteSpace(seed))// if it is null create a random seed 
        {
            Debug.Log("Generate Randomly");
            seed = DateTime.Now.ToString("YYYY-MM-DD HH:mm:ss.ffffff"); //determines the calendar time at the currrent time in seconds
        }

        System.Random prng = new System.Random(seed.GetHashCode()); //converting string into an integer 


        for (int x = 0; x < width; x++)  //setting up size of map width 
        {
            for (int y = 0; y < height; y++) //setting up size of map height
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;// If pixel is near the borders of our map then they are automatticaly set to wall
                }
                else // else continue generating as normal 
                {
                    // map[x, y] = (prng.Next(0, 100) < randomFillPercent) ? 1 : 0;// the random fill percentage if the number is greate then it it is a wall if it is not it is open space 
                    int randomNum = prng.Next(0, 100);
                    if (randomNum < randomFillPercent)
                    {
                        map[x, y] = 1;
                    }
                    else
                    {
                        map[x, y] = 0;
                    }
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)  //check through each tile 
        {
            for (int y = 0; y < height; y++) //check through each tile 
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);//shows us the amount of wall tiles around the given tile 

                if (neighbourWallTiles > 4) {

                    map[x, y] = 1; // if there are more than 4 walls surrounding then the tile that we is at the center becomes a wall
                }
                else if (neighbourWallTiles < 4)
                {
                    map[x, y] = 0;// if there are less than 4 walls surrounding then the tile that is at the center becomes open space 
                }

            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)// iterating through a 3x3 grid centered on grid [x.y]
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) //looking at all neighbours 
            {
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)//Check that x and y are within the map and not -1 for example 
                {
                    if (neighbourX != gridX || neighbourY != gridY)//when neighbour y != gridy and neighbourx != grid x then we will be look at neighboring tiles
                    {
                        wallCount += map[neighbourX, neighbourY]; //wall count up by one when map[neighbourX, neighbourY] = wall
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    void DrawElements()
    {
        if (map != null)
        {
            for (int x = 0; x < width; x++) //go across the x value of the map  
            {
                for (int y = 0; y < height; y++)//go across the y value of the map 
                {
                    Vector3 position = new Vector3(-width / 2 + x + .5f, -height / 2 + y + .5f, 0); // find vector of given position 
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)//if near the edge of map 
                    {
                        var go = Instantiate(bedrock, position, Quaternion.identity, container.transform); //spawn bedrock 
                        continue;
                    }

                    if (map[x, y] == 1) //if position x,y is a one then it is a wall 
                    {
                        var go = Instantiate(dirtBlock, position, Quaternion.identity, container.transform);//spawn in dirt on this vector 
                        go.name = $"Block: {x},{y}";
                    }
                    else {
                        // If its in range of out block
                        // && and if the line below block is a solid block
                        if (y - 1 > 0 && map[x, y - 1] == 1) //if the given position is inside the map and one below it is a dirt block  
                        {
                            spawnPositions.Add(position);//add the potential position into the given list 
                        }

                        if (VolumeTest(x, y, out WaterVolume volume)) // if value gotten 
                        {
                            waterPools.Add(volume); //add the position to the waterpools list 
                        }
                    }
                }
            }
        }
    }
    //not sure what this does use chatgpt on it 
    private bool VolumeTest(int x, int y, out WaterVolume volume)
    {
        if (!VolumeWidth(x, y, out volume)) // calls functions and if failed returns false
            
        {
            return false;
        }

        if (!Depth(ref volume))//calls false if the called function does not pass
        {
            return false;
        }
        // if (!VolumeChecker(x,y,ref volume))
        // {
        //     return false;
        // }
        //DOESNT WORK 

        return true;
    }

    private bool Depth(ref WaterVolume volume) // ref allows for that value to be updated not just a copy made
    {
        bool hasFoundMinDepth = false;

        foreach (int[] surface in volume.SurfaceLocations)//for each location on the surface 
        {
            for (int i = 0; i < maxDepth; i++) //from 0 to however large the max depth is 
            {
                if (map[surface[0], surface[1] - i] == 0)// if the given coordinates are open space
                {
                    CheckForWalls();
                    volume.VolumeBlocks.Add(new int[] { surface[0], surface[1] - i });//add coordinates to list Volumeblocks
                    if(i == maxDepth - 1)//if we reach max depth 
                    {
                        //Debug.Log("Max depth reached");
                        return false;// can not spawn here 
                    }

                    continue;
                }

                if (map[surface[0], surface[1] - i] == 1)//if i hit a wall i have 
                {
                    // Found the bottom
                    if (i > minDepth)
                    {
                        hasFoundMinDepth = true;
                    }

                    break;
                }
            }
        }

        return hasFoundMinDepth;// return true if we have reached the mininum depth 
    }

    private bool VolumeWidth(int x, int y, out WaterVolume volume)
    {
        int xLeftOffset = 1;//left and right offset 
        int xRightOffset = 1;//going in both directions 
        volume = new WaterVolume();//go to this class 
        volume.SurfaceLocations.Add(new int[] { x, y });//add all surfaces to list 

        for (int i = 0; i < maxWidth; i++) //range is how big the max width is i +1 
        {
            if (map[x - xLeftOffset, y] == 0)// if coordinates = open space 
            {
                volume.SurfaceLocations.Add(new int[] { x - xLeftOffset, y });//add those coordinates to the surface locations list  
                xLeftOffset++;//offset plus one 
            }

            if (map[x + xRightOffset, y] == 0) // if coordinates = open space 
            {
            volume.SurfaceLocations.Add(new int[] { x + xRightOffset, y });//add those coordinates to surface location list 
                xRightOffset++;//offset plus one 
            }

            if (map[x - xLeftOffset, y] == 1 
                && map[x + xRightOffset, y] == 1 // if we have hit walls on both sides 
                && volume.SurfaceLocations.Count >= minWidth)//and it is greater than minimum width
            {
                return true;//can spawn here 
            }
        }

        return false;//otherwise if we surpass midwidth and we have not hit walls with both of our "pointers" then we can not spawn water here 
    }
     

    private void CheckForWalls()
    {
        int xLeftOffset = 1;
        int xRightOffset = 1;
        for (int i = 0; i < maxWidth; i++)
        {
            

        }

    }

    private void SpawnWater()
    {
        if (waterPools.Count <= 0)// if there are no water pool spawn locations 
        {
            Debug.LogWarning("We cannot spwn water on this seed");
            return;
        }

        List<int[]> cords = waterPools[UnityEngine.Random.Range(0, waterPools.Count)].GetVolume();//get a random location where we can spawn water 

        foreach (int[] cord in cords)//for each location 
        {
            Vector3 position = new Vector3(-width / 2 + cord[0] + .5f, -height / 2 + cord[1] + .5f, 0);
            Instantiate(waterBlock, position, Quaternion.identity, waterContainer.transform);//we spawn a water block at that location 
        }
    }
     

    
    
    //void OnDrawGizmos()
    //{
    //    if (map != null)
    //    {
    //        for (int x = 0; x < width; x++)  // going through each pixel and drawing a cube 
    //        {
    //            for (int y = 0; y < height; y++) 
    //            {
    //                Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white; // if the cube given coordinate is equal to 1 then we set it as a wall set as black to indicate wall else set as white for open spaxce
    //                Vector3 pos = new Vector3(-width / 2 + x + .5F, -height / 2 + y + .5f, 0);//where is the cube
    //                Gizmos.DrawCube(pos, Vector3.one);// put cube there 
    //            }
    //        }
    //    }
    //}
}


//TO DO (if possible) at the moment I just check the volume of the water pool and not if it is enclosed by walls on the side
// I loop through the surface location and then go down and count the number of free spaces that are there
//Thoughts : As I go down then also go across (could just copy the code for the surface) however this seems really inefficient Better way ?
