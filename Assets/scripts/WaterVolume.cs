using System.Collections.Generic;
using System.Diagnostics;

public class WaterVolume
{
    //public int TotalVolume { get { return SurfaceLocations.Count + VolumeBlocks.Count; } }

    public List<int[]> SurfaceLocations = new List<int[]>();
    public List<int[]> VolumeBlocks = new List<int[]>();

    public List<int[]> GetVolume()
    {
        List<int[]> items = new List<int[]>(); //new list called items 
        items.AddRange(VolumeBlocks);//adds list volumeblocks into items list 
        return items; //returns items
    }
}

// ADD UP NUMBER OF BLOCKS BY DOING THE COUNT OF SURFACE LOCATIONS PLUS 
//COUNT OF VOLUME BLOCKS 
//IF IT IS GREATER THEN GIVEN VOLUME DONT SPAWN IT IN 