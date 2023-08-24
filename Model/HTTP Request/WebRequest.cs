using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace student_buchkod;

public enum RefPolicy {
    no_referrer, no_referrer_when_downgrade, origin,
    origin_when_cross_origin, same_origin, strict_origin,
    strict_origin_when_cross_origin, unsafe_url
};

/// <summary>
/// Sends requests with appropriate cookies, process responses
/// </summary>
public static class WebRequest
{
    static readonly string[] mainPage = {"Web Browser",
    "Welcome to Web Browser. Insert your URL to the top bar"};
    static readonly string[] wrongProtocol = {"Wrong Protocol",
    "Your protocol must be either http or https"};
    static readonly string[] noRespond = {"No response received",
    "Your site currently is not responding. Check you Internet connection and try again"};
    static readonly string[] unknownError = {"Unknown error",
    "Unknown error occured"};
    static readonly string[] wrongURLformat = {"Wrong URL format",
    "Recheck your URL for misspellings and try again"};
    
    static readonly string[] htmlMimeType = {"text/html", "image/svg",
    "image/svg+xml", "application/xhtml", "application/xhtml+xml",
    "application/vnd.wap.xhtml+xml"}; 

    /// <summary>
    /// Sends request with appropriate cookies. Response assigned to Request argument
    /// </summary>
    /// <param name="r">Request and response assigned to this value</param>
    public static async Task sendRequest(Request r)
    {
        // check in case of internal request
        if (r.url.Scheme=="browser") {
            if (r.url.Host=="error" && r.url.AbsolutePath=="/Wrong-protocol") {
                generateResponse(r, wrongProtocol);
                return;
            }
            if (r.url.Host=="error" && r.url.AbsolutePath=="/Wrong-url-format") {
                generateResponse(r, wrongURLformat);
                return;
            }
            else if (r.url.Host=="home") {
                generateResponse(r, mainPage);
                return;
            }
            else if (r.url.Host=="local") {
                r.mimeType = new string[] {"text", "html"};
                r.response = Encoding.ASCII.GetBytes("<html><head><title>Local");
                return;
            }
            else {
                generateResponse(r, unknownError);
                return;
            }
        }
        // create Cookie header
        string cookieHeader = "";
        foreach(Cookie cookie in Cookies.getCookies(r)) {
            cookieHeader+=cookie.name+"="+cookie.value+"; ";
        }
        if (cookieHeader.Length>0) {
            cookieHeader.Substring(0,cookieHeader.Length-2);
        }
        try {
            // create HTTP request
            HttpClientHandler httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            HttpClient client = new HttpClient(httpClientHandler);
            HttpRequestMessage request = new HttpRequestMessage(r.httpMethod, r.url.ToString());
            request.Headers.Add("User-Agent", "Web Browser 1.0");
            request.Headers.Add("Accept", r.acceptHeader);
            if (cookieHeader!="") request.Headers.Add("Cookie", cookieHeader);
            if (r.referer.Scheme!="browser") {
                string refStr = getReferer(r.url, r.referer, r.refPolicy);
                if (refStr!="") request.Headers.Add("Referer", refStr);
            }
            if (r.data!=null) {
                request.Content = r.data;
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(r.mediaType);
            }

            // proccess response
            HttpResponseMessage response = await client.SendAsync(request);
            var headers = response.Headers.Concat(response.Content.Headers);
            string? location = null;
            foreach (var header in headers) {
                if (header.Key.ToLower()=="location") {
                    location = header.Value.First();
                }
                else if (header.Key.ToLower()=="content-type") {
                    r.mimeType = header.Value.First().ToLower().Split(";")[0].Split('/');
                }
                else if (header.Key.ToLower()=="set-cookie") {
                    Cookies.addCookie(r, header.Value.First());
                }
                else if (header.Key.ToLower()=="referrer-policy") {
                    r.refPolicyOutHeader = getRefPolicy(header.Value.First().ToLower());
                }
            }
            // save cookie to local file
            Cookies.SerializeCookie();
            // handle Location header if presented
            if (location!=null) {
                r.url = new Uri(r.url, location);
                await sendRequest(r);
            }
            else {
                r.response = await response.Content.ReadAsByteArrayAsync();
            }
        }
        catch(ProtocolViolationException) { generateResponse(r, wrongProtocol);}
        catch (HttpRequestException) { generateResponse(r, noRespond);}  
        catch (WebException) { generateResponse(r, noRespond);} 
        catch (Exception e) {
            Console.WriteLine(e.Message);
            generateResponse(r, unknownError);}
    }

