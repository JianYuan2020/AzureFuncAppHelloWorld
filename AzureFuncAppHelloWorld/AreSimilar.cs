
using System;
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
    public static class AreSimilar
    {
        static bool areSimilar(int[] a, int[] b)
        {
            int lastA = -1, lastB = -1, swapCnt = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == b[i])
                    continue;
                if (lastA < 0)
                {    // 1st time diff
                    lastA = a[i];
                    lastB = b[i];
                    continue;
                }
                if (lastA == b[i] && lastB == a[i])
                {
                    swapCnt++;
                    if (swapCnt > 1)
                        return false;
                }
                else
                    return false;
            }
            return true;
        }
        static string IsSimilar(string[] twoNumberList)
        {
            if (twoNumberList.Length != 2)
                return "Input should be two number list";

            int[] a = Array.ConvertAll(twoNumberList[0].Split(','), arrTemp => Convert.ToInt32(arrTemp));
            int[] b = Array.ConvertAll(twoNumberList[1].Split(','), arrTemp => Convert.ToInt32(arrTemp));
            return areSimilar(a, b).ToString();
        }

        [FunctionName("AreSimilar")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // http://localhost:7071/api/AreSimilar?sNumbers=1,1,4;1,2,3
            string s = req.Query["sNumbers"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            s = s ?? data?.s;

            string responseMessage = string.IsNullOrEmpty(s)
                ? "This HTTP triggered function executed successfully. Pass a s(string) in the query string or in the request body for response."
                : $"Hello, the are similar for {s} is {IsSimilar(s.Split(';'))}.";

            return new OkObjectResult(responseMessage);
        }
    }
}
