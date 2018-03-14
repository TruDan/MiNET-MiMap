using System;

namespace MiMap.Drawing.Utils
{
	public class Matrix3D
	{

		private double _m11, _m12, _m13, _m21, _m22, _m23, _m31, _m32, _m33;

		public Matrix3D()
		{
			_m11 = _m22 = _m33 = 1.0;
			_m12 = _m13 = _m21 = _m23 = _m31 = _m32 = 0.0;
		}
		public Matrix3D(double m11, double m12, double m13, double m21, double m22, double m23, double m31, double m32, double m33)
		{
			_m11 = m11; _m12 = m12; _m13 = m13;
			_m21 = m21; _m22 = m22; _m23 = m23;
			_m31 = m31; _m32 = m32; _m33 = m33;
		}
		public void Multiply(Matrix3D mat)
		{
			var newM11 = mat._m11 * _m11 + mat._m12 * _m21 + mat._m13 * _m31;
			var newM12 = mat._m11 * _m12 + mat._m12 * _m22 + mat._m13 * _m32;
			var newM13 = mat._m11 * _m13 + mat._m12 * _m23 + mat._m13 * _m33;
			var newM21 = mat._m21 * _m11 + mat._m22 * _m21 + mat._m23 * _m31;
			var newM22 = mat._m21 * _m12 + mat._m22 * _m22 + mat._m23 * _m32;
			var newM23 = mat._m21 * _m13 + mat._m22 * _m23 + mat._m23 * _m33;
			var newM31 = mat._m31 * _m11 + mat._m32 * _m21 + mat._m33 * _m31;
			var newM32 = mat._m31 * _m12 + mat._m32 * _m22 + mat._m33 * _m32;
			var newM33 = mat._m31 * _m13 + mat._m32 * _m23 + mat._m33 * _m33;
			_m11 = newM11; _m12 = newM12; _m13 = newM13;
			_m21 = newM21; _m22 = newM22; _m23 = newM23;
			_m31 = newM31; _m32 = newM32; _m33 = newM33;
		}
		public void Scale(double s1, double s2, double s3)
		{
			var scalemat = new Matrix3D(s1, 0, 0, 0, s2, 0, 0, 0, s3);
			Multiply(scalemat);
		}
		public void RotateXY(double rotDeg)
		{
			var rotRad = MathUtils.ToRadians(rotDeg);
			var sinRot = Math.Sin(rotRad);
			var cosRot = Math.Cos(rotRad);
			var rotmat = new Matrix3D(cosRot, sinRot, 0, -sinRot, cosRot, 0, 0, 0, 1);
			Multiply(rotmat);
		}
		public void RotateXZ(double rotDeg)
		{
			var rotRad = MathUtils.ToRadians(rotDeg);
			var sinRot = Math.Sin(rotRad);
			var cosRot = Math.Cos(rotRad);
			var rotmat = new Matrix3D(cosRot, 0, -sinRot, 0, 1, 0, sinRot, 0, cosRot);
			Multiply(rotmat);
		}
		public void RotateYZ(double rotDeg)
		{
			var rotRad = MathUtils.ToRadians(rotDeg);
			var sinRot = Math.Sin(rotRad);
			var cosRot = Math.Cos(rotRad);
			var rotmat = new Matrix3D(1, 0, 0, 0, cosRot, sinRot, 0, -sinRot, cosRot);
			Multiply(rotmat);
		}
		public void ShearZ(double xFact, double yFact)
		{
			var shearmat = new Matrix3D(1, 0, 0, 0, 1, 0, xFact, yFact, 1);
			Multiply(shearmat);
		}
		public void Transform(double[] v)
		{
			var v1 = _m11 * v[0] + _m12 * v[1] + _m13 * v[2];
			var v2 = _m21 * v[0] + _m22 * v[1] + _m23 * v[2];
			var v3 = _m31 * v[0] + _m32 * v[1] + _m33 * v[2];
			v[0] = v1; v[1] = v2; v[2] = v3;
		}
		public void Transform(Vector3D v)
		{
			var v1 = _m11 * v.X + _m12 * v.Y + _m13 * v.Z;
			var v2 = _m21 * v.X + _m22 * v.Y + _m23 * v.Z;
			var v3 = _m31 * v.X + _m32 * v.Y + _m33 * v.Z;
			v.X = v1; v.Y = v2; v.Z = v3;
		}
		public void Transform(Vector3D v, out Vector3D outv)
		{
			outv = new Vector3D(
				_m11 * v.X + _m12 * v.Y + _m13 * v.Z,
				_m21 * v.X + _m22 * v.Y + _m23 * v.Z,
				_m31 * v.X + _m32 * v.Y + _m33 * v.Z
			);
		}

		public override string ToString()
		{
			return "[ [" + _m11 + " " + _m12 + " " + _m13 + "] [" + _m21 + " " + _m22 + " " + _m23 + "] [" + _m31 + " " + _m32 + " " + _m33 + "] ]";
		}
	}
}
