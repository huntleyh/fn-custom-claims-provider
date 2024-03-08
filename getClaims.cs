// Reference for incoming payload
//https://learn.microsoft.com/en-us/entra/identity-platform/custom-extension-troubleshoot

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using TextJson = System.Text.Json.Serialization;

namespace contoso.demo
{
    public class getClaims
    {
        private readonly ILogger<getClaims> _logger;

        public getClaims(ILogger<getClaims> logger)
        {
            _logger = logger;
        }

        [Function("getClaims")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("triggered custom claims provider function");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                _logger.LogInformation("Incoming request: " + requestBody);
                if (requestBody == null)
                {
                    return new BadRequestObjectResult("No request body found");
                }

                dynamic data = JsonConvert.DeserializeObject(requestBody);

                // Read the correlation ID from the Azure AD  request    
                string correlationId = data?.data.authenticationContext.correlationId;
                if (correlationId == null)
                {
                    correlationId = Guid.NewGuid().ToString();
                }

                // Claims to return to Azure AD
                TokenIssuanceStartResponse r = new TokenIssuanceStartResponse();
                // r.data.actions[0].claims.CorrelationId = correlationId;
                r.data.actions[0].claims.apiVersion = "1.0.0";
                r.data.actions[0].claims.dateOfBirth = RandomDay().ToString("MM/dd/yyyy");
                // r.data.actions[0].claims.CustomRoles.Add("Writer");
                // r.data.actions[0].claims.CustomRoles.Add("Editor");

                _logger.LogInformation("Sending response: " + JsonConvert.SerializeObject(r));
                return new OkObjectResult(r);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new BadRequestObjectResult(e.Message);
            }
        }
        private Random gen = new Random();
        DateTime RandomDay()
        {
            DateTime start = new DateTime(1995, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(gen.Next(range));
        }
        // public class ResponseContent
        // {
        //     [JsonProperty("data")]
        //     public Data data { get; set; }
        //     public ResponseContent()
        //     {
        //         data = new Data();
        //     }
        // }

        // public class Data
        // {
        //     [JsonProperty("@odata.type")]
        //     public string odatatype { get; set; }
        //     public List<Action> actions { get; set; }
        //     public Data()
        //     {
        //         odatatype = "microsoft.graph.onTokenIssuanceStartResponseData";
        //         actions = new List<Action>();
        //         actions.Add(new Action());
        //     }
        // }

        // public class Action
        // {
        //     [JsonProperty("@odata.type")]
        //     public string odatatype { get; set; }
        //     public Claims claims { get; set; }
        //     public Action()
        //     {
        //         odatatype = "microsoft.graph.tokenIssuanceStart.ProvideClaimsForToken";
        //         claims = new Claims();
        //     }
        // }

        // public class Claims
        // {
        //     // [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //     // public string CorrelationId { get; set; }
        //     [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //     public string dateOfBirth { get; set; }
        //     public string apiVersion { get; set; }
        //     // public List<string> CustomRoles { get; set; }
        //     public Claims()
        //     {
        //         // CustomRoles = new List<string>();
        //     }
        // }

        public class TokenIssuanceStartResponse
        {
            [TextJson.JsonPropertyName("data")]
            public TokenIssuanceStartResponse_Data data { get; set; }
            public TokenIssuanceStartResponse()
            {
                data = new TokenIssuanceStartResponse_Data();
                data.odatatype = "microsoft.graph.onTokenIssuanceStartResponseData";

                this.data.actions = new List<TokenIssuanceStartResponse_Action>();
                this.data.actions.Add(new TokenIssuanceStartResponse_Action());
            }
        }

        public class TokenIssuanceStartResponse_Data
        {
            [TextJson.JsonPropertyName("@odata.type")]
            public string odatatype { get; set; }
            public List<TokenIssuanceStartResponse_Action> actions { get; set; }
        }

        public class TokenIssuanceStartResponse_Action
        {
            [TextJson.JsonPropertyName("@odata.type")]
            public string odatatype { get; set; }

            [TextJson.JsonIgnore(Condition = TextJson.JsonIgnoreCondition.WhenWritingNull)]
            public TokenIssuanceStartResponse_Claims claims { get; set; }

            public TokenIssuanceStartResponse_Action()
            {
                odatatype = "microsoft.graph.tokenIssuanceStart.provideClaimsForToken";
                claims = new TokenIssuanceStartResponse_Claims();
            }
        }

        public class TokenIssuanceStartResponse_Claims
        {
            [TextJson.JsonIgnore(Condition = TextJson.JsonIgnoreCondition.WhenWritingNull)]
            public string CorrelationId { get; set; }

            [TextJson.JsonIgnore(Condition = TextJson.JsonIgnoreCondition.WhenWritingNull)]
            public string apiVersion { get; set; }

            [TextJson.JsonIgnore(Condition = TextJson.JsonIgnoreCondition.WhenWritingNull)]
            public string dateOfBirth { get; set; }

            // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            // public string LoyaltyTier { get; set; }

            // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            // public string ApiVersion { get; set; }

            // public List<string> CustomRoles { get; set; }
            public TokenIssuanceStartResponse_Claims()
            {
                // CustomRoles = new List<string>();
            }
        }
    }
}
