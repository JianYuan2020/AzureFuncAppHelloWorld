
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFuncAppHelloWorld
{
    public static class CommonCharacterCount
    {
        static int commonCharCount(string s1, string s2)
        {
            List<char> s2List = new List<char>(s2.ToCharArray());
            int sum = 0;
            foreach (char c in s1)
            {
                if (s2List.Contains(c))
                {
                    s2List.Remove(c);
                    sum++;
                }
            }
            return sum;
        }

        [FunctionName("CommonCharacterCount")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // http://localhost:7071/api/CommonCharacterCount?s1=aabcc&s2=adcaa
            string s1 = req.Query["s1"];
            string s2 = req.Query["s2"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            s1 = s1 ?? data?.s1;
            s2 = s2 ?? data?.s2;

            string responseMessage = string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)
                ? "This HTTP triggered function executed successfully. Pass a s(string) in the query string or in the request body for response."
                : $"Hello, the common character count for {s1} and {s2} is {commonCharCount(s1, s2)}.";

            return new OkObjectResult(responseMessage);
        }
    }
}
