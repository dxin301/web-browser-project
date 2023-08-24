using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web;
using Avalonia.Controls;
using Avalonia;

namespace student_buchkod;

/// <summary>
/// Parses HTML document to HTML tree
/// </summary>
public static class HTMLParser {
    // HTML Singleton Tags With No Closing Tag
    static readonly string[] singletonTags = {"area", "base", "br", "col",
    "command", "embed", "hr", "img", "input", "keygen", "link", "meta", "param",
    "source", "track", "wbr"};
    /// <summary>
    /// Parses HTML document to HTML tree with <c>root</c> as a root tag
    /// </summary>
    /// <param name="content">HTML document content</param>
    /// <returns>root of HTML tree</returns>
    public static HTMLtag Parse (string content) {
        // delete all space between tags
        content = Regex.Replace(content, ">[\t\n ]*<","><").Trim();

        HTMLtag root = new HTMLtag("root");
        HTMLtag current = root;

        string tagName = "";
        bool tagNameFinished = false;
        Dictionary<string,string> attributes = new Dictionary<string, string>();
        bool attrNameFinished = false;
        string currentAttrName = "";
        string currentAttrVal = "";
        char? attrOpenedBy = null;
        string contentTag = "";

        for (int c = 0; c<content.Length; c++) {
            if (tagName=="" && content[c]=='<' && contentTag=="") {
                // some tag after closed tag
                c++;
                if (content[c] == '!' || content[c] == '/' ||
                    (content[c] >= 'a' && content[c] <= 'z') ||
                    (content[c] >= 'A' && content[c] <= 'Z')) {
                        tagName += content[c];
                }
                else contentTag+='<'+content[c];
            }
            else if (tagName=="" && content[c]=='<') {
                // we have content and some new tag starts
                tagFinished();
                c--;
            }
            else if (tagName=="") {
                // just continue of content
                contentTag+=content[c];
            }
            else if (tagName=="!--") {
                // special case for comments
                tagName = "";
                tagName+=content[c];
                while(tagName.Length<3 || tagName.Substring(tagName.Length-3)!="-->") {
                    c++;
                    tagName+=content[c];
                }
                tagName = "";
            }
            // from this point we consider only tags
            else if (content[c]=='>' && attrOpenedBy==null) {
                if (currentAttrName!="") attrFinished();
                tagFinished();
            }
            else if (!tagNameFinished && content[c]!=' ' && content[c]!='/') {
                // Mozilla and Google accept also / as attribute divider
                tagName += content[c];
            }
            else if (!tagNameFinished) {
                tagNameFinished = true;
            }
            // from this point we consider tags with closed tagName
            else if (attrOpenedBy==null && (content[c]==' ' || content[c]=='/')) {
                if (currentAttrName!="") attrFinished();
            }
            else if (attrOpenedBy==null && !attrNameFinished && content[c]=='=') {
                attrNameFinished = true;
            }
            else if (attrOpenedBy==null && !attrNameFinished) {
                currentAttrName += content[c];
            }
            else if (attrOpenedBy==null && (content[c]=='"' || content[c]=='\'')) {
                attrOpenedBy = content[c];
            } 
            else if (attrOpenedBy==null) {
                currentAttrVal += content[c];
            }
            else if (attrOpenedBy==content[c]) {
                attrOpenedBy = null;
            }
            else {
                currentAttrVal += content[c];
            }
        }
        if (currentAttrName!="") attrFinished();
        if (tagName!="" || contentTag!="") tagFinished();

        // for the case when "<html><head><p>" is the all input
        if (current.parent!=null && current.children.Count()==0
        && Array.Exists(singletonTags, x => x==current.name)) {
            current.children.Add(new HTMLtag(null, current));
        }
        return root;




        void attrFinished() {
            // first attribute has advantage
            if (!attributes.ContainsKey(currentAttrName)) {
                attributes[currentAttrName.ToLower()] = currentAttrVal;
            }
            attrNameFinished = false;
            currentAttrName = "";
            currentAttrVal = "";
            attrOpenedBy = null;
        }
        void tagFinished() {
            // close Tag after unclosed descendant is considered as error
            if (contentTag!="") {
                HTMLtag newTag = new HTMLtag(null, current, null, contentTag);
                current.children.Add(newTag);
            }
            else if (tagName=="/"+current.name && current.parent!=null) {
                if (current.children.Count()==0) {
                    HTMLtag newTag = new HTMLtag(null, current);
                    current.children.Add(newTag);
                }
                current = current.parent;
            }
            else if (tagName[0]!='!' && tagName[0]!='/') {
                HTMLtag newTag = new HTMLtag(tagName.ToLower(), current, attributes);
                current.children.Add(newTag);
                if (!Array.Exists(singletonTags, x => x==newTag.name)) {
                    current = newTag;
                }
            }

            tagName = "";
            tagNameFinished = false;
            attributes = new Dictionary<string, string>();
            contentTag = "";
        }
    }
}

/// <summary>
/// Node in HTML tree
/// </summary>
public class HTMLtag {
    public HTMLtag? parent;
    /// <summary>
    /// null only when tag is a text (not a text tag)
    /// </summary>
    public string? name;
    /// <summary>
    /// contains text, is not null only if name is null
    /// </summary>
    public string? content;
    public Dictionary<string,string> attributes;
    public List<HTMLtag> children = new List<HTMLtag>();

    // Tag styling
    public bool startNewLine = false;
    public bool endNewLine = false;
    public string color = "#333333";
    public bool bold = false;
    public bool crossed = false;
    public bool underline = false;
    public bool italic = false;
    public bool monospace = false;
    public double fontsize = 16;
    /// <remarks>left, top, right, bottom</remarks>
    public int[] margin = {0, 0, 0, 0};
    public string background = "#ffffff";
    /// <summary>subscript</summary>
    public bool sub = false;
    /// <summary>superscript</summary>
    public bool sup = false;
    public string? title = null;
    public bool pointer = false;
    public Click? click = null;
    public Form? form = null;
    public bool pre = false;

    public HTMLtag(string? name, HTMLtag? parent = null,
        Dictionary<string,string>? attributes = null, string content = "") {
        this.name = name;
        if(name==null) this.content = HttpUtility.HtmlDecode(content);
        this.parent = parent;
        if (attributes==null) this.attributes = new Dictionary<string, string>();
        else this.attributes = attributes;
    }
    /// <summary>
    /// Inherit style values to children
    /// </summary>
    /// <param name="inheritNewLine">should new lines properties be inherited</param>
    public void inheritStyleToChildren(bool inheritNewLine = true)
    {
        for (int i = 0; i<children.Count; i++) {
            children[i].color = color;
            children[i].bold = bold;
            children[i].crossed = crossed;
            children[i].underline = underline;
            children[i].italic = italic;
            children[i].monospace = monospace;
            children[i].fontsize = fontsize;
            children[i].background = background;
            children[i].sub = sub;
            children[i].sup = sup;
            children[i].title = title;
            children[i].pointer = pointer;
            children[i].click = click;
            children[i].form = form;
            children[i].pre = pre;
            // handle newLines
            if (inheritNewLine) {
                if (i==0) children[i].startNewLine = startNewLine;
                if (i==children.Count-1) children[i].endNewLine = endNewLine;
            }
            // handle margin
            if (i==0 && margin[1]!=0) {
                children[i].margin[1] = margin[1];
            }
            if (i==children.Count-1 && margin[3]!=0) {
                children[i].margin[3] = margin[3];
            }
            children[i].margin[0] = margin[0];
            children[i].margin[2] = margin[2];
        }
    }
}