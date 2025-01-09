using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using Unity.Mathematics;
using UnityEngine.UIElements;

// https://www.youtube.com/watch?v=yOgIncKp0BE&list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9&index=2
//https://www.youtube.com/watch?v=v7yyZZjF1z4&list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9&index=1
public class CaveGeneration : MonoBehaviour, IDataPersistence
{
    [Header("Cave Options")]
    public int width;
    public int height;
    public int passes;

    public string seed; // store a  seed 
    public bool useRandomSeed;

    public void LoadData(GameData data)
    {
        this.seed = data.seed;
        this.pos = data.pos;
        useRandomSeed = false;
        Debug.Log(pos);
    } 
    public void SaveData(GameData data)
    {     
        
        data.seed = this.seed; 
        data.pos=this.pos;
        Debug.Log("saved seed");
        Debug.Log(pos);
        
    } 


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
    private Vector3 pos;
    private int[,] map; //container 
    private List<Vector3> spawnPositions = new();
    private List<WaterVolume> waterPools = new();


    void Start()
    {
        container = new GameObject();
        waterContainer = new GameObject("Water Container");
        GenerateMap(); // calling the function as soon as this script is loaded
        SpawnWater();

        int index = UnityEngine.Random.Range(0, spawnPositions.Count);
        pos = (spawnPositions[index]);
        GameManager.Instance.SpawnPlayer(pos);
    

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
    

    
    
    void GenerateMap()
    {
        map = new int[width, height]; // seting the parameters for how big we want the map
        RandomFillMap();
        for (int i = 0; i < passes; i++) //set number of passes 
        {
            SmoothMap();
        }

        processmap();

        DrawElements();

        SpawnWater();
    }
    void RandomFillMap()
    {
        if (useRandomSeed || string.IsNullOrWhiteSpace(seed))// if it is null create a random seed 
        {
            seed = DateTime.Now.ToString("YYYY-MM-DD HH:mm:ss.ffffff"); //determines the calendar time at the currrent time in seconds
        }

        Debug.Log(seed);
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
    
    
    
    
    
    // Function to get all connected tiles of the same type startingfrom (startx, starty)



    List<coord> getregiontiles(int startx, int starty)
    {
        List<coord> tiles = new List<coord>();
        Dictionary<(int, int), int> mapflags = new Dictionary<(int, int), int>();
        int tiletype = map[startx, starty];

        Queue<coord> queue = new Queue<coord>();
        queue.Enqueue(new coord(startx, starty));
        mapflags[(startx, starty)] = 1;

        while (queue.Count > 0)
        {
            coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tilex - 1; x <= tile.tilex + 1; x++)
            {
                for (int y = tile.tiley - 1; y <= tile.tiley + 1; y++)
                {
                    if (isinmaprange(x, y) && (y == tile.tiley || x == tile.tilex))
                    {
                        if (!mapflags.ContainsKey((x, y)) && map[x, y] == tiletype)
                        {
                            mapflags[(x, y)] = 1;
                            queue.Enqueue(new coord(x, y));
                        }
                    }
                }
            }
        }    
        return tiles;
    }

    List<List<coord>> getregions(int tiletype)
    {
        List<List<coord>> regions = new List<List<coord>>();
        Dictionary<(int, int), int> mapflags = new Dictionary<(int, int), int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!mapflags.ContainsKey((x, y)) && map[x, y] == tiletype)
                {
                    List<coord> newregion = getregiontiles(x, y);
                    regions.Add(newregion);

                    foreach (coord tile in newregion)
                    {
                        mapflags[(tile.tilex, tile.tiley)] = 1;
                    }
                }
            }
        }    
        return regions;
    }

    bool isinmaprange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height; // Returns true if (x, y) is within the map range
    }



    void processmap()
    {
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
    
        List<List<coord>> roomregions = getregions(0);
        int roomthreshholdsize = 50;
        List<room> survivingrooms = new List<room> ();

        foreach (List<coord> roomregion in roomregions)
        {
            if (roomregion.Count < roomthreshholdsize)
            {
                foreach (coord tile in roomregion)
                {
                    map[tile.tilex, tile.tiley] = 1;
                }
            }
            else
            {
                survivingrooms.Add(new room(roomregion, map));
            }
        }
        survivingrooms = Mergesort(survivingrooms);//make this into a mergesort algorithm 
        survivingrooms[0].ismainroom= true;
        survivingrooms[0].isaccesiblefrommainroom= true;



        connectclosestrooms(survivingrooms);

    }

    private List<room> Mergesort (List<room>list)
    {
        if(list.Count<=1)
        {
            return list;
        }

        int middle = list.Count/2;
        List<room> left = new List<room>();
        List<room> right = new List<room>();

        for(int i = 0; i < middle;i++)
        {
            left.Add(list[i]);
        }
        for(int i = middle; i < list.Count;i++)
        {
            right.Add(list[i]);
        }

        left = Mergesort(left);
        right = Mergesort(right);
        return Merge(left,right);
    }

    private List<room> Merge(List<room> left, List<room>right)
    {
        List<room> result = new List<room>();
        int leftIndex = 0;
        int rightIndex =0;

        while(leftIndex < left.Count && rightIndex < right.Count)
        {
            if (left[leftIndex].tiles.Count>right[rightIndex].tiles.Count)
            {
                result.Add(left[leftIndex]);
                leftIndex ++;
            }
            else
            {
                result.Add(right[rightIndex]);
                rightIndex ++;

            }

        }
        while (leftIndex<left.Count)
        {
            result.Add(left[leftIndex]);
            leftIndex++;
        }
        while (rightIndex<right.Count)
        {
            result.Add(right[rightIndex]);
            rightIndex++;
        }

        return result;

    } 
            

    void connectclosestrooms(List<room> allrooms, bool forceaccesibilityfrommainroom = false )
    {
        List<room> roomlista = new List<room>();
        List<room> roomlistb = new List<room>();

        if (forceaccesibilityfrommainroom)
        {
            foreach(room Room in allrooms)
            {
                if(Room.isaccesiblefrommainroom)
                {
                    roomlistb.Add(Room);    
                }
                else
                {
                    roomlista.Add(Room);
                }
            }
        }

        else
        {
        roomlista = allrooms;
        roomlistb = allrooms;    
        }

        int bestdistance = 0;
        coord besttilea  = new coord ();
        coord besttileb  = new coord ();
        room bestrooma = new room ();
        room bestroomb = new room ();
        bool possibleconnectionfound = false;

        foreach(room rooma in roomlista)
        {
            if(!forceaccesibilityfrommainroom)
            {
                possibleconnectionfound = false;

                if (rooma.connectedrooms.Count>0)
                {
                    continue;
                }
            }

            foreach(room roomb in roomlistb )
            {
                if(rooma == roomb || rooma.isconnected(roomb))
                {
                    continue;
                }

                for (int tileindexa = 0; tileindexa < rooma.edgetiles.Count; tileindexa ++ )
                {
                    for (int tileindexb= 0; tileindexb < roomb.edgetiles.Count; tileindexb ++ )
                    {
                        coord tilea = rooma.edgetiles[tileindexa];
                        coord tileb = roomb.edgetiles[tileindexb];
                        int distancebetweenrooms = (int)(Mathf.Pow(tilea.tilex - tileb.tilex,2) + Mathf.Pow(tilea.tiley - tileb.tiley,2)); 

                        if (distancebetweenrooms < bestdistance || !possibleconnectionfound)
                        {
                            bestdistance = distancebetweenrooms;
                            possibleconnectionfound = true;
                            besttilea = tilea;
                            besttileb = tileb;
                            bestrooma = rooma;
                            bestroomb = roomb;
                        }

                    }
                }
            }
            if (possibleconnectionfound && !forceaccesibilityfrommainroom)
            {
                createpassage(bestrooma, bestroomb, besttilea, besttileb);
            }
        }

        if (possibleconnectionfound && forceaccesibilityfrommainroom)
            {
                createpassage(bestrooma, bestroomb, besttilea, besttileb);
                connectclosestrooms(allrooms,true);
            }

        if (!forceaccesibilityfrommainroom)
        {
            connectclosestrooms(allrooms, true );
        }
    }

    void createpassage(room rooma, room roomb, coord tilea, coord tileb)
    {
        
        room.connectrooms(rooma,roomb);
        Debug.DrawLine(coordtoworldpoint(tilea),coordtoworldpoint(tileb),Color.green,100); //works but drawing the line doesnt work     
    
        List<coord> line = getline(tilea,tileb);
        foreach(coord c in line)
        {
            drawcircle(c,1);
        }

    } 

    void drawcircle(coord c, int r)
    {
        for(int x = -r; x<=r;x++)
        {
            for(int y = -r; y<=r;y++)
            {
                if (x*x + y*y <= r*r)
                {
                    int drawx = c.tilex + x;
                    int drawy = c.tiley+y;
                    if(isinmaprange(drawx, drawy))
                    {
                        map[drawx,drawy] = 0;
                       
                    }
                   
                }

            }
        }
    }

    List<coord> getline(coord from,coord to)
    {
        List<coord> line = new List<coord>();
        int x =from.tilex;
        int y = from.tiley;

        int dx = to.tilex - from.tilex;
        int dy = to.tiley - from.tiley;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientstep = Math.Sign (dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if(longest<shortest)
        {
            inverted = true;
            longest = Math.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientstep = Math.Sign(dx);


        }

        int gradientaccumulation  = longest/2;
        for(int i = 0;i<longest;i++)
        {
            line.Add(new coord(x,y));
            if(inverted)
            {
                y+=step;
            }
            else
            {
                x+=step;
            }

            gradientaccumulation += shortest;
            if(gradientaccumulation>= longest)
            {
               if(inverted)
               {
                    x+= gradientstep;
               }
               else
               {
                    y+=gradientstep;
               }
               gradientaccumulation -= longest;
            }
        }

        return line;
    }
        
    



    Vector3  coordtoworldpoint(coord tile)
    {
        
        return new Vector3(-width/2+.5f+tile.tilex,2,-height/2 + .5f + tile.tiley);
    }


    //void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))// when mouse is pressed call function
    //    {
    //        GenerateMap();
    //    }
    //}


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
     

    
    private void SpawnWater()
    {
        if (waterPools.Count <= 0)// if there are no water pool spawn locations 
        {
            //Debug.LogWarning("We cannot spwn water on this seed");
            return;
        }

        List<int[]> cords = waterPools[UnityEngine.Random.Range(0, waterPools.Count)].GetVolume();//get a random location where we can spawn water 

        foreach (int[] cord in cords)//for each location 
        {
            Vector3 position = new Vector3(-width / 2 + cord[0] + .5f, -height / 2 + cord[1] + .5f, 0);
            Instantiate(waterBlock, position, Quaternion.identity, waterContainer.transform);//we spawn a water block at that location 

            
        }
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
    class room : IComparable<room>
    {
    public List<coord> tiles; // List of all tiles in the room
    public List<coord> edgetiles; // List of tiles on the edge of the room (for pathfinding or connecting rooms)
    public List<room> connectedrooms; // List of rooms connected to this room
    public int roomsize; // Size of the room (number of tiles)
    public bool isaccesiblefrommainroom;
    public bool ismainroom;
    


    public room(){
    }


    // Constructor that takes the room's tiles and the map
    public room(List<coord> roomtiles, int[,] map) 
    {
        tiles = roomtiles; // Initialize the tiles of the room
        roomsize = tiles.Count; // Set the size of the room
        connectedrooms = new List<room>(); // Initialize the list of connected rooms

        edgetiles = new List<coord>(); // Initialize the list of edge tiles
        foreach (coord tile in tiles) 
        {
            for (int x = tile.tilex-1; x <= tile.tilex+1; x++) 
            {
				for (int y = tile.tiley-1; y <= tile.tiley+1; y++) 
                {
					if (x == tile.tilex || y == tile.tiley) 
                    {
						if (map[x,y] == 1) 
                        {
							edgetiles.Add(tile);//addd connect rooms function from ep 6 
						}  // Logic to determine if a tile is an edge tile goes here
                    }
                }         
            }       
        }
    }
        public void setaccesiblefrommainroom()
        {
            if(!isaccesiblefrommainroom)
            {
                isaccesiblefrommainroom = true;
            
                foreach(room connectedroom in connectedrooms)
                {
                    connectedroom.setaccesiblefrommainroom();

                }
            }
        }

        public static void connectrooms (room rooma, room roomb )
        {
            if(rooma.isaccesiblefrommainroom)
            {
                roomb.setaccesiblefrommainroom();
            }
            else if (roomb.isaccesiblefrommainroom)
            {
                rooma.setaccesiblefrommainroom();
            }
            rooma.connectedrooms.Add(roomb);
            roomb.connectedrooms.Add(rooma);        
        }

        public bool isconnected(room otherroom)
        {
            return connectedrooms.Contains(otherroom);
        }

        public int CompareTo (room otherroom)
        {
            return otherroom.roomsize.CompareTo(roomsize);
        }

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


//addd connect rooms function from ep 6 
//TO DO (if possible) at the moment I just check the volume of the water pool and not if it is enclosed by walls on the side
// I loop through the surface location and then go down and count the number of free spaces that are there
//Thoughts : As I go down then also go across (could just copy the code for the surface) however this seems really inefficient Better way ?