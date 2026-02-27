using SFML.Graphics;
using SFML.System;

public class KPlayer
{
    public float Speed;
    public KSprite Sprite;
    public Vector2f Direction;
    public KGameManager GameManager;

    public KPlayer(KGameManager gameManager)
    {
        GameManager = gameManager;

        Speed = 2;
        Sprite = new()
        {
            Layer = (int)KProgram.KLayers.DEFAULT,
            Rotation = 0,
            Color = Color.White,
            Rotocenter = (0,0),
            Bounds = new(),
            TextureBounds = new(),//Need better resolution.
            Frames = [],
        };
        Direction = (0,0);
    }

    public void Update(KInputManager input)
    {
        var isMoving = false;
        Direction = (0,0);

        if (input.IsKeyPressed(SFML.Window.Keyboard.Key.W))
        {
            isMoving = true;
            Direction.X = -1;
        } 
        else if (input.IsKeyPressed(SFML.Window.Keyboard.Key.S))
        {
            isMoving = true;
            Direction.X = 1;
        }

        if (input.IsKeyPressed(SFML.Window.Keyboard.Key.A))
        {
            isMoving = true;
            Direction.Y = -1;
        }
        else if (input.IsKeyPressed(SFML.Window.Keyboard.Key.D))
        {
            isMoving = true;
            Direction.Y = 1;
        }

        if (isMoving)
        {
            Sprite.Bounds.Position += Direction.Normalized() * Speed;
        }
    }

    public void FrameUpdate(KRenderManager renderer, int gameLayer)
    {
        renderer.DrawSprite(Sprite, gameLayer);
    }
}