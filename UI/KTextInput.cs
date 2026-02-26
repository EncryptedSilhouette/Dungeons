public struct KInputField
{
    public bool HasFocus;
    public KButton Button;

    public KInputField()
    {
        
    }

    public void Update(KInputManager input)
    {
        if (Button.CheckState(KButtonState.PRESSED))
        {
            input.StartTextRead();
        }

        if (HasFocus)
        {
            var s = input.StopTextRead();
        }
    }
}