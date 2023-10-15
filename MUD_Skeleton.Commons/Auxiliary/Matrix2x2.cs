namespace MUD_Skeleton.Commons.Auxiliary
{
    public enum TypeOfRotationY { CounterClockwise = 0, Clockwise = 1 }

    public class Matrix2x2
    {
        public float M11 = 0;
        public float M12 = 0;
        public float M21 = 0;
        public float M22 = 0;

        public Matrix2x2()
        {
        }

        public Matrix2x2(float m11, float m12, float m21, float m22)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M21 = m21;
            this.M22 = m22;
        }

        public Matrix2x2(float angles, TypeOfRotationY typeOfRotationY = TypeOfRotationY.Clockwise)
        {
            float cosTheta = (float)Math.Cos(UtilityAssistant.DegreesToRadians(angles));
            float sinTheta = (float)Math.Sin(UtilityAssistant.DegreesToRadians(angles));

            if (typeOfRotationY == TypeOfRotationY.Clockwise)
            {
                this.M11 = cosTheta;
                this.M12 = -sinTheta;
                this.M21 = sinTheta;
                this.M22 = cosTheta;
            }
            else
            {
                this.M11 = cosTheta;
                this.M12 = sinTheta;
                this.M21 = -sinTheta;
                this.M22 = cosTheta;
            }
        }

        public static System.Numerics.Vector2 operator *(Matrix2x2 f, System.Numerics.Vector2 g)
        {
            float x = (f.M11 * g.X) + (f.M12 * g.Y);
            float y = (f.M21 * g.X) + (f.M22 * g.Y);
            return new System.Numerics.Vector2(x, y);
        }
    }
}
