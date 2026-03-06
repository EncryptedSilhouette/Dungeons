using SFML.Graphics;
using SFML.System;

public class KGameManager
{
    public KPlayer Player;
    public KGameWorld GameWorld;
    public KInputManager InputManager;
    public KRenderManager Renderer; 

    public KGameManager(KRenderManager renderer, KInputManager inputManager)
    {
        Player = new(this);
        GameWorld = new(Player);
        Renderer = renderer;
        InputManager = inputManager;
    }

    public void Update()
    {
    }

    public void FrameUpdate()
    {
    }

    public void GenerateWorld()
    {
        
    }

    public void LoadWorld()
    {
        
    }

    public void SaveWorld()
    {
        
    }
}

public enum KChunkPriority
{
    NONE,
    TIMED_OUT,    
    LOW,
    MEDIUM,
    HIGH,
}

public struct KGameChuck
{
    public bool Initialized;
    public KChunkPriority Priority;
    public KGrid Tiles;
    
    public FloatRect Bounds => new(Tiles.Position, 
        (Tiles.CellWidth * Tiles.Columns,
        Tiles.CellHeight * Tiles.Rows));

    public KGameChuck()
    {
        Initialized = false;
    }

    public void Update()
    {
        
    }

    public void FrameUpdate()
    {
        
    }
}


public class KGameWorld
{
    public KPlayer Player;
    public KGameChuck[] ActiveChunks;

    public KGameWorld(KPlayer player)
    {
        Player = player;
        ActiveChunks = [];
    }   

    public void Init()
    {
        
    }

    public void Update(KInputManager input)
    {
        Player.Update(input);
        for (int i = 0; i < ActiveChunks.Length; i++)
        {
            ActiveChunks[i].Update();
        }
    }

    public void FrameUpdate(KRenderManager renderer)
    {
        Player.FrameUpdate(renderer, (int)KProgram.KLayers.DEFAULT);
        for (int i = 0; i < ActiveChunks.Length; i++)
        {
            ActiveChunks[i].FrameUpdate();
        }
    }

    public void CreateChuck(Vector2f Position)
    {
        var chunk = new KGameChuck
        {
            Priority = KChunkPriority.HIGH,
            Tiles = new KGrid(
                16, 16,
                Position,
                new(4,4))
        };

        var cells = chunk.Tiles.Cells;
        for (int i = 0; i < cells.Length; i++)
        {
            
        }
    }

    public void SaveChuck()
    {
        
    }

    public void UnloadChuck()
    {
        
    }
}