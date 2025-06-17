using Microsoft.Xna.Framework;

using Quadris.Source;

public class MenuScreen : IScreen
{
	private readonly Quadris.Source.Quadris _game;
	private readonly Menu _menu;

	public MenuScreen(Quadris.Source.Quadris game, Menu menu)
	{
		_game = game;
		_menu = menu;
	}

	public void Enter() { }
	public void Exit() { }

	public void Update(GameTime gameTime) { }

	public void Draw(GameTime gameTime, Renderer? renderer)
	{
		if (_menu.CurrentView == MenuView.Main)
		{
			renderer?.DrawMenu((int)_menu.SelectedButton);
		}
		else if (_menu.CurrentView == MenuView.Highscores)
		{
			renderer?.DrawHighscores(_game.Highscore.Scores);
		}
	}
}
