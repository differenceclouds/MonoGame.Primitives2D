﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework {
	public static class Primitives2D {
		#region Private Members

		private static readonly Dictionary<String, List<Vector2>> circleCache = new Dictionary<string, List<Vector2>>();
		//private static readonly Dictionary<String, List<Vector2>> arcCache = new Dictionary<string, List<Vector2>>();
		private static Texture2D pixel;

		#endregion


		#region Private Methods
		private static void CreateThePixel(SpriteBatch spriteBatch) {
			pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
			pixel.SetData(new[] { Color.White });
		}


		/// <summary>
		/// Draws a list of connecting points
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// /// <param name="position">Where to position the points</param>
		/// <param name="points">The points to connect with lines</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		public static void DrawPoints(SpriteBatch spriteBatch, Vector2 position, List<Vector2> points, Color color, float thickness) {
			if (points.Count < 2)
				return;

			for (int i = 1; i < points.Count; i++) {
				DrawLine(spriteBatch, points[i - 1] + position, points[i] + position, color, thickness);
			}
		}


		/// <summary>
		/// Creates a list of vectors that represents a circle
		/// </summary>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="sides">The number of sides to generate</param>
		/// <returns>A list of vectors that, if connected, will create a circle</returns>
		private static List<Vector2> CreateCircle(double radius, int sides) {
			// Look for a cached version of this circle
			String circleKey = radius + "x" + sides;
			if (circleCache.ContainsKey(circleKey)) {
				return circleCache[circleKey];
			}

			List<Vector2> vectors = new List<Vector2>();

			const double max = 2.0 * Math.PI;
			double step = max / sides;

			for (double theta = 0.0; theta < max; theta += step) {
				vectors.Add(new Vector2((float)(radius * Math.Cos(theta)), (float)(radius * Math.Sin(theta))));
			}

			// then add the first vector again so it's a complete loop
			vectors.Add(new Vector2((float)(radius * Math.Cos(0)), (float)(radius * Math.Sin(0))));

			// Cache this circle so that it can be quickly drawn next time
			circleCache.Add(circleKey, vectors);

			return vectors;
		}


		/// <summary>
		/// Creates a list of vectors that represents an arc
		/// </summary>
		/// <param name="radius">The radius of the arc</param>
		/// <param name="sides">The number of sides to generate in the circle that this will cut out from</param>
		/// <param name="startingAngle">The starting angle of arc, 0 being to the east, increasing as you go clockwise</param>
		/// <param name="radians">The radians to draw, clockwise from the starting angle</param>
		/// <returns>A list of vectors that, if connected, will create an arc</returns>
		private static List<Vector2> CreateArc(float radius, int sides, float startingAngle, float radians) {
			List<Vector2> points = new List<Vector2>();
			points.AddRange(CreateCircle(radius, sides));
			points.RemoveAt(points.Count - 1); // remove the last point because it's a duplicate of the first

			// The circle starts at (radius, 0)
			double curAngle = 0.0;
			double anglePerSide = MathHelper.TwoPi / sides;

			// "Rotate" to the starting point
			while ((curAngle + (anglePerSide / 2.0)) < startingAngle) {
				curAngle += anglePerSide;

				// move the first point to the end
				points.Add(points[0]);
				points.RemoveAt(0);
			}

			// Add the first point, just in case we make a full circle
			points.Add(points[0]);

			// Now remove the points at the end of the circle to create the arc
			int sidesInArc = (int)(((radians) / anglePerSide) + 0.5);
			points.RemoveRange(sidesInArc + 1, points.Count - sidesInArc - 1);

			return points;
		}
		#endregion


		#region FillRectangle
		/// <summary>
		/// Draws a filled rectangle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="rect">The rectangle to draw</param>
		/// <param name="color">The color to draw the rectangle in</param>
		public static void FillRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color) {
			if (pixel == null) {
				CreateThePixel(spriteBatch);
			}

			// Simply use the function already there
			spriteBatch.Draw(pixel, rect, color);
		}


		/// <summary>
		/// Draws a filled rectangle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="rect">The rectangle to draw</param>
		/// <param name="color">The color to draw the rectangle in</param>
		/// <param name="angle">The angle in radians to draw the rectangle at</param>
		/// <param name="layerDepth">Depth from 0 to 1 when using sprite sorting</param>
		public static void FillRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color, float angle, float layerDepth = 0) {
			if (pixel == null) {
				CreateThePixel(spriteBatch);
			}

			spriteBatch.Draw(pixel, rect, null, color, angle, Vector2.Zero, SpriteEffects.None, layerDepth);
		}


		/// <summary>
		/// Draws a filled rectangle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="location">Where to draw</param>
		/// <param name="size">The size of the rectangle</param>
		/// <param name="color">The color to draw the rectangle in</param>
		public static void FillRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color) {
			FillRectangle(spriteBatch, location, size, color, 0.0f);
		}


		/// <summary>
		/// Draws a filled rectangle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="location">Where to draw</param>
		/// <param name="size">The size of the rectangle</param>
		/// <param name="angle">The angle in radians to draw the rectangle at</param>
		/// <param name="color">The color to draw the rectangle in</param>
		/// <param name="layerDepth">Depth from 0 to 1 when using sprite sorting</param>
		public static void FillRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color, float angle, float layerDepth = 0) {
			if (pixel == null) {
				CreateThePixel(spriteBatch);
			}

			// stretch the pixel between the two vectors
			spriteBatch.Draw(pixel,
							 location,
							 null,
							 color,
							 angle,
							 Vector2.Zero,
							 size,
							 SpriteEffects.None,
							 layerDepth);
		}


		/// <summary>
		/// Draws a filled rectangle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="x">The X coord of the left side</param>
		/// <param name="y">The Y coord of the upper side</param>
		/// <param name="w">Width</param>
		/// <param name="h">Height</param>
		/// <param name="color">The color to draw the rectangle in</param>
		public static void FillRectangle(this SpriteBatch spriteBatch, float x, float y, float w, float h, Color color, float layerDepth = 0) {
			FillRectangle(spriteBatch, new Vector2(x, y), new Vector2(w, h), color, 0.0f, layerDepth);
		}


		/// <summary>
		/// Draws a filled rectangle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="x">The X coord of the left side</param>
		/// <param name="y">The Y coord of the upper side</param>
		/// <param name="w">Width</param>
		/// <param name="h">Height</param>
		/// <param name="color">The color to draw the rectangle in</param>
		/// <param name="angle">The angle of the rectangle in radians</param>
		public static void FillRectangle(this SpriteBatch spriteBatch, float x, float y, float w, float h, Color color, float angle = 0, float layerDepth = 0) {
			FillRectangle(spriteBatch, new Vector2(x, y), new Vector2(w, h), color, angle, layerDepth);
		}
		#endregion


		#region DrawRectangle
		/// <summary>
		/// Draws a rectangle with the thickness provided
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="rect">The rectangle to draw</param>
		/// <param name="color">The color to draw the rectangle in</param>
		public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color) {
			DrawRectangle(spriteBatch, rect, color, 1.0f);
		}


		/// <summary>
		/// Draws a rectangle with the thickness provided
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="rect">The rectangle to draw</param>
		/// <param name="color">The color to draw the rectangle in</param>
		/// <param name="thickness">The thickness of the lines</param>
		public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color, float thickness) {

			// TODO: Handle rotations
			// TODO: Figure out the pattern for the offsets required and then handle it in the line instead of here

			DrawLine(spriteBatch, new Vector2(rect.X, rect.Y), new Vector2(rect.Right, rect.Y), color, thickness); // top
			DrawLine(spriteBatch, new Vector2(rect.X + 1f, rect.Y), new Vector2(rect.X + 1f, rect.Bottom + thickness), color, thickness); // left
			DrawLine(spriteBatch, new Vector2(rect.X, rect.Bottom), new Vector2(rect.Right, rect.Bottom), color, thickness); // bottom
			DrawLine(spriteBatch, new Vector2(rect.Right + 1f, rect.Y), new Vector2(rect.Right + 1f, rect.Bottom + thickness), color, thickness); // right
		}


		/// <summary>
		/// Draws a rectangle with the thickness provided
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="location">Where to draw</param>
		/// <param name="size">The size of the rectangle</param>
		/// <param name="color">The color to draw the rectangle in</param>
		public static void DrawRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color) {
			DrawRectangle(spriteBatch, new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color, 1.0f);
		}


		/// <summary>
		/// Draws a rectangle with the thickness provided
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="location">Where to draw</param>
		/// <param name="size">The size of the rectangle</param>
		/// <param name="color">The color to draw the rectangle in</param>
		/// <param name="thickness">The thickness of the line</param>
		public static void DrawRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color, float thickness) {
			DrawRectangle(spriteBatch, new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color, thickness);
		}
		#endregion


		#region DrawLine
		/// <summary>
		/// Draws a line from point1 to point2 with an offset
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="x1">The X coord of the first point</param>
		/// <param name="y1">The Y coord of the first point</param>
		/// <param name="x2">The X coord of the second point</param>
		/// <param name="y2">The Y coord of the second point</param>
		/// <param name="color">The color to use</param>
		public static void DrawLine(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color, float layerDepth = 0) {
			DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), color, 1.0f, layerDepth);
		}


		/// <summary>
		/// Draws a line from point1 to point2 with an offset
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="x1">The X coord of the first point</param>
		/// <param name="y1">The Y coord of the first point</param>
		/// <param name="x2">The X coord of the second point</param>
		/// <param name="y2">The Y coord of the second point</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the line</param>
		public static void DrawLine(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color, float thickness, float layerDepth = 0) {
			DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), color, thickness, layerDepth);
		}


		/// <summary>
		/// Draws a line from point1 to point2 with an offset
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="point1">The first point</param>
		/// <param name="point2">The second point</param>
		/// <param name="color">The color to use</param>
		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float layerDepth = 0) {
			DrawLine(spriteBatch, point1, point2, color, 1.0f, layerDepth);
		}


		/// <summary>
		/// Draws a line from point1 to point2 with an offset
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="point1">The first point</param>
		/// <param name="point2">The second point</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the line</param>
		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness, float layerDepth = 0) {
			// calculate the distance between the two vectors
			float distance = Vector2.Distance(point1, point2);

			// calculate the angle between the two vectors
			float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

			DrawLine(spriteBatch, point1, distance, angle, color, thickness, layerDepth);
		}


		/// <summary>
		/// Draws a line from point1 to point2 with an offset
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="point">The starting point</param>
		/// <param name="length">The length of the line</param>
		/// <param name="angle">The angle of this line from the starting point in radians</param>
		/// <param name="color">The color to use</param>
		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float layerDepth = 0) {
			DrawLine(spriteBatch, point, length, angle, color, 1.0f, layerDepth);
		}


		/// <summary>
		/// Draws a line from point1 to point2 with an offset
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="point">The starting point</param>
		/// <param name="length">The length of the line</param>
		/// <param name="angle">The angle of this line from the starting point</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the line</param>
		/// <param name="layerDepth">Draw depth from 0 to 1 when using sprite sorting</param>
		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness, float layerDepth = 0) {
			if (pixel == null) {
				CreateThePixel(spriteBatch);
			}

			// stretch the pixel between the two vectors
			spriteBatch.Draw(pixel,
							 point,
							 null,
							 color,
							 angle,
							 Vector2.Zero,
							 new Vector2(length, thickness),
							 SpriteEffects.None,
							 layerDepth);
		}


		public static void DrawLineBresenham(this SpriteBatch spriteBatch, int x1, int y1, int x2, int y2, Color color, float layerDepth = 0) {
			int dx = Math.Abs(x2 - x1), slope_x = x1 < x2 ? 1 : -1;
			int dy = -Math.Abs(y2 - y1), slope_y = y1 < y2 ? 1 : -1;
			int err = dx + dy;
			int e2;

			while (true) {
				PutPixel(spriteBatch, new(x1, y1), color);
				if (x1 == x2 && y1 == y2) break;
				e2 = 2 * err;
				if (e2 >= dy) { err += dy; x1 += slope_x; }
				if (e2 <= dx) { err += dx; y1 += slope_y; }
			}
		}

		public static void DrawLineBresenham(this SpriteBatch spriteBatch, Point point1, Point point2, Color color, float layerDepth = 0) {
			DrawLineBresenham(spriteBatch, point1.X, point1.Y, point2.X, point2.Y, color);
		}
		#endregion


		#region PutPixel
		public static void PutPixel(this SpriteBatch spriteBatch, float x, float y, Color color) {
			PutPixel(spriteBatch, new Vector2(x, y), color);
		}


		public static void PutPixel(this SpriteBatch spriteBatch, Vector2 position, Color color) {
			if (pixel == null) {
				CreateThePixel(spriteBatch);
			}

			spriteBatch.Draw(pixel, position, color);
		}
		#endregion


		#region DrawCircle

		/// <summary>
		/// Draw a filled circle. Uses different method than non-filled, so sides are not a parameter.
		/// Adapted from http://fredericgoset.ovh/mathematiques/courbes/en/filled_circle.html.
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="center">The center of the circle</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="color">The color of the circle</param>
		public static void FillCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, Color color, float layerDepth = 0) {
			float x = 0;
			float y = radius;
			float m = 5 - 4 * radius;

			while (x <= y) {
				DrawLine(spriteBatch, center.X - y, center.Y - x, center.X + y, center.Y - x, color, layerDepth);
				DrawLine(spriteBatch, center.X - y, center.Y + x, center.X + y, center.Y + x, color, layerDepth);

				if (m > 0) {
					DrawLine(spriteBatch, center.X - x, center.Y - y, center.X + x, center.Y - y, color, layerDepth);
					DrawLine(spriteBatch, center.X - x, center.Y + y, center.X + x, center.Y + y, color, layerDepth);
					y--;
					m -= 8 * y;
				}
				x++;
				m += 8 * x + 4;
			}
		}





		/// <summary>
		/// Draw a circle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="center">The center of the circle</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="sides">The number of sides to generate</param>
		/// <param name="color">The color of the circle</param>
		/// <param name="thickness">The thickness of the lines used</param>
		public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, Color color, float thickness = 1.0f) {
			DrawPoints(spriteBatch, center, CreateCircle(radius, sides), color, thickness);
		}



		/// <summary>
		/// Draw a circle
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="x">The center X of the circle</param>
		/// <param name="y">The center Y of the circle</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="sides">The number of sides to generate</param>
		/// <param name="color">The color of the circle</param>
		/// <param name="thickness">The thickness of the lines used</param>
		public static void DrawCircle(this SpriteBatch spriteBatch, float x, float y, float radius, int sides, Color color, float thickness = 1.0f) {
			DrawPoints(spriteBatch, new Vector2(x, y), CreateCircle(radius, sides), color, thickness);
		}

		public static void DrawCircleBresenham(this SpriteBatch spriteBatch, int xc, int yc, int r, Color color) {
			int x = -r, y = 0, err = 2 - 2 * r;

			do {
				PutPixel(spriteBatch, new(xc - x, yc + y), color);
				PutPixel(spriteBatch, new(xc - y, yc - x), color);
				PutPixel(spriteBatch, new(xc + x, yc - y), color);
				PutPixel(spriteBatch, new(xc + y, yc + x), color);
				r = err;
				if (r <= y) err += ++y * 2 + 1;
				if (r > x || err > y) err += ++x * 2 + 1;
			} while (x < 0);
		}

		public static void DrawCircleBresenham(this SpriteBatch spriteBatch, Vector2 center, float radius, Color color) {
			DrawCircleBresenham(spriteBatch, (int)center.X, (int)center.Y, (int)radius, color);
		}
		#endregion



		#region DrawEllipse
		/// <summary>
		/// Draw a filled ellipse from a bounding rectangle.
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="rect">the rectangle which bounds the ellipse</param>
		/// <param name="color">the color of the ellipse</param>
		public static void DrawEllipse(this SpriteBatch spriteBatch, Rectangle rect, Color color) {
			DrawEllipse(spriteBatch, rect.X + rect.Width / 2, rect.Y + rect.Height / 2, rect.Width / 2, rect.Height / 2, color);
		}

		public static void FillEllipse(this SpriteBatch spriteBatch, Rectangle rect, Color color) {
			FillEllipse(spriteBatch, rect.X + rect.Width / 2, rect.Y + rect.Height / 2, rect.Width / 2, rect.Height / 2, color);
		}


		public static void DrawEllipse(this SpriteBatch spriteBatch, float xc, float yc, float rx, float ry, Color color) {
			var points = CreateEllipse(xc, yc, rx, ry);
			foreach (var p in points) {
				PutPixel(spriteBatch, p, color);
			}
		}

		public static void FillEllipse(this SpriteBatch spriteBatch, float xc, float yc, float rx, float ry, Color color) {
			var points = CreateEllipse(xc, yc, rx, ry);
			DrawPoints(spriteBatch, Vector2.Zero, points, color, 1);
		}

		/// <param name="xc">center X of the ellipse</param>
		/// <param name="yc">center Y of the ellipse</param>
		/// <param name="rx">horizontal radius of the ellipse</param>
		/// <param name="ry">vertical radius of the ellipse</param>
		private static List<Vector2> CreateEllipse(float xc, float yc, float rx, float ry) {
			List<Vector2> points = new();

			float dx, dy, d1, d2, x, y;
			x = 0;
			y = ry;
			d1 = (ry * ry) - (rx * rx * ry) + (0.25f * rx * rx);
			dx = 2 * ry * ry * x;
			dy = 2 * rx * rx * y;
			// For region 1
			while (dx < dy) {
				points.Add(new(x + xc, y + yc));
				points.Add(new(-x + xc, y + yc));
				points.Add(new(x + xc, -y + yc));
				points.Add(new(-x + xc, -y + yc));

				if (d1 < 0) {
					x++;
					dx = dx + (2 * ry * ry);
					d1 = d1 + dx + (ry * ry);
				} else {
					x++;
					y--;
					dx = dx + (2 * ry * ry);
					dy = dy - (2 * rx * rx);
					d1 = d1 + dx - dy + (ry * ry);
				}
			}

			// Decision parameter of region 2
			d2 = ((ry * ry) * ((x + 0.5f) * (x + 0.5f)))
				+ ((rx * rx) * ((y - 1) * (y - 1)))
				- (rx * rx * ry * ry);

			// Plotting points of region 2
			while (y >= 0) {
				points.Add(new(x + xc, y + yc));
				points.Add(new(-x + xc, y + yc));
				points.Add(new(x + xc, -y + yc));
				points.Add(new(-x + xc, -y + yc));

				if (d2 > 0) {
					y--;
					dy = dy - (2 * rx * rx);
					d2 = d2 + (rx * rx) - dy;
				} else {
					y--;
					x++;
					dx = dx + (2 * ry * ry);
					dy = dy - (2 * rx * rx);
					d2 = d2 + dx - dy + (rx * rx);
				}
			}
			return points;
		}

		#endregion


		#region DrawArc

		/// <summary>
		/// Draw a arc
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="center">The center of the arc</param>
		/// <param name="radius">The radius of the arc</param>
		/// <param name="sides">The number of sides to generate</param>
		/// <param name="startingAngle">The starting angle of arc, 0 being to the east, increasing as you go clockwise</param>
		/// <param name="radians">The number of radians to draw, clockwise from the starting angle</param>
		/// <param name="color">The color of the arc</param>
		public static void DrawArc(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, float startingAngle, float radians, Color color) {
			DrawArc(spriteBatch, center, radius, sides, startingAngle, radians, color, 1.0f);
		}


		/// <summary>
		/// Draw a arc
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="center">The center of the arc</param>
		/// <param name="radius">The radius of the arc</param>
		/// <param name="sides">The number of sides to generate</param>
		/// <param name="startingAngle">The starting angle of arc, 0 being to the east, increasing as you go clockwise</param>
		/// <param name="radians">The number of radians to draw, clockwise from the starting angle</param>
		/// <param name="color">The color of the arc</param>
		/// <param name="thickness">The thickness of the arc</param>
		public static void DrawArc(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, float startingAngle, float radians, Color color, float thickness = 1.0f) {
			List<Vector2> arc = CreateArc(radius, sides, startingAngle, radians);
			DrawPoints(spriteBatch, center, arc, color, thickness);
		}

		#endregion


	}
}
