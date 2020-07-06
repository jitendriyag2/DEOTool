using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
            return  View();
        }
        [HttpPost]
        public async Task<IActionResult> GetSearchLinks(SearchVM searchVM)
        {
            searchVM.EngineName = "Yahoo";
            Regex regex = null;
            int totalPageCout = searchVM.PageNo ;
            string currentSearchEngine = string.Empty;
            string SearcUrl = string.Empty;
            int pageNo = 0;
            List<LinkData> links = new List<LinkData>();
            for (int i = 1; i < totalPageCout; i++)
            {
                HttpClient client = new HttpClient();
                if (searchVM.EngineName == "Google")
                {
                    pageNo = (i - 1) * 10;
                    currentSearchEngine = "https://www.google.com/search?q=";
                    SearcUrl = $"{currentSearchEngine}{searchVM.Keyword}&start={pageNo}";
                    regex = new Regex("<div class=\"BNeawe UPmit AP7Wnd\">.*?</div>");

                }
                else if (searchVM.EngineName == "Yahoo")
                {
                    pageNo = ((i -1) * 10) + i;
                    currentSearchEngine = "https://in.search.yahoo.com/search;?p=";
                    SearcUrl = $"{currentSearchEngine}{searchVM.Keyword}&b={pageNo}";
                    regex = new Regex("<span class=\" fz-ms fw-m fc-12th wr-bw lh-17\">.*?</span>");
                }
                var msg = await client.GetAsync(SearcUrl);
                if (msg.IsSuccessStatusCode)
                {
                    string HtmlCode = await msg.Content.ReadAsStringAsync();
                          
                    //Regex regex = new Regex("<div class=\"BNeawe UPmit AP7Wnd\">.*?</div>");
                    MatchCollection matches = regex.Matches(HtmlCode);
                    HttpClient clientValidation = new HttpClient();
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            LinkData link = new LinkData();
                            link.EngineName = "Google";
                            string linkk = match.Groups[0].Value;
                            link.Label = linkk;
                            string httpFilter = @"((|http\://|https\://|ftp\://)|(www.))+(([a-zA-Z0-9\.-]+\.[a-zA-Z]{2,4})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(/[a-zA-Z0-9%:/-_\?\.'~]*)?";
                            Regex mm = new Regex(httpFilter);
                            linkk = (mm.Match(linkk).Groups[4].Value).Replace("www.", "");
                            //link.Label = mm.Match(linkk).Groups[0].Value;
                            var valMsg = await clientValidation.GetAsync($"http://jitus.in/validate.php?username={linkk}");
                            var valOutput = await valMsg.Content.ReadAsStringAsync();
                            var val = JsonConvert.DeserializeObject<ValidationData>(valOutput);
                            if (val != null)
                            {
                                if (val.result.Contains("Success"))
                                {
                                    link.Avaliable = "Yes";
                                    link.Link = linkk;
                                    link.PageNo = pageNo;
                                    links.Add(link);
                                }
                            }
                           
                        }
                    }
                    else
                    {
                        break;
                    }
                }
               
            }
            if (links.Count > 0)
            {
                var builder = new StringBuilder();
                builder.AppendLine("EngineName,Link,Label, Avaliable, Page no");
                foreach (var link in links)
                {
                    builder.AppendLine($"{link.EngineName},{link.Link},{link.Label},{link.Avaliable}, {link.PageNo}");
                }
                return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "users.csv");

            }
            return View("Error");

            //return View("Show", links);


        }
    }
}
