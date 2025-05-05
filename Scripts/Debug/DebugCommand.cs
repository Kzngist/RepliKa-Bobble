using System;

public class DebugCommandBase
{
    internal string id { get; }
    internal string description { get; }
    internal string format { get; }

    protected DebugCommandBase(string id, string description, string format)
    {
        this.id = id;
        this.description = description;
        this.format = format;
    }
}

public class DebugCommand : DebugCommandBase
{
    Action command;

    internal DebugCommand(string id, string description, string format, Action command) : base(id, description, format)
    {
        this.command = command;
    }

    internal void Invoke()
    {
        command.Invoke();
    }
}

public class DebugCommand<T> : DebugCommandBase
{
    Action<T> command;

    internal DebugCommand(string id, string description, string format, Action<T> command) : base(id, description, format)
    {
        this.command = command;
    }

    internal void Invoke(T value)
    {
        command.Invoke(value);
    }
}

public class DebugCommand<T, T2> : DebugCommandBase
{
    Action<T, T2> command;

    internal DebugCommand(string id, string description, string format, Action<T, T2> command) : base(id, description, format)
    {
        this.command = command;
    }

    internal void Invoke(T value, T2 value2)
    {
        command.Invoke(value, value2);
    }
}
