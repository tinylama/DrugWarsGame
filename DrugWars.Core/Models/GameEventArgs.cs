using System;

namespace DrugWars.Core.Models;

public class GameEventArgs : EventArgs
{
    public string Message { get; }

    public GameEventArgs(string message)
    {
        Message = message;
    }
}