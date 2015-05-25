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
		public Move2XY (Vector2 from, Vector2 to)
		{
			From = from;
			To = to;
		}
	}
}

