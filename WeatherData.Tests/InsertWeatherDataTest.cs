using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Moq;
using Newtonsoft.Json;
using Xunit;
using System.Threading.Tasks;
using System.Threading;

using WeatherData;

namespace WeatherData.Tests
{
    public class InsertWeatherDataTest
    {

        private APIGatewayProxyRequest request;
        public InsertWeatherDataTest()
        {
            request = new APIGatewayProxyRequest();
            request.Headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { { "content-type", "application/json" } };
            request.Body = @"{""event"": ""hum_temp"",""data"": ""12.34, 56.78"",""published_at"": ""2019-01-23T13:52:10.193Z"",""coreid"": ""api""}";

        }
        [Fact]
        public async void InsertJobReturnsAPIGatewayResponse()
        {
            Mock<IDynamoDBContext> dbContextMock = new Mock<IDynamoDBContext>();
            InsertWeatherData sut = new InsertWeatherData();
            TestLambdaContext context = new TestLambdaContext();

            sut.ProcessIncomingService = new Lazy<ILambdaAPIFunctionHandler>(() => new InsertWeatherDataFunction(dbContextMock.Object));

            APIGatewayProxyResponse response = await sut.FunctionHandlerAsync(request, context);

            Assert.IsType<APIGatewayProxyResponse>(response);
        }

        [Fact]
        public async void InsertJobBadContentReturns400()
        {
            Mock<IDynamoDBContext> dbContextMock = new Mock<IDynamoDBContext>();
            InsertWeatherData sut = new InsertWeatherData();
            TestLambdaContext context = new TestLambdaContext();
            request.Headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { { "content-Type", "text" } };
            sut.ProcessIncomingService = new Lazy<ILambdaAPIFunctionHandler>(() => new InsertWeatherDataFunction(dbContextMock.Object));

            APIGatewayProxyResponse response = await sut.FunctionHandlerAsync(request, context);

            Assert.IsType<APIGatewayProxyResponse>(response);
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public async void InsertJobCallsSaveAsync()
        {
            Mock<IDynamoDBContext> dbContextMock = new Mock<IDynamoDBContext>();
            dbContextMock.Setup(m => m.SaveAsync<WeatherEventData>(It.IsAny<WeatherEventData>(), It.IsAny<CancellationToken>())).Returns(Task.Delay(0));

            InsertWeatherData sut = new InsertWeatherData();
            TestLambdaContext context = new TestLambdaContext();

            sut.ProcessIncomingService = new Lazy<ILambdaAPIFunctionHandler>(() => new InsertWeatherDataFunction(dbContextMock.Object));

            APIGatewayProxyResponse response = await sut.FunctionHandlerAsync(request, context);

            Assert.Equal(200, response.StatusCode);
            dbContextMock.Verify(_ => _.SaveAsync<WeatherEventData>(It.IsAny<WeatherEventData>(), default(CancellationToken)), Times.Once);
        }
    }
}
