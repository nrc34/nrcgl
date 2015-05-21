using System;
using nrcgl.nrcgl.shapes;

namespace nrcgl
{
	public class Torus : Shape3D
	{
		public Torus (string name,GLView glView): base(name, glView)
		{
			Load ("Torus3D_smooth.xml", "vShader_Torus.txt", "fShader_Torus.txt");
		}
	}
}

