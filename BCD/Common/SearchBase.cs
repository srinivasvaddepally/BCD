using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Web;





namespace Downloads.Common
{
    public abstract class SearchBase 
    {
        internal static bool IS_IP_CHECKED = false;
        string TestUrl = "http://knowledge.amia.org";
        internal string Accept = "text/html, application/xhtml+xml, */*";
        internal const int BUFF_SIZE = 1024;
        internal int RetrySleepMultiplier = 5000;
        internal int Timeout = 20000;
        internal string DOMAIN_URL = "";
        internal string DOMAIN = "";
        internal string _lastResponsePayload = "";
        internal string _lastUrl = "";
        internal string Referer = "";
        internal string LastResponseEncoding = "";
        internal HttpStatusCode LastResponseStatusCode = HttpStatusCode.OK;
        internal Uri LastResponseUri = null;
        internal WebHeaderCollection LastResponseHeaders = null;
        internal bool ClearCookieJarOnTimeout = false;
        internal bool Compression = false;
        internal bool RetryIfRedirected = false;
        internal bool XmlHttpRequest = false;
        internal bool XmlWebService = false;
        private CookieCollection _lastResponseCookies = null;

        internal int _sleepInSecondsHigh = 120;
        internal int _sleepInSecondsLow = 30;

        internal int _sleepHigh = 120000;
        internal int _sleepLow = 30000;

        public SearchBase()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SleepInSecondsLow"]))
                _sleepInSecondsLow = Int32.Parse(ConfigurationManager.AppSettings["SleepInSecondsLow"].ToString());

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SleepInSecondsHigh"]))
                _sleepInSecondsHigh = Int32.Parse(ConfigurationManager.AppSettings["SleepInSecondsHigh"].ToString());

            _sleepHigh = _sleepInSecondsHigh * 1000;
            _sleepLow = _sleepInSecondsLow * 1000;
        }

        internal CookieContainer _cookieJar = new CookieContainer();

        internal string GetWebPageNoCookies(string webPageUrl)
        {
            if (!IS_IP_CHECKED)
            {
                IS_IP_CHECKED = true;
                string testPage = GetWebPage(TestUrl, false, null);
                Console.WriteLine(testPage);
                //WriteLog(testPage);
            }

            return GetWebPage(webPageUrl, false, null);
        }

        public string GetWebPage(string webPageUrl)
        {

         
            if (!IS_IP_CHECKED)
            {
                IS_IP_CHECKED = true;
                string testPage = GetWebPage(TestUrl, false, null);
                Console.WriteLine(testPage);
               // WriteLog(testPage);
            }

            return GetWebPage(webPageUrl, true, null);
        }
        //public string GetWebPageSelenium(string webPageUrl)
        //{

        //    var webPage = string.Empty;
        //    IWebDriver driver = new FirefoxDriver();
        //    driver.Navigate().GoToUrl(webPageUrl);
        //    driver.Manage().Window.Maximize();
        //    System.Threading.Thread.Sleep(5000);
        //    webPage = driver.PageSource;
        //    driver.Close();

        //    #region fixing Html Tags
        //    PostSubmitter post1 = new PostSubmitter();
        //    post1.Url = "http://fixmyhtml.com/";
        //    post1.PostItems.Add("html", webPage);
        //    post1.Type = PostSubmitter.PostTypeEnum.Post;
        //    webPage = post1.Post();
        //    #endregion

        //    return Mono.Web.HttpUtility.HtmlDecode(webPage).ToString();



        //}


        public string GetResponseURL(string webPageUrl,string postData)
        {
            if (!IS_IP_CHECKED)
            {
                IS_IP_CHECKED = true;
                string testPage = GetWebPage(TestUrl, false, null);
                Console.WriteLine(testPage);
                // WriteLog(testPage);
            }

            return GetWebPage(webPageUrl, true, null);
        }


