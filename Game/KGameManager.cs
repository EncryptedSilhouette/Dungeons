using SFML.Graphics;
using SFML.System;

public class KGameManager
{
    public KPlayer Player;
    public KInputManager InputManager;
    public KRenderManager Renderer;

    public Dictionary<ulong, KGameChunk> ChunkCache;

    public KGameManager(KRenderManager renderer, KInputManager inputManager)
    {
        Player = new(this);
        ChunkCache = new();
        Renderer = renderer;
        InputManager = inputManager;
    }

    public void GenerateSpawn()
    {

    }

    public void GenerateChunks(Vector2i position, uint columns, uint rows)
    {
        var chunks = new KGameChunk[columns * rows];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                chunks[i] = new KGameChunk(position + (j, i))
                {
                    Initialized = true,
                    Priority = KChunkPriority.HIGH
                };
            }
        }
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

public class KGameChunk
{
    public const int CHUNK_SIZE = 16;

    #region Static
    public static long GetHandle(Vector2i postion) => postion.X + postion.Y * uint.MaxValue;

    public static Vector2i GetPosition(long handle) => new Vector2i
    {
        X = (int)(handle % uint.MaxValue),
        Y = (int)(handle / uint.MaxValue),
    };
    #endregion

    public long Handle;
    public bool Initialized;
    public KChunkPriority Priority;
    public KWorldTile[] Tiles;

    public Vector2i Position => GetPosition(Handle);

    public KGameChunk(long handle)
    {
        Handle = handle;
        Initialized = false;
        Priority = KChunkPriority.NONE;
        Tiles = new KWorldTile[CHUNK_SIZE * CHUNK_SIZE];
        Array.Fill(Tiles, new KWorldTile
        {
            Ground = new KTile
            {
                Bounds = new(),
                Sprite = new(),
                Type = KTileType.NONE,
            },
            Wall = new KTile
            {
                Bounds = new(),
                Sprite = new(),
                Type = KTileType.NONE,
            },
        });
    }

    public KGameChunk(Vector2i position) : this(GetHandle(position)) { }
}

public enum KTileType
{
    NONE,
    VOID,
    GROUND,
    EDGE,
    WALL,
}

public struct KTile
{
    public FloatRect Bounds;
    public FloatRect Sprite;
    public KTileType Type;
}

public struct KWorldTile
{
    public KTile Ground;
    public KTile Wall;
}


