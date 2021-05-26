namespace CKFinder.Connector.CommandHandlers
{
    using CKFinder.Connector;
    using CKFinder.Settings;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Web.Security;
    using System.Xml;

    internal class InitCommandHandler : XmlCommandHandlerBase
    {
        public const string K_CHARS = "123456789ABCDEFGHJKLMNPQRSTUVWXYZ";

        protected override void BuildXml()
        {
            //string[] strArray;
            List<string> strArray = new List<string>();
            int num;
            Config current = Config.Current;
            bool flag = current.CheckAuthentication();
            XmlNode node = XmlUtil.AppendElement(base.ConnectorNode, "ConnectorInfo");
            XmlUtil.SetAttribute(node, "enabled", flag.ToString().ToLower());
            if (!flag)
            {
                ConnectorException.Throw(500);
            }
            //string attributeValue = "";
            //string str2 = current.LicenseKey.ToUpper();
            ////if (1 == ("123456789ABCDEFGHJKLMNPQRSTUVWXYZ".IndexOf(str2[0]) % 5))
            ////{
            //attributeValue = current.LicenseName.ToLower();
            ////}


            //XmlUtil.SetAttribute(node, "s", "");
            //XmlUtil.SetAttribute(node, "c", "");
            ////XmlUtil.SetAttribute(node, "c", string.Concat(new object[] { str2[11], str2[0], str2[8], str2[12], str2[0x1a], str2[2], str2[3], str2[0x19], str2[1] }).Trim());


            string ln = "";
            string lc = current.LicenseKey.ToUpper();

            //if (1 == (K_CHARS.IndexOf(lc[0]) % 5))
                ln = current.LicenseName.ToLower();

            XmlUtil.SetAttribute(node, "s", ln);
            XmlUtil.SetAttribute(node, "c", string.Concat(new object[] { lc[11], lc[0], lc[8], lc[12], lc[0x1a], lc[2], lc[3], lc[0x19], lc[1] }).Trim());
            //XmlUtil.SetAttribute(node, "c", string.Concat(lc[11], lc[0], lc[8], lc[12]).Trim());


            XmlUtil.SetAttribute(node, "imgWidth", current.Images.MaxWidth.ToString());
            XmlUtil.SetAttribute(node, "imgHeight", current.Images.MaxHeight.ToString());
            XmlUtil.SetAttribute(node, "thumbsEnabled", current.Thumbnails.Enabled.ToString().ToLower());
            if (current.Thumbnails.Enabled)
            {
                XmlUtil.SetAttribute(node, "thumbsUrl", current.Thumbnails.Url);
                XmlUtil.SetAttribute(node, "thumbsDirectAccess", current.Thumbnails.DirectAccess.ToString().ToLower());
            }
            XmlNode node2 = XmlUtil.AppendElement(base.ConnectorNode, "ResourceTypes");
            XmlNode node3 = XmlUtil.AppendElement(base.ConnectorNode, "PluginsInfo");
            if ((base.Request.QueryString["type"] != null) && (base.Request.QueryString["type"].Length > 0))
            {
                // strArray = new string[] { base.Request.QueryString["type"] };
                strArray = new List<string>() { base.Request.QueryString["type"] };
            }
            else
            {
                string defaultResourceTypes = Config.Current.DefaultResourceTypes;
                if (defaultResourceTypes.Length == 0)
                {
                    // strArray = new string[current.ResourceTypes.Count];
                    for (num = 0; num < current.ResourceTypes.Count; num++)
                    {
                        // strArray[num] = current.ResourceTypes.GetByIndex(num).Name;
                        strArray.Add(current.ResourceTypes.GetByIndex(num).Name);
                    }
                }
                else
                {
                    //strArray = defaultResourceTypes.Split(new char[] { ',' });
                    strArray = new List<string>();// defaultResourceTypes.Split(new char[] { ',' });
                }

            }
            try
            {
                var temp = HttpContext.Current.User.Identity;
                if (temp != null)
                {
                    var username = temp.Name;
                    if (!strArray.Contains("[" + username.ToUpper() + "]")) strArray.Add("[" + username.ToUpper() + "]");
                }
            }
            catch (Exception)
            {
            }
            for (num = 0; num < strArray.Count; num++)
            {
                string resourceType = strArray[num];
                int computedMask = Config.Current.AccessControl.GetComputedMask(resourceType, "/");
                if ((computedMask & 1) == 1)
                {
                    ResourceType resourceTypeConfig = current.GetResourceTypeConfig(resourceType);
                    string targetDirectory = resourceTypeConfig.GetTargetDirectory();
                    bool flag2 = Directory.Exists(targetDirectory) && (Directory.GetDirectories(resourceTypeConfig.GetTargetDirectory()).Length > 0);
                    XmlNode node4 = XmlUtil.AppendElement(node2, "ResourceType");
                    XmlUtil.SetAttribute(node4, "name", resourceType);
                    XmlUtil.SetAttribute(node4, "url", resourceTypeConfig.Url);
                    XmlUtil.SetAttribute(node4, "allowedExtensions", string.Join(",", resourceTypeConfig.AllowedExtensions));
                    XmlUtil.SetAttribute(node4, "deniedExtensions", string.Join(",", resourceTypeConfig.DeniedExtensions));
                    XmlUtil.SetAttribute(node4, "hash", Util.GetMd5Hash(targetDirectory).Substring(0, 0x10));
                    XmlUtil.SetAttribute(node4, "hasChildren", flag2 ? "true" : "false");
                    XmlUtil.SetAttribute(node4, "acl", computedMask.ToString());
                }
            }
            if ((base.Connector.JavascriptPlugins != null) && (base.Connector.JavascriptPlugins.Count > 0))
            {
                string str6 = "";
                foreach (string str7 in base.Connector.JavascriptPlugins)
                {
                    if (str6.Length > 0)
                    {
                        str6 = str6 + ",";
                    }
                    str6 = str6 + str7;
                }
                XmlUtil.SetAttribute(node, "plugins", str6);
            }
            base.Connector.CKFinderEvent.ActivateEvent(CKFinderEvent.Hooks.InitCommand, new object[] { base.ConnectorNode });
        }

        protected override bool MustCheckRequest()
        {
            return false;
        }

        protected override bool MustIncludeCurrentFolder()
        {
            return false;
        }
    }
}

