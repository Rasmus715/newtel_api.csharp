using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace newtel_api.csharp.Services
{
    public interface INewtelService
    {
        Task<string> CallPassword(string phoneNumber);
    }

    public class NewtelService : INewtelService
    {
        private readonly IConfiguration _configuration;
        public  NewtelService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> CallPassword(string phoneNumber)
        {
            //Generate sequence of variables
            var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            var generatedPin = new Random().Next(1000, 9999);

            var bodyAsString = new {dstNumber = $"{phoneNumber}", pin = $"{generatedPin}", timeout = 30};
            var json = JsonConvert.SerializeObject(bodyAsString);

            var source =
                $"call-password/start-password-call\n{currentTime}\n{_configuration["Newtel:AccessKey"]}\n{json}\n{_configuration["Newtel:WriteKey"]}";

            //Hashsing the string into SHA256
            using var sha256Hash = SHA256.Create();
            var sourceBytes = Encoding.UTF8.GetBytes(source);
            var hashBytes = sha256Hash.ComputeHash(sourceBytes);
            var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();


            //Generate newtel bearer token
            var newtelToken = _configuration["Newtel:AccessKey"] + currentTime + hash;


            //Initialize HttpClient for newtel request
            var client = new HttpClient();
            const string route = "https://api.new-tel.net/call-password/start-password-call";

            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", newtelToken);

            var stringContent = new StringContent(JsonConvert.SerializeObject(bodyAsString), Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(route, stringContent);
            client.Dispose();
            try
            {
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Newtel api response: " + responseString);
                response.Dispose();
                return responseString;
            }
            catch (HttpRequestException)
            {
                var body = await response.Content.ReadAsStringAsync();
                response.Dispose();
                throw new HttpRequestException(body);
            }
        }
    }
}