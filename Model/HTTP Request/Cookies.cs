using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace student_buchkod;

public enum SameSite {None, Lax, Strict};

/// <summary>
/// Manage cookies
/// </summary>
public static class Cookies
{
    /* When program starts we load all cookies from the file .web-browser-cookies
    to cookies variable, all cookies are saved in this variable. After
    each request Serialization must be done to not lose saved cookies */
    /// <summary>
    /// Keys are domain values, values are list of Cookie with such domain value
    /// </summary>
    /// <remarks>.example.com and example.com are considered as different keys</remarks>
    static Dictionary<string, List<Cookie>> cookies = DeSerializeCookie();
    
    /// <summary>
    /// Returns cookies that allowed to sent with given Request
    /// </summary>
    public static IEnumerable<Cookie> getCookies(Request r) {
        string domain = r.url.Host;
        while (!COOKIE_TLDS.tlds.Contains("."+domain)) {
            foreach(string el in new string[]{domain, "."+domain}){
                if (cookies.ContainsKey(el)) {
                    foreach(Cookie cookie in cookies[el]) {
                        if (cookie.access(r)) yield return cookie;
                    }
                }
            }
            int firstDotIndex = domain.IndexOf('.');
            if (firstDotIndex==-1) break;
            domain = domain.Substring(firstDotIndex+1);
        }
        yield break;
    }
    public static void addCookie(Request r, string cookie){
        string name = "";
        string value = "";
        string domain = r.url.Host;
        string path = "/";
        double expire = 0;
        bool httpOnly = false;
        bool secure = false;
        SameSite sameSite = SameSite.Lax;

        string[] splittedCookie = cookie.Split(';');
        string[] splittedNameValue = cookie.Split('=');
        name = splittedNameValue[0].Trim();
        if (splittedNameValue.Count()>1) value = splittedNameValue[1].Trim();

        for (int i = 1; i<splittedCookie.Count(); i++) {
            string[] splittedParam = splittedCookie[i].Split('=');
            switch (splittedParam[0].Trim().ToLower()) {
                case "domain":
                    if (splittedParam.Count()>1) {
                        string domainToSet = splittedParam[1].Trim();
                        if (domainToSet[0]!='.') domainToSet = "."+domainToSet;
                        if (("."+r.url.Host).LastIndexOf(domainToSet)+domainToSet.Length
                            ==r.url.Host.Length+1
                            && !COOKIE_TLDS.tlds.Contains(splittedParam[1].Trim())) {
                            domain = splittedParam[1].Trim();
                        }
                    }
                    break;
                case "path":
                    if (splittedParam.Count()>1) {
                        splittedParam[1] = splittedParam[1].Trim();
                        string closedPath = r.url.AbsolutePath;
                        if (closedPath[closedPath.Length-1]!='/') closedPath+="/";
                        if (r.url.AbsolutePath!=splittedParam[1] && closedPath!=splittedParam[1]
                        && (path+"/").IndexOf(closedPath)!=0) break;
                        path = splittedParam[1];
                    }
                    break;
                case "expires":
                    if (splittedParam.Count()>1) {
                        string exp = splittedParam[1].Trim();
                        if (double.TryParse(exp, out double expire1)) {
                            // because out expire can make it null when tryparse is false
                            expire = expire1;
                        }
                    }
                    break;
                case "httponly":
                    httpOnly = true;
                    break;
                case "secure":
                    secure = true;
                    break;
                case "samesite":
                    if (splittedParam.Count()>1) {
                        string val = splittedParam[1];
                        val = val.Trim().ToLower();
                        if (val=="strict") sameSite = SameSite.Strict;
                        else if (val=="none") sameSite = SameSite.None;
                    }
                    break;
            }
        }
        if (sameSite==SameSite.None && !secure) return;
        if (!cookies.ContainsKey(domain)) cookies[domain] = new List<Cookie>();
        else {
            for (int i = 0; i<cookies[domain].Count(); i++) {
                if (cookies[domain][i].name==name) {
                    if (!cookies[domain][i].access(r)) return;
                    cookies[domain].RemoveAt(i);
                    break;
                }
            }
        }
        Cookie newCookie = new Cookie(name, value, domain, path, expire, httpOnly,
        secure, sameSite);
        cookies[domain].Add(newCookie);
    }
    /// <summary>
    /// Serealize cookie and save its value in .web-browser-cookies
    /// </summary>
    /// <remarks>Must be done after every request</remarks>
    public static void SerializeCookie() {
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(@".web-browser-cookies", FileMode.Create, FileAccess.Write);
        formatter.Serialize(stream, cookies);
        stream.Close();
    }
    /// <summary>
    /// Deserealize cookies from .web-browser-cookies
    /// </summary>
    /// <returns>deserialized variable</returns>
    static Dictionary<string, List<Cookie>> DeSerializeCookie() {
        try {
            if (File.Exists(@".web-browser-cookies")) {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(@".web-browser-cookies",FileMode.Open,FileAccess.Read);
                var cookies = (Dictionary<string, List<Cookie>>)formatter.Deserialize(stream);
                // delete expired and session cookies
                double currentTimestamp = DateTime.Now.Subtract(
                new DateTime(1970, 1, 1)).TotalMilliseconds;
                foreach(var site in cookies.Keys) {
                    for (int i = 0; i<cookies[site].Count(); i++) {
                        if (cookies[site][i].expire<currentTimestamp) {
                            cookies[site].RemoveAt(i);
                            i--;
                        }
                    }
                }
                return cookies;
            }
        }
        catch(Exception ex) {Console.WriteLine(ex.Message);}
        return new Dictionary<string, List<Cookie>>();
    }
}

/// <summary>
/// Represent cookie and its parameters
/// </summary>
[Serializable] public class Cookie {
    public string name;
    public string value;
    public string domain;
    public string path;
    /// <summary>
    /// Unix timestamp when it must expire
    /// </summary>
    /// <remarks>Equals 0 in case of session cookies</remarks>
    public double expire;
    public bool httpOnly;
    public bool secure;
    public SameSite sameSite;

    public Cookie(string name, string value, string domain, string path, double expire,
    bool httpOnly, bool secure, SameSite sameSite){
        this.name = name;
        this.value = value;
        this.domain = domain;
        this.path = path;
        this.expire = expire;
        this.httpOnly = httpOnly;
        this.secure = secure;
        this.sameSite = sameSite;
    }
    /// <summary>
    /// Checks permission of passed method to access to current Cookie
    /// </summary>
    public bool access(Request r) {
        // check path
        if (path[path.Length-1]!='/') path = path+"/";
        string closedPath = r.url.AbsolutePath;
        if (closedPath[closedPath.Length-1]!='/') closedPath+="/";
        if (closedPath.IndexOf(path)!=0) return false;
        // expire
        double unixTimestamp = DateTime.Now.Subtract(
            new DateTime(1970, 1, 1)).TotalMilliseconds;
        if (expire!=0 && expire<unixTimestamp) return false;
        // secure
        if (secure && r.url.Scheme!="https") return false;
        // samesite
        if(
        (sameSite==SameSite.Strict &&
            (r.referer.Host!=r.url.Host || r.referer.Scheme!=r.url.Scheme))
        ||
        (sameSite==SameSite.Lax && r.referer.Host!=r.url.Host
        && r.httpMethod!=System.Net.Http.HttpMethod.Get)
        ) return false;
        return true;
    }
}