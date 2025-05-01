using System;
using System.Windows;
using DrugWars.Core.Models;

namespace DrugWars.Wpf.Windows;

public class GameWindowBase : Window
{
    private GameEngine? _gameEngine;

    public virtual GameEngine GameEngine
    {
        get => _gameEngine ?? throw new InvalidOperationException("GameEngine not initialized");
        set => _gameEngine = value;
    }

    public GameWindowBase()
    {
        _gameEngine = null;
    }

    public GameWindowBase(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }
}