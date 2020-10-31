
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
        class Vendors
        {
            public int[] m_VenIndex;
            public int m_MaxDel;
            public int m_MinPrice;
            public bool m_HasAllItems;

            public Vendors()
            {
                m_VenIndex = null;
            }

            public void SetVendors(int[] venIndex, int[] vendorsDelivery, int[][] vendorsProducts)
            {
                m_VenIndex = venIndex;
                m_MaxDel = 0;
                m_MinPrice = 0;
                m_HasAllItems = true;

                List<int[]> venProd = new List<int[]>();
                foreach (int v in m_VenIndex)
                {
                    if (vendorsDelivery[v] > m_MaxDel) m_MaxDel = vendorsDelivery[v];
                    venProd.Add(vendorsProducts[v]);
                }

                int numItems = vendorsProducts[0].Length;
                int[] minSums = new int[numItems];
                for (int i = 0; i < numItems; i++)
                {
                    minSums[i] = -1;
                    foreach (int[] vp in venProd)
                    {
                        if (vp[i] != -1)
                        {
                            minSums[i] = minSums[i] < 0 ? vp[i] : Math.Min(minSums[i], vp[i]);
                        }
                    }
                    if (minSums[i] == -1)
                        m_HasAllItems = false;
                    else
                        m_MinPrice += minSums[i];
                }
            }
        } // class Vendors

        public static IEnumerable<T[]> Permutations<T>(IEnumerable<T> source)
        {
            if (null == source)
                throw new ArgumentNullException(nameof(source));

            T[] data = source.ToArray();

            return Enumerable
              .Range(0, 1 << (data.Length))
              .Select(index => data
                 .Where((v, i) => (index & (1 << i)) != 0)
                 .ToArray());
        }

        static int[] GetSortedVenIndex(int[] minPriceVenIndex)
        {
            List<int> retList = new List<int>(minPriceVenIndex);
            retList.Sort();
            return retList.ToArray();
        }

        static int[] minimalBasketPrice(int maxPrice, int[] vendorsDelivery, int[][] vendorsProducts)
        {
            int[] allVenIndex = Enumerable.Range(0, vendorsDelivery.Length).ToArray();
            var allVenIndexCombinations = Permutations(allVenIndex);

            int minPrice = int.MaxValue, minDelivery = int.MaxValue;
            int[] minPriceVenIndex = null, minDeliveryVenIndex = null;
            int minPriceMaxDelivery = 0, minDeliveryMinPrice = 0;

            Vendors ven;
            foreach (int[] venIndex in allVenIndexCombinations)
            {
                // Console.WriteLine($"[{string.Join(", ", venIndex)}]");
                if (venIndex.Length == 0)
                    continue;

                ven = new Vendors();
                ven.SetVendors(venIndex, vendorsDelivery, vendorsProducts);
                if (ven.m_HasAllItems)
                {
                    if (ven.m_MinPrice <= maxPrice && ven.m_MinPrice < minPrice)
                    {
                        minPrice = ven.m_MinPrice;
                        minPriceVenIndex = ven.m_VenIndex;
                        minPriceMaxDelivery = ven.m_MaxDel;
                    }
                    if (ven.m_MinPrice <= maxPrice && ven.m_MaxDel < minDelivery)
                    {
                        minDelivery = ven.m_MaxDel;
                        minDeliveryVenIndex = ven.m_VenIndex;
                        minDeliveryMinPrice = ven.m_MinPrice;
                    }
                }
            }

            Console.WriteLine($"The best (maxPrice={maxPrice}) price: {minPrice}, vendor list: [{string.Join(", ", minPriceVenIndex)}], its max delivery: {minPriceMaxDelivery}");
            Console.WriteLine($"The best delivery: {minDelivery}, vendor list: [{string.Join(", ", minDeliveryVenIndex)}], its min price: {minDeliveryMinPrice}");
            return GetSortedVenIndex(minDeliveryVenIndex);
        }


        [FunctionName("MinimalBasketPrice")]
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
            vendorsProducts[1] = new int[] { 3, -1, -1 };
            vendorsProducts[2] = new int[] { -1, 2, -1 };
            vendorsProducts[3] = new int[] { -1, -1, 1 };

            string responseMessage = string.IsNullOrEmpty(s)
                ? "This HTTP triggered function executed successfully. Pass a s(string) in the query string or in the request body for response."
                : $"Hello, the almost increasing sequence for {s} is {string.Join(",", minimalBasketPrice(maxPrice, vendorsDelivery, vendorsProducts))}.";

            return new OkObjectResult(responseMessage);
        }
    }
}
