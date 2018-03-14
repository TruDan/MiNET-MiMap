using System.Numerics;

namespace MiMap.Drawing.Utils
{
	public class Vector3D
	{
		public static Vector3D Zero
		{
			get
			{
				return new Vector3D(0, 0, 0);
			}
		}

		public double X, Y, Z;

		public Vector3D() { X = Y = Z = 0.0; }

		public Vector3D(double x, double y, double z)
		{
			this.X = x; this.Y = y; this.Z = z;
		}

		public Vector3D(Vector3D v)
		{
			this.X = v.X; this.Y = v.Y; this.Z = v.Z;
		}


		public void Set(Vector3D v)
		{
			X = v.X; Y = v.Y; Z = v.Z;
		}

		public void Subtract(Vector3D v)
		{
			X = X - v.X; Y = Y - v.Y; Z = Z - v.Z;
		}

		public void Add(Vector3D v)
		{
			X = X + v.X; Y = Y + v.Y; Z = Z + v.Z;
		}

		public double InnerProduct(Vector3D v)
		{
			return (v.X * X) + (v.Y * Y) + (v.Z * Z);
		}

		/* this = this X v */
		public void CrossProduct(Vector3D v)
		{
			double newx = (Y * v.Z) - (Z * v.Y);
			double newy = (Z * v.X) - (X * v.Z);
			double newz = (X * v.Y) - (Y * v.X);
			X = newx; Y = newy; Z = newz;
		}

		public override string ToString()
		{
			return "( " + X + ", " + Y + ", " + Z + " )";
		}

		public override bool Equals(object v)
		{
			if (v is Vector3D)
			{
				Vector3D vv = (Vector3D)v;
				return (vv.X == X) && (vv.Y == Y) && (vv.Z == Z);
			}
			return false;
		}


		public override int GetHashCode()
		{
			var hX = X.GetHashCode();
			var hY = X.GetHashCode();
			var hZ = X.GetHashCode();
			return (((hX << 5) + hX) ^ (((hY << 5) + hY) ^ hZ));
		}

		public static Vector3D operator -(Vector3D v, double d)
		{
			return new Vector3D(v.X - d, v.Y - d, v.Z - d);
		}

		public static Vector3D operator +(Vector3D v, double d)
		{
			return new Vector3D(v.X + d, v.Y + d, v.Z + d);
		}
		public static Vector3D operator *(Vector3D v, double d)
		{
			return new Vector3D(v.X * d, v.Y * d, v.Z * d);
		}
		public static Vector3D operator /(Vector3D v, double d)
		{
			return new Vector3D(v.X / d, v.Y / d, v.Z / d);
		}

		public static implicit operator Vector3(Vector3D v)
		{
			return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
		}
		public static implicit operator Vector3D(Vector3 v)
		{
			return new Vector3D((double)v.X, (double)v.Y, (double)v.Z);
		}

		public static Vector3D From(Vector3D x, Vector3D y, Vector3D z)
		{
			return new Vector3D(x.X, y.Y, z.Z);
		}
		public static Vector3D From(Vector3D v)
		{
			return new Vector3D(v.X, v.Y, v.Z);
		}
	}
}
