using Microsoft.Xna.Framework;

namespace Quadris
{
	public class Tetromino
	{
		public enum Type
		{
			I, J, L, O, S, T, Z
		}

		public int posx;
		public int posy;

		public Type type;
		public int rotation;

		public static Color GetColor(Tetromino.Type type)
		{
			switch (type)
			{
				case Tetromino.Type.I: // cyan
					return new Color(0xFFFF00);
				case Tetromino.Type.J: // blue
					return new Color(0xFF0000);
				case Tetromino.Type.L: // orange
					return new Color(0x00A5FF);
				case Tetromino.Type.O: // yellow
					return new Color(0x00FFFF);
				case Tetromino.Type.S: // green
					return new Color(0x00FF00);
				case Tetromino.Type.T: // magenta
					return new Color(0xFF00FF);
				case Tetromino.Type.Z: // red
					return new Color(0x0000FF);
				default:
					return new Color(0xFFFFFF);
			}
		}
	}
}
