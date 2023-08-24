using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;

namespace student_buchkod;

/// <summary>
/// Base class for clickable elements
/// </summary>
/// <remarks>
/// This base class should not be used for clickable elements which trigger
/// <c>form</c> (it has separate Form class)
/// </remarks>
public class Click {
    /// <summary>href attribute</summary>
    public Uri? href = null;
    /// <summary>URL address from which clickable event was initiated</summary>
    public Uri referrer;
    /// <summary>ping attribute, if applied</summary>
    public Uri? ping = null;
    /// <summary>download attribute, if applied</summary>
    public bool download = false;
    /// <summary>Referer Policy of redirect request, if applied</summary>
    public RefPolicy referrerpolicy = RefPolicy.strict_origin_when_cross_origin;
    /// <summary>frame that contains this clickable element</summary>
    public Frame? frame;

    /// <summary>
    /// Triggered by click on clickable item to hich this class was created
    /// </summary>
    public virtual void onClick(object? f, PointerReleasedEventArgs s) {}
}

/// <summary>
/// Clickable item which redirects to other page
/// </summary>
public class Link : Click
{
    public override void onClick(object? f, PointerReleasedEventArgs? s)
    {
        if (href==null || frame==null) return;
        if (ping!=null) {
            _ = WebRequest.sendRequest(
                new Request(ping, referrer, HttpMethod.Post, new StringContent("ping"))
            );
        }
        Request r = new Request(href, referrer, HttpMethod.Get, refPolicy: referrerpolicy);
        if (!download) {
            _ = frame.DisplayURL(r);
        }
        else {
            string filename = href.AbsolutePath.Substring(href.AbsolutePath.LastIndexOf('/')+1);
            if (filename=="") filename = "unknown_" + s.Timestamp.ToString();
            Thread thread = new Thread(async () =>
            {
                await WebRequest.sendRequest(r);
                if (r.response!=null) {
                    try {File.WriteAllBytes("~/Downloads/"+filename, r.response);}
                    catch {
                        try{File.WriteAllBytes("~/"+filename, r.response);}
                        catch{
                            try {File.WriteAllBytes(filename, r.response);}
                            catch{}
                        }
                    }
                }
            });
            thread.Start();
        }

    }
}

/// <summary>
/// Clickable element summary, closes or opens some content
/// </summary>
public class Summary : Click
{
    bool isopen = false;
    /// <summary>
    /// Arrow which indicates whether Summary is open or closed
    /// </summary>
    TextBlock arrow;
    /// <summary>
    /// Content which should appear/disappear by open/close of Summary
    /// </summary>
    List<Control> content;
    public Summary(bool isopen) {
        this.isopen = isopen;
    }

    public void setArrow(TextBlock arrow)
    {
        this.arrow = arrow;
    }

    public void setContent(List<Control> content)
    {
        this.content = content;
    }
    public override void onClick(object? f, PointerReleasedEventArgs s)
    {
        isopen = isopen ? false : true;
        if (isopen) {
            arrow.Text = "▼ ";
            foreach (var el in content) el.IsVisible = true;
        }
        else {
            arrow.Text = "▶ ";
            foreach (var el in content) el.IsVisible = false;
        }
    }
}