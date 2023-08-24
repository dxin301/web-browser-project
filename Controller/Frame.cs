using System;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace student_buchkod;

/// <summary>Represents HTML frame</summary>
public class Frame {
    /// <summary>InlinePanel to which content appended</summary>
    public InlinePanel inlinePanel;
    TextBox? textBox;
    Action<string>? title;
    bool addToHistory;

    public Frame(InlinePanel inlinePanel, TextBox? textBox = null,
    Action<string>? title = null, bool addToHistory = false)
    {
        this.inlinePanel = inlinePanel;
        this.textBox = textBox;
        this.title = title;
        this.addToHistory = addToHistory;
    }
    /// <summary>
    /// Displays content from passed <c>Request</c>
    /// </summary>
    public async Task DisplayURL(Request r) {
        try{
            inlinePanel.Clear();
            if (textBox!=null) textBox.Text = r.url.ToString();
            inlinePanel.Children.Add(new TextBlock
            {
                Text = "Loading...",
                FontSize = 30,
                FontWeight = Avalonia.Media.FontWeight.DemiBold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 20, 0, 0)
            });
            string response = await WebRequest.htmlRequest(r);
            if (textBox!=null) textBox.Text = r.url.ToString();
            HTMLtag root = HTMLParser.Parse(response);
            inlinePanel.Clear();
            AXAMLtags AXAMLtags = new AXAMLtags(r, this);
            AXAMLtags.addToInlinePanel(AXAMLtags.display(root), inlinePanel);
            if (title!=null) title(AXAMLtags.title);
            if (addToHistory && History.current.url.ToString()!=r.url.ToString()) {
                History.Push(r);
            }
        }
        catch(Exception e) {
            Console.WriteLine(e.Message);
            r.url = new Uri("browser://error/Unknown-error");
            _ = DisplayURL(r);
        }
    }
    /// <summary>
    /// Display html data in frame
    /// </summary>
    public void showHtml(string htmlCode) {
        Uri localUri = new Uri("browser://local");
        Request r = new Request(localUri, localUri, HttpMethod.Get);
        HTMLtag root = HTMLParser.Parse(htmlCode);
        inlinePanel.Clear();
        AXAMLtags AXAMLtags = new AXAMLtags(r, this);
        AXAMLtags.addToInlinePanel(AXAMLtags.display(root), inlinePanel);
    }
}