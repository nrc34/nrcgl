using System;

namespace nrcgl
{
	public class Torus : Shape3D
	{
		public Torus (GLView glView): base(glView)
		{
			Load ("Torus3D_smooth.xml", "vShader_Torus.txt", "fShader_Torus.txt");
		}
	}
}

