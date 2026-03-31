using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;
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

public enum KChunkPriority : byte
{
    NONE,
    TIMED_OUT,    
    LOW,
    MEDIUM,
    HIGH,
}

public struct KGameChunk
{
    public const int CHUNK_SIZE = 16;

    #region Static
    public static ulong GetHandle(long x, long y)
    {
        //removes negative value range to fit uint32 range.
        x -= int.MinValue;
        y -= int.MinValue;
        //Converts from grid coords to handle.
        return (ulong)x + (ulong)y * uint.MaxValue;
    }

    public static Vector2i GetPosition(ulong handle)
    {
        //Convert to world coords.
        long x = (long)(handle % uint.MaxValue);
        long y = (long)(handle / uint.MaxValue);
        //Add the negative value range to fit to sint32 range.
        x = x + int.MinValue;
        y = y + int.MinValue;
        //should cast safely x_x.
        return new((int)x, (int)y);
    }
    #endregion

    public ulong Handle;
    public bool Initialized;
    public KChunkPriority Priority;
    public Vector2i Position => GetPosition(Handle);

    public KGameChunk(ulong handle)
    {
        Handle = handle;
        Initialized = false;
        Priority = KChunkPriority.NONE;
    }

    public KGameChunk(int x, int y)
    {
        Handle = GetHandle(x, y);
        Initialized = false;
        Priority = KChunkPriority.NONE;
    }
}

public struct KChunkRegion
{
    public KBufferRegion Region; 
    public IntRect Area;
    public KChunkHandler ChunkHandler;
    public Span<KGameChunk> Chunks => ChunkHandler.ChunkBuffer.AsSpan((int)Region.Offset, (int)Region.Capacity);

    public KChunkRegion(KChunkHandler handler, KBufferRegion region, Vector2i pos, Vector2i size)
    {
        Region = region;
        Area = new(pos, size);
        ChunkHandler = handler;
    }

    public bool ContainsChunk(ulong handle) => Area.Contains(KGameChunk.GetPosition(handle));
    public bool ContainsChunk(Vector2i point) => Area.Contains(point);
}

public class KChunkHandler
{
    public Dictionary<ulong, KGameChunk> chunkCache = new();
}

public struct KPlayerChunkCluster
{
    public int LoadDist;
    public int SimDist;

    public Action LoadChunkRegion;

}

public class KGameWorld
{
    public KPlayer Player;


    public KGameWorld(KPlayer player)
    {
        Player = player;

    }   

    public void Init()
    {
        
    }

    public void Update(KInputManager input)
    {
        
    }

    public void FrameUpdate(KRenderManager renderer)
    {
        
    }
}