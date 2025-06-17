using Microsoft.Xna.Framework;

namespace Quadris.Source;

public interface IScreen
{
	void Enter();
	void Exit();
	void Update(GameTime gameTime);
	void Draw(GameTime gameTime, Renderer? renderer);
}
