using System;
using nrcgl.nrcgl.shapes;
using OpenTK;
using nrcgl.nrcgl;

namespace nrcgl
{
	public class GameActions
	{
		public static ShapeAction Move2XY(Shape3D shape, Move2XY move2xy)
		{
			return new ShapeAction (
				new Action<Shape3D, LifeTime, object> (
					(lhape3D, lifeTime, vec2XY) => {

						shape.Position = 
							new Vector3 (
								(-(move2xy.To.X * 2 - 270) / 270 - move2xy.From.X) * 2f / (float)lifeTime.Max,
								shape.Position.Y,
								(-(move2xy.To.Y * 2 - 404) / 404 - move2xy.From.Y) * 2f / (float)lifeTime.Max
							);
					}),
				new LifeTime (5),
				move2xy);
			
		}
	}
}

