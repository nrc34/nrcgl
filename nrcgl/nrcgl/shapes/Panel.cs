using System;
using nrcgl.nrcgl.shapes;
using nrcgl.nrcgl;

namespace nrcgl
{
	public class Panel : Shape3D
	{
		private static VertexsIndicesData vid;

		public Panel (string name, GLView glView): base(name, glView)
		{
			string modelName = "Panel3D.xml";

			if (vid is VertexsIndicesData) {

				VertexsIndicesData = vid;

			} else {
				var model = glView.mainActivity.Assets.Open (modelName);

				vid = Tools.DeserializeModel (model);

				VertexsIndicesData = vid;
			}

			Load ("vShader_Torus.txt", "fShader_Torus.txt");
		}
	}
}

