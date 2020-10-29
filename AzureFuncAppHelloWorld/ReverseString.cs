
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFuncAppHelloWorld
{
    public static class ReverseString
    {
        static string RevString(char[] arrayChar)
        {
            if (arrayChar.Length == 0)
                return "";

            char tempC;
            int halfLen = arrayChar.Length / 2;
            int beginIndex = 0, endIndex = arrayChar.Length - 1;
            while (beginIndex < halfLen)
            {
                tempC = arrayChar[beginIndex];
                arrayChar[beginIndex++] = arrayChar[endIndex];
                arrayChar[endIndex--] = tempC;
            }
            return new string(arrayChar);
        }

        [FunctionName("ReverseString")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // http://localhost:7071/api/ReverseString?s=hello
            string s = req.Query["s"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            s = s ?? data?.s;

            string responseMessage = string.IsNullOrEmpty(s)
                ? "This HTTP triggered function executed successfully. Pass a s(string) in the query string or in the request body for response."
                : $"Hello, the reverse string for {s} is {RevString(s.ToCharArray())}.";

            return new OkObjectResult(responseMessage);
        }
    }
}
