using System.Buffers;
using SFML.Graphics;
using SFML.System;

public struct KGameMap
{
    public KGrid Grid;
}


public class KGame 
{
    public KGameMap GameMap;

    public KGame()
    {
        GameMap = new()
        {
            Grid = new()
            {
                
            }
        };
    }

    public void Init()
    {
        
    }

    public void Update(uint currentFrame)
    {
        
    }

    public void FrameUpdate(uint currentFrame, KRenderManager renderer)
    {
        
    }

    public void GenerateMap()
    {
        
    }
}

