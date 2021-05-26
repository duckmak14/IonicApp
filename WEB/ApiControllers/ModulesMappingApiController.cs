using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebModels;

namespace WEB.ApiControllers
{
    public class ModulesMappingApiController : ApiController
    {
        WebContext db = new WebContext();

        [HttpGet]
        public JArray TreeTable()
        {
            var roots = db.WebModules.Where(x => x.ParentID == null).OrderBy(x => x.Order).AsEnumerable();
            var allItems = db.WebModules.OrderBy(x => x.Order).AsEnumerable();
            JArray jArray = new JArray();
            foreach (var item in roots)
            {
                var subs = allItems.Where(x => x.ParentID == item.ID);
                var hasChild = false;
                if (subs.Count() > 0)
                    hasChild = true;
                jArray.Add(new JObject(
                        new JProperty("ID", item.ID),
                        new JProperty("Title", item.Title),
                        new JProperty("Parent", null),
                        new JProperty("HasChild", hasChild)));
                if (subs.Count() > 0)
                    SubTreeTable(ref subs, ref allItems, ref jArray);
            }
            return jArray;
        }

        [NonAction]
        private void SubTreeTable(ref IEnumerable<WebModule> subs, ref IEnumerable<WebModule> allItems, ref JArray jArray)
        {
            foreach (var item in subs)
            {
                var subSubs = allItems.Where(x => x.ParentID == item.ID);
                var hasChild = false;
                if (subSubs.Count() > 0)
                    hasChild = true;
                jArray.Add(new JObject(
                        new JProperty("ID", item.ID),
                        new JProperty("Title", item.Title),
                        new JProperty("Parent", item.ParentID.Value),
                        new JProperty("HasChild", hasChild)));
                if (subSubs.Count() > 0)
                    SubTreeTable(ref subSubs, ref allItems, ref jArray);
            }
        }
    }
}
