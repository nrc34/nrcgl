using System;
using nrcgl.nrcgl.shapes;
using OpenTK;
using nrcgl.nrcgl;

namespace nrcgl
{
	public class GameActions
	{
		public static ShapeAction Move2XY(Move2XY move2xy)
		{
			return new ShapeAction (
				new Action<Shape3D, LifeTime, object> (
					(shape, lifeTime, _move2xy) => {

						float percentage = (float)lifeTime.Counter / 
										   (float)lifeTime.Max;

						shape.Position = 
							new Vector3 (
								(_move2xy as Move2XY).From.X + ((-((_move2xy as Move2XY).To.X * 2f - 270f - 270f) / 
									(float)270 - (_move2xy as Move2XY).From.X)) * percentage,
								shape.Position.Y,
								(_move2xy as Move2XY).From.Y +((-((_move2xy as Move2XY).To.Y * 2f - 404f - 600f) / 
									(float)404 - (_move2xy as Move2XY).From.Y)) * percentage
							);
					}),
				new LifeTime (5),
				move2xy);

		}
	}
}

