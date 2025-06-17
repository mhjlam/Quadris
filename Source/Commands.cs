namespace Quadris.Source;

public interface IGameCommand
{
	void Execute();
}

public class MoveLeftCommand : IGameCommand
{
	private readonly Quadris _game;
	public MoveLeftCommand(Quadris game) => _game = game;
	public void Execute() => _game.TranslatePiece(-1, 0);
}

public class MoveRightCommand : IGameCommand
{
	private readonly Quadris _game;
	public MoveRightCommand(Quadris game) => _game = game;
	public void Execute() => _game.TranslatePiece(1, 0);
}

public class MoveDownCommand : IGameCommand
{
	private readonly Quadris _game;
	public MoveDownCommand(Quadris game) => _game = game;
	public void Execute() => _game.TranslatePiece(0, 1);
}

public class RotateCommand : IGameCommand
{
	private readonly Quadris _game;
	public RotateCommand(Quadris game) => _game = game;
	public void Execute() => _game.RotatePiece();
}

public class DropCommand : IGameCommand
{
	private readonly Quadris _game;
	public DropCommand(Quadris game) => _game = game;
	public void Execute() => _game.HardDrop();
}

public class EscapeCommand : IGameCommand
{
	private readonly Quadris _game;
	public EscapeCommand(Quadris game) => _game = game;
	public void Execute() => _game.EscapeToMenu();
}

public class MenuUpCommand : IGameCommand
{
	private readonly Quadris _game;
	public MenuUpCommand(Quadris game) => _game = game;
	public void Execute() => _game.Menu.MoveUp();
}

public class MenuDownCommand : IGameCommand
{
	private readonly Quadris _game;
	public MenuDownCommand(Quadris game) => _game = game;
	public void Execute() => _game.Menu.MoveDown();
}

public class MenuSelectCommand : IGameCommand
{
	private readonly Quadris _game;
	public MenuSelectCommand(Quadris game) => _game = game;

	public void Execute()
	{
		var menu = _game.Menu;
		menu.Select();
	}
}

public class MenuBackCommand : IGameCommand
{
	private readonly Quadris _game;
	public MenuBackCommand(Quadris game) => _game = game;
	public void Execute() => _game.Menu.Back();
}

public class HighscoresBackCommand : IGameCommand
{
	private readonly Quadris _game;
	public HighscoresBackCommand(Quadris game) => _game = game;

	public void Execute()
	{
		_game.ShowMenu();
	}
}

public class PauseCommand : IGameCommand
{
	private readonly Quadris _game;
	public PauseCommand(Quadris game) => _game = game;
	public void Execute() => _game.ShowPause();
}

public class ResumeCommand : IGameCommand
{
	private readonly Quadris _game;
	public ResumeCommand(Quadris game) => _game = game;
	public void Execute() => _game.ResumeGame();
}
