using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Google.Apis.Auth.OAuth2;
using System.Net.Http;
using Microsoft.Extensions.Configuration;


namespace get_token
{
    public static class get_token
    {
        [FunctionName("get-token")]
        public static async Task<IActionResult> Run( [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log, ExecutionContext context)
        {

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();


            GoogleKey googleKey = new GoogleKey
            {
                type = config["type"],
                project_id = config["project_id"],
                private_key_id = config["private_key_id"],
                private_key = config["private_key"],
                client_email = config["client_email"],
                client_id = config["client_id"],
                auth_uri = config["auth_uri"],
                token_uri = config["token_uri"],
                auth_provider_x509_cert_url = config["auth_provider_x509_cert_url"],
                client_x509_cert_url = config["client_x509_cert_url"]
            };

            var googleKeyJson = JsonConvert.SerializeObject(googleKey);

            log.LogError(googleKeyJson);
            string token = GetAccessTokenFromJSONKey( googleKeyJson, "https://www.googleapis.com/auth/analytics.readonly" );

            return token != null
                ? (ActionResult)new OkObjectResult(token)
                : new BadRequestObjectResult("Token not retrieve");
        }



        class GoogleKey
        {
            public string type { get; set; }
            public string project_id { get; set; }
            public string private_key_id { get; set; }
            public string private_key { get; set; }
            public string client_email { get; set; }
            public string client_id { get; set; }
            public string auth_uri { get; set; }
            public string token_uri { get; set; }
            public string auth_provider_x509_cert_url { get; set; }
            public string client_x509_cert_url { get; set; }
        }




        public static string GetAccessTokenFromJSONKey(string jsonkey, params string[] scopes)
        {
            return GetAccessTokenFromJSONKeyAsync(jsonkey, scopes).Result;
        }




        public static async Task<string> GetAccessTokenFromJSONKeyAsync(string jsonkey, params string[] scopes)
        {
            return await GoogleCredential.FromJson(jsonkey)
                    .CreateScoped(scopes)  
                    .UnderlyingCredential 
                    .GetAccessTokenForRequestAsync(); 

        }

    }
}
