
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
    public static class ReverseInParentheses
    {
        static string reverseString(char[] arrayChar)
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

        static List<int> GetIndexListForChar(string inputString, char c)
        {
            List<int> myIndexList = new List<int>();
            int myIndex = inputString.IndexOf(c, 0);
            while (myIndex >= 0)
            {
                myIndexList.Add(myIndex);
                myIndex = myIndex < inputString.Length - 1 ? inputString.IndexOf(c, myIndex + 1) : -1;
            }
            return myIndexList;
        }

        static int GetClosestOpen(List<int> openPIndexList, int closePIndex)
        {
            int closestOpen = -1, tempOpen = -1;
            for (int i = 0; i < openPIndexList.Count; i++)
            {
                tempOpen = openPIndexList[i];
                if (tempOpen < closePIndex)
                    closestOpen = tempOpen;
                else
                    break;
            }
            return closestOpen;
        }

        static string reverseOneParenthese(string inputString, int openPIndex, int closeIndex)
        {
            string outputString = "";

            if (openPIndex > 0)
                outputString += inputString.Substring(0, openPIndex);
            outputString += reverseString(inputString.Substring(openPIndex, closeIndex - openPIndex + 1).ToCharArray());
            if (closeIndex < inputString.Length - 1)
                outputString += inputString.Substring(closeIndex + 1);

            return outputString;
        }

        static string reverseInParentheses(string inputString)
        {
            List<int> openPIndexList = GetIndexListForChar(inputString, '(');
            List<int> closePIndexList = GetIndexListForChar(inputString, ')');
            if (openPIndexList.Count != closePIndexList.Count)
                return "Not all parentheses are paired";

            int openP, closeP, len = inputString.Length;
            while (closePIndexList.Count > 0)
            {
                closeP = closePIndexList[0];
                openP = GetClosestOpen(openPIndexList, closeP);
                openPIndexList.Remove(openP);
                closePIndexList.Remove(closeP);
                inputString = reverseOneParenthese(inputString, openP, closeP);
            }
            inputString = inputString.Replace("(", "");
            return inputString.Replace(")", "");
        }

        [FunctionName("ReverseInParentheses")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // http://localhost:7071/api/ReverseInParentheses?s=foo(bar)baz(blim)
            // http://localhost:7071/api/ReverseInParentheses?s=(())(((())))
            string s = req.Query["s"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            s = s ?? data?.s;

            string responseMessage = string.IsNullOrEmpty(s)
                ? "This HTTP triggered function executed successfully. Pass a s(string) in the query string or in the request body for response."
                : $"Hello, the reverse in parentheses for {s} is {reverseInParentheses(s)}.";

            return new OkObjectResult(responseMessage);
        }
    }
}
