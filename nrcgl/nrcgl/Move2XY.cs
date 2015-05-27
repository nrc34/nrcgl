using System;
using OpenTK;

namespace nrcgl
{
	public class Move2XY
	{
		public Vector2 From {
			get;
			set;
		}

		public Vector2 To {
			get;
			set;
		}

		public float ViewportHeight {
			get;
			set;
		}

		public float ViewportWidth {
			get;
			set;
		}

		public Move2XY (Vector2 from, Vector2 to, int viewportWidth, int viewportHeight)
		{
			From = from;
			To = to;
			ViewportWidth = (float)viewportWidth;
			ViewportHeight = (float)viewportHeight;
		}
	}
}

