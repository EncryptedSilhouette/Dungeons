public class KGameManager
{
    public KPlayer Player;
    public KInputManager InputManager;
    public KRenderManager Renderer; 

    public KGameManager(KRenderManager renderer, KInputManager inputManager)
    {
        Renderer = renderer;
        InputManager = inputManager;
        Player = new(this);
    }

    public void Update()
    {
        Player.Update(InputManager);
    }

    public void FrameUpdate()
    {
        Player.FrameUpdate(Renderer, (int)KProgram.KLayers.DEFAULT);
    }
}