        internal string GetWebPage(string webPageUrl, bool useCookieJar, byte[] payload)
        {
            int retries = 0;
            HttpWebResponse resp = null;
            HttpWebRequest req = null;
        RETRY:
            _lastUrl = webPageUrl;

            try
            {
                //string urlEncoded = System.Web.HttpUtility.UrlEncode(webPageUrl);
                Uri url = new Uri(webPageUrl);
                req = (HttpWebRequest)WebRequest.Create(url);
                //if (!string.IsNullOrEmpty(OutboundIp))
                //    req.ServicePoint.BindIPEndPointDelegate = new BindIPEndPoint(Bind);
                //req.KeepAlive = true;
                if (XmlWebService)
                {
                    req.Accept = "application/xml";
                }
                else if (XmlHttpRequest)
                {
                    req.Accept = "text/javascript, text/html, application/xml, text/xml, */*";
                    req.Headers.Add("X-Requested-With", "XMLHttpRequest");
                }
                else
                {
                    req.Accept = Accept;
                }
                //req.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.21022; .NET CLR 3.5.30729; OfficeLiveConnector.1.5; OfficeLivePatch.1.3; .NET4.0C; .NET CLR 3.0.30729; Creative AutoUpdate v1.40.01)";
                req.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
                req.Headers.Add("Accept-Language", "en-gb,en;q=0.5");
              //  req.Headers.Add("Accept-Language", "en-us");
                if (Compression)
                    req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                if (Referer.Any())
                    req.Referer = Referer;
                req.ProtocolVersion = HttpVersion.Version11;
                if (useCookieJar)
                    req.CookieContainer = _cookieJar;
                req.Timeout = Timeout;

                if (payload != null)
                {
                 
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.ContentLength = payload.Length;
                    Stream payloadStream = req.GetRequestStream();
                    payloadStream.Write(payload, 0, payload.Length);
                    payloadStream.Close();
                }

              //  WriteLog("START Web Rq " + webPageUrl);
                Console.WriteLine(DateTime.Now + " " + webPageUrl);
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {

                if (retries==1)
                {
                    Console.WriteLine("trying post submitter with other method of getting webpage..........");
                    PostSubmitter post = new PostSubmitter();
                    post.Url = webPageUrl;
                    post.Type = PostSubmitter.PostTypeEnum.Post;
                    string result = post.Post();
                    return result;

                }

                //TODO: Test with PLOS URLs
                if (!SearchBase.isValidUrl(webPageUrl))
                {
                    InvalidURLException appExc = new InvalidURLException("Url Is Not Valid: " + webPageUrl, ex);
                   // ConsoleError(DateTime.Now + " Url Is Not Valid: " + webPageUrl);
                    throw appExc;
                }

                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    if (retries > 5)
                        throw;

                    retries++;
                    if (retries == 5)
                        System.Threading.Thread.Sleep(4 * 60 * 60 * 1000);
                    else
                        System.Threading.Thread.Sleep(RetrySleepMultiplier * retries);
                   // ConsoleError(DateTime.Now + " RETRY (TO) " + webPageUrl);
                  //  WriteWarning("RETRY(WE-TO) " + webPageUrl);
                    if (useCookieJar && ClearCookieJarOnTimeout)
                        _cookieJar = new CookieContainer();
                    goto RETRY;
                }
                else if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    if (retries >= 1)
                    {
                        throw;
                    }
                        

                    retries++;
                    resp = (HttpWebResponse)ex.Response;

                    //Common errors - least to most serious
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        System.Threading.Thread.Sleep(60 * 1000);
                       // ConsoleError(DateTime.Now + " RETRY (PE-NF) " + webPageUrl);
                      //  WriteWarning("RETRY(WE-PE-NF) " + webPageUrl);
                    }
                    else if (resp.StatusCode == HttpStatusCode.GatewayTimeout)
                    {
                        if (retries == 2)
                            System.Threading.Thread.Sleep(4 * 60 * 60 * 1000);
                        else
                            System.Threading.Thread.Sleep(RetrySleepMultiplier * retries);
                      //  ConsoleError(DateTime.Now + " RETRY (PE-GT) " + webPageUrl);
                        //WriteWarning("RETRY(WE-PE-GT) " + webPageUrl);
                    }
                    else if (resp.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        if (retries == 2)
                            System.Threading.Thread.Sleep(4 * 60 * 60 * 1000);
                        else
                            System.Threading.Thread.Sleep(RetrySleepMultiplier * retries);
                       // ConsoleError(DateTime.Now + " RETRY (PE-IS) " + webPageUrl);
                        //WriteWarning("RETRY(WE-PE-IS) " + webPageUrl);
                    }
                    else if (resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        if (retries == 2)
                            System.Threading.Thread.Sleep(4 * 60 * 60 * 1000);
                        else
                            System.Threading.Thread.Sleep(RetrySleepMultiplier * retries);
                       // ConsoleError(DateTime.Now + " RETRY (PE-SU) " + webPageUrl);
                       // WriteWarning("RETRY(WE-PE-SU) " + webPageUrl);
                    }
                    else if (resp.StatusCode == HttpStatusCode.Forbidden)
                    {
                        if (retries == 2)
                            System.Threading.Thread.Sleep(24 * 60 * 60 * 1000);
                        else
                            System.Threading.Thread.Sleep(RetrySleepMultiplier * retries);
                       // ConsoleError(DateTime.Now + " RETRY (PE-FO) " + webPageUrl);
                       // WriteWarning("RETRY(WE-PE-FO) " + webPageUrl);
                    }
                    else
                    {
                        if (retries == 2)
                            System.Threading.Thread.Sleep(4 * 60 * 60 * 1000);
                        else
                            System.Threading.Thread.Sleep(RetrySleepMultiplier * retries);
                       // ConsoleError(DateTime.Now + " RETRY (PE) " + webPageUrl);
                       // WriteWarning("RETRY(WE-PE) " + webPageUrl);
                    }
                    goto RETRY;
                }
                else if (ex.Status == WebExceptionStatus.ConnectFailure)
                {
                    if (retries > 2)
                        throw;

                    retries++;
                    if (retries == 2)
                        System.Threading.Thread.Sleep(4 * 60 * 60 * 1000);
                    else
                        System.Threading.Thread.Sleep(RetrySleepMultiplier * retries);
                  //  ConsoleError(DateTime.Now + " RETRY (CF) " + webPageUrl);
                    //WriteWarning("RETRY(WE-CF) " + webPageUrl);
                    goto RETRY;
                }
                else if (ex.Status == WebExceptionStatus.ReceiveFailure)
                {
                    if (retries > 2)
                        throw;

                    retries++;
                    if (retries == 2)
                        System.Threading.Thread.Sleep(4 * 60 * 60 * 1000);
                    else
                        System.Threading.Thread.Sleep(RetrySleepMultiplier * retries);
                   // ConsoleError(DateTime.Now + " RETRY (RF) " + webPageUrl);
                    //WriteWarning("RETRY(WE-RF) " + webPageUrl);
                    goto RETRY;
                }
                else
                {
                    if (retries > 1)
                        throw;

                    retries++;
                    System.Threading.Thread.Sleep(4 * 60 * 60 * 1000);
                 //   ConsoleError(DateTime.Now + " RETRY (OE) " + webPageUrl);
                  //  WriteWarning("RETRY(WE-OE) " + webPageUrl);
                    goto RETRY;
                }
            }
            catch (UriFormatException uriEx)
            {
               // UpdateConsoleTitle(CrawlerName + " - " + uriEx.Message);
                var appExc = new InvalidURLException("Url Is Not Valid: " + webPageUrl, uriEx);
               // ConsoleError(DateTime.Now + " Url Is Not Valid: " + webPageUrl);
                throw appExc;
            }

            if (resp != null)
            {
                LastResponseHeaders = resp.Headers;
                _lastResponseCookies = resp.Cookies;
                foreach (System.Net.Cookie c in _lastResponseCookies)
                {
                    //WriteLog("cookie: " + c);
                }

                LastResponseStatusCode = resp.StatusCode;
               // UpdateConsoleTitle(CrawlerName + " - " + LastResponseStatusCode);
                LastResponseUri = resp.ResponseUri;

                //CharacterSet defaults to "ISO-8859-1" - Elsevier can have a null CharacterSet
                if (!string.IsNullOrEmpty(resp.CharacterSet))
                    LastResponseEncoding = resp.CharacterSet.ToUpper();
            }
            if (resp.ResponseUri.AbsoluteUri != req.RequestUri.AbsoluteUri)
            {
                if (retries == 0 && RetryIfRedirected)
                {
                  //  WriteLog("RetryIfRedirected " + webPageUrl);
                    Console.WriteLine(DateTime.Now + " RetryIfRedirected");
                    System.Threading.Thread.Sleep(2000);
                    retries++;
                    goto RETRY;
                }
                else
                {
                 //   WriteLog("Redirected To " + resp.ResponseUri.AbsoluteUri);
                    Console.WriteLine(DateTime.Now + " Redirected");
                }
            }

          //  WriteLog("END Web Rq");

            //http://link.aip.org/link/ASMECP/v2005/i41855a/p103/s1&Agg=doi

            MemoryStream localStream = new MemoryStream();
            byte[] serverBytes = new byte[BUFF_SIZE];
            Stream serverFile = null;

            try
            {

                //if (resp.Headers["set-cookie"] != null)
                //{
                //    string rawHeader = resp.Headers["Set-Cookie"];

                //}
                //DateTime modifiedDate = resp.LastModified;
                serverFile = resp.GetResponseStream();

                int bytesRead = 1;

                do
                {
                    bytesRead = serverFile.Read(serverBytes, 0, serverBytes.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    localStream.Write(serverBytes, 0, bytesRead);

                } while (bytesRead > 0);

            }
            catch (IOException ex)
            {
                //UpdateConsoleTitle(CrawlerName + " - " + ex.Message);
                //WriteWarning(DateTime.Now + " " + ex);
               // WriteWarning(DateTime.Now + " " + webPageUrl);

                if (retries > 5)
                    throw;

                retries++;
                System.Threading.Thread.Sleep(RetrySleepMultiplier * retries);
               // WriteWarning("RETRY(IO) " + webPageUrl);
                goto RETRY;

            }
            catch (WebException ex)
            {
               // UpdateConsoleTitle(CrawlerName + " - " + ex.Message);
               // WriteWarning(DateTime.Now + " " + ex);
               // WriteWarning(DateTime.Now + " " + webPageUrl);
               // WriteWarning("WebException.Status: " + ex.Status);

                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    if (retries > 5)
                        throw;

                    retries++;
                    if (retries == 5)
                        System.Threading.Thread.Sleep(4 * 60 * 60 * 1000);
                    else
                        System.Threading.Thread.Sleep(RetrySleepMultiplier*retries);
                    //ConsoleError(DateTime.Now + " RETRY (TO) " + webPageUrl);
                    //WriteWarning("RETRY(WE-TO) " + webPageUrl);
                    if (useCookieJar && ClearCookieJarOnTimeout)
                        _cookieJar = new CookieContainer();
                    goto RETRY;
                }
                else
                {
                    if (retries > 1)
                        throw;

                    retries++;
                    System.Threading.Thread.Sleep(4 * 60 * 60 * 1000);
                    //ConsoleError(DateTime.Now + " RETRY (OE) " + webPageUrl);
                   // WriteWarning("RETRY(WE-OE) " + webPageUrl);
                    goto RETRY;
                }
            }
            finally
            {
                if (serverFile != null) serverFile.Close();
                if (localStream != null) localStream.Close();
                if (resp != null) resp.Close();
            }

            //File.SetLastWriteTime(localFile, modifiedDate);

            //This assumes LastResponseEncoding (HttpWebResponse.CharacterSet) is an acceptable character set
            _lastResponsePayload = Encoding.GetEncoding(LastResponseEncoding).GetString(localStream.ToArray());

            //Look inside content to override encoding
            //UTF-16 is untested
            //<meta charset="utf-8"/>
            //<meta content="text/html; charset=UTF-8" http-equiv="Content-Type">
            //<meta content='text/html; charset=UTF-8' http-equiv='Content-Type' />
            var matchCollection = Regex.Matches(_lastResponsePayload, @"[;\s]charset=([^/>\s]+)", RegexOptions.Multiline);
            if (matchCollection.Count > 0 && matchCollection[0].Groups.Count > 1)
            {
                var charset = matchCollection[0].Groups[1].Value.Replace("\"", "").Replace("'", "").ToUpper();
                if (String.CompareOrdinal(LastResponseEncoding, charset) != 0)
                {
                    if (String.CompareOrdinal(charset, "ISO-8859-1") == 0 || String.CompareOrdinal(charset, "US-ASCII") == 0
                        || String.CompareOrdinal(charset, "UTF-8") == 0 || String.CompareOrdinal(charset, "UTF-16") == 0)
                    {
                        LastResponseEncoding = charset;
                        _lastResponsePayload = Encoding.GetEncoding(LastResponseEncoding).GetString(localStream.ToArray());
                    }
                    else
                    {
                      //  WriteWarning("Ignoring Unknown Character Set: " + charset);
                    }
                }
            }
           // return _lastResponsePayload;

        //  Reparing all tags------------------------------
            PostSubmitter post1 = new PostSubmitter();
            post1.Url = "http://fixmyhtml.com/";
            post1.PostItems.Add("html", _lastResponsePayload);
            post1.Type = PostSubmitter.PostTypeEnum.Post;
            string result1 = post1.Post();
         // reapiring all tags
        //   string a= HttpContext.Current.Server.HtmlEncode(_lastResponsePayload);
           //return _lastResponsePayload;
            //_lastResponsePayload = _lastResponsePayload.Replace("&#150;", "-");
            return Mono.Web.HttpUtility.HtmlDecode(result1).ToString();
                
               
           // return Mono.Web.HttpUtility.HtmlDecode(_lastResponsePayload).ToString();
        }

      
        //public override string CrawlerName
        //{
        //    get { return "SpiderBase"; }
        //}

     

        /// <summary>
        /// method for validating a url with regular expressions
        /// </summary>
        /// <param name="url">url we're validating</param>
        /// <returns>true if valid, otherwise false</returns>
        public static bool isValidUrl(string url)
        {
            string pattern = @"^(http|https|ftp)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
            Regex reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return reg.IsMatch(url);
        }

        public string BuildUrl(string url, bool htmlDecode = true)
        {
            //TODO: Do we still need DOMAIN_URL?
            url = Regex.Replace(url, "#.*", "");
            if (htmlDecode) url = Mono.Web.HttpUtility.HtmlDecode(url);
            //if (url.Length == 0 || DOMAIN_URL.Length == 0) return url;
            //BuildUrl should only be used after the initial request is made so LastResponseUri should never be null
            if (url.Length == 0 || LastResponseUri == null) return url;
            //url can override the LastResponseUri hostname
            //Don't use ToString() - Uri class decodes query parameters
           // return new Uri(new Uri(LastResponseUri), url).AbsoluteUri;
           return new Uri(LastResponseUri, url).AbsoluteUri;
        }

        public void SetDomain(string url)
        {
           var uri = new Uri(url);
        
            DOMAIN_URL = url;//String.Format("{0}:{1}", uri.Scheme, uri.Host);
        }

      


    }

    public class InvalidURLException : ApplicationException
    {
        public InvalidURLException() { }

        public InvalidURLException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }



    //internal class WebPage
    //{
    //    public string Cookie = string.Empty;
    //    public string Html = string.Empty;
    //}

}
