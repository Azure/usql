using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Images
{
    public class ImageProcessor : IProcessor
        { 
            public override IRow Process(IRow input, IUpdatableRow output)
            {
                var img = input.Get<byte[]>("image_data");

                if (output.Schema.IndexOf("equipment_make") != -1)
                    output.Set<string>("equipment_make", Images.ImageOps.getImageProperty(img, ImageProperties.equipment_make)); 
                if (output.Schema.IndexOf("equipment_model") != -1)
                    output.Set<string>("equipment_model", Images.ImageOps.getImageProperty(img, ImageProperties.equipment_model));
                if (output.Schema.IndexOf("description") != -1)
                    output.Set<string>("description", Images.ImageOps.getImageProperty(img, ImageProperties.description));
                if (output.Schema.IndexOf("copyright") != -1)
                    output.Set<string>("copyright", Images.ImageOps.getImageProperty(img, ImageProperties.copyright));
                if (output.Schema.IndexOf("thumbnail") != -1)
                    output.Set<byte[]>("thumbnail", Images.ImageOps.scaleImageTo(img, 150, 150));
                return output.AsReadOnly();
            }
        }
}


