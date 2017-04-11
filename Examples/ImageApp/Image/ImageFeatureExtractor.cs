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
    public static class UpdatableRowExtensions
    {
        public static void SetColumnIfRequested<T>(this IUpdatableRow source, string colName, Func<T> expr)
        {
            var colIdx = source.Schema.IndexOf(colName);
            if (colIdx != -1)
            { source.Set<T>(colIdx, expr()); }
        } 
    }

    [SqlUserDefinedExtractor(AtomicFileProcessing = true)]
    public class ImageFeatureExtractor : IExtractor
    {
        private int _scaleWidth, _scaleHeight;

        public ImageFeatureExtractor(int scaleWidth = 150, int scaleHeight = 150) 
        { _scaleWidth = scaleWidth; _scaleHeight = scaleHeight; }

       public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
       {
            byte[] img = ImageOps.GetByteArrayforImage(input.BaseStream);

            // load image only once into memory per row
            using (StreamImage inImage = new StreamImage(img))
            {
                output.SetColumnIfRequested("image", () => img);
                output.SetColumnIfRequested("equipment_make", () => inImage.getStreamImageProperty(ImageProperties.equipment_make));
                output.SetColumnIfRequested("equipment_model", () => inImage.getStreamImageProperty(ImageProperties.equipment_model));
                output.SetColumnIfRequested("description", () => inImage.getStreamImageProperty(ImageProperties.description));
                output.SetColumnIfRequested("copyright", () => inImage.getStreamImageProperty(ImageProperties.copyright));
                output.SetColumnIfRequested("thumbnail", () => inImage.scaleStreamImageTo(this._scaleWidth, this._scaleHeight));
            }
            yield return output.AsReadOnly();
        }
    }
}



