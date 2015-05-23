using System;
using OpenTK;

namespace nrcgl
{
	public interface IWGameable
	{
		void Load(string vShaderName, string fShaderName);
		void Update(Camera camera, Matrix4 ProjectionMatrix);
		void Render();
	}
}

