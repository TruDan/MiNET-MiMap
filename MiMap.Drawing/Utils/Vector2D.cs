namespace MiMap.Drawing.Utils
{
	public class Vector2D
	{
		public double X, Y;

		public Vector2D()
		{
			X = Y = 0.0;
		}

		public Vector2D(double x, double y)
		{
			X = x;
			Y = y;
		}

		public void Set(Vector2D v)
		{
			X = v.X;
			Y = v.Y;
		}

		public void Set(double x, double y)
		{
			X = x;
			Y = y;
		}

		public static Vector2D operator -(Vector2D v1, Vector2D v2)
		{
			return new Vector2D(v1.X - v2.X, v1.Y - v2.Y);
		}
		public static Vector2D operator +(Vector2D v1, Vector2D v2)
		{
			return new Vector2D(v1.X + v2.X, v1.Y + v2.Y);
		}
		public static Vector2D operator *(Vector2D v1, Vector2D v2)
		{
			return new Vector2D(v1.X * v2.X, v1.Y * v2.Y);
		}
		public static Vector2D operator /(Vector2D v1, Vector2D v2)
		{
			return new Vector2D(v1.X / v2.X, v1.Y / v2.Y);
		}


		public static Vector2D operator -(Vector2D v, double d)
		{
			return new Vector2D(v.X - d, v.Y - d);
		}

		public static Vector2D operator +(Vector2D v, double d)
		{
			return new Vector2D(v.X + d, v.Y + d);
		}

		public static Vector2D operator *(Vector2D v, double d)
		{
			return new Vector2D(v.X * d, v.Y * d);
		}

		public static Vector2D operator /(Vector2D v, double d)
		{
			return new Vector2D(v.X / d, v.Y / d);
		}

		public override string ToString()
		{
			return $"({X}, {Y})";
		}
	}
}
