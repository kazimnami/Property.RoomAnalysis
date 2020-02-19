using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Reflection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using HtmlAgilityPack;

using PropertyScraper.Entities;
using System.Threading;

namespace PropertyScraper
{
    class Program
    {
        private static string url = "https://flatmates.com.au/share-houses+student-accommodation+studios+homestays/sydney/min-500?search_source=search_function";
        private static bool devMode = true;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            try
            {
                var suburbList = new Dictionary<string, Suburb>();

                // finding the chrome executable path (bin)
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                using(var driver = new ChromeDriver(path))
                {
                    driver.Navigate().GoToUrl(url);
                    
                    // doing some web storage manipulation
                    driver.ExecuteScript("localStorage.setItem('language', 'en');");
                    driver.ExecuteScript("localStorage.setItem('lat', '30');");
                    driver.ExecuteScript("localStorage.setItem('lng', '30');");
                    
                    // refreshing is important for the website js to handle the updated localStorage
                    driver.Navigate().Refresh();
                    
                    do
                    {
                        // sleeping/throttling waiting for everything to settle
                        Thread.Sleep(5 * 1000);
                        
                        var searchResults = driver.FindElementByCssSelector(".content-wrapper");
                        Console.WriteLine("Search Results: " + searchResults.GetAttribute("innerHTML"));
                        
                        var doc = new HtmlDocument();
                        doc.LoadHtml(searchResults.GetAttribute("innerHTML"));

                        Console.WriteLine();
                        Console.WriteLine($"***************************************************************************************");

                        var nextButton = doc.DocumentNode.SelectNodes("//a[starts-with(@aria-label, 'Go to next page')]");
                        var searchResult = doc.DocumentNode.SelectNodes("//p[starts-with(@class, 'styles__address')]");

                        foreach (var resultNode in searchResult)
                        {
                            var address = resultNode.InnerText.Split(',');
                            var suburbName = address.ElementAt(1);

                            Suburb.AddUpdate(suburbList, suburbName);
                        }

                        if (nextButton == null || nextButton.Count() <= 0) break;

                        driver.Navigate().GoToUrl("https://flatmates.com.au" + nextButton.First().GetAttributeValue("href", ""));

                    } while (true);

                    Console.WriteLine($"***************************************************************************************");

                    CreateFile (suburbList);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
                Console.ReadLine();
            }
        }

        private static void CreateFile(Dictionary<string, Suburb> suburbList)
        {
            Console.WriteLine();
            Console.WriteLine($"***************************************************************************************");
            Console.WriteLine("Creating File");

            var fileName = $"Flatmates.com.au Suburb List {DateTime.Now.ToString("yyyyMMddTHHmmss")}";
            var directoryLocation = @"c:\Import";
            var filePath = Path.Combine(directoryLocation, fileName + ".csv");
            var directoryInfo = Directory.CreateDirectory(directoryLocation);

            using (var file = File.CreateText(filePath))
            {
                var headerList = new List<string>
                {
                    "Suburb", // 0
                    "Count", // 1
                };

                file.WriteLine(string.Join(',', headerList));

                var line = new StringBuilder();
                foreach (var product in suburbList.Values)
                {
                    line.Clear();
                    line.Append(product.Name + ","); //"Name", // 0
                    line.Append(product.Count + ","); //"Count", // 1

                    file.WriteLine(line);
                }
            }

            File.Move(filePath, Path.Combine(directoryLocation, fileName + ".CSV"));
        }
    }
}
