using System.Drawing;

namespace MP1.ConfigurationStudio
{
    internal static class Typography
    {
        public const string Family = "Segoe UI";
        public const string HeadingFamily = "Segoe UI Semibold";
        public const float Body = 10f;
        public const float Small = 9f;
        public const float Heading = 18f;
        public const float DialogBody = 10f;
        public const float NativeBody = 9f;

        public static Font Create(float points, FontStyle style)
        {
            return new Font(Family, points, style, GraphicsUnit.Point);
        }

        public static Font CreateHeading(float points)
        {
            try
            {
                return new Font(HeadingFamily, points, FontStyle.Regular, GraphicsUnit.Point);
            }
            catch (System.ArgumentException)
            {
                return Create(points, FontStyle.Bold);
            }
        }
    }
}
