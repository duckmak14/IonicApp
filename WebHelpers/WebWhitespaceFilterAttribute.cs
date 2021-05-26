using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Common
{
    public class WebWhitespaceFilterAttribute : ActionFilterAttribute
    {
        private WebWhitespaceFilterContentType _contentType;

        public WebWhitespaceFilterAttribute()
        {
            _contentType = WebWhitespaceFilterContentType.Xml;
        }
        public WebWhitespaceFilterAttribute(WebWhitespaceFilterContentType contentType)
        {
            _contentType = contentType;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        { 
            var request = filterContext.HttpContext.Request;
            var response = filterContext.HttpContext.Response; 
            switch (_contentType)
            {
                case WebWhitespaceFilterContentType.Xml: 
                    response.Filter = new StringFilterStream(response.Filter, s =>
                    {
                        s = Regex.Replace(s, @"\s+", " ");
                        s = Regex.Replace(s, @"\s*\n\s*", "\n");
                        s = Regex.Replace(s, @"\s*\>\s*\<\s*", "><"); 
                        var firstEndBracketPosition = s.IndexOf(">");
                        if (firstEndBracketPosition >= 0)
                        {
                            s = s.Remove(firstEndBracketPosition, 1);
                            s = s.Insert(firstEndBracketPosition, ">\n");
                        }
                        return s;
                    });
                    break; 
                case WebWhitespaceFilterContentType.Css:
                case WebWhitespaceFilterContentType.Javascript: 
                    response.Filter = new StringFilterStream(response.Filter, s =>
                    {
                        s = Regex.Replace(s, @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/", "");
                        s = Regex.Replace(s, @"\s+", " ");
                        s = Regex.Replace(s, @"\s*{\s*", "{");
                        s = Regex.Replace(s, @"\s*}\s*", "}");
                        s = Regex.Replace(s, @"\s*;\s*", ";");
                        return s;
                    });
                    break;
            }
        }
    }
    class StringFilterStream : Stream
    {
         private System.IO.Stream Base; 
         public StringFilterStream(System.IO.Stream ResponseStream)
         { 
            if (ResponseStream == null)
                throw new ArgumentNullException("ResponseStream");
            this.Base = ResponseStream;
        }

        private Stream _sink;
        private Func<string, string> _filter; 
        public StringFilterStream(Stream sink, Func<string, string> filter)
        {
            _sink = sink;
            _filter = filter;
        }

        #region Mixin Properties/Methods
        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanWrite { get { return true; } }
        public override void Flush() { _sink.Flush(); }
        public override long Length { get { return 0; } }
        private long _position;
        public override long Position
        {
            get { return _position; }
            set { _position = value; }
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _sink.Read(buffer, offset, count);
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _sink.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            _sink.SetLength(value);
        }
        public override void Close()
        {
            _sink.Close();
        }
        #endregion

        public override void Write(byte[] buffer, int offset, int count)
        {
            // intercept the data and convert to string
            byte[] data = new byte[count];
            Buffer.BlockCopy(buffer, offset, data, 0, count);
            string s = Encoding.UTF8.GetString(buffer);

            // apply the filter
            s = _filter(s);

            // write the data back to stream
            byte[] outdata = Encoding.UTF8.GetBytes(s);
            _sink.Write(outdata, 0, outdata.GetLength(0));
        }
    }

    public enum WebWhitespaceFilterContentType
    {
        Xml = 0, Css = 1, Javascript = 2
    }
   
}
