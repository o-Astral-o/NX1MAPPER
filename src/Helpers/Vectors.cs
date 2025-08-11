using System;

namespace C2M
{
    /// <summary>
    /// A class to hold a double precision Vector2
    /// </summary>
    public class Vector2D
    {
        public double X { get; set; }

        public double Y { get; set; }

        public Vector2D()
        {
            X = 0.0;
            Y = 0.0;
        }

        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// A class to hold a double precision Vector3
    /// </summary>
    public class Vector3D : Vector2D
    {
        public double Z { get; set; }

        public Vector3D()
        {
            X = 0.0;
            Y = 0.0;
            Z = 0.0;
        }
        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float Max()
        {
            return (float)Math.Max(X, Math.Max(Y, Z));
        }

    }

    /// <summary>
    /// A class to hold a double precision Vector4
    /// </summary>
    public class Vector4D : Vector3D
    {
        public double W { get; set; }

        public Vector4D()
        {
            X = 0.0;
            Y = 0.0;
            Z = 0.0;
            W = 0.0;
        }
        
        public Vector4D(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Vector4D(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }
}
