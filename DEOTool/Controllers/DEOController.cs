using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DEOTool.Controllers
{
    public class SearchVM
    {
        public string Keyword { get; set; }
        public int PageNo { get; set; }
        public string EngineName { get; set; }

    }
    public class LinkData
    {
        public string Link { get; set; }
        public string Avaliable { get; set; }
        public string Label { get; set; }
        public string EngineName { get; set; }
        public int PageNo { get; set; }
    }
    public class ValidationData
    {
        public string result { get; set; }
        public string code { get; set; }
    }

    public class DEOController : Controller
    {

        [HttpGet]
        public async Task<IActionResult> GetSearchLinks()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetSearchLinks(SearchVM searchVM)
        {
            //searchVM.EngineName = "Yahoo";
            //Regex regex = null;
            //int totalPageCout = searchVM.PageNo;
            //string currentSearchEngine = string.Empty;
            //string SearcUrl = string.Empty;
            //int pageNo = 0;
            //List<LinkData> links = new List<LinkData>();
            //for (int i = 1; i < totalPageCout; i++)
            //{
            //    HttpClient client = new HttpClient();
            //    if (searchVM.EngineName == "Google")
            //    {
            //        pageNo = (i - 1) * 10;
            //        currentSearchEngine = "https://www.google.com/search?q=";
            //        SearcUrl = $"{currentSearchEngine}{searchVM.Keyword}&start={pageNo}";
            //        regex = new Regex("<div class=\"BNeawe UPmit AP7Wnd\">.*?</div>");

            //    }
            //    else if (searchVM.EngineName == "Yahoo")
            //    {
            //        pageNo = ((i - 1) * 10) + i;
            //        currentSearchEngine = "https://in.search.yahoo.com/search;?p=";
            //        SearcUrl = $"{currentSearchEngine}{searchVM.Keyword}&b={pageNo}";
            //        regex = new Regex("<span class=\" fz-ms fw-m fc-12th wr-bw lh-17\">.*?</span>");
            //    }
            //    var msg = await client.GetAsync(SearcUrl);
            //    if (msg.IsSuccessStatusCode)
            //    {
            //        string HtmlCode = await msg.Content.ReadAsStringAsync();

            //        //Regex regex = new Regex("<div class=\"BNeawe UPmit AP7Wnd\">.*?</div>");
            //        MatchCollection matches = regex.Matches(HtmlCode);
            //        HttpClient clientValidation = new HttpClient();
            //        if (matches.Count > 0)
            //        {
            //            foreach (Match match in matches)
            //            {
            //                LinkData link = new LinkData();
            //                link.EngineName = "Google";
            //                string linkk = match.Groups[0].Value;
            //                link.Label = linkk;
            //                string httpFilter = @"((|http\://|https\://|ftp\://)|(www.))+(([a-zA-Z0-9\.-]+\.[a-zA-Z]{2,4})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(/[a-zA-Z0-9%:/-_\?\.'~]*)?";
            //                Regex mm = new Regex(httpFilter);
            //                linkk = (mm.Match(linkk).Groups[4].Value).Replace("www.", "");
            //                //link.Label = mm.Match(linkk).Groups[0].Value;
            //                var valMsg = await clientValidation.GetAsync($"http://jitus.in/validate.php?username={linkk}");
            //                var valOutput = await valMsg.Content.ReadAsStringAsync();
            //                var val = JsonConvert.DeserializeObject<ValidationData>(valOutput);
            //                if (val != null)
            //                {
            //                    if (val.result.Contains("Success"))
            //                    {
            //                        link.Avaliable = "Yes";
            //                        link.Link = linkk;
            //                        link.PageNo = pageNo;
            //                        links.Add(link);
            //                    }
            //                }

            //            }
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }

            //}
            //if (links.Count > 0)
            //{
            //    var builder = new StringBuilder();
            //    builder.AppendLine("EngineName,Link,Label, Avaliable, Page no");
            //    foreach (var link in links)
            //    {
            //        builder.AppendLine($"{link.EngineName},{link.Link},{link.Label},{link.Avaliable}, {link.PageNo}");
            //    }
            //    return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "users.csv");

            //}
            //return View("Error");

            //return View("Show", links);


            List<LinkData> links = new List<LinkData>();
            if (searchVM.PageNo == 1)
                links.AddRange(await GoogleSearch(searchVM.Keyword));
            if (searchVM.PageNo == 2)
                links.AddRange(await YahooSearch(searchVM.Keyword));
            //links.AddRange(await BingSearch(searchVM.Keyword));
            if (searchVM.PageNo == 3)
                links.AddRange(await AskSearch(searchVM.Keyword));
            return DownloadLinks(links);
        }



        public async Task<List<LinkData>> GoogleSearch(string Keyword)
        {
            const string SearchEngineName = "Google";
            const string GoogleRegExString = "<div class=\"BNeawe UPmit AP7Wnd\">.*?</div>";
            List<LinkData> GoogleLinks = new List<LinkData>();
            HttpClient GoogleClient = new HttpClient();
            int pageNo = 0;
            for (int i = 1; i < 20; i++)
            {
                pageNo = (i - 1) * 10;
                string googleSearchResult = await GetGoogleResult(GoogleClient, Keyword, pageNo);
                await LinkExtractor(googleSearchResult, GoogleRegExString, SearchEngineName, GoogleLinks);
            }
            return GoogleLinks;
        }

        public async Task<List<LinkData>> YahooSearch(string Keyword)
        {
            const string SearchEngineName = "Yahoo";
            const string YahooRegExString = "<span class=\"fz-ms fw-m fc-12th wr-bw lh-17\">.*?</span>";
            List<LinkData> YahooLinks = new List<LinkData>();
            HttpClient YahooClient = new HttpClient();
            int pageNo = 0;
            for (int i = 1; i < 20; i++)
            {
                pageNo = (i - 1) * 10;
                string YahooSearchResult = await GetYahooResult(YahooClient, Keyword, pageNo);
                await LinkExtractor(YahooSearchResult, YahooRegExString, SearchEngineName, YahooLinks);
            }
            return YahooLinks;
        }

        public async Task<List<LinkData>> BingSearch(string Keyword)
        {
            const string SearchEngineName = "Bing";
            //const string BingRegExString = "<div class=\"b_attribution\" .*><cite>.*?</cite>";
            const string BingRegExString = "<div class=\"b_attribution\" .*><cite>.*?</cite>.*<a href=\"#\" class=\"trgr_icon\"";
            List<LinkData> BingLinks = new List<LinkData>();
            HttpClient BingClient = new HttpClient();
            int pageNo = 0;
            for (int i = 1; i < 20; i++)
            {
                pageNo = (i - 1) * 10;
                string BingSearchResult = await GetBingResult(BingClient, Keyword, pageNo);
                await LinkExtractor(BingSearchResult, BingRegExString, SearchEngineName, BingLinks);
            }
            return BingLinks;
        }

        public async Task<List<LinkData>> AskSearch(string Keyword)
        {
            const string SearchEngineName = "Ask";
            const string AskRegExString = "<p class=\"PartialSearchResults-item-url\">.*?</p>";
            List<LinkData> AskLinks = new List<LinkData>();
            HttpClient AskClient = new HttpClient();
            int pageNo = 0;
            for (int i = 1; i < 20; i++)
            {
                pageNo = i;
                string AskSearchResult = await GetAskResult(AskClient, Keyword, pageNo);
                await LinkExtractor(AskSearchResult, AskRegExString, SearchEngineName, AskLinks);
            }
            return AskLinks;
        }

        public FileContentResult DownloadLinks(List<LinkData> links, string SearchResultFileName = "")
        {
            if (links.Count > 0)
            {
                var builder = new StringBuilder();
                builder.AppendLine("EngineName,Link,Label, Avaliable, Page no");
                foreach (var link in links)
                {
                    builder.AppendLine($"{link.EngineName},{link.Link},{link.Label},{link.Avaliable}, {link.PageNo}");
                }
                return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", SearchResultFileName + "SearchResult.csv");

            }
            else
                return null;
        }

        public async Task<string> GetGoogleResult(HttpClient GoogleClient, string Keyword, int pageNo)
        {
            string GoogleEngineURL = $"https://www.google.com/search?q={Keyword}&start={pageNo}";
            return await GoogleClient.GetStringAsync(GoogleEngineURL);
        }

        public string ExtractDomain(string Link)
        {
            string httpFilter = @"((|http\://|https\://|ftp\://)|(www.))+(([a-zA-Z0-9\.-]+\.[a-zA-Z]{2,4})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(/[a-zA-Z0-9%:/-_\?\.'~]*)?";
            Regex mm = new Regex(httpFilter);
            return (mm.Match(Link).Groups[4].Value).Replace("www.", "");
        }
        HttpClient linkValidationClient = new HttpClient();
        public async Task<bool> CheckLinkAvailability(string LinkUrl)
        {

            var valOutput = await linkValidationClient.GetStringAsync($"http://jitus.in/validate.php?username={LinkUrl}");
            var val = JsonConvert.DeserializeObject<ValidationData>(valOutput);
            if (val != null)
            {
                return val.result.Contains("Success");
            }
            return false;
        }

        public async Task LinkExtractor(string SearchResult, string RegExString, string SearchEngineName, List<LinkData> links)
        {
            Regex regex = new Regex(RegExString);
            MatchCollection linkMatches = regex.Matches(SearchResult);
            if (linkMatches.Count > 0)
            {
                foreach (Match match in linkMatches)
                {
                    LinkData link = new LinkData();
                    string RawUrl = match.Groups[0].Value;
                    string Domain = ExtractDomain(RawUrl);
                    link.Label = RawUrl;
                    link.EngineName = SearchEngineName;
                    link.Link = Domain;
                    if (await CheckLinkAvailability(Domain))
                    {
                        link.Avaliable = "Yes";
                    }
                    else
                    {
                        link.Avaliable = "No";
                    }
                    links.Add(link);
                }
            }
        }

        public async Task<string> GetYahooResult(HttpClient YahooClient, string Keyword, int pageNo)
        {
            string YahooEngineURL = $"https://search.yahoo.com/search?p={Keyword}&b={pageNo}";
            return await YahooClient.GetStringAsync(YahooEngineURL);
        }

        public async Task<string> GetBingResult(HttpClient BingClient, string Keyword, int pageNo)
        {
            string BingEngineURL = $"https://www.bing.com/search?q={Keyword}&first={pageNo}";
            return await BingClient.GetStringAsync(BingEngineURL);
        }

        public async Task<string> GetAskResult(HttpClient AskClient, string Keyword, int pageNo)
        {
            string AskEngineURL = $"https://www.ask.com/web?q={Keyword}&page={pageNo}";
            return await AskClient.GetStringAsync(AskEngineURL);
        }


    }


}
