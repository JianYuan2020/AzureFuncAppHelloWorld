using System;
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
    public static class LongestPalindromicSubstring
    {
        static bool IsPalindromic(string s, int i, int len)
        {
            int max = i + len / 2, tailIndex = i + i + len - 1;
            for (int j = i; j < max; j++)
            {
                if (s[j] != s[tailIndex - j])
                    return false;
            }
            return true;
        }
        static string HasPalindromic(string s, int len)
        {
            for (int i = 0; i <= s.Length - len; i++)
            {
                if (IsPalindromic(s, i, len))
                {
                    return s.Substring(i, len);
                }
            }
            return "";
        }
        static string LongestPalindrome(string s)
        {
            s = s.Replace(" ", "");
            if (s.Length <= 1)
                return s;
            int len = s.Length;
            string temp;
            while (len >= 2)
            {
                temp = HasPalindromic(s, len);
                if (temp.Length > 0)
                {
                    return temp;
                }
                len--;
            }
            return s[0].ToString();
        }

        [FunctionName("LongestPalindromicSubstring")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string s = req.Query["s"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            s = s ?? data?.s;

            string responseMessage = string.IsNullOrEmpty(s)
                ? "This HTTP triggered function executed successfully. Pass a s(string) in the query string or in the request body for a personalized response."
                : $"Hello, the longest palindromic substring for {s} is {LongestPalindrome(s)}.";

            return new OkObjectResult(responseMessage);
        }
    }
}
