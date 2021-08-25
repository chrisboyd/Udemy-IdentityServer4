using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BankOfDotNet.ConsoleClient
{
    class Program
    {
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            var clientRO = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:5000/connect/token")
            };
            //var discoRO = await HttpClientDiscoveryExtensions.GetDiscoveryDocumentAsync(clientRO);

            //if (discoRO.IsError)
            //{
            //    Console.WriteLine(discoRO.Error);
            //    return;
            //}

            //Grab a bearer token using ResourceOwnerPassword Grant type
            var tokenClientRO = new TokenClient(clientRO, new TokenClientOptions
            {
                ClientId = "ro.client",
                ClientSecret = "secret"
            });
            var tokenResponseRO = await tokenClientRO.RequestPasswordTokenAsync
                ("chris", "password", "bankOfDotNetApi");

            if (tokenResponseRO.IsError)
            {
                Console.WriteLine(tokenResponseRO.Error);
                return;
            }

            Console.WriteLine(tokenResponseRO.Json);
            Console.WriteLine("\n\n");


            //discover all endpoints using metadata of identity server
            var connection = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:5000/connect/token")
            };
            //var disco = await c.GetDiscoveryDocumentAsync("http://localhost:5000");
            //if (disco.IsError)
            //{
            //    Console.WriteLine(disco.Error);
            //    return;
            //}
            
            //Grab a bearer token using client credentioal grant type
            var tokenClient = new TokenClient(connection, new TokenClientOptions
            {
                ClientId = "client", ClientSecret = "secret"
            });
            var tokenResponse = await tokenClient.RequestClientCredentialsTokenAsync("bankOfDotNetApi");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            //Consume customer API
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var customerInfo = new StringContent(
                JsonConvert.SerializeObject(
                        new { Id = 10, FirstName = "Luke", LastName = "Skywalker" }),
                        Encoding.UTF8, "application/json");
            var createCustomerResponse = await client.PostAsync("http://localhost:12414/api/customers"
                                                      , customerInfo);
            if (!createCustomerResponse.IsSuccessStatusCode)
            {
                Console.WriteLine(createCustomerResponse.StatusCode);
            }

            var getCustomerResponse = await client.GetAsync("http://localhost:12414/api/customers");
            if (!getCustomerResponse.IsSuccessStatusCode)
            {
                Console.WriteLine(getCustomerResponse.StatusCode);
            }
            else
            {
                var content = await getCustomerResponse.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }

            Console.Read();

        }
    }
}
