using Microsoft.Xna.Framework;

namespace Quadris.Source;

public class GameScreen : IScreen
{
	private readonly Quadris _game;

	public GameScreen(Quadris game)
	{
		_game = game;
	}

	public void Enter()
	{
		// No-op: game state is already reset by Quadris.ShowGame()
	}

	public void Exit()
	{
		// No-op for now, but could be used for cleanup if needed
	}

	public void Update(GameTime gameTime)
	{
		// Game logic is handled by Quadris.Update, so nothing needed here
	}

	public void Draw(GameTime gameTime, Renderer? renderer)
	{
		var session = _game.Session;
		renderer?.Draw(GameState.Game, session, _game.Highscore.Scores);
	}
}
