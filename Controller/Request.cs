using System;
using System.Net.Http;

namespace student_buchkod;

public class Request {
    /// <summary>Request URL</summary>
    public Uri url;
    /// <summary>URL from which request was made</summary>
    public Uri referer;
    /// <summary>Request http method</summary>
    public HttpMethod httpMethod;
    /// <summary>Data for request body
    /// (for POST, PUT and other requests with body)</summary>
    public HttpContent? data;
    /// <summary>Media Type of request body if data setted</summary>
    public string mediaType;
    /// <summary>Value for <c>Accept</c> header</summary>
    public string acceptHeader;
    /// <summary>Current request referer policy</summary>
    public RefPolicy refPolicy;

    // response:

    /// <summary>Response mime-type obtained from
    /// <c>Content-Type</c> response header</summary>
    public string[] mimeType = new string[2];
    /// <summary>Response `Referrer-Policy` header</summary>
    public RefPolicy? refPolicyOutHeader = null;
    /// <summary>Response body</summary>
    public byte[]? response = null;
    public Request(Uri url, Uri referer, HttpMethod httpMethod, HttpContent? data = null,
    string mediaType = "application/x-www-form-urlencoded",
    string acceptHeader = "text/html, text/plain;q=0.9, */*;q=0.5",
    RefPolicy refPolicy = RefPolicy.strict_origin_when_cross_origin) {
        this.url = url;
        this.referer = referer;
        this.httpMethod = httpMethod;
        this.data = data;
        this.mediaType = mediaType;
        this.acceptHeader = acceptHeader;
        this.refPolicy = refPolicy;
    }
}