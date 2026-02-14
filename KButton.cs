using SFML.Graphics;
using SFML.System;

public struct KButton
{
    private bool _isDown = false;

    public Color Color;
    public Color HeldColor;
    public Color DownColor;
    public KSprite Sprite;
    //public KText TextBox;

    public Action? OnHover;
    public Action? OnPressed;
    public Action? OnHold;
    public Action? OnReleased;
    public Action? OnExit;

    public KButton(KSprite sprite, string text)
    {
        Color = new(200, 200, 200);
        HeldColor = new(125, 125, 125);
        DownColor = new(100, 100, 100);
        //TextBox = new(text, new Vertex[text.Length * 6]);
        Sprite = sprite;
    }

    public void Update(KInputManager inputManager, Vector2f position)
    {
        if (KProgram.CheckPointRectCollision(position, Sprite.Bounds))
        {
            OnHover?.Invoke();
            Color = HeldColor;

            if (inputManager.IsMouseDown(KMouseStates.M1_DOWN))
            {
                if (!_isDown && !inputManager.PrevMouseStates.HasFlag(KMouseStates.M1_DOWN))
                {
                    _isDown = true;
                    OnPressed?.Invoke();
                }
                OnHold?.Invoke();
            }
            else if (_isDown)
            {
                _isDown = false;
                OnReleased?.Invoke();
            }
        }
        else if (_isDown)
        {
            _isDown = false;
            Color = Sprite.Color;
            OnReleased?.Invoke();
            OnExit?.Invoke();
        }
        else
        {
            Color = Sprite.Color;
        }
    }

    public void FrameUpdate(KRenderManager renderManager)
    {
        renderManager.DrawSprite(Sprite, Color);
        //renderManager.DrawText(TextBox, Sprite.Bounds.Position.X, Sprite.Bounds.Position.Y, wrapThreshold: (int)Sprite.Bounds.Width, layer: 1);
    }

    public void FrameUpdate(KRenderManager renderManager, int layer)
    {
        renderManager.DrawSprite(Sprite, Color, layer);
    }
}