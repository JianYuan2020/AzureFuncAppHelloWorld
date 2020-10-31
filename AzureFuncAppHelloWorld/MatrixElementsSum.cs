
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
    public static class MatrixElementsSum
    {
        static int matrixElementsSum(int[][] matrix)
        {
            int r = 0, c = 0, rowCnt = matrix.Length, colCnt = matrix[0].Length, sum = 0;
            List<int> ghostList = new List<int>();
            for (r = 0; r < rowCnt; r++)
            {
                for (c = 0; c < colCnt; c++)
                {
                    if (!ghostList.Contains(c))
                        sum += matrix[r][c];
                    if (matrix[r][c] == 0)
                        ghostList.Add(c);
                }
            }
            return sum;
        }

        [FunctionName("MatrixElementsSum")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // http://localhost:7071/api/MatrixElementsSum?sNumbers=1,1,1;2,2,2
            string s = req.Query["sNumbers"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            s = s ?? data?.s;

            string[] rowStrs = s.Split(';');
            int rowCnt = rowStrs.Length;
            int[][] matrix = new int[rowCnt][];
            for (int i = 0; i < rowCnt; i++)
            {
                matrix[i] = Array.ConvertAll(rowStrs[i].Split(','), arrTemp => Convert.ToInt32(arrTemp));
            }

            string responseMessage = string.IsNullOrEmpty(s)
                ? "This HTTP triggered function executed successfully. Pass a s(string) in the query string or in the request body for response."
                : $"Hello, the matrix elements sum for {s} is {matrixElementsSum(matrix)}.";

            return new OkObjectResult(responseMessage);
        }
    }
}
