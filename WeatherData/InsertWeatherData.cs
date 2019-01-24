using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace WeatherData
{
    public class InsertWeatherData : ILambdaAPIFunctionHandler
    {
        public InsertWeatherData()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
        }

        private static Lazy<ILambdaAPIFunctionHandler> processIncomingService = new Lazy<ILambdaAPIFunctionHandler>(() =>
            new InsertWeatherDataFunction(new DynamoDBContext(new AmazonDynamoDBClient())));

        public Lazy<ILambdaAPIFunctionHandler> ProcessIncomingService { get => processIncomingService; set => processIncomingService = value; }

        public virtual async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest input, ILambdaContext context) =>
            await ProcessIncomingService.Value.FunctionHandlerAsync(input, context);
    }


    public interface ILambdaAPIFunctionHandler
    {
        Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest input, ILambdaContext context);
    }


    public class WeatherEvent
    {
        [JsonProperty(PropertyName = "event")]
        public string EventName { get; set; }

        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }

        [JsonProperty(PropertyName = "published_at")]
        public DateTime Published { get; set; }

        [JsonProperty(PropertyName = "coreid")]
        public string ID { get; set; }
    }

    [DynamoDBTable("WeatherData")]
    public class WeatherEventData
    {
        [DynamoDBHashKey]
        public string Device { get; set; }
        [DynamoDBRangeKey]
        public DateTime DateTime { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }

    }

}
