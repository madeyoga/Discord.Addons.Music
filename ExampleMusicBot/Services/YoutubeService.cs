using System;
using System.Threading.Tasks;
using YoutubeSearchApi.Net;

namespace Nano.Net.Services
{
    public class YoutubeService
    {
        private YoutubeApiV3Client ytsClient;

        public YoutubeService()
        {
            ytsClient = new YoutubeApiV3Client(Environment.GetEnvironmentVariable("DEVELOPER_KEY"));
        }

        public async Task <dynamic> SearchVideosByQuery(string query)
        {
            dynamic response = await ytsClient.Search(query);
            return response;
        }
    }
}
