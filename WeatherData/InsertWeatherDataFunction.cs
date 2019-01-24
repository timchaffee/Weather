using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace WeatherData
{
    public class InsertWeatherDataFunction : ILambdaAPIFunctionHandler
    {
        public InsertWeatherDataFunction(IDynamoDBContext dbContext)
        {
            DBContext = dbContext;
        }

        private IDynamoDBContext DBContext;
        public async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest input, ILambdaContext context)
        {
            context.Logger.LogLine($"Beginning to process {nameof(InsertWeatherDataFunction)} ...");
            APIGatewayProxyResponse response = new APIGatewayProxyResponse
            {
                Headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { { "Content-Type", "application/json" } },
            };
            try
            {

                if (input.Headers?.ContainsKey("content-type") == true &&
                    input.Headers?["content-type"].IndexOf("application/json", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    WeatherEvent weather;
                    try
                    {
                        weather = JsonConvert.DeserializeObject<WeatherEvent>(input.Body, new JsonSerializerSettings
                        {
                            MissingMemberHandling = MissingMemberHandling.Error
                        });
                    }
                    catch (Exception ex)
                    {
                        context.Logger.LogLine($"Invalid Body: {input.Body}");
                        context.Logger.LogLine($"Exception: {ex}");
                        response.StatusCode = 400;
                        response.Body = @"{""code"":400,""message"":""Bad Request - Body must be valid JSON WeatherEvent""}";
                        return response;
                    }
                    WeatherEventData data = new WeatherEventData
                    {
                        DateTime = weather.Published,
                        Device = weather.ID,
                        Humidity = GetDouble(weather.Data.Split(',')[0]),
                        Temperature = GetDouble(weather.Data.Split(',')[1]),
                    };

                    await DBContext.SaveAsync<WeatherEventData>(data, default(CancellationToken));
                    response.StatusCode = 200;
                    response.Body = @"{""code"":200,""message"":""Success""}";
                    return response;
                }
                else
                {
                    context.Logger.LogLine($"Invalid Content-Type: {(input.Headers?.ContainsKey("content-type") == true ? input.Headers["content-type"] : "Unknown") }");
                    response.StatusCode = 400;
                    response.Body = @"{""code"":400,""message"":""Bad Request - Content-Type must be application/json""}";
                    return response;
                }
            }

            catch (Exception ex)
            {
                context.Logger.LogLine($"Error: {ex}");
                response.StatusCode = 500;
                response.Body = @"{""code"":500,""message"":""Internal Server Error""}";
                return response;
            }
            finally
            {
                context.Logger.LogLine($"Finished process {nameof(InsertWeatherDataFunction)} ...");
            }
        }
        private double GetDouble(string data)
        {
            if (double.TryParse(data, out double d))
            {
                return d;
            }
            return 0;
        }
    }
}
