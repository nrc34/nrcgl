using System;

namespace nrcgl.nrcgl.game
{
	public interface IGame
	{
		void Load();
		void Update();
		void Render();
		void OnTouch();
	}
}

