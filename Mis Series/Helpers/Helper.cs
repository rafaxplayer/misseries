using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;


namespace Mis_Series
{
    public static class Helper
    {
        public static string URLBASE_SERIE = "http://seriesdanko.com/serie.php?serie=";
        public static string URLBASE_CAPITULO = "http://seriesdanko.com/";
        public static string CONNECTION_STRING = "Data Source={0};Version=3;";

        public static RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        public static Icon GetIconNum(int num)
        {
            Icon icon = null;
            string text;
            try
            {

                //Create bitmap, kind of canvas
                Bitmap bitmap = new Bitmap(32, 32);

                icon = new Icon(@"tv.ico");

                System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 10, FontStyle.Bold);

                System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);

                System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
                System.Drawing.SolidBrush drawBrushBlue = new System.Drawing.SolidBrush(System.Drawing.Color.OrangeRed);
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;

                graphics.DrawIcon(icon, 0, 0);


                if (!(num > 0))
                {
                    return icon;
                }
                text = num.ToString();
                if (num < 10)
                {
                    graphics.FillEllipse(drawBrushBlue, new Rectangle(15, 12, 16, 16));
                    graphics.DrawString(num.ToString(), drawFont, drawBrush, 17, 12);
                }
                else if (num > 9)
                {

                    if (num > 99)
                    {
                        text = num.ToString().Remove(2);
                    }

                    //Debug.WriteLine("text lenght >1:" + num.ToString().Length, "mis_series");
                    graphics.FillEllipse(drawBrushBlue, new Rectangle(15, 12, 18, 16));
                    graphics.DrawString(text, drawFont, drawBrush, 15, 12);
                }


                //To Save icon to disk
                bitmap.Save("ico.ico", System.Drawing.Imaging.ImageFormat.Icon);

                icon = Icon.FromHandle(bitmap.GetHicon());

                drawFont.Dispose();
                drawBrush.Dispose();
                drawBrushBlue.Dispose();
                graphics.Dispose();
                bitmap.Dispose();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message, "mis_series");
            }

            return icon;

        }



        public static System.Drawing.Bitmap CombineLangBitmap(string langs)
        {
            List<System.Drawing.Bitmap> images = new List<System.Drawing.Bitmap>();
            System.Drawing.Bitmap finalImage = null;
            List<string> listLangs = langs.Split(',').ToList();

            foreach (string lg in listLangs)
            {
                switch (lg)
                {
                    case "es":
                        images.Add(Mis_Series.Properties.Resources.es);
                        break;
                    case "la":
                        images.Add(Mis_Series.Properties.Resources.la);
                        break;
                    case "ca":
                        images.Add(Mis_Series.Properties.Resources.ca);
                        break;
                    case "vo":
                        images.Add(Mis_Series.Properties.Resources.vo);
                        break;
                    case "vos":
                        images.Add(Mis_Series.Properties.Resources.vos);
                        break;
                    default:
                        break;
                }

            }
            try
            {
                int width = 0;
                int height = 0;

                foreach (Bitmap bitmap in images)
                {

                    width += bitmap.Width;
                    height = bitmap.Height;

                }

                finalImage = new System.Drawing.Bitmap(width + 8, height);

                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(finalImage))
                {
                    int offset = 2;
                    foreach (System.Drawing.Bitmap image in images)
                    {
                        g.DrawImage(image,
                          new System.Drawing.Rectangle(offset, 0, image.Width - 4, image.Height - 4));
                        offset += image.Width + 2;
                    }
                }

                return finalImage;
            }
            catch (Exception e)
            {
                if (finalImage != null)
                {
                    finalImage.Dispose();
                }

                Debug.WriteLine(e.Message);
            }
            finally
            {

                foreach (System.Drawing.Bitmap image in images)
                {
                    image.Dispose();
                }
            }
            return finalImage;
        }
    }
}