    /// <summary>
    /// Sends request from passed argument with appropriate cookies,
    /// proccess response, and saves to Request generated html response
    /// </summary>
    /// <param name="r"></param>
    /// <returns></returns>
    public static async Task<string> htmlRequest(Request r)
    {
        await sendRequest(r);
        string type = r.mimeType[0];
        string subtype = r.mimeType[1];

        if (Array.Exists(htmlMimeType, x => x==type+"/"+subtype)) {
                return Encoding.Default.GetString(r.response);
        }
        else if (type=="image" || type=="audio" || type=="video") {
            string? localAdress = saveFile(r.response);
            if (localAdress==null) {
                generateResponse(r, unknownError);
                return Encoding.Default.GetString(r.response);
            }
            string title = r.url.AbsolutePath.Replace("<", "&lt;").Replace(">", "&gt;");
            string tag = "";
            switch(type) {
                case "image":
                    tag = "img";
                    break;
                case "audio":
                    tag = "audio";
                    break;
                case "video":
                    tag = "video";
                    break;
            }
            return "<html><head><title>"+title+"</title></head><body><br><" + tag
            + " src='browser:/" + localAdress+"'></body></html>";
        }
        else {
            string title = r.url.AbsolutePath.Replace("<", "&lt;").Replace(">", "&gt;");
            return "<html><head><title>" + title
            + "</title></head><body><code>" +
            Encoding.Default.GetString(r.response).Replace("<", "&lt;") +
            "</code></html>";
        }
    }
    /// <summary>
    /// Requests file and saves it to /tmp
    /// </summary>
    /// <returns>Absolute Path to saved file</returns>
    public static async Task<string?> requestFile (Request r) {
        await sendRequest(r);
        return saveFile(r.response);
    }
    static string? saveFile(byte[] content)
    {
        try {
        string path = "/tmp/"+new Random().Next();
        File.WriteAllBytes(path, content);
        return path;
        }
        catch {return null;}
    }
    static Request generateResponse(Request r, string[] page)
    {
        string htmlResponse = "<html><head><title>"+page[0]+
        "</title></head><body><h1>"+page[0]+"</h1><p>"+page[1]+
        "</p></body></html>";
        r.mimeType = new string[] {"text", "html"};
        r.response = Encoding.ASCII.GetBytes(htmlResponse);
        return r;
    }
    /// <summary>
    /// Converts string referrer policy to RefPolicy type
    /// </summary>
    public static RefPolicy getRefPolicy(string s) {
        s = s.ToLower().Trim();
        switch (s) {
            case "no-referrer":
                return RefPolicy.no_referrer;
            case "no-referrer-when-downgrade":
                return RefPolicy.no_referrer_when_downgrade;
            case "origin":
                return RefPolicy.origin;
            case "origin-when-cross-origin":
                return RefPolicy.origin_when_cross_origin;
            case "same-origin":
                return RefPolicy.same_origin;
            case "strict-origin":
                return RefPolicy.strict_origin;
            case "strict-origin-when-cross-origin":
                return RefPolicy.strict_origin_when_cross_origin;
            case "unsafe-url":
                return RefPolicy.unsafe_url;
            default:
                return RefPolicy.strict_origin_when_cross_origin;
        }
    }
    /// <summary>
    /// Generates Referer header value
    /// </summary>
    /// <param name="url">Requested url</param>
    /// <param name="referer">Actual referer URL</param>
    /// <param name="refPolicy">Referal Policy</param>
    /// <returns>Referer header value</returns>
    static string getReferer(Uri url, Uri referer, RefPolicy refPolicy)
    {
        if (refPolicy==RefPolicy.unsafe_url) return referer.ToString();
        else if (
            (
                refPolicy==RefPolicy.no_referrer_when_downgrade &&
                !(url.Scheme=="https" && referer.Scheme=="http")
            ) || (
                (refPolicy==RefPolicy.same_origin ||
                 refPolicy==RefPolicy.origin_when_cross_origin
                ) && url.Host==referer.Host
            ) || (
                refPolicy==RefPolicy.strict_origin_when_cross_origin &&
                !(url.Scheme=="https" && referer.Scheme=="http") &&
                url.Host==referer.Host
            )
        )
        {
            return referer.Scheme+"://"+referer.Host+referer.AbsolutePath;
        }
        else if (refPolicy==RefPolicy.origin ||
                refPolicy==RefPolicy.origin_when_cross_origin ||
                (   refPolicy==RefPolicy.strict_origin &&
                    !(url.Scheme=="https" && referer.Scheme=="http")
                ) ||
                (
                    refPolicy==RefPolicy.strict_origin_when_cross_origin &&
                    !(url.Scheme=="https" && referer.Scheme=="http")
                ) 
        ) {
            return referer.Scheme+"://"+referer.Host+"/";
        }
        else return "";
    }
}