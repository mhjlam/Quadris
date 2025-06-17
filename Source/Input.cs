using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Quadris.Source;

public class Input
{
    private bool _isNewPiece = true;
    private readonly Dictionary<Keys, int> _keyDownTime = new();
    private KeyboardState _prevKeyState = Keyboard.GetState();
	private Dictionary<Keys, IGameCommand> _activeCommandMap = new();

	public bool IsNewPiece
    {
        get => _isNewPiece;
        set => _isNewPiece = value;
    }

    public void Reset()
    {
        _keyDownTime.Clear();
        _prevKeyState = Keyboard.GetState();
        _isNewPiece = true;
    }

    public void UpdatePrevState(KeyboardState keyState)
    {
        _prevKeyState = keyState;
    }

    public bool IsKeyPressed(Keys key)
    {
        var current = Keyboard.GetState();
        return current.IsKeyDown(key) && !_prevKeyState.IsKeyDown(key);
    }

    public bool IsKeyDown(Keys key)
    {
        var current = Keyboard.GetState();
        return current.IsKeyDown(key);
    }

    public int GetKeyDownTime(Keys key)
    {
        return _keyDownTime.TryGetValue(key, out var value) ? value : 0;
    }

    public void SetKeyDownTime(Keys key, int value)
    {
        _keyDownTime[key] = value;
    }

    public void IncrementKeyDownTime(Keys key)
    {
        if (_keyDownTime.TryGetValue(key, out int value))
        {
            _keyDownTime[key] = ++value;
        }
        else
        {
            _keyDownTime[key] = 1;
        }
    }

	public void SetActiveCommandMap(Dictionary<Keys, IGameCommand> commandMap)
	{
		_activeCommandMap = commandMap ?? new();
	}

	public void HandleInput()
	{
		foreach (var kvp in _activeCommandMap)
		{
			if (IsKeyPressed(kvp.Key))
			{
				kvp.Value.Execute();
			}
		}
	}
}
