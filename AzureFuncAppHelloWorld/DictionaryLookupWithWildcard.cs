using System;
using System.Collections.Generic;
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
    public static class DictionaryLookupWithWildcard
    {
        static bool IsMatch(string query, string word)
        {
            // word has the same char[0] and length as query
            for (int i = 1; i < query.Length; i++)
            {
                if (query[i] == '*')
                    continue;
                if (word[i] != query[i])
                    return false;
            }
            return true;
        }
        static bool IsMemberSubset(string query, HashSet<string> matchChar0Subset)
        {
            foreach (string word in matchChar0Subset)
            {
                if (IsMatch(query, word))
                    return true;
            }
            return false;
        }
        // Note: 
        //      Both input strings are neither null nor empty from below
        //      No need to Trim() since they are http query inputs
        static bool IsMember(string query, string words)
        {
            HashSet<string> matchChar0Subset = new HashSet<string>();
            //Dictionary<char, HashSet<string>> myDiction = new Dictionary<char, HashSet<string>>();
            string[] wordArray = words.Split('_');
            string word;
            for (int i=0; i<wordArray.Length; i++)
            {
                word = wordArray[i];
                if (word.Length == 0 || word.Length != query.Length)
                    continue;

                if (word[0] == query[0])
                    matchChar0Subset.Add(word);
            }
            return IsMemberSubset(query, matchChar0Subset);
        }

        [FunctionName("DictionaryLookupWithWildcard")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // http://localhost:7071/api/DictionaryLookupWithWildcard?query=f*x&words=foo_bar_fox
            string query = req.Query["query"];
            string words = req.Query["words"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            query = query ?? data?.query;
            words = words ?? data?.words;

            string responseMessage = string.IsNullOrEmpty(query) || string.IsNullOrEmpty(words)
                ? "This HTTP triggered function executed successfully. Pass a query and words in the query string or in the request body for a response."
                : $"Hello, the dictionary lookup with wildcard for {query} in {words} is {IsMember(query, words).ToString()}.";

            return new OkObjectResult(responseMessage);
        }
    }
}
