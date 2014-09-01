using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;

namespace Krkal.GEn
{
	static class MatrixUtilities
	{
		public static Matrix CreateFromColumns(Vector3 v1, Vector3 v2, Vector3 v3) {
			Matrix m = new Matrix();
			m.M11 = v1.X;
			m.M21 = v1.Y;
			m.M31 = v1.Z;
			m.M41 = 0;

			m.M12 = v2.X;
			m.M22 = v2.Y;
			m.M32 = v2.Z;
			m.M42 = 0;

			m.M13 = v3.X;
			m.M23 = v3.Y;
			m.M33 = v3.Z;
			m.M43 = 0;

			m.M14 = 0;
			m.M24 = 0;
			m.M34 = 0;
			m.M44 = 1;
			return m;
		}

		public static Matrix CreateFromRows(Vector3 v1, Vector3 v2, Vector3 v3) {
			Matrix m = new Matrix();
			m.M11 = v1.X;
			m.M12 = v1.Y;
			m.M13 = v1.Z;
			m.M14 = 0;

			m.M21 = v2.X;
			m.M22 = v2.Y;
			m.M23 = v2.Z;
			m.M24 = 0;

			m.M31 = v3.X;
			m.M32 = v3.Y;
			m.M33 = v3.Z;
			m.M34 = 0;

			m.M41 = 0;
			m.M42 = 0;
			m.M43 = 0;
			m.M44 = 1;
			return m;
		}

	}
}
