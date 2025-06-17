using Microsoft.Xna.Framework;
using Quadris.Source;

public class PauseScreen : IScreen
{
	private readonly Quadris.Source.Quadris _game;
	public PauseScreen(Quadris.Source.Quadris game) => _game = game;

	public void Enter() { }
	public void Exit() { }
	public void Update(GameTime gameTime) { }

	public void Draw(GameTime gameTime, Renderer? renderer)
	{
		// Draw the game view in the background
		renderer?.Draw(GameState.Game, _game.Session, _game.Highscore.Scores);

		// Draw the pause overlay on top
		renderer?.DrawPauseOverlay();
	}
}
