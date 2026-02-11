public ref struct KCommandResult 
{
    public int Code; 
    public Span<string> Args;
    public Exception? Exception;

    public KCommandResult()
    {
        Code = 0;
        Exception = null;
        Args = [];    
    }

    public KCommandResult(Span<string> args, int code = 0, Exception? exception = null)
    {
        Code = code;
        Args = args;
        Exception = exception;
    }
}

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
        KCommandResult result;
        while(_running)
        {
            _userInput = Console.ReadLine() ?? string.Empty;
            
            if (_userInput == string.Empty) continue;

            var args = _userInput.Split(' ').AsSpan();

            try
            {
                switch (args[0])
                {
                    case "fail":
                        fail(args.Slice(1), 1);
                        break;

                    case "exit":
                        result = exit(args.Slice(1));
                        break;

                    case "test":
                        Console.WriteLine("Working!");
                        break;

                    case "csv_etrim":
                        if (args.Length < 4) fail(args, 1);
                        args = csv_trim(args.Slice(1)).Args;
                        break;

                    default:
                        Console.WriteLine("Unknown command.");
                        break;                
                }
            }
            catch (Exception e)
            {
                fail(args, 1, e);
            }
        }
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

    private KCommandResult fail(Span<string> args, int code, Exception? e = null)
    {
        Console.WriteLine($"Failed Operation: {code} ");
        foreach (var a in args) Console.Write($"{a} ");
        Console.WriteLine($", {e?.Message}");
        return new(args, code, e);
    }

    private KCommandResult exit(Span<string> args)
    {
        KProgram.Running = _running = false;
        return new(args);
    }

    private KCommandResult csv_trim(Span<string> args)
    {
        const int FIRST_ARG = 0, FILE_PATH = 1, TRIM = 2;

        string[] lines;
        StreamWriter writer;

        if (args[FIRST_ARG] == "-r")
        {
            lines = File.ReadAllLines(args[FILE_PATH]);
            File.Delete(args[FILE_PATH]);
            writer = new(File.Create(args[FILE_PATH]));

            if (!int.TryParse(args[TRIM], out int trimAmount)) return fail(args, 1);

            foreach (var line in lines)
            {   
                if (line == string.Empty) continue;
                var values = line.Split(',');
                if (values.Length - trimAmount < 0) continue;
                
                for (int i = 0; i < values.Length - trimAmount; i++)
                {
                    writer.Write($"{values[i]}");
                    if (i < values.Length - trimAmount - 1) writer.Write(",");
                }    
                writer.Write('\n');
            }
            writer.Close();

            Console.WriteLine($"Successfully trimmed {trimAmount} elements");
        }
        return new(args);
    }
}