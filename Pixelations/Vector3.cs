namespace Pixelations
{
    public class Vector3
    {
        public float x, y, z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 operator+(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        public static Vector3 operator *(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
        public static Vector3 operator /(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
        public static Vector3 operator *(Vector3 a, float b)
        {
            return new Vector3(a.x * b, a.y * b, a.z * b);
        }
        public static Vector3 operator /(Vector3 a, float b)
        {
            return new Vector3(a.x / b, a.y / b, a.z / b);
        }
        public static Vector3 operator +(Vector3 a, float b)
        {
            return new Vector3(a.x + b, a.y + b, a.z + b);
        }
        public static Vector3 operator -(Vector3 a, float b)
        {
            return new Vector3(a.x - b, a.y - b, a.z - b);
        }

        public override string ToString()
        {
            return "V3: (" + x + ", " + y + ", " + z + ")";
        }

        public override bool Equals(object obj)
        {
            if (typeof(Vector3) != obj.GetType())
                return false;
            return Equals((Vector3)obj);
        }

        public bool Equals(Vector3 obj)
        {
            return x == obj.x && x == obj.x && x == obj.x;
        }
    }
}