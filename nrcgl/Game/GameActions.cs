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
								shape.Position.X + (-(move2xy.To.X * 2f - 270f - 270f) / (float)270 - move2xy.From.X) / (float)lifeTime.Max,
								shape.Position.Y,
								shape.Position.Z +(-(move2xy.To.Y * 2f - 404f - 600f) / (float)404 - move2xy.From.Y) / (float)lifeTime.Max
							);
					}),
				new LifeTime (5),
				move2xy);
			
		}
	}
}

