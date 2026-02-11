public class KConsole
{
    private bool _running;
    private string _userInput;
    private Task _task;

    public KConsole()
    {
        _running = true;
        _userInput = string.Empty;
        _task = Task.CompletedTask;
    }

    private async Task Run()
    {
        while(_running)
        {
            _userInput = Console.ReadLine() ?? string.Empty;
            
            if (_userInput == string.Empty) continue;

            var values = _userInput.Split(' ').AsSpan();

            switch (values[0])
            {
                case "test":
                    Console.WriteLine("Working!");
                    break;

                case "exit":
                    KProgram.Running = _running = false;
                    break;

                default:
                    break;                
            }
        }
        Console.WriteLine("Unknown command.");
    }

    public void Start()
    {
        _running = true;
        _task = Task.Run(Run);
    }

    public async void Stop()
    {
        _running = false;
        await _task;
    }
}