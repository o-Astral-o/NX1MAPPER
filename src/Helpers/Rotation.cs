namespace C2M
{
    /// <summary>
    /// Rotation Utilities
    /// </summary>
    public class Rotation
    {
        /// <summary>
        /// Class to hold a 4x4 Matrix
        /// </summary>
        public class Matrix
        {
            /// <summary>
            /// Values
            /// </summary>
            public double[] Values = new double[16];

            /// <summary>
            /// Converts a Matrix to a Quaternion
            /// </summary>
            /// <returns>Resulting Quaternion</returns>
            public Vector4D ToQuaternion()
            {
                Vector4D result = new Vector4D(0, 0, 0, 1.0);

                double divisor;

                double transRemain = Values[0] + Values[5] + Values[10];

                if (transRemain > 0)
                {
                    divisor = Math.Sqrt(transRemain + 1.0) * 2.0;
                    result.W = 0.25 * divisor;
                    result.X = (Values[6] - Values[9]) / divisor;
                    result.Y = (Values[8] - Values[2]) / divisor;
                    result.Z = (Values[1] - Values[4]) / divisor;
                }
                else if ((Values[0] > Values[5]) && (Values[0] > Values[10]))
                {
                    divisor = Math.Sqrt(
                        1.0 + Values[0] - Values[5] - Values[10]) * 2.0;
                    result.W = (Values[6] - Values[9]) / divisor;
                    result.X = 0.25 * divisor;
                    result.Y = (Values[4] + Values[1]) / divisor;
                    result.Z = (Values[8] + Values[2]) / divisor;
                }
                else if (Values[5] > Values[10])
                {
                    divisor = Math.Sqrt(
                        1.0 + Values[5] - Values[0] - Values[10]) * 2.0;
                    result.W = (Values[8] - Values[2]) / divisor;
                    result.X = (Values[4] + Values[1]) / divisor;
                    result.Y = 0.25 * divisor;
                    result.Z = (Values[9] + Values[6]) / divisor;
                }
                else
                {
                    divisor = Math.Sqrt(
                        1.0 + Values[10] - Values[0] - Values[5]) * 2.0;
                    result.W = (Values[1] - Values[4]) / divisor;
                    result.X = (Values[8] + Values[2]) / divisor;
                    result.Y = (Values[9] + Values[6]) / divisor;
                    result.Z = 0.25 * divisor;
                }

                // Return resulting vector
                return result;
            }

            /// <summary>
            /// Converts a Matrix to Euler Angles
            /// </summary>
            /// <returns>Resulting Euler Vector</returns>
            public Vector3D ToEuler()
            {
                var quaternion = ToQuaternion();
                return QuatToEul(quaternion);
            }

        }

        /// <summary>
        /// Converts Euler value to degrees
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Value in degrees</returns>
        public static double ToDegrees(double value)
        {
            return (value / (2 * Math.PI)) * 360;
        }

        /// <summary>
        /// Converts Euler value to degrees
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Value in degrees</returns>
        public static Vector3D ToDegrees(Vector3D value)
        {
            // Return new vector
            return new Vector3D(
                ToDegrees(value.X),
                ToDegrees(value.Y),
                ToDegrees(value.Z)
                );
        }
        

        // Quat -> Euler
        const float EULER_HYPOT_EPSILON = 1.0e-4f;
        const double M_SQRT2 = 1.4142135623730951;

        public static Vector3D QuatToEul(Vector4D quat)
        {
            float[,] unitMat = new float[3, 3];
            float[] eul = new float[3];
                
            QuatToMat3(unitMat, quat);
            Mat3NormalizedToEul(eul, unitMat);
                
            var euler = new Vector3D(eul[0], eul[1], eul[2]);
            return ToDegrees(euler);
        }

        static void QuatToMat3(float[,] m, Vector4D q)
        {
            double q0 = M_SQRT2 * q.W;
            double q1 = M_SQRT2 * q.X;
            double q2 = M_SQRT2 * q.Y;
            double q3 = M_SQRT2 * q.Z;

            double qda = q0 * q1;
            double qdb = q0 * q2;
            double qdc = q0 * q3;
            double qaa = q1 * q1;
            double qab = q1 * q2;
            double qac = q1 * q3;
            double qbb = q2 * q2;
            double qbc = q2 * q3;
            double qcc = q3 * q3;

            m[0, 0] = (float)(1.0 - qbb - qcc);
            m[0, 1] = (float)(qdc + qab);
            m[0, 2] = (float)(-qdb + qac);

            m[1, 0] = (float)(-qdc + qab);
            m[1, 1] = (float)(1.0 - qaa - qcc);
            m[1, 2] = (float)(qda + qbc);

            m[2, 0] = (float)(qdb + qac);
            m[2, 1] = (float)(-qda + qbc);
            m[2, 2] = (float)(1.0 - qaa - qbb);
        }

        static Vector3D Mat3NormalizedToEul(float[] eul, float[,] mat)
        {
            float[] eul1 = new float[3];
            float[] eul2 = new float[3];

            Mat3NormalizedToEul2(mat, eul1, eul2);

            float sum1 = Math.Abs(eul1[0]) + Math.Abs(eul1[1]) + Math.Abs(eul1[2]);
            float sum2 = Math.Abs(eul2[0]) + Math.Abs(eul2[1]) + Math.Abs(eul2[2]);

            if (sum1 > sum2)
            {
                CopyV3V3(eul, eul2);
            }
            else
            {
                CopyV3V3(eul, eul1);
            }

            return new Vector3D(eul[0], eul[1], eul[2]);
        }

        public static void Mat3NormalizedToEul2(float[,] mat, float[] eul1, float[] eul2)
        {
            float cy = (float)Math.Sqrt(mat[0, 0] * mat[0, 0] + mat[0, 1] * mat[0, 1]);

            if (cy > EULER_HYPOT_EPSILON)
            {
                eul1[0] = (float)Math.Atan2(mat[1, 2], mat[2, 2]);
                eul1[1] = (float)Math.Atan2(-mat[0, 2], cy);
                eul1[2] = (float)Math.Atan2(mat[0, 1], mat[0, 0]);

                eul2[0] = (float)Math.Atan2(-mat[1, 2], -mat[2, 2]);
                eul2[1] = (float)Math.Atan2(-mat[0, 2], -cy);
                eul2[2] = (float)Math.Atan2(-mat[0, 1], -mat[0, 0]);
            }
            else
            {
                eul1[0] = (float)Math.Atan2(-mat[2, 1], mat[1, 1]);
                eul1[1] = (float)Math.Atan2(-mat[0, 2], cy);
                eul1[2] = 0.0f;

                CopyV3V3(eul2, eul1);
            }
        }

        public static void CopyV3V3(float[] dst, float[] src)
        {
            dst[0] = src[0];
            dst[1] = src[1];
            dst[2] = src[2];
        }
    }
}