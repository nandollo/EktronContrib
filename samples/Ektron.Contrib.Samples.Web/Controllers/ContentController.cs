using System.Collections.Generic;
using System.Web.Http;
using Ektron.Cms;
using Ektron.Cms.Framework;
using Ektron.Cms.Framework.Content;

namespace Ektron.Contrib.Samples.Web.Controllers
{
    public class ContentController : ApiController
    {
		public IEnumerable<ContentData> Get()
        {
			var contentManager = new ContentManager(ApiAccessMode.Admin);
			ContentData contentData = contentManager.GetItem(30, returnMetadata: true);

			return new ContentData[] { contentData };
        }

		public ContentData Get(int id)
        {
			var contentManager = new ContentManager(ApiAccessMode.Admin);
			return contentManager.GetItem(30, returnMetadata: true);
        }

        public void Post([FromBody]string value)
        {
        }

        public void Put(int id, [FromBody]string value)
        {
        }

        public void Delete(int id)
        {
        }
    }
}
