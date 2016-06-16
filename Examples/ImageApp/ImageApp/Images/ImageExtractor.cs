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
    public class ImageExtractor : IExtractor
    {
        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            byte[] imageArray = ImageOps.GetByteArrayforImage(input.BaseStream);
            output.Set<byte[]>(0, imageArray);
            yield return output.AsReadOnly();
        }
    }
}



