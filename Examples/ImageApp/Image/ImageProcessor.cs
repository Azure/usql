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

                // load image only once into memory per row
                using (StreamImage inImage = new StreamImage(img))
                {
                    output.SetColumnIfExists("equipment_make", inImage.getStreamImageProperty(ImageProperties.equipment_make));
                    output.SetColumnIfExists("equipment_model", inImage.getStreamImageProperty(ImageProperties.equipment_model));
                    output.SetColumnIfExists("description", inImage.getStreamImageProperty(ImageProperties.description));
                    output.SetColumnIfExists("copyright", inImage.getStreamImageProperty(ImageProperties.copyright));
                    output.SetColumnIfExists("thumbnail", inImage.scaleStreamImageTo(150, 150));
                }
                return output.AsReadOnly();
            }
        }
}


