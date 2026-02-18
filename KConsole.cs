public delegate void KCommandAction(object? Sender, string[] args);

public struct KCommandResult 
{
    public int Code; 
    public Memory<string> Args;
    public Exception? Exception;

    public KCommandResult()
    {
        Code = 0;
        Exception = null;
    }

    public KCommandResult(Memory<string> args, int code = 0, Exception? exception = null)
    {
        Args = args;
        Code = code;
        Exception = exception;
    }
}

public struct KCommand
{
    public object? Sender; 
    public string[] Args;
    public KCommandAction? Action;

    public KCommand()
    {
        Sender = null;
        Args = [];
    }

    public void Execute() => Action?.Invoke(Sender, Args);
}

public class KConsole
{
    private int _queueHead;
    private int _queueTail;
    private bool _running;
    private string _userInput;
    private Task _task;
    private KCommand[] _commandQueue;


    public KConsole()
    {
        _queueHead = _queueTail = 0;
        _running = true;
        _userInput = string.Empty;
        _task = Task.CompletedTask;
        _commandQueue = new KCommand[64];
    }

    private async Task Run()
    {
        while(_running)
        {
            _userInput = Console.ReadLine() ?? string.Empty;
            
            if (_userInput == string.Empty) continue;

            var args = _userInput.Split(' ');

            try
            {
                switch (args[0])
                {
                    case "fail":
                        
                        break;

                    case "exit":
                        //result = exit(args, 1);
                        break;

                    case "test":
                        Console.WriteLine("Working!");
                        break;

                    case "csv_etrim":
                        //if (args.Length < 4) fail(args, 1);
                        //args = csv_trim(args.Slice(1)).Args;
                        break;

                    default:
                        Console.WriteLine("Unknown command.");
                        break;                
                }
            }
            catch (Exception e)
            {
                //fail(args, 1, e);
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

    public bool EnqueueCommand(in KCommand command)
    {
        lock (_commandQueue)
        {
            var x = (_queueHead + 1) % _commandQueue.Length;
            
            if (x != _queueTail)
            {
                _queueHead = x;
                _commandQueue[_queueHead] = command;
                return true;
            }
            else return false;
        }
    }

    public bool DequeueCommand(out KCommand command)
    {
        lock (_commandQueue)
        {
            var x = (_queueTail + 1) % _commandQueue.Length;
            
            if (x != _queueHead)
            {
                command = _commandQueue[_queueHead];
                _queueTail = x;
                return true;
            }
            else
            {
                command = new();
                return false;
            }
        }
    }

    //private KCommandResult fail(string[] args, int code, Exception? e = null)
    //{
    //    Console.WriteLine($"Failed Operation: {code} ");
    //    foreach (var a in args) Console.Write($"{a} ");
    //    Console.WriteLine($", {e?.Message}");
    //    return new(args, code, e);
    //}

    //private KCommandResult exit(string[] args)
    //{
    //    KProgram.Running = _running = false;
    //    return new(args);
    //}

    //private KCommandResult csv_trim(string[] args)
    //{
    //    const int FIRST_ARG = 0, FILE_PATH = 1, TRIM = 2;

    //    string[] lines;
    //    StreamWriter writer;

    //    if (args[FIRST_ARG] == "-r")
    //    {
    //        lines = File.ReadAllLines(args[FILE_PATH]);
    //        File.Delete(args[FILE_PATH]);
    //        writer = new(File.Create(args[FILE_PATH]));

    //        if (!int.TryParse(args[TRIM], out int trimAmount)) return fail(args, 1);

    //        foreach (var line in lines)
    //        {   
    //            if (line == string.Empty) continue;
    //            var values = line.Split(',');
    //            if (values.Length - trimAmount < 0) continue;
                
    //            for (int i = 0; i < values.Length - trimAmount; i++)
    //            {
    //                writer.Write($"{values[i]}");
    //                if (i < values.Length - trimAmount - 1) writer.Write(",");
    //            }    
    //            writer.Write('\n');
    //        }
    //        writer.Close();

    //        Console.WriteLine($"Successfully trimmed {trimAmount} elements");
    //    }
    //    return new(args);
    //}
}