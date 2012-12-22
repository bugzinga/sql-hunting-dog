
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace HuntingDog
{
    public sealed class ImageTranslator : System.Windows.Forms.AxHost
    {
        #region Constructors
        /// <summary>
        /// Constructs a new instance of the <see cref="ImageTranslator" /> class.
        /// </summary>
        private ImageTranslator()
            : base(string.Empty)
        {
        }
        #endregion

        #region Methods
        public static stdole.StdPicture GetIPictureDisp(Image image)
        {
            return (stdole.StdPicture)GetIPictureDispFromPicture(image);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static Image GetImage(object pictureDisp)
        {
            return GetPictureFromIPictureDisp(pictureDisp);
        }

        public static stdole.StdPicture GetIPictureDispMask(Image image, Color mask, Color unmask, params Color[] toMask)
        {
            return GetIPictureDisp(GetMask(image, mask, unmask, toMask));
        }

        public static Image GetMask(Image image, Color mask, Color unmask, params Color[] toMask)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            if (toMask == null)
                throw new ArgumentNullException("toMask");

            List<Color> sources = new List<Color>(toMask);

            bool useUnmask = unmask != Color.Empty;

            Bitmap bitmap = new Bitmap(image);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixel = bitmap.GetPixel(x, y);

                    if (sources.Exists(delegate(Color color)
                    {
                        // Bitmap.GetPixel does not return named colors, and if a caller specifies a named color
                        // then Color.Equals will actually return false.  Hence, this code.
                        return color.R == pixel.R && color.G == pixel.G && color.B == pixel.B && color.A == pixel.A;
                    }))
                        bitmap.SetPixel(x, y, mask);
                    else if (useUnmask)
                        bitmap.SetPixel(x, y, unmask);
                }
            }

            return bitmap;
        }
        #endregion
    }
}