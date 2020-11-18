
using System.Linq;
using System;
using System.Collections;
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
    public static class MinimalBasketPrice
    {
        static void GetVendorsParams(int[] venIndex, int[] vendorsDelivery, int[][] vendorsProducts,
            out int m_MaxDel, out int m_MinPrice, out bool m_HasAllItems)
        {
            m_MaxDel = m_MinPrice = 0;
            m_HasAllItems = true;

            int vIndex, itemPrice, itemMinPrice, numItems = vendorsProducts[0].Length;
            for (int i = 0; i < numItems; i++)
            {
                itemMinPrice = int.MaxValue;
                for (int v = 0; v < venIndex.Length; v++)
                {
                    vIndex = venIndex[v];
                    if (vendorsDelivery[vIndex] > m_MaxDel) m_MaxDel = vendorsDelivery[vIndex];

                    itemPrice = vendorsProducts[vIndex][i];
                    if (itemPrice > 0 && itemMinPrice > itemPrice)
                        itemMinPrice = itemPrice;

                } // for (int v = 0; v < venIndex.Length; v++)

                if (itemMinPrice == int.MaxValue)
                    m_HasAllItems = false;
                else
                    m_MinPrice += itemMinPrice;
            } // for (int i = 0; i < numItems; i++)
        }

        static IEnumerable<IEnumerable<T>> GetKCombs<T>(IEnumerable<T> list, int length) where T : IComparable
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetKCombs(list, length - 1)
                .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        static int[] GetSortedVenIndex(int[] minPriceVenIndex)
        {
            List<int> retList = new List<int>(minPriceVenIndex);
            retList.Sort();
            return retList.ToArray();
        }

        static int[] GetFastSubVenIndex(int[] vendorsDelivery, int maxVenCnt)
        {
            if (vendorsDelivery.Length <= maxVenCnt)
                return Enumerable.Range(0, vendorsDelivery.Length).ToArray();

            // vendorsDelivery.Length > maxVenCnt
            int i, venIndex;
            SortedList delVenIndexList = new SortedList();
            for (i = 0; i < vendorsDelivery.Length; i++)
                delVenIndexList.Add(vendorsDelivery[i], i);

            i = 0;
            List<int> venIndexList = new List<int>();
            while (i < maxVenCnt)
            {
                if (i >= delVenIndexList.Count)
                    break;

                venIndex = (int)delVenIndexList.GetByIndex(i++);
                venIndexList.Add(venIndex);
            }
            return venIndexList.ToArray();
        }

        static int[] minimalBasketPrice(int maxPrice, int[] vendorsDelivery, int[][] vendorsProducts)
        {
            /*// Get the fastest sub group of vendors
            int maxVenCnt = 10;
            int[] fastSubVenIndex = GetFastSubVenIndex(vendorsDelivery, maxVenCnt);

            int[] minPriceVenIndex = null, minDeliveryVenIndex = null;
            int minPrice = int.MaxValue, minPriceMaxDelivery = 0;
            int minDelivery = int.MaxValue, minDeliveryMinPrice = 0;

            int[] venIndArr;
            int checkInterval = fastSubVenIndex.Length < 5 ? fastSubVenIndex.Length : 5;
            for (int l = 1; l <= fastSubVenIndex.Length; l++) {

                var fastSubVenIndexComb = GetKCombs(fastSubVenIndex, l);
                foreach (var venInd in fastSubVenIndexComb)
                {
                    venIndArr = venInd.ToArray();
                    Console.WriteLine($"[{string.Join(", ", venIndArr)}]");
                    if (venIndArr.Length == 0)
                        continue;

                    GetVendorsParams(venIndArr, vendorsDelivery, vendorsProducts,
                        out int m_MaxDel, out int m_MinPrice, out bool m_HasAllItems);
                    if (m_HasAllItems && m_MinPrice <= maxPrice)
                    {
                        if (m_MinPrice < minPrice)
                        {
                            minPrice = m_MinPrice;
                            minPriceVenIndex = venIndArr;
                            minPriceMaxDelivery = m_MaxDel;
                        }
                        if (m_MaxDel < minDelivery)
                        {
                            minDelivery = m_MaxDel;
                            minDeliveryVenIndex = venIndArr;
                            minDeliveryMinPrice = m_MinPrice;
                        }
                    }
                }

                // Check at interval
                if (l % checkInterval == 0)
                {
                    if (minPriceVenIndex != null && minDeliveryVenIndex != null && minDelivery < minPriceMaxDelivery)
                        break;
                }
            }

            Console.WriteLine($"The best (maxPrice={maxPrice}) price: {minPrice}, vendor list: [{string.Join(", ", minPriceVenIndex)}], its max delivery: {minPriceMaxDelivery}");
            Console.WriteLine($"The best delivery: {minDelivery}, vendor list: [{string.Join(", ", minDeliveryVenIndex)}], its min price: {minDeliveryMinPrice}");
            return minDeliveryVenIndex != null? GetSortedVenIndex(minDeliveryVenIndex) : GetSortedVenIndex(minPriceVenIndex);*/

            /* Here is the 1st place C# winner (the 23rd overall winners) ... is the return array sorted? No, the test case below can show the bug.
            object minimalBasketPrice(int maxPrice, int[] vendorsDelivery, int[][] vendorsProducts)
            {
            */
            var g = new HashSet<int>();
            for (int i = vendorsDelivery.Min(); i <= vendorsDelivery.Max(); i++)
            {
                g.Clear();

                // Get a list of venders with delivery <= i
                var x = new List<int>();
                for (int q = 0; q < vendorsDelivery.Length; q++)
                {
                    // The list order is ascending
                    if (vendorsDelivery[q] <= i) { x.Add(q); }
                }

                int sum = 0;
                bool hasAllItems = true;
                for (int q = 0; q < vendorsProducts[0].Length; q++)
                {
                    int minPriceVenIndex = -1, venIndex;
                    int minItemPrice = 100000000, itemPrice;
                    for (int v = 0; v < x.Count; v++)
                    {
                        venIndex = x.ElementAt(v);
                        itemPrice = vendorsProducts[venIndex][q];
                        if (itemPrice > 0)
                        {
                            if (minItemPrice > itemPrice)
                            {
                                minItemPrice = itemPrice;
                                minPriceVenIndex = venIndex;
                            }
                        }
                    } // for item q, go over all venders with delivery <= i, get the min price and venIndex pair or 100000000 and -1

                    if (minPriceVenIndex < 0)
                        hasAllItems = false;
                    else
                    {
                        sum += minItemPrice;
                        g.Add(minPriceVenIndex);
                    }
                } // go over all items 

                // If we found an answer, return ... is the return array sorted? No, the test case below can show the bug.
                if (hasAllItems && sum <= maxPrice) return g.ToArray();
            }
            return g.ToArray();
            //}
        }

        [FunctionName("MinimalBasketPrice")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // http://localhost:7071/api/MinimalBasketPrice?sNumbers=1,2,3,4,3,6
            string s = req.Query["sNumbers"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            s = s ?? data?.s;

            // Ignore the "sNumbers" for now, here is the test case input

            /*int maxPrice = 7;
            int[] vendorsDelivery = new int[] { 5, 4, 2, 3 };

            int[][] vendorsProducts = new int[4][];
            vendorsProducts[0] = new int[] { 1, 1, 1 };
            vendorsProducts[1] = new int[] { 3, -1, 3 };
            vendorsProducts[2] = new int[] { -1, 2, 2 };
            vendorsProducts[3] = new int[] { 5, -1, -1 };*/

            int maxPrice = 6;
            int[] vendorsDelivery = new int[] { 1, 5, 10, 12 };
            int[][] vendorsProducts = new int[4][];
            vendorsProducts[0] = new int[] { -1, -1, -1 };
            vendorsProducts[1] = new int[] { -1, -1, 1 }; //vendorsProducts[1] = new int[] { 3, -1, -1 };
            vendorsProducts[2] = new int[] { -1, 2, -1 };
            vendorsProducts[3] = new int[] { 3, -1, -1 }; //vendorsProducts[3] = new int[] { -1, -1, 1 };

            string responseMessage = string.IsNullOrEmpty(s)
                ? "This HTTP triggered function executed successfully. Pass a s(string) in the query string or in the request body for response."
                : $"Hello, the minimal basket price for {s} is {string.Join(",", minimalBasketPrice(maxPrice, vendorsDelivery, vendorsProducts))}.";

            return new OkObjectResult(responseMessage);
        }
    }
}
