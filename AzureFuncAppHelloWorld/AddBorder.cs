
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
    public static class AddBorder
    {
        static string[] addBorder(string[] picture)
        {
            int rowNum = picture.Length;
            int colNum = picture[0].Length;

            int len = 0;
            string starStr = "**";
            while (len++ < colNum)
            {
                starStr += '*';
            }

            string[] newPic = new string[rowNum + 2];
            newPic[0] = starStr;
            for (int r = 0; r < rowNum; r++)
            {
                newPic[r + 1] = '*' + picture[r] + '*';
            }
            newPic[rowNum + 1] = starStr;
            return newPic;
        }

        [FunctionName("AddBorder")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // http://localhost:7071/api/AddBorder?sPic=abc;ded
            string s = req.Query["sPic"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            s = s ?? data?.s;

            string responseMessage = string.IsNullOrEmpty(s)
                ? "This HTTP triggered function executed successfully. Pass a s(string) in the query string or in the request body for response."
                : $"Hello, the add border for {s} is {string.Join(";", addBorder(s.Split(';')))}.";

            return new OkObjectResult(responseMessage);
        }
    }
}
