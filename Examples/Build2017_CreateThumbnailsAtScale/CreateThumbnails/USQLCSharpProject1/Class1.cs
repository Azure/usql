using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Demo // Register as DemoAsm
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
    }
}