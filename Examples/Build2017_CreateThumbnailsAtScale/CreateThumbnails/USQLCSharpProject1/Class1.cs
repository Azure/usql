using System.IO;

// Register as DemoAsm
namespace Demo 
{
    public static class Helpers
    {
        public static byte[] CreateThumbnail(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                var bmp = new System.Drawing.Bitmap(ms);
                var resized = new System.Drawing.Bitmap(bmp, new System.Drawing.Size(bmp.Width / 4, bmp.Height / 4));
                var converter = new System.Drawing.ImageConverter();
                var outbytes = (byte[])converter.ConvertTo(resized, typeof(byte[]));
                return outbytes;
            }
        }

        public static byte[] CreateThumbnailNull(byte[] bytes)
        {
            return null;
        }
    }
}