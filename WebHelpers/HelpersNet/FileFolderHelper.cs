using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace System
{
    public static class FileFolderHelper
    {

        

        public static string FileSave(this HttpPostedFileBase file,long maxsize, string localpath)
        {
            //if (file.InputStream.Length > maxsize) return "";

            var name = file.FileName;
            var newName = name.GeneratorFileName();
            var dir = new System.IO.DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath(localpath));
            if (!dir.Exists) dir.Create();
            var fullpath = localpath + "/" + newName;
            file.SaveAs(System.Web.HttpContext.Current.Server.MapPath(fullpath));    
            return fullpath;
        }
        public static string ImageSave(this HttpPostedFileBase image,string localpath)
        {

            var name = image.FileName;
            var newName = name.GeneratorFileName();                           
            var dir = new System.IO.DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath(localpath));
            if (!dir.Exists) dir.Create();
            var fullpath = localpath + "/" + newName;
            image.SaveAs(System.Web.HttpContext.Current.Server.MapPath(fullpath));
            try
            {
                if (ImageTools.ValidateImage(System.Web.HttpContext.Current.Server.MapPath(fullpath)))
                {
                    var result = ImageTools.ResizeImage(System.Web.HttpContext.Current.Server.MapPath(fullpath), System.Web.HttpContext.Current.Server.MapPath(fullpath), 500, 500, true, 80);
                }
                else
                {
                    Utility.DeleteFile(fullpath);
                    fullpath = "";
                }
            }
            catch (Exception)
            { }
            return fullpath;
        }
        public static string ImageSave(this HttpPostedFileBase image, string localpath,int maxwidth,int maxheight)
        {

            var name = image.FileName;
            var newName = name.GeneratorFileName();
            var dir = new System.IO.DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath(localpath));
            if (!dir.Exists) dir.Create();
            var fullpath = localpath + "/" + newName;
            image.SaveAs(System.Web.HttpContext.Current.Server.MapPath(fullpath));
            try
            {
                if (ImageTools.ValidateImage(System.Web.HttpContext.Current.Server.MapPath(fullpath)))
                {
                    var result = ImageTools.ResizeImage(System.Web.HttpContext.Current.Server.MapPath(fullpath), System.Web.HttpContext.Current.Server.MapPath(fullpath), maxwidth, maxheight, true, 80);
                }
                else
                {
                    Utility.DeleteFile(fullpath); fullpath = "";
                }
            }
            catch (Exception)
            { }
            return fullpath;
        }
    }
}
