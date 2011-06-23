using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using DevExpress.XtraEditors;

namespace HuntingDog
{
    public class PictureButton:PictureEdit
    {
        bool _checked = false;
        public PictureButton()
        {
            Image = UncheckedImage;
           
            
        }

        protected override void OnGotFocus(EventArgs e)
        {
            
        }

        protected override void OnGotCapture()
        {
            base.OnGotCapture();
        }

        Image _unchecked;
        public Image UncheckedImage
        {
            get
            {
                return _unchecked;
            }
            set
            {
                _unchecked = value;
                UpdateCheck();
            }
        }

        public event Action CheckedStateChanged;
        Image _checkedImage;
        public Image CheckedImage
        {
            get
            {
                return _checkedImage;
            }
            set
            {
                _checkedImage = value;
                UpdateCheck();
            }
        }

       

        public bool Checked
        {
            get { return _checked; }
            set {

                _checked = value;
                UpdateCheck();
            }
        }


        public static Image MakeGrayscale3(Image original)
        {
            Image image = (Bitmap)original.Clone();
            //create a blank bitmap the same size as original
           

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(image);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][] 
                  {
                     new float[] {.3f, .3f, .3f, 0, 0},
                     new float[] {.59f, .59f, .59f, 0, 0},
                     new float[] {.11f, .11f, .11f, 0, 0},
                     new float[] {0, 0, 0, 1, 0},
                     new float[] {0, 0, 0, 0, 1}
                  });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return image;
        }

        void UpdateCheck()
        {
            if (Checked)
            {
                Image = CheckedImage;
                BackColor = System.Drawing.Color.Transparent;
                //BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                //SizeMode = PictureBoxSizeMode.CenterImage;
            }
            else
            {
                if (UncheckedImage == null)
                {
                    UncheckedImage = MakeGrayscale3(CheckedImage);
                }
                Image = UncheckedImage;

                BackColor = System.Drawing.Color.Gainsboro;
                //SizeMode = PictureBoxSizeMode.Zoom;
                //BorderStyle = System.Windows.Forms.BorderStyle.None;
            }

            if (CheckedStateChanged != null)
                CheckedStateChanged();

        }

        protected override void  OnMouseClick(MouseEventArgs e)
        {
            FocusOnMouseDown = false;  
            
            Checked = !Checked;
           
        }
     
    }
}
