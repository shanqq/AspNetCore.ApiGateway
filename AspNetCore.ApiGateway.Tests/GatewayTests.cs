using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using GatewayAPI = ApiGateway.API;
using WeatherAPI = Weather.API;
using StockAPI = Stock.API;
using System.Net.Http;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.JsonPatch;

namespace AspNetCore.ApiGateway.Tests
{
    public class APIInitialize
    {
        public TestServer GatewayAPI { get; set; }

        public APIInitialize()
        {
            IWebHostBuilder weatherAPI = new WebHostBuilder()
                                     .UseStartup<WeatherAPI.Startup>()
                                     .UseKestrel(options => options.Listen(IPAddress.Any, 63969));

            weatherAPI.Start();

            IWebHostBuilder stockAPI = new WebHostBuilder()
                                     .UseStartup<StockAPI.Startup>()
                                     .UseKestrel(options => options.Listen(IPAddress.Any, 63967));

            stockAPI.Start();

            IWebHostBuilder gatewayAPI = new WebHostBuilder()
                                     .UseStartup<GatewayAPI.Startup>()
                                     .UseKestrel(options => options.Listen(IPAddress.Any, 80));

            TestServer gatewayServer = new TestServer(gatewayAPI);

            this.GatewayAPI = gatewayServer;
        }
    }

    public class GatewayTests : IClassFixture<APIInitialize>
    {
        readonly APIInitialize _apiInit;

        public GatewayTests(APIInitialize apiInit)
        {
            _apiInit = apiInit;
        }

        [Fact]
        public async Task Test_Get_Pass()
        {
            var client = _apiInit.GatewayAPI.CreateClient();

            //Gateway API url with Api key and Route key
            var gatewayUrl = "http://localhost/api/Gateway/weatherservice/forecast";

            var response = await client.GetAsync(gatewayUrl);

            response.EnsureSuccessStatusCode();

            var forecasts = JsonConvert.DeserializeObject<WeatherForecast[]>(await response.Content.ReadAsStringAsync());

            Assert.True(forecasts.Length > 0);
        }

        [Fact]
        public async Task Test_Multiple_Get_Pass()
        {
            var client = _apiInit.GatewayAPI.CreateClient();

            //Gateway API url with Api key and Route key
            //Weather API call
            var gatewayUrl = "http://localhost/api/Gateway/weatherservice/forecast";

            var response = await client.GetAsync(gatewayUrl);

            response.EnsureSuccessStatusCode();

            var forecasts = JsonConvert.DeserializeObject<WeatherForecast[]>(await response.Content.ReadAsStringAsync());

            Assert.True(forecasts.Length > 0);

            //Stock API call
            gatewayUrl = "http://localhost/api/Gateway/stockservice/stocks";

            response = await client.GetAsync(gatewayUrl);

            response.EnsureSuccessStatusCode();

            var stockQuotes = JsonConvert.DeserializeObject<StockQuote[]>(await response.Content.ReadAsStringAsync());

            Assert.True(stockQuotes.Length > 0);
        }

        [Fact]
        public async Task Test_Get_WithParam_Pass()
        {
            var client = _apiInit.GatewayAPI.CreateClient();

            //Gateway API url with Api key, Route key and Param
            var gatewayUrl = "http://localhost/api/Gateway/weatherservice/type?parameters=3";

            var response = await client.GetAsync(gatewayUrl);

            response.EnsureSuccessStatusCode();

            var weatherType = JsonConvert.DeserializeObject<WeatherTypeResponse>(await response.Content.ReadAsStringAsync());

            Assert.NotNull(weatherType);
            Assert.True(!string.IsNullOrEmpty(weatherType.Type));

            gatewayUrl = "http://localhost/api/Gateway/weatherservice/typewithparams?parameters=index=3";

            response = await client.GetAsync(gatewayUrl);

            response.EnsureSuccessStatusCode();

            weatherType = JsonConvert.DeserializeObject<WeatherTypeResponse>(await response.Content.ReadAsStringAsync());

            Assert.NotNull(weatherType);
            Assert.True(!string.IsNullOrEmpty(weatherType.Type));
        }

