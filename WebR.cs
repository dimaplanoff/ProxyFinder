using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;




namespace ProxyService
{

    class WebR
    {
        public static string ua = @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";

        private static string GetHtml(string url)
        {

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = 5000;
                req.AllowAutoRedirect = true;
                req.MaximumAutomaticRedirections = 50;
                req.UserAgent = ua;
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch
            {
                return "";
            }   
        }





        /// <summary>
        /// Возвращает список сайтов с проксями, которые нашел по передаваемой ссылке на яндекс и т.д.
        /// </summary>
        /// <param name="_url"></param>
        /// <returns></returns>
        public static string[] GetYandexHrefs(string _url)
        {
            var browser = new AngleSharp.Html.Parser.HtmlParser();
            string code = GetHtml(_url);
            if (code != "")
            {
                AngleSharp.Html.Dom.IHtmlDocument doc = browser.ParseDocument(code);
                return (from m in doc.Links select m.GetAttribute("HREF")).Where(m=>m.StartsWith("http")).Distinct().Where(m=> (
                        !m.Contains("yandex") && !m.Contains("google") && !m.Contains("mail") && !m.Contains("rambler") && !m.Contains("youtube")
                )).ToArray();
            }
            else return new string[0];
        }



        /// <summary>
        /// Возвращает список проксей с сайта, который нам нашел поисковик
        /// </summary>
        /// <param name="_url"></param>
        /// <returns></returns>
        public static string[] GetProxys(string _url)
        {
            List<string> Prxs = new List<string>();

            var browser = new AngleSharp.Html.Parser.HtmlParser();

            string code = GetHtml(_url);

            if (code != "")
            {
                AngleSharp.Html.Dom.IHtmlDocument doc = browser.ParseDocument(code);

                var TRs = doc.GetElementsByTagName("TR");

                foreach (var TR in TRs)
                {
                    try
                    {
                        string tds = "";
                        var TDs = TR.GetElementsByTagName("TD");
                        foreach (var TD in TDs)
                            tds += " " + TD.TextContent + " ";

                        string tr = Regex.Replace(tds, @"[^0-9\.]", " ");
                        while (tr.Contains("  ")) tr = tr.Replace("  ", " ");
                        MatchCollection M = Regex.Matches(tr, @"(?<prx>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3} \d{1,5} )+");
                        foreach (Match m in M)
                        {
                            string prx_ = m.Groups["prx"].Value.Replace(" ", ":");
                            if (prx_.EndsWith(":")) prx_ = prx_.Substring(0, prx_.Length - 1);
                            prx_ = prx_.Trim();
                            if (!Prxs.Contains(prx_))
                                Prxs.Add(prx_);
                        }
                    }
                    catch { }
                }
            }

            return Prxs.ToArray();

        }


        //проверка прокси на целевом сайте (3 попытки)
        public static bool Check(string proxy)
        {
            int n = 3;

            for (int i = 0; i < n; i++)
            {
                try
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(@"http://xxxxxx.xxx.ru/exxxlic/home.html");
                    req.Timeout = 5000;
                    req.AllowAutoRedirect = true;
                    req.MaximumAutomaticRedirections = 50;
                    req.Proxy = new WebProxy(proxy.Trim());
                    req.UserAgent = WebR.ua;
                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    {

                        using (var sw = new StreamReader(resp.GetResponseStream()))
                        {
                            string innertext = sw.ReadToEnd();
                            if (innertext.Contains(@"/epxxxxxxch/search.html"))
                            {
                                Console.WriteLine("check proxy : " + proxy + " > accepted");
                                return true;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //Console.WriteLine("web_exception : " + proxy + " > " + e.Message);
                }

            }
            Console.WriteLine("check proxy : " + proxy + " > no_working");
            return false;
        }


    }
}
