﻿using System;
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
								(_move2xy as Move2XY).From.X + ((-((_move2xy as Move2XY).To.X * 3.8f - (_move2xy as Move2XY).ViewportWidth * 1.9f) / 
									((_move2xy as Move2XY).ViewportWidth)- (_move2xy as Move2XY).From.X)) * percentage,
								shape.Position.Y,
								(_move2xy as Move2XY).From.Y +((-((_move2xy as Move2XY).To.Y * 6f - (_move2xy as Move2XY).ViewportHeight * 3f) / 
									((_move2xy as Move2XY).ViewportHeight) - (_move2xy as Move2XY).From.Y) + 0.8f) * percentage
							);
					}),
				new LifeTime (4),
				move2xy);

		}
	}
}

