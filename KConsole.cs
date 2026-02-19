public delegate int KCommandAction(ref KCommandData data);

public struct KCommandData 
{
    public int Code; 
    public object? Sender;
    public string[] Args;
    public Exception? Exception;

    public KCommandData()
    {
        Code = 0;
        Args = [];
        Sender = null;
        Exception = null;
    }

    public KCommandData(string[] args, int code = 0, object? Sender = null, Exception? exception = null)
    {
        Args = args;
        Code = code;
        Exception = exception;
    }
}

public struct KCommand
{
    public KCommandData Data;
    public KCommandAction? Action;

    public KCommand(in KCommandData data, KCommandAction action)
    {
        Data = data;
        Action = action;
    }

    public KCommand(string[] args, KCommandAction action)
    {
        Data = new(args);
        Action = action;
    }

    public void Execute() => Action?.Invoke(ref Data);
}

//A person with more time and money would care about the multithreading and shared state.
//Luckily I am someone with neither and and will deal with the problem if and when it arrives.
//I see the potential, but I see not an issue atm - EncSil 2/19/26. 
public class KConsole
{
    private static int exit(ref KCommandData data)
    {
        if (data.Sender is KConsole c)
        {
            KProgram.Running = c._running = false;
        }
        return data.Code;
    }

    private static int fail(ref KCommandData data)
    {
        Console.WriteLine($"Failed Operation: {data.Code} ");
        foreach (var a in data.Args) Console.Write($"{a} ");
        Console.WriteLine($", Err:{data.Exception?.Message}");
        return data.Code;
    }

    private bool _running;
    private string _userInput;
    private Task _task;
    private Queue<KCommand> _queuedCommands;

    public KConsole()
    {
        _running = true;
        _userInput = string.Empty;
        _task = Task.CompletedTask;
        _queuedCommands = new(128);
    }

    public void Update()
    {
        lock (_queuedCommands)
        {
            while (_queuedCommands.Count > 0)
            {
                _queuedCommands.Dequeue().Execute();
            }
        }
    }

    private async Task Run()
    {
        //There's definitely a better way :) 
        KCommand cmd_exit = new()
        {
            Data = new()
            {
                Sender = this,
            },
            Action = exit,
        };
        
        KCommand cmd_fail = new()
        {
            Data = new()
            {
                Code = 1,
            },
            Action = fail,
        };

        while(_running)
        {
            _userInput = Console.ReadLine() ?? string.Empty;
            
            if (_userInput == string.Empty) continue;

            var args = _userInput.Split(' ');

            try
            {
                switch(args[0])
                {
                    case "exit":
                        EnqueueCommand(cmd_exit);
                        break;

                    case "fail":
                        EnqueueCommand(cmd_fail);
                        break;
                }
            }
            catch (Exception e)
            {
                var err = cmd_fail;
                err.Data.Exception = e;
                EnqueueCommand(err);
            }
        }
    }

    public void EnqueueCommand(in KCommand command)
    {
        lock (_queuedCommands)
        {
            _queuedCommands.Enqueue(command);
        }
    }
    public void DequeueCommand(out KCommand command) 
    {
        lock (_queuedCommands)
        {
            command = _queuedCommands.Dequeue();
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