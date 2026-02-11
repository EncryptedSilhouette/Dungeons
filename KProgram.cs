using SFML.Graphics;
using SFML.System;
using SFML.Window;

public struct KTextureAtlas
{
    public Texture Texture;
    public Dictionary<string, FloatRect> TexCoords; 
}

public class KProgram
{
    public const uint FRAME_RATE = 60;
    
    public static bool Running;
    public static KConsole Console;
    public static RenderWindow Window;
    public static KRenderer Renderer;
    public static KEditor Editor;
    public static KGame Game;
    public static KTextureAtlas[] Atlases;

    static KProgram()
    {
        Running = false;
        
        Console = new();
        Window = new(VideoMode.DesktopMode, "Dungeons");
        Window.SetFramerateLimit(FRAME_RATE);
        Window.Closed += (_, _) => Running = false;

        Renderer = new(Window, new(180000, PrimitiveType.Triangles, VertexBuffer.UsageSpecifier.Dynamic));
        Editor = new();
        Game = new();
        Atlases = [];
    }

    public static void Main()
    {
        var atlasData = File.ReadAllLines("Assets/atlas.csv");
        Atlases =
        [
            LoadTextureAtlas(atlasData)
        ];

        var bufferRegions = KRenderer.CreateBufferRegions([600, 600]);
        var drawLayers = new KDrawLayer[] 
        {
            new() //Background Layer
            {
                IsStatic = true,
                Upscale = true,
                Resolution = (320, 240),
                Primitive = PrimitiveType.Triangles,
                States = RenderStates.Default,
                Region = bufferRegions[0],
            },
            new()
            {
                IsStatic = false,
                Upscale = true,
                Resolution = (320, 240),
                Primitive = PrimitiveType.Triangles,
                States = RenderStates.Default,
                Region = bufferRegions[1],
            } 
        };

        uint currentFrame = 0;
        Running = true;

        Renderer.Init(bufferRegions[0], drawLayers);
        Editor.Init(Atlases[0], Window, Renderer);

        GC.Collect();
        Console.Start();

        while(Running)
        {
            Update(currentFrame);

            Window.Clear();

            FrameUpdate(currentFrame);
            currentFrame++;

            Window.Display();
            Window.DispatchEvents();
        }

        Console.Stop();
    }

    private static void Update(uint currentFrame)
    {
        Editor.Update();
        //Game.Update(currentFrame);
    }

    private static void FrameUpdate(uint currentFrame)
    {
        Editor.FrameUpdate(Renderer, currentFrame);
        //Game.FrameUpdate(currentFrame);
        Renderer.FrameUpdate();
    }

    public static KTextureAtlas LoadTextureAtlas(IEnumerable<string> atlasData)
    {
        KTextureAtlas atlas = new()
        {
            TexCoords = new(128)
        };

        foreach (var spriteData in atlasData)
        {
            var values = spriteData.Split(',');

            switch(values[0])
            {
                case "atlas":
                    atlas.Texture = new Texture(values[1]);
                    break;

                case "sprite":
                    atlas.TexCoords.Add(values[1], new()
                    {
                        Position = (int.Parse(values[2]), int.Parse(values[3])),
                        Size = (int.Parse(values[4]), int.Parse(values[5])),   
                    });
                    break;

                default:
                    break;
            }

            if (atlas.Texture is null) atlas.Texture = CreateErrorTexture(640, 480);
        }
        return atlas;
    }

    public static Texture CreateErrorTexture(uint width, uint height)
    {
        Color color = new(0,0,0);
        Image img = new((width, height));

        for (uint i = 0; i < height; i++)
        {
            for (uint j = 0; j < width; j++)
            {
                if (color.R + 1 > byte.MaxValue)
                {
                    color.R = 0;

                    if (color.G + 1 > byte.MaxValue)
                    {
                        color.G = 0;

                        if (color.B + 1 > byte.MaxValue)
                        {
                            color.B = 0;
                        }
                        else color.B++;
                    }
                    else color.G++;
                }
                else color.R++;

                img.SetPixel(new Vector2u(i, j), color);
            }
        }

        img.SetPixel(new Vector2u(0, 0), Color.White);
        return new(img);
    }
}