        [Fact]
        public async Task Test_Post_Pass()
        {
            var client = _apiInit.GatewayAPI.CreateClient();

            //Gateway API url with Api key and Route key
            var gatewayUrl = "http://localhost/api/Gateway/weatherservice/add";

            AddWeatherTypeRequest request = new AddWeatherTypeRequest
            {
                WeatherType = "Windy"
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var httprequest = new HttpRequestMessage
            {
                RequestUri = new Uri(gatewayUrl),
                Content = content,
                Method = HttpMethod.Post
            };

            var response = await client.SendAsync(httprequest);

            response.EnsureSuccessStatusCode();

            var weatherTypes = JsonConvert.DeserializeObject<string[]>(await response.Content.ReadAsStringAsync());

            Assert.True(weatherTypes.Last() == "Windy");
        }

        [Fact]
        public async Task Test_Put_Pass()
        {
            var client = _apiInit.GatewayAPI.CreateClient();

            //Gateway API url with Api key and Route key
            var gatewayUrl = "http://localhost/api/Gateway/weatherservice/update";

            UpdateWeatherTypeRequest request = new UpdateWeatherTypeRequest
            {
                WeatherType = "Coooooooool",
                Index = 3
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var httprequest = new HttpRequestMessage
            {
                RequestUri = new Uri(gatewayUrl),
                Content = content,
                Method = HttpMethod.Put
            };

            var response = await client.SendAsync(httprequest);

            response.EnsureSuccessStatusCode();

            //Gateway API url with Api key and Route key
            gatewayUrl = "http://localhost/api/Gateway/weatherservice/types";

            response = await client.GetAsync(gatewayUrl);

            response.EnsureSuccessStatusCode();

            var weatherTypes = JsonConvert.DeserializeObject<string[]>(await response.Content.ReadAsStringAsync());

            Assert.True(weatherTypes[3] == "Coooooooool");
        }        

        [Fact]
        public async Task Test_Patch_Pass()
        {
            var client = _apiInit.GatewayAPI.CreateClient();

            //Gateway API url with Api key and Route key
            var gatewayUrl = "http://localhost/api/Gateway/weatherservice/patch";            

            JsonPatchDocument<WeatherForecast> jsonPatch = new JsonPatchDocument<WeatherForecast>();
            jsonPatch.Add(x => x.TemperatureC, 35);

            var content = new StringContent(JsonConvert.SerializeObject(jsonPatch), Encoding.UTF8, "application/json-patch+json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

            var httprequest = new HttpRequestMessage
            {
                RequestUri = new Uri(gatewayUrl),
                Content = content,
                Method = HttpMethod.Patch
            };

            var response = await client.SendAsync(httprequest);

            response.EnsureSuccessStatusCode();

            var weatherForecast = JsonConvert.DeserializeObject<WeatherForecast>(await response.Content.ReadAsStringAsync());

            Assert.True(weatherForecast.TemperatureC == 35);
        }

        [Fact]
        public async Task Test_Delete_Pass()
        {
            var client = _apiInit.GatewayAPI.CreateClient();

            //Gateway API url with Api key, Route key and Param
            var gatewayUrl = "http://localhost/api/Gateway/weatherservice/remove?parameters=0";

            var response = await client.DeleteAsync(gatewayUrl);

            response.EnsureSuccessStatusCode();

            //Gateway API url with Api key and Route key
            gatewayUrl = "http://localhost/api/Gateway/weatherservice/types";

            response = await client.GetAsync(gatewayUrl);

            response.EnsureSuccessStatusCode();

            var weatherTypes = JsonConvert.DeserializeObject<string[]>(await response.Content.ReadAsStringAsync());

            Assert.DoesNotContain(weatherTypes, x => x == "Freezing");
        }

        [Fact]
        public async Task Test_Get_Invalid_ApiKey_Fail()
        {
            var client = _apiInit.GatewayAPI.CreateClient();

            //Gateway API url with invalid Api key and Route key
            var gatewayUrl = "http://localhost/api/Gateway/xyzservice/forecast";

            var response = await client.GetAsync(gatewayUrl);

            Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Test_Get_Invalid_RouteKey_Fail()
        {
            var client = _apiInit.GatewayAPI.CreateClient();

            //Gateway API url with Api key and invalid Route key
            var gatewayUrl = "http://localhost/api/Gateway/weatherservice/xyz";

            var response = await client.GetAsync(gatewayUrl);

            Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Test_GetOrchestration_Pass()
        {
            var client = _apiInit.GatewayAPI.CreateClient();

            //Gateway API Orchestration url
            var gatewayUrl = "http://localhost/api/Gateway/orchestration";

            var response = await client.GetAsync(gatewayUrl);

            response.EnsureSuccessStatusCode();

            var orchestration = JsonConvert.DeserializeObject<Orchestration[]>(await response.Content.ReadAsStringAsync());

            Assert.True(orchestration.Length > 0);
        }
    }
}
