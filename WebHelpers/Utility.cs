namespace System
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    public class Utility
    {

        public static string UseStringHasValue(params string[] values)
        {

            var result = "";
            foreach (var value in values)
            {
                if(!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value))
                {
                    result =value;
                    break;
                }
            }
            return result;
        }
        public static void CreateFloder(string uripath)
        {
            try
            {
                if (!Directory.Exists(HttpContext.Current.Server.MapPath(uripath)))
                {
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(uripath));
                }
            }
            catch (Exception)
            {
            }
        }

        public static void DeleteFile(string uripath)
        {
            try
            {
                string path = HttpContext.Current.Server.MapPath(uripath);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            catch (Exception)
            {
            }
        }
        
        public static byte[] FileToByteArray(string _FileName)
        {
            byte[] buffer = null;
            try
            {
                FileStream input = new FileStream(_FileName, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(input);
                long length = new FileInfo(_FileName).Length;
                buffer = reader.ReadBytes((int) length);
                input.Close();
                input.Dispose();
                reader.Close();
            }
            catch (Exception)
            {
                throw;
            }
            return buffer;
        }

        public static string GeneratorFileName(string input)
        {
            string extension = Path.GetExtension(input);
            return (Path.GetFileNameWithoutExtension(input) + "_" + DateTime.Now.ToString().GetHashCode().ToString("x") + extension);
        }

        public static string GetThumbImagePath(string imgpath)
        {

            try
            {
                var t1 = imgpath.LastIndexOf("/");
                var t2 = imgpath.Substring(0, t1);
                var filename = Path.GetFileName(imgpath);
                var t3 = t2 + "/_thumbs/" + filename;
                return t3;
            }
            catch (Exception)
            {

                return imgpath;
            }
        }
        public static string GetContentWidthTemplate(string tempPath, List<KeyValuePair<string, string>> keyValueReplace)
        {
            try
            {
                string filepath = HttpContext.Current.Server.MapPath(tempPath);
                string str2 = string.Empty;
                if (HttpContext.Current.Cache[filepath] != null)
                {
                    str2 = HttpContext.Current.Cache[filepath].ToString();
                }
                else
                {
                    str2 = ReadFileContent(filepath);
                    HttpContext.Current.Cache[filepath] = str2;
                }
                foreach (KeyValuePair<string, string> pair in keyValueReplace)
                {
                    try
                    {
                        str2 = str2.Replace(pair.Key, pair.Value);
                    }
                    catch (Exception)
                    {
                    }
                }
                return str2;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetIDByTitleID(string input)
        {
            int num = input.LastIndexOf("-");
            return input.Substring(num + 1);
        }

        public static int GetObjectSize(object obj)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream serializationStream = new MemoryStream();
                formatter.Serialize(serializationStream, obj);
                return serializationStream.ToArray().Length;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static string GetTitleByTitleID(string input)
        {
            int length = input.LastIndexOf("-");
            return input.Substring(0, length);
        }

        private static string PostData(string url)
        {
            try
            {
                HttpWebRequest request = null;
                Uri requestUri = new Uri(url);
                request = (HttpWebRequest) WebRequest.Create(requestUri);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                string str = "";
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            str = reader.ReadToEnd();
                        }
                    }
                }
                return str;
            }
            catch (Exception)
            {
                return "Lỗi kết nối";
            }
        }

        public static string ReadFileContent(string filepath)
        {
            string str;
            try
            {
                StreamReader reader = null;
                try
                {
                    reader = new StreamReader(filepath);
                    str = reader.ReadToEnd();
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }
            catch (Exception)
            {
                str = "";
            }
            return str;
        }

        public static string RemoveHtmlTag(string htmlContent)
        {
            return Regex.Replace(htmlContent, "<[^>]*>", string.Empty).Replace("&nbsp;", "").Trim();
        }

        public static void SearchAndDeleteFile(string FolderPath, string FileName)
        {
            try
            {
                if (System.IO.File.Exists(FolderPath + FileName))
                {
                    System.IO.File.Delete(FolderPath + FileName);
                }
                string[] directories = Directory.GetDirectories(FolderPath);
                if (directories.Length > 0)
                {
                    for (int i = 0; i < directories.Length; i++)
                    {
                        SearchAndDeleteFile(directories[i] + @"\", FileName);
                        System.IO.File.Delete(FolderPath + @"\" + FileName);
                    }
                }
            }
            catch
            {
            }
        }
        public static bool ValidateFile(string uri)
        {
            try
            {
                var file = new FileInfo(HttpContext.Current.Server.MapPath(uri));
                return file.Exists; 
            }
            catch
            {
                return false;
            }
        }
     

          

        public static void WriteLog(string Type, string Msg)
        {
            try
            {
                DateTime now = DateTime.Now;
                string str = string.Concat(new object[] { now.Year, now.Month.ToString(), now.Day, ".txt" });
                FileStream stream = new FileStream(HttpContext.Current.Server.MapPath("/") + "/" + str, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(stream);
                writer.BaseStream.Seek(0L, SeekOrigin.End);
                writer.WriteLine(string.Concat(new object[] { DateTime.Now, " ", Type, " - ", Msg }));
                writer.Flush();
                writer.Close();
            }
            catch (Exception)
            {
            }
        }

    
    }
}

