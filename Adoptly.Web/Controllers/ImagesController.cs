using Microsoft.AspNetCore.Mvc;

namespace Adoptly.Web.Controllers
{
	public class ImagesController : Controller
	{
        private readonly IHttpClientFactory _clientFactory;
        private HttpClient Client => _clientFactory.CreateClient();

        public ImagesController(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;

        // Frontend can call this method to retrieve and display an image.
        // E.g., <img src="/Images/Get?containerName=brand-assets&imgName=greyhound-test.jpg&contentType=image/jpg">

        public ActionResult Get(string containerName, string imgName, string contentType)
        {
            HttpResponseMessage response = Client.GetAsync($"blobs/{containerName}/{imgName}").Result;

            if (!response.IsSuccessStatusCode)
                return null;

            byte[] imgByteArr = response.Content.ReadAsByteArrayAsync().Result;

            return File(imgByteArr, contentType);
        }
    }
}