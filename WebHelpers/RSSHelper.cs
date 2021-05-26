using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Common
{
    public class RSS
    {
        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        private string link;

        public string Link
        {
            get { return link; }
            set { link = value; }
        }
        private string image = "";

        public string Image
        {
            get { return image; }
            set { image = value; }
        }
        private string description;

        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        private string pubDate;

        public string PubDate
        {
            get { return pubDate; }
            set { pubDate = value; }
        }

    }
    public class RSSHelper
    {
        private static string GetHtmlWeb(string link)
        {
            string strContent = "";
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(link);
                httpWebRequest.Method = "GET";
                httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                WebResponse objWebResponse = httpWebRequest.GetResponse();
                Stream receiveStream = objWebResponse.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, System.Text.Encoding.UTF8);
                strContent = readStream.ReadToEnd();
                objWebResponse.Close();
                readStream.Close();
            }
            catch (Exception ex)
            {
            }

            return strContent;
        }
        public static void RemoveTags(HtmlDocument html, string tagName)
        {
            var tags = html.DocumentNode.SelectNodes("//" + tagName);
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    if (!tag.HasChildNodes)
                    {
                        tag.ParentNode.RemoveChild(tag);
                        continue;
                    }

                    for (var i = tag.ChildNodes.Count - 1; i >= 0; i--)
                    {
                        var child = tag.ChildNodes[i];
                        tag.ParentNode.InsertAfter(child, tag);
                    }
                    tag.ParentNode.RemoveChild(tag);
                }
            }
        }

        public static string GetContentByLink(string link, string pathcontent, List<string> removechild, string pathimg)
        {
            string htmlContent = string.Empty;
            try
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(GetHtmlWeb(link));
                var content = doc.DocumentNode;
                if (pathcontent.Length > 0)
                    content = content.SelectNodes(pathcontent).ToList<HtmlNode>().First();
                foreach (var c in removechild)
                {
                    try
                    {
                        if (c.ToLower().Equals("lastchild")) content.RemoveChild(content.LastChild);
                        else if (c.ToLower().Equals("firstchild")) content.RemoveChild(content.FirstChild);
                        else content.RemoveChild(content.SelectNodes(c).First());
                    }
                    catch (Exception)
                    { }
                }
                if (link.ToLower().Contains(@"vnexpress.net/"))
                { }
                if (pathimg.Length > 0)
                {
                    var imgnodes = content.SelectNodes("//img");
                    foreach (HtmlNode imgnode in imgnodes)
                    {
                        try
                        {
                            var src = imgnode.Attributes["src"].Value;
                            if (!src.StartsWith("http://"))
                            {
                                var nodefix = imgnode;
                                nodefix.Attributes["src"].Value = pathimg + src;
                                content = content.ReplaceChild(nodefix, imgnode);
                            }
                        }
                        catch (Exception)
                        { }
                    }
                }
                htmlContent = content.InnerHtml;
            }
            catch (Exception)
            {


            }
            return htmlContent;
        }

        public static RSS GetRss(string rsschannel, string title)
        {


            var rss = new RSS();
            XDocument feedXml;
            if (HttpRuntime.Cache["RSSChannel_" + HttpUtility.UrlDecode(rsschannel)] == null)
            {

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(HttpUtility.UrlDecode(rsschannel));
                httpWebRequest.Method = "GET"; httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
                HttpWebResponse response = httpWebRequest.GetResponse() as HttpWebResponse;
                XmlTextReader reader = new XmlTextReader(response.GetResponseStream());
                feedXml = XDocument.Load(reader);
                reader.Close();
                response.Close();
                HttpRuntime.Cache.Add("RSSChannel_" + rsschannel, feedXml, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 5, 0), System.Web.Caching.CacheItemPriority.Default, null);
            }
            else
                feedXml = (XDocument)HttpRuntime.Cache["RSSChannel_" + HttpUtility.UrlDecode(rsschannel)];



            var feeds = (from feed in feedXml.Descendants("item") select feed).ToList();
            var nodes = from x in feeds where Utility.RemoveHtmlTag(x.Element("title").Value.ToLower()).Equals(Utility.RemoveHtmlTag(title.ToLower())) select x;
            if (nodes.Count() > 0)
            {
                var node = nodes.First();
                rss.Title = node.Element("title").Value.Trim();
                rss.Description = Utility.RemoveHtmlTag(node.Element("description").Value).Replace("<", "").Replace(">", "");
                rss.Link = node.Element("link").Value.Trim();
                rss.PubDate = node.Element("pubDate").Value;
                try
                {
                    var tem = new HtmlDocument();
                    tem.LoadHtml(node.Element("description").Value);
                    var t = tem.DocumentNode.SelectNodes("//img");
                    if (t.Count > 0)
                    {
                        rss.Image = t.First().Attributes["src"].Value;
                    }
                    else
                    {
                        rss.Image = "";
                    }
                }
                catch (Exception)
                { }
            }
            return rss;
        }
        public static DataTable FillDataRSS(string rsschannel)
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(string));
            table.Columns.Add("Title", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Link", typeof(string));
            table.Columns.Add("PubDate", typeof(string));
            table.Columns.Add("Image", typeof(string));
            // table.Columns.Add("Content", typeof(string));
            DataRow row;
            try
            {
                XDocument feedXml;
                if (HttpRuntime.Cache["RSSChannel_" + HttpUtility.UrlDecode(rsschannel)] == null)
                {
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(HttpUtility.UrlDecode(rsschannel));
                    httpWebRequest.Method = "GET"; httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
                    HttpWebResponse response = httpWebRequest.GetResponse() as HttpWebResponse;
                    XmlTextReader reader = new XmlTextReader(response.GetResponseStream());
                    feedXml = XDocument.Load(reader);
                    reader.Close();
                    response.Close();
                    HttpRuntime.Cache.Add("RSSChannel_" + rsschannel, feedXml, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 5, 0), System.Web.Caching.CacheItemPriority.Default, null);
                }
                else
                    feedXml = (XDocument)HttpRuntime.Cache["RSSChannel_" + HttpUtility.UrlDecode(rsschannel)];

                var feeds = (from feed in feedXml.Descendants("item") select feed).ToList();
                var index = 0;
                foreach (var item in feeds)
                {
                    row = table.NewRow();
                    row["ID"] = index;
                    row["Title"] = item.Element("title").Value.Trim();
                    row["Description"] = Utility.RemoveHtmlTag(item.Element("description").Value).Replace("<", "").Replace(">", "");
                    row["Link"] = item.Element("link").Value.Trim();
                    row["PubDate"] = item.Element("pubDate").Value;
                    try
                    {
                        var tem = new HtmlDocument();
                        tem.LoadHtml(item.Element("description").Value);
                        var t = tem.DocumentNode.SelectNodes("//img");
                        if (t.Count > 0)
                        {
                            row["Image"] = t.First().Attributes["src"].Value;
                        }
                        else
                        {
                            row["Image"] = ConfigurationManager.AppSettings["NoNewsImg"].ToString(); ;
                        }
                    }
                    catch (Exception)
                    { }
                    table.Rows.Add(row);
                    index++;
                }
            }
            catch (Exception)
            {


            }
            return table;
        }
    }
}

