
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
    public static class AlmostIncreasingSequence
    {
        static int ProcessNextInt(Stack<int> tempStack, int nextInt, int removeCount)
        {
            // tempStack is never empty here so we can peek
            int stockTop = tempStack.Peek();
            if (stockTop < nextInt)
            {
                tempStack.Push(nextInt);
                return removeCount;
            }

            // stockTop >= next1Int, decide whether to remove the stockTop or next1Int
            removeCount++;
            if (stockTop > nextInt)
            {
                tempStack.Pop();
                if (tempStack.Count > 0 && tempStack.Peek() >= nextInt)
                {
                    tempStack.Push(stockTop);
                }
                else
                {
                    tempStack.Push(nextInt);
                }
            }
            return removeCount;
        }

        static bool almostIncreasingSequence(int[] sequence)
        {

            if (sequence.Length <= 2)
                return true;

            int removeCount = 0;
            Stack<int> tempStack = new Stack<int>();
            tempStack.Push(sequence[0]);

            for (int i = 1; i < sequence.Length; i++)
            {
                removeCount = ProcessNextInt(tempStack, sequence[i], removeCount);
                if (removeCount > 1)
                {
                    return false;
                }
            }
            return true;
        }

        [FunctionName("AlmostIncreasingSequence")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // http://localhost:7071/api/AlmostIncreasingSequence?sNumbers=1,2,3,4,3,6
            string s = req.Query["sNumbers"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            s = s ?? data?.s;

            int[] numbers = s.Split(',').Select(int.Parse).ToArray();

            string responseMessage = string.IsNullOrEmpty(s)
                ? "This HTTP triggered function executed successfully. Pass a s(string) in the query string or in the request body for response."
                : $"Hello, the almost increasing sequence for {s} is {almostIncreasingSequence(numbers).ToString()}.";

            return new OkObjectResult(responseMessage);
        }
    }
}
