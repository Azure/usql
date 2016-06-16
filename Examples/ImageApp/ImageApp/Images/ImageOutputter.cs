using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Images
{
    [SqlUserDefinedExtractor(AtomicFileProcessing = true)]
    public class ImageOutputter : IOutputter
    {
        public override void Output(IRow input, IUnstructuredWriter output)
        {
            var obj = input.Get<object>(0);
            byte[] imageArray = (byte[])obj;
            using (MemoryStream ms = new MemoryStream(imageArray))
            {
                var image = Image.FromStream(ms);
                image.Save(output.BaseStream, ImageFormat.Jpeg);
            }
        }
    }
}

