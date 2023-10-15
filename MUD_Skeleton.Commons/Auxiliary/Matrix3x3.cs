namespace MUD_Skeleton.Commons.Auxiliary
{
    public class Matrix3x3
    {
        public float M11 = 0;
        public float M12 = 0;
        public float M13 = 0;

        public float M21 = 0;
        public float M22 = 0;
        public float M23 = 0;

        public float M31 = 0;
        public float M32 = 0;
        public float M33 = 0;

        public Matrix3x3()
        {
        }

        public Matrix3x3(float m11, float m12, float m13, 
                        float m21, float m22, float m23, 
                        float m31, float m32, float m33)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M12 = m13;

            this.M21 = m21;
            this.M22 = m22;
            this.M22 = m23;

            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
        }

        //EXPERIMENTAL
        public Matrix3x3(float angles, TypeOfRotationY typeOfRotationY = TypeOfRotationY.Clockwise)
        {
            float cosTheta = (float)Math.Cos(UtilityAssistant.DegreesToRadians(angles));
            float sinTheta = (float)Math.Sin(UtilityAssistant.DegreesToRadians(angles));

            if (typeOfRotationY == TypeOfRotationY.Clockwise)
            {
                this.M11 = cosTheta;
                this.M12 = -sinTheta;
                this.M13 = 0;

                this.M21 = sinTheta;
                this.M22 = cosTheta;
                this.M23 = 0;

                this.M31 = 0;
                this.M32 = 0;
                this.M33 = 1;
            }
            else
            {
                this.M11 = cosTheta;
                this.M12 = sinTheta;
                this.M13 = 0;

                this.M21 = -sinTheta;
                this.M22 = cosTheta;
                this.M23 = 0;

                this.M31 = 0;
                this.M32 = 0;
                this.M33 = 1;
            }
        }

        public static System.Numerics.Vector3 operator *(Matrix3x3 f, System.Numerics.Vector3 g)
        {
            float x = (f.M11 * g.X) + (f.M12 * g.Y) + (f.M13 * g.Z);
            float y = (f.M21 * g.X) + (f.M22 * g.Y) + (f.M23 * g.Z);
            float z = (f.M31 * g.X) + (f.M32 * g.Y) + (f.M33 * g.Z);
            return new System.Numerics.Vector3(x, y, z);
        }
    }
}
