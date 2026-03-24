using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
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

public struct KGameChunck
{
    public static ulong GetHandle(uint x, uint y) => x + y * uint.MaxValue;
    public static Vector2u GetPosition(ulong handle) => new Vector2u
    {
        X = (uint)(handle % uint.MaxValue),
        Y = (uint)(handle / uint.MaxValue),
    };

    public ulong Handle;
    public bool Initialized;
    public KChunkPriority Priority;

    public Vector2u Position => GetPosition(Handle);

    public KGameChunck(ulong handle)
    {
        Handle = handle;
        Initialized = false;
        Priority = KChunkPriority.NONE;
    }
}

 public struct KChunkRegion
{
    public uint X;
    public uint Y;
    public uint Width;
    public uint Height;
    
    public KChunkRegion(uint x, uint y, uint width, uint height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}

public class KChunkManager
{
    public uint HEAP_SIZE = 65_536;
    public KBufferRegion H0;
    public KBufferRegion H1;
    public KBufferRegion H2;
    public KBufferRegion H3;

    public KGameChunck[] ChunkHeap;

    //public Thread HeapThread;
    //public Thread LoadingThread;

    public KChunkManager()
    {
        uint offset = 0;
        H0 = new KBufferRegion(offset, HEAP_SIZE / 4);
        offset += HEAP_SIZE / 4;
        H1 = new KBufferRegion(offset, HEAP_SIZE / 4);
        offset += HEAP_SIZE / 4;
        H2 = new KBufferRegion(offset, HEAP_SIZE / 4);
        offset += HEAP_SIZE / 4;
        H3 = new KBufferRegion(offset, HEAP_SIZE / 4);
     
        ChunkHeap = new KGameChunck[HEAP_SIZE];
        Array.Fill(ChunkHeap, new KGameChunck
        {
            Initialized = false,
        });
    }

    public void LoadChunk(ulong handle)
    {
        
    }

    public void LoadChunkFromFile()
    {
        
    }

    public void LoadChunkRegion(in KChunkRegion region)
    {
        
    }
}


public class KGameWorld
{
    public KPlayer Player;
    public KChunkManager ChunkManager;

    public KGameWorld(KPlayer player)
    {
        Player = player;
        ChunkManager = new();
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

    public void Generate(uint x, uint y, uint width, uint height)
    {
        KChunkRegion chunkRegion = new(x, y, width, height);
    }
}

public class KWorldgenerator()
{
   
}