using System;
using System.Net.Http;
using System.Threading.Tasks;
using YoutubeSearchApi.Net;
using YoutubeSearchApi.Net.Backends;
using YoutubeSearchApi.Net.Objects;

namespace Nano.Net.Services
{
    public class YoutubeService
    {
        private DefaultSearchClient client;

        public YoutubeService()
        {
            client = new DefaultSearchClient(new YoutubeSearchBackend());
        }

        public async Task <DefaultResponse> SearchVideosByQuery(string query)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                DefaultResponse response = await client.SearchAsync(httpClient, query, 5);
                return response;
            }
        }
    }
}
