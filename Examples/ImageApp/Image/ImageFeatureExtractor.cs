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
    public class ImageFeatureExtractor : IExtractor
    {
        private int _scaleWidth, _scaleHeight;

        public ImageFeatureExtractor(int scaleWidth = 150, int scaleHeight = 150) 
        { _scaleWidth = scaleWidth; _scaleHeight = scaleHeight; }

        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            byte[] img = ImageOps.GetByteArrayforImage(input.BaseStream);
            if (output.Schema.IndexOf("image") != -1)
                output.Set<byte[]>("image", img);
            if (output.Schema.IndexOf("equipment_make") != -1)
                output.Set<string>("equipment_make", Images.ImageOps.getImageProperty(img, ImageProperties.equipment_make));
            if (output.Schema.IndexOf("equipment_model") != -1)
                output.Set<string>("equipment_model", Images.ImageOps.getImageProperty(img, ImageProperties.equipment_model));
            if (output.Schema.IndexOf("description") != -1)
                output.Set<string>("description", Images.ImageOps.getImageProperty(img, ImageProperties.description));
            if (output.Schema.IndexOf("copyright") != -1)
                output.Set<string>("copyright", Images.ImageOps.getImageProperty(img, ImageProperties.copyright));
            if (output.Schema.IndexOf("thumbnail") != -1)
                output.Set<byte[]>("thumbnail", Images.ImageOps.scaleImageTo(img, this._scaleWidth, this._scaleHeight));
            yield return output.AsReadOnly();
        }
    }
}



