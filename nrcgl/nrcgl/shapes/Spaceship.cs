using System;
using nrcgl.nrcgl.shapes;
using nrcgl.nrcgl;

namespace nrcgl
{
	public class Spaceship : Shape3D
	{
		private static VertexsIndicesData vid;

		public Spaceship (string name, GLView glView): base(name, glView)
		{
			string modelName = "Spaceship_smooth.xml";

			if (vid is VertexsIndicesData) {

				VertexsIndicesData = vid;

			} else {
				var model = glView.mainActivity.Assets.Open (modelName);

				vid = Tools.DeserializeModelTask (model).Result;

				VertexsIndicesData = vid;
			}

			Load ("vShader_Torus.txt", "fShader_Torus.txt");
		}
	}
}

