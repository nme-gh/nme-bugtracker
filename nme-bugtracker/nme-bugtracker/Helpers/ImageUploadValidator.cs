using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace nme_bugtracker.Helpers
{
    public static class ImageUploadValidator
    {
        public static bool IsWebFriendlyImage(HttpPostedFileBase file)
        {
            if (file == null)
                return false;

            if (file.ContentLength > 2 * 1024 * 1024 || file.ContentLength < 0)
                return false;
            //if (file.ContentLength > 2 * 1024 * 1024 || file.ContentLength < 1024)
            //    return false;

            //try
            //{
            //    using (var img = Image.FromStream(file.InputStream))
            //    {
            //        return ImageFormat.Jpeg.Equals(img.RawFormat) ||
            //                ImageFormat.Png.Equals(img.RawFormat) ||
            //                ImageFormat.Gif.Equals(img.RawFormat);
            //    }
            //}
            //catch
            //{
            //    return false;
            //}

            return true;
        }

        public static string GetPathToImage(HttpPostedFileBase image)
        {
            string fileName = "";
            if (ImageUploadValidator.IsWebFriendlyImage(image))
            {
                fileName = Path.GetFileName(image.FileName);
                image.SaveAs(Path.Combine(HttpContext.Current.Server.MapPath("~/Uploads/"), fileName));
            }
            return fileName;
        }


    }
}