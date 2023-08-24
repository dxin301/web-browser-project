using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace student_buchkod;

/// <summary>
/// Recursively parse HTML tree into AXAML tags
/// </summary>
public class AXAMLtags
{
    /// <summary>
    /// Title from parsed html document
    /// </summary>
    public string title = "Untitled document";
    /// <summary>
    /// Base URL for all links here
    /// </summary>
    Uri baseUri;
    /// <summary>
    /// Frame in which these AXAML tags will be displayed
    /// </summary>
    Frame frame;
    /// <summary>
    /// Initial request from which these html content was obtained
    /// </summary>
    Request request;
    /// <summary>
    /// Recursively parse HTML tree into AXAML tags
    /// </summary>
    /// <param name="r">Initial request from which HTML tree was obtained</param>
    /// <param name="frame">Frame in which returned <c>AXAMLtag</c>s will be displayed</param>
    public AXAMLtags (Request r, Frame frame) {
        baseUri = r.url;
        request = r;
        this.frame = frame;
    }
    /// <summary>
    /// Tags that considered as endpoints and involve creating special AXAML tag for them
    /// </summary>
    static readonly string?[] endpoints = new string?[]{null, "img", "form",
    "input", "head", "br", "button", "table", "hr", "iframe",
    "wbr", "select", "datalist", "details", "meter", "progress",
    "object", "textarea", "ul", "ol", "embed"};
    /// <summary>
    /// Recursively parse HTML tree into AXAML tags
    /// </summary>
    /// <param name="tag">root tag of HTML tree</param>
    public IEnumerable<AXAMLtag> display (HTMLtag tag)
    {
        if (tag.attributes.ContainsKey("hidden")) yield break;
        if (Array.Exists(endpoints, x => x==tag.name)) {
            foreach (AXAMLtag el in createEndpoint(tag)) {
                yield return el;
            }
            yield break;
        }
        switch(tag.name) {
            case "article": case "header": case "section":
            case "footer": case "aside": case "p": case "caption":
            case "canvas": case "figure": case "figcaption":
            case "div": case "noscript": case "li":
            case "dl": case "dt": case "td": case "th":
                tag.startNewLine = true;
                tag.endNewLine = true;
                break;
            case "base": case "title": case "meta": case "link":
            case "col": case "colgroup": case "dialog": case "svg":
            case "style": case "script": case "template": case "wbr":
            case "map": case "summary":
                // fully ignore these tags and their children
                // we ignore <wbr> because it already broked word during parsing
                // and if possible these two pices will display together,
                // otherwise second piece transfer to the next line
                yield break;
            case "nav": case "time": case "span": case "bdo": case "picture":
            case "data": case "output": case "ruby": case "rp": case "rt":
            case "bdi": case "video": case "audio": case "label": case "fieldset":
                // ignore these tags (no changes to children)
                // parse their children
                break;                
            case "a":
                tag.color = "#0040a0";
                tag.underline =true;
                tag.pointer = true;
                tag.click = new Link();
                if(tag.attributes.TryGetValue("href", out string href)) {
                    tag.click.href = new Uri(baseUri, href);
                }
                if(tag.attributes.TryGetValue("ping", out string ping)) {
                    tag.click.ping = new Uri(baseUri, ping);
                }
                if(tag.attributes.ContainsKey("download")) {
                    tag.click.download = true;
                }
                if (request.refPolicyOutHeader!=null) {
                    tag.click.referrerpolicy = (RefPolicy) request.refPolicyOutHeader;
                }
                if(tag.attributes.TryGetValue("reffererpolicy",
                out string reffererpolicy)) {
                    tag.click.referrerpolicy = WebRequest.getRefPolicy(reffererpolicy);
                }
                tag.click.referrer = request.referer;
                tag.click.frame = frame;
                break;
            case "abbr":
                tag.underline = true;
                break;
            case "address":
                tag.italic = true;
                tag.startNewLine = true;
                tag.endNewLine = true;
                break;
            case "b": case "strong":
                tag.bold = true;
                break;
            case "blockquote":
                tag.margin[0] += 16;
                tag.margin[1] += 40;
                tag.margin[2] += 16;
                tag.margin[3] += 40;
                tag.startNewLine = true;
                tag.endNewLine = true;
                break;
            case "del": case "s":
                tag.crossed = true;
                break;
            case "cite": case "i": case "dfn": case "em": case "var":
                tag.italic = true;
                break;
            case "h1":
                tag.fontsize = 32;
                tag.bold = true;
                tag.startNewLine = true;
                tag.endNewLine = true;
                break;
            case "h2":
                tag.fontsize = 24;
                tag.bold = true;
                tag.startNewLine = true;
                tag.endNewLine = true;
                break;
            case "h3":
                tag.fontsize = 18.72;
                tag.bold = true;
                tag.startNewLine = true;
                tag.endNewLine = true;
                break;
            case "h4":
                tag.fontsize = 16;
                tag.bold = true;
                tag.startNewLine = true;
                tag.endNewLine = true;
                break;
            case "h5":
                tag.fontsize = 13.28;
                tag.bold = true;
                tag.startNewLine = true;
                tag.endNewLine = true;
                break;
            case "h6":
                tag.fontsize = 10.72;
                tag.bold = true;
                tag.startNewLine = true;
                tag.endNewLine = true;
                break;
            case "ins": case "u":
                tag.underline = true;
                break;
            case "kbd": case "samp":
                tag.monospace = true;
                tag.startNewLine = true;
                tag.endNewLine = true;
                break;
            case "mark":
                tag.color = "#333333";
                tag.background = "#ede732";
                tag.startNewLine = true;
                tag.endNewLine = true;
                break;
            case "q":
                tag.children.Insert(0, new HTMLtag("", tag, null, "\""));
                tag.children.Add(new HTMLtag("", tag, null, "\""));
                break;
            case "small":
                tag.fontsize = 10;
                break;
            case "sub":
                tag.sub = true;
                break;
            case "sup":
                tag.sup = true;
                break;
            case "dd":
                tag.margin[0] += 40;
                break;
            case "pre":
                tag.italic = true;
                tag.fontsize = 12;
                tag.startNewLine = true;
                tag.endNewLine = true;
                tag.pre = true;
                tag.monospace = true;
                break;
        }
        if (tag.attributes.ContainsKey("title")) tag.title = tag.attributes["title"];
        // recursively parse children
        foreach (var child in tag.children) {
            tag.inheritStyleToChildren();
            foreach (AXAMLtag el in display(child)) {
                yield return el;
            }
        }
    }
    /// <summary>
    /// Creates AXAML tags (Avalonia Control) from HTMLtag
    /// </summary>
    /// <remarks>The HTMLtag.name must be in endpoints array</remarks>
    IEnumerable<AXAMLtag> createEndpoint(HTMLtag tag)
    {
        switch (tag.name)
        {
            case null:
                foreach(var el in parseText(tag)) yield return el;
                yield break;
            case "img":
                foreach(var el in createImage(tag)) yield return el;
                yield break;
            case "form":
                foreach(var el in createForm(tag)) yield return el;
                yield break;
            case "input":
                tag.attributes.TryGetValue("type", out string? type);
                Control input;
                switch (type) {
                    case "button":
                        // any button inside form is considered as a submission button
                        if (!tag.attributes.ContainsKey("formaction")) tag.form = null;
                        yield return new AXAMLtag
                        (tag.startNewLine, tag.endNewLine, createButton(tag, true, ""));
                        yield break;
                    case "checkbox":
                        foreach(var el in createInputCheckBox(tag)) yield return el;
                        yield break;
                    case "hidden":
                        if (tag.attributes.TryGetValue("name", out string? hiddenName)
                        && tag.form!=null) {
                            tag.attributes.TryGetValue("value", out string? hiddenValue);
                            if (hiddenValue==null) hiddenValue = "";
                            if (tag.form!=null) tag.form.addInput(hiddenName, value: hiddenValue);
                        }
                        yield break;
                    case "image":
                        foreach (var el in createImage(tag)) {
                            if (tag.form!=null) {
                                el.Control.PointerReleased += tag.form.submit;
                                tag.attributes.TryGetValue("name", out string? imageName);
                                tag.attributes.TryGetValue("value", out string? imageValue);
                                if (imageValue==null) imageValue = "";
                                tag.form.addInput(imageName, value: imageValue);
                            }
                            yield return el;
                        }
                        yield break;
                    case "number":
                        foreach(var el in createInputNumber(tag)) yield return el;
                        yield break;
                    case "reset":
                        tag.form = null;
                        Button resetButton = createButton(tag, true, "Reset");
                        resetButton.PointerReleased += tag.form.resetForm;
                        yield return new AXAMLtag
                            (tag.startNewLine, tag.endNewLine, resetButton);
                        break;
                    case "submit":
                        yield return new AXAMLtag
                            (tag.startNewLine, tag.endNewLine, createButton(tag, true, "Submit"));
                        yield break;
                    default:
                        TextBox textBox = createTextBox(tag);
                        yield return new AXAMLtag(
                            tag.startNewLine, tag.endNewLine, textBox);
                        yield break;
                }
                yield break;
            case "textarea":
                foreach (var el in createMultilineText(tag)) yield return el;
                yield break;
            case "head":
                proccessHead(tag);
                yield break;
            case "br":
                yield return new AXAMLtag(false,
                false, new Separator{
                    Width = frame.inlinePanel.PanelViewWidth,
                    Height = 5
                });
                yield break;
            case "table":
                foreach (var el in createTable(tag)) yield return el;
                yield break;
            case "hr":
                Rectangle rect = new Rectangle
                {
                    Height = 2,
                    Width = 1190,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    Fill = new SolidColorBrush(Colors.Gray),
                    Margin = new Thickness(5, 8)
                };
                if (tag.click!=null) rect.PointerReleased += tag.click.onClick;
                yield return new AXAMLtag(true,
                true, rect);
                yield break;
            case "meter": case "progress":
                foreach (var el in createMeter(tag)) yield return el;
                yield break;
            case "button":
                yield return new AXAMLtag(
                    tag.startNewLine, tag.endNewLine, createButton(tag));
                yield break;
            case "select": case "datalist":
                foreach (var el in createSelect(tag)) yield return el;
                yield break;
            case "iframe":
                foreach (var el in createIframe(tag)) yield return el;
                yield break;
            case "details":
                foreach(var el in createDetails(tag)) yield return el;
                yield break;
            case "embed":
                if (tag.attributes.ContainsKey("type") &&
                tag.attributes["type"].Split('/')[0]=="image") {
                    foreach(var el in createImage(tag)) yield return el;
                }
                // video, audio, svg are not supported
                else foreach(var el in createIframe(tag)) yield return el;
                yield break;
            case "object":
                // will be displayed as iframe
                if (tag.attributes.ContainsKey("data")) {
                    tag.attributes["src"] = tag.attributes["data"];
                    foreach (var el in createIframe(tag)) yield return el;
                }
                yield break;
            case "ul":
                foreach(var el in createUnorderedList(tag)) yield return el;
                yield break;
            case "ol":
                foreach(var el in createNumeratedList(tag)) yield return el;
                yield break;
        }
    }




    /// <summary>
    /// Creates TextBlock based on given arguments
    /// </summary>
    /// <param name="sub">subscript</param>
    /// <param name="sup">superscript</param>
    /// <returns></returns>
    TextBlock createTextBlock(string? text,
    string color, bool bold, bool crossed, bool underline, bool italic,
    bool monospace, double fontsize, int[] margin, string background,
    bool sub, bool sup, string? title, bool pointer, Click? click)
    {
        
        var decorationCollection = new TextDecorationCollection();
        if (crossed) {
            decorationCollection.Add(new TextDecoration
            {
                Location = TextDecorationLocation.Strikethrough,
                Stroke = new SolidColorBrush(Color.Parse(color)),
                StrokeThicknessUnit = TextDecorationUnit.FontRecommended
            });
        }
        if (underline) {
            decorationCollection.Add(new TextDecoration
            {
                Location = TextDecorationLocation.Underline,
                Stroke = new SolidColorBrush(Color.Parse(color)),
                StrokeThicknessUnit = TextDecorationUnit.FontRecommended
            });
        }
        VerticalAlignment verticalAlignment = VerticalAlignment.Center;
        if (sub && !sup) {
            fontsize = fontsize * 0.45;
            verticalAlignment = VerticalAlignment.Bottom;
        }
        else if (!sub && sup) {
            fontsize = fontsize * 0.45;
            verticalAlignment = VerticalAlignment.Top;
        }
        else if (sub && sup) {
            fontsize = fontsize * 0.45 * 0.45;
        }
        TextBlock textBlock = new TextBlock
        {
            Text = text,
            Foreground = new SolidColorBrush(Color.Parse(color)),
            Background = new SolidColorBrush(Color.Parse(background)),
            FontWeight = bold ? FontWeight.Bold : FontWeight.Normal,
            FontStyle = italic ? FontStyle.Italic : FontStyle.Normal,
            TextDecorations = decorationCollection,
            FontFamily = monospace ? FontFamily.Parse("monospace") : FontFamily.Default,
            FontSize = fontsize,
            VerticalAlignment = verticalAlignment,
            Cursor = pointer ? new Cursor(StandardCursorType.Hand) : Cursor.Default,
            Margin = new Thickness(margin[0], margin[1], margin[2], margin[3])
        };
        if (title!=null) {
            var tooltip = new ToolTip
            {
                Content = new TextBlock
                {
                    Text = title
                }
            };
            ToolTip.SetTip(textBlock, tooltip);
        }
        if (click!=null) textBlock.PointerReleased += click.onClick;
        return textBlock;
    }
    /// <summary>
    /// Proccess <c>textarea</c> html tag
    /// </summary>
    IEnumerable<AXAMLtag> createMultilineText(HTMLtag tag) {
        TextBox textarea = createTextBox(tag);
        textarea.AcceptsReturn = true;
        textarea.TextWrapping = TextWrapping.Wrap;
        textarea.FontFamily = FontFamily.Parse("monospace");
        if (tag.attributes.TryGetValue("cols", out string? colsStr)) {
            if (uint.TryParse(colsStr, out uint cols)) {
                textarea.Width = 8.44*cols+19;
            }
        }
        if (tag.attributes.TryGetValue("rows", out string? rowsStr)) {
            if (uint.TryParse(rowsStr, out uint rows)) {
                textarea.Height = 18.67*rows+23.3;
            }
        }
        yield return new AXAMLtag(tag.startNewLine,
        tag.endNewLine, textarea);
        yield break;
    }
    /// <summary>
    /// Proccess <c>ol</c> html tag
    /// </summary>
    IEnumerable<AXAMLtag> createUnorderedList(HTMLtag tag) {
        tag.margin[0] += 10;
        tag.inheritStyleToChildren();
        foreach (var child in tag.children) {
            if (child.name=="li") {
                string toAppend = "• ";
                child.children.Insert(0, new HTMLtag(null, child, null, toAppend));
            }
            foreach (var el in display(child)) yield return el;
        }
        yield break;
    }
    /// <summary>
    /// Proccess <c>head</c> tag
    /// </summary>
    void proccessHead(HTMLtag tag) {
        foreach (HTMLtag child in tag.children) {
            if (child.name=="title") {
                // in case of tag in <title> childrens - sum together all text
                string tmpTitle = "";
                foreach (HTMLtag titleChild in child.children) {
                    if (titleChild.name==null) tmpTitle += titleChild.content;
                }
                if (tmpTitle!="") this.title = tmpTitle;
            }
            else if (child.name=="base" && child.attributes.ContainsKey("href")) {
                baseUri = new Uri(baseUri, child.attributes["href"]);
            }
        }
    }
    /// <summary>
    /// Proccess <c>input</c> with type <c>checkbox</c>
    /// </summary>
    IEnumerable<AXAMLtag> createInputCheckBox(HTMLtag tag){
        bool defValue = false;
        if (tag.attributes.ContainsKey("checked")) defValue = true;
        CheckBox checkBox = new CheckBox{
            IsChecked = defValue
        };
        if (tag.form!=null) {
            tag.attributes.TryGetValue("name", out string? nameCheckBox);
            tag.attributes.TryGetValue("value", out string? valueCheckBox);
            if (valueCheckBox==null) valueCheckBox = "";
            tag.form.addInput(nameCheckBox, checkBox, defValue,
            required: tag.attributes.ContainsKey("required"),
            value: valueCheckBox);
        }
        yield return new AXAMLtag(tag.startNewLine,
        tag.endNewLine, checkBox);
        yield break;
    }
    /// <summary>
    /// Parse text html tags (HTMLtag with null name and non-null content)
    /// </summary>
    IEnumerable<AXAMLtag> parseText(HTMLtag tag) {
        string text = tag.content;
        string[] splittedBySpace;
        if (!tag.pre) {
            text = Regex.Replace(text, @"\t|\n|\r", " ");
            text = Regex.Replace(text, @"\s+", " ");
            splittedBySpace = text.Split(' ');
        }
        else splittedBySpace = new string[]{text};
        // create inline panel and put all words there
        // we divide content to words to allow InlinePanel
        // move them to the next line
        // we put words in InlinePanel instead of directly return them,
        // because this allows us to correctly assign pargin to text part
        InlinePanel inlinePanelText = new InlinePanel(frame.inlinePanel.PanelViewWidth) {
            Margin = new Thickness(tag.margin[0], tag.margin[1], tag.margin[2], tag.margin[3])
        };
        for (int i = 0; i<splittedBySpace.Length; i++) {
            inlinePanelText.Children.Add(createTextBlock(
            splittedBySpace[i] + (i==splittedBySpace.Length-1 ? "" : " "),
            tag.color, tag.bold, tag.crossed, tag.underline, tag.italic, tag.monospace, tag.fontsize,
            new int[] {0,0,0,0}, tag.background, tag.sub, tag.sup, tag.title, tag.pointer, tag.click
            ));
        }
        yield return new AXAMLtag (tag.startNewLine,
        tag.endNewLine, inlinePanelText);
        yield break;
    }
    /// <summary>
    /// Proccess <c>select</c> and <c>datalist</c> tag
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    IEnumerable<AXAMLtag> createSelect(HTMLtag tag) {
        List<string> options = new List<string>();
        int? selected = null;
        ComboBox comboBox = new ComboBox{
            Width = 150
        };
        foreach(var child in tag.children) {
            if (child.name=="option") {
                string option = "";
                foreach(var partOfOption in child.children) {
                    if (partOfOption.name==null) {
                        option+=partOfOption.content;
                    }
                }
                options.Add(option);
                if (child.attributes.ContainsKey("selected")) {
                    selected = options.Count-1;
                }
            }
        }
        comboBox.Items = options;
        if (selected!=null) comboBox.SelectedIndex = (int)selected;
        tag.attributes.TryGetValue("name", out string? nameComboBox);
        if (tag.form!=null) {
            tag.form.addInput(nameComboBox, comboBox, selected,
            required: tag.attributes.ContainsKey("required"));
        }
        yield return new AXAMLtag(tag.startNewLine,
        tag.endNewLine, comboBox);
        yield break;
    }
    /// <summary>
    /// Proccess <c>form</c> tag
    /// </summary>
    IEnumerable<AXAMLtag> createForm(HTMLtag tag) {
        HttpMethod httpMethod = HttpMethod.Get;
        if(!tag.attributes.TryGetValue("action", out string? action)) {
            action = "";
        }
        tag.attributes.TryGetValue("enctype", out string? enctype);
        if (tag.attributes.TryGetValue("method", out string? methodStr)) {
            switch(methodStr.ToLower()) {
                case "post":
                    httpMethod = HttpMethod.Post;
                    break;
                case "put":
                    httpMethod = HttpMethod.Put;
                    break;
                case "delete":
                    httpMethod = HttpMethod.Delete;
                    break;
                case "head":
                    httpMethod = HttpMethod.Head;
                    break;
                default:
                    httpMethod = HttpMethod.Get;
                    break;
            }
        }
        tag.form = new Form(new Uri(baseUri, action), frame, request, enctype, httpMethod);
        tag.inheritStyleToChildren();
        foreach (var child in tag.children) {
            foreach (var el in display(child)) yield return el;
        }
        yield break;
    }
    /// <summary>
    /// Proccess <c>input</c> tag with type number from tag
    /// </summary>
    IEnumerable<AXAMLtag> createInputNumber(HTMLtag tag) {
        double numberWidth = 200;
        double numberHeight = 30;
        double numberMax = double.MaxValue;
        double numberMin = double.MinValue;
        double numberValue = 0;
        if (tag.attributes.TryGetValue("width", out string? numberWidthStr)) {
            if (double.TryParse(numberWidthStr, out double numberWidthDouble)) {
                numberWidth = numberWidthDouble;
            }
        }
        if (tag.attributes.TryGetValue("height", out string? numberHeightStr)) {
            if (double.TryParse(numberHeightStr, out double numberHeightDouble)) {
                numberHeight = numberHeightDouble;
            }
        }
        if (tag.attributes.TryGetValue("max", out string? numberMaxStr)) {
            if (double.TryParse(numberMaxStr, out double numberMaxDouble)) {
                numberMax = numberMaxDouble;
            }
        }
        if (tag.attributes.TryGetValue("min", out string? numberMinStr)) {
            if (double.TryParse(numberMinStr, out double numberMinDouble)) {
                numberMin = numberMinDouble;
            }
        }
        if (tag.attributes.TryGetValue("value", out string? numberValStr)) {
            if (double.TryParse(numberValStr, out double numberValDouble)) {
                numberValue = numberValDouble;
            }
        }
        tag.attributes.TryGetValue("name", out string? numberName);
        NumericUpDown numericUpDown = new NumericUpDown{
            Width = numberWidth,
            Height = numberHeight,
            Minimum = numberMin,
            Maximum = numberMax,
            Value = numberValue
        };
        if (tag.form!=null) tag.form.addInput(numberName, numericUpDown, numberValue,
        required: tag.attributes.ContainsKey("required"));
        yield return new AXAMLtag
            (tag.startNewLine, tag.endNewLine, numericUpDown);
        yield break;
    }
    /// <summary>
    /// Parse <c>meter</c> and <c>progress</c> html tags
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    IEnumerable<AXAMLtag> createMeter(HTMLtag tag) {
        double high = double.PositiveInfinity;
        double low = double.NegativeInfinity;
        double min = 0;
        double max = 1;
        double value = 0;
        if (tag.attributes.TryGetValue("min", out string? minStr)) {
            double.TryParse(minStr, out min);
        }
        if (tag.attributes.TryGetValue("max", out string? maxStr)) {
            double.TryParse(maxStr, out max);
        }
        if (tag.attributes.TryGetValue("value", out string? valueStr)) {
            double.TryParse(valueStr, out value);
        }
        if (tag.attributes.TryGetValue("high", out string? highStr)) {
            double.TryParse(highStr, out high);
        }
        if (tag.attributes.TryGetValue("low", out string? lowStr)) {
            double.TryParse(lowStr, out low);
        }
        SolidColorBrush colorBar;
        if (tag.name=="progress") {
            colorBar = new SolidColorBrush(Color.Parse("blue"));
        }
        else if (value>high || value<low) {
            colorBar = new SolidColorBrush(Color.Parse("yellow"));
        }
        else {
            colorBar = new SolidColorBrush(Color.Parse("green"));
        }
        ProgressBar progressBar = new ProgressBar() {
            Minimum = min,
            Maximum = max,
            Value = value,
            ShowProgressText = false,
            Height = 10,
            Width = 20,
            Cursor = tag.pointer ? new Cursor(StandardCursorType.Hand) : Cursor.Default,
            Margin = Thickness.Parse(String.Join(", ", tag.margin)),
            HorizontalAlignment = HorizontalAlignment.Left,
            Foreground = colorBar
        };
        if (title!=null) {
            var tooltip = new ToolTip
            {
                Content = new TextBlock
                {
                    Text = tag.title
                }
            };
            ToolTip.SetTip(progressBar, tooltip);
        }
        yield return new AXAMLtag(tag.startNewLine,
        tag.endNewLine, progressBar);
        yield break;
    }
    /// <summary>
    /// Proccess <c>ol</c> tag
    /// </summary>
    IEnumerable<AXAMLtag> createNumeratedList(HTMLtag tag) {
        tag.margin[0] += 10;
        bool reversed = false;
        int start = 1;
        if (tag.attributes.ContainsKey("reversed")) reversed = true;
        if (tag.attributes.TryGetValue("start", out string startStr)) {
            int.TryParse(startStr, out start);
        }
        if (reversed) {
            foreach (var child in tag.children) {
                if (child.name=="li") start++;
            }
        }
        tag.inheritStyleToChildren();
        foreach (var child in tag.children) {
            if (child.name=="li") {
                if (child.attributes.TryGetValue("value", out string valStr)) {
                    int.TryParse(valStr, out start);
                }
                string toAppend = start.ToString() + ". ";
                if (reversed) start--;
                else start++;
                child.children.Insert(0, new HTMLtag(null, child, null, toAppend));
            }
            foreach (var el in display(child)) yield return el;
        }
        yield break;
    }
    /// <summary>
    /// Creates table from provided <c>table</c> tag
    /// </summary>
    /// <param name="tag">html tag <c>table</c></param>
    /// <returns></returns>
    IEnumerable<AXAMLtag> createTable(HTMLtag tag) {
        var grid = new Grid();
        int definedRows = 0;
        int definedCols = 0;
        int currentRow = -1;

        List<(int,int)> occupied = new List<(int,int)>();
        tag.inheritStyleToChildren();
        for(int i = 0; i<tag.children.Count; i++) {
            switch (tag.children[i].name) {
                case "caption":
                    foreach(var item in display(tag.children[i])) {
                        yield return item;
                    }
                    break;
                case "tr":
                    tag.children[i].inheritStyleToChildren();
                    currentRow++;
                    while (currentRow>=definedRows) {
                        defineRow();
                    }
                    int currentCol = -1;
                    foreach (var child in tag.children[i].children) {
                        if (child.name=="td" || child.name=="th") {
                            currentCol++;
                            while (currentCol>=definedCols ||
                            occupied.Contains((currentRow, currentCol))) {
                                if (currentCol>=definedCols) defineCol();
                                else currentCol++;
                            }

                            // create InlinePanel for field and then put it
                            // in border
                            InlinePanel tableField = new InlinePanel(150){
                                Margin = new Thickness(5),
                                Orientation = Orientation.Horizontal
                            };
                            addToInlinePanel(display(child), tableField);
                            Border border = new Border {
                                BorderBrush = new SolidColorBrush(Color.Parse("black")),
                                BorderThickness = new Thickness(1),
                                Child = tableField
                            };
                            grid.Children.Add(border);
                            Grid.SetRow(border, currentRow);
                            Grid.SetColumn(border, currentCol);

                            // manage colspan and rowspan
                            if (child.attributes.TryGetValue("colspan",
                            out string colspanStr)) {
                                if (int.TryParse(colspanStr, out int colspan)
                                && colspan>1) {
                                    while(definedCols<=currentCol+colspan) defineCol();
                                    Grid.SetColumnSpan(border, colspan);
                                    for(int j = 1; j<colspan; j++) currentCol++;
                                }
                            }
                            if (child.attributes.TryGetValue("rowspan",
                            out string rowspanStr)) {
                                if (int.TryParse(rowspanStr, out int rowspan)) {
                                    while(definedRows<=currentRow+rowspan) defineRow();
                                    Grid.SetRowSpan(border, rowspan);
                                    for(int j = 1; j<rowspan; j++) {
                                        occupied.Add((currentRow+j, currentCol));
                                    }
                                }
                            }
                        }
                    }
                    void defineRow() {
                        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                        definedRows++;
                    }
                    void defineCol() {
                        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                        definedCols++;
                    }
                    break;
                case "colgroup": case "col":
                    // ignore
                    break;
                default:
                    foreach(var el in display(tag.children[i])) yield return el;
                    break;
            }
        }
        Border border1 = new Border{
            BorderBrush = new SolidColorBrush(Color.Parse("black")),
            BorderThickness = new Thickness(1),
            Child = grid
        };
        yield return new AXAMLtag (true, true, border1);
        yield break;
    }
    /// <summary>
    /// Parse provided `<details>` tag
    /// </summary>
    IEnumerable<AXAMLtag> createDetails(HTMLtag tag) {
        bool isopen = false;
        if (tag.attributes.ContainsKey("open")) isopen = true;
        var arrow = new HTMLtag(null, content: isopen ? "▼" : "▶");
        Summary onclickEvent = new Summary(isopen);
        arrow.click = onclickEvent;
        yield return new AXAMLtag(false, false, new Separator());
        foreach (var el in display(arrow)) {
            onclickEvent.setArrow((TextBlock)((InlinePanel)el.Control).Children[0]);
            yield return el;
        }
        tag.inheritStyleToChildren();
        List<Control> content = new List<Control>();
        bool summaryExist = false;
        foreach(var el in tag.children) {
            if (el.name=="summary") {
                el.click = onclickEvent;
                el.inheritStyleToChildren();
                foreach(var child in el.children) {
                    foreach(var x in display(child)) {
                        yield return x;
                    }
                }
                yield return new AXAMLtag(false, false, new Separator());
                summaryExist = true;
                break;
            }
        }
        if (!summaryExist) {
            yield return new AXAMLtag(false, false, 
            createTextBlock("Details", tag.color, tag.bold, tag.crossed,
            tag.underline, tag.italic, tag.monospace, tag.fontsize,
            tag.margin, tag.background, tag.sub, tag.sup, tag.title,
            tag.pointer, onclickEvent)
            );
        }
        foreach (var el in tag.children) {
            foreach (var contenttags in display(el)) {
                content.Add(contenttags.Control);
                if (!isopen) contenttags.Control.IsVisible = false;
                yield return contenttags;
            }
        }
        onclickEvent.setContent(content);
        yield break;
    }
    /// <summary>
    /// Generates button content based on given arguments
    /// </summary>
    /// <param name="valueInsteadContent">In content show value attribute instead of children</param>
    /// <param name="defValue">Value in case of empty button</param>
    /// <param name="buttonContent">Button content if already prepared</param>
    /// <returns></returns>
    Button createButton(HTMLtag tag, bool valueInsteadContent = false,
    string defValue = "", Control? buttonContent = null) {
        bool disabled = false;
        if (tag.attributes.ContainsKey("disabled")) disabled = true;
        tag.attributes.TryGetValue("name", out string? nameBtn);
        if (!tag.attributes.TryGetValue("value", out string? valueBtn)) {
            valueBtn = defValue;
        }
        // generate button content
        if (buttonContent==null && !valueInsteadContent) {
            buttonContent = new InlinePanel(frame.inlinePanel.PanelViewWidth){
                Orientation = Orientation.Horizontal
            };
            tag.background = "#e9e9ed";
            tag.inheritStyleToChildren(false);
            foreach (var child in tag.children) {
                addToInlinePanel(display(child), (InlinePanel)buttonContent);
            }
        }
        else if (buttonContent==null) {
            buttonContent = new TextBlock{
                Text = valueBtn
            };
        }
        Button button = new Button{
            Content = buttonContent,
            IsEnabled = disabled ? false : true,
            BorderThickness = new Thickness(2),
            BorderBrush = new SolidColorBrush(Color.Parse("gray")),
            Background = new SolidColorBrush(Color.Parse("#e9e9ed")),
            Margin = new Thickness(tag.margin[0], tag.margin[1], tag.margin[2], tag.margin[3]),
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        if (tag.title!=null) {
            var tooltip = new ToolTip
            {
                Content = new TextBlock
                {
                    Text = tag.title
                }
            };
            ToolTip.SetTip(button, tooltip);
        }
        if (!disabled && tag.form!=null) tag.form.addButton(button, nameBtn, valueBtn);
        return button;
    }
    /// <summary>
    /// Requests image and waits until image loads, then replace alt content with loaded image
    /// </summary>
    /// <param name="tag"><c>img</c> tag</param>
    /// <param name="image">Image Control that should be updated and shown</param>
    /// <param name="allAltEls">all alt content of this image</param>
    /// <returns></returns>
    async Task loadImage(HTMLtag tag, Image image, List<Control> allAltEls)
    {
        string? localSrc;
        if (tag.attributes["src"].Substring(0,14)=="browser://tmp/") {
            if (int.TryParse(tag.attributes["src"].Substring(14), out _)) {
                localSrc = tag.attributes["src"].Substring(9);
            }
            else return; //not secure
        }
        else {
            Request r = new Request(new Uri(baseUri, tag.attributes["src"]),
            request.url, HttpMethod.Get,
            acceptHeader: "image/png, image/jpeg, image/*;q=0.9, image/svg;q=0, image/svg+xml;q=0");
            localSrc = await WebRequest.requestFile(r);
        }
        if (localSrc!=null) {
            try
            {
                double width = -1;
                double height = -1;
                image.Source = new Avalonia.Media.Imaging.Bitmap(localSrc);
                image.Width = image.Source.Size.Width;
                image.Height = image.Source.Size.Height;
                ((Control)image.Parent).Height = image.Parent.Height+image.Height;
                image.Margin = new Thickness(tag.margin[0], tag.margin[1], tag.margin[2], tag.margin[3]);
                image.Cursor = tag.pointer ? new Cursor(StandardCursorType.Hand) : Cursor.Default;
                if (tag.title!=null) {
                    var tooltip = new ToolTip
                    {
                        Content = new TextBlock
                        {
                            Text = tag.title
                        }
                    };
                    ToolTip.SetTip(image, tooltip);
                }
                if (tag.click!=null) image.PointerReleased += tag.click.onClick;
                image.IsVisible = true;
                foreach(var el in allAltEls) el.IsVisible = false;
                if (tag.attributes.TryGetValue("width", out string widthStr)) {
                    if (double.TryParse(widthStr, out width)) {
                        image.Width = width;
                    }
                }
                if (tag.attributes.TryGetValue("height", out string heightStr)) {
                    if (double.TryParse(heightStr, out height)) {
                        image.Height = height;
                    }
                }
                if (width==-1 || height==-1) image.Stretch = Stretch.Uniform;
            }
            catch {}
        }
    }
    /// <summary>
    /// Creates TextBox Avalonia control from <c>input</c> html tag
    /// </summary>
    TextBox createTextBox(HTMLtag tag) {
        TextBox textBox = new TextBox();
        double width = 200;
        if (tag.attributes.TryGetValue("width", out string widthStr)) {
            double.TryParse(widthStr, out width);
        }
        textBox.Width = width;
        if (tag.attributes.TryGetValue("height", out string heightStr)) {
            if (double.TryParse(heightStr, out double height)) {
                textBox.Height = height;
            }
        }
        if (tag.attributes.TryGetValue("maxlength", out string maxlengthStr)) {
            if (int.TryParse(maxlengthStr, out int maxlength)) {
                textBox.MaxLength = maxlength;
            }
        }
        if (tag.attributes.ContainsKey("disabled")) {
            textBox.IsEnabled = false;
        }
        if (tag.attributes.ContainsKey("readonly")) {
            textBox.IsReadOnly = true;
        }
        if (tag.attributes.TryGetValue("value", out string? inputValue)) {
            textBox.Text = inputValue;
        }
        else {
            inputValue = "";
        }
        if (tag.children.Count>0) {
            inputValue = "";
            foreach (var el in tag.children) {
                if (el.content!=null && el.content!="") inputValue+=el.content;
            }
            textBox.Text = inputValue;
        }
        if (tag.attributes.TryGetValue("placeholder", out string placeholder)) {
            textBox.Watermark = placeholder;
        }
        tag.attributes.TryGetValue("type", out string? type);
        // regular expression to which input must fuly match before form submission
        string? regex = "(.*)";
        switch (type) {
            case "email":
                regex = "[a-z0-9\\.]+@[a-z]+\\.[a-z]+";
                break;
            case "password":
                textBox.PasswordChar = '•';
                break;
            case "url":
                regex = "^http[s]?:\\/\\/[A-Za-z0-9-\\.]+\\/(.*)$";
                break;
            default:
                if (!tag.attributes.TryGetValue("pattern", out regex)) {
                    regex = "(.*)";
                }
                break;
        }
        if (tag.form!=null) {
            tag.attributes.TryGetValue("name", out string? name);
            tag.form.addInput(name, textBox, inputValue, regex,
            tag.attributes.ContainsKey("required"));
        }
        return textBox;
    }
    /// <summary>
    /// Genereate Image Control from <c>img</c> html tag
    /// </summary>
    IEnumerable<AXAMLtag> createImage(HTMLtag tag) {
        // if alt setted pass text to html parser as htmltag content
        // when image loaded hide all alt words
        List<Control> allAltEls = new List<Control>();
        if (tag.attributes.ContainsKey("alt")) {
            // text alt
            HTMLtag alt = new HTMLtag(null, tag, null, tag.attributes["alt"]);
            tag.children.Add(alt);
            tag.inheritStyleToChildren();
            foreach(var el in display(alt)) {
                allAltEls.Add(el.Control);
                yield return el;
            }
        }
        else {
            // not text alt -> show Assets/broken-file.png
            Image brokenFile = new Image
            {
                Width = 20,
                Height = 20,
                Source = new Avalonia.Media.Imaging.Bitmap("Assets/broken-file.png"),
                Margin = new Thickness(tag.margin[0], tag.margin[1], tag.margin[2], tag.margin[3]),
                Cursor = tag.pointer ? new Cursor(StandardCursorType.Hand) : Cursor.Default
            };
            if (tag.title!=null) {
                var tooltip = new ToolTip
                {
                    Content = new TextBlock
                    {
                        Text = tag.title
                    }
                };
                ToolTip.SetTip(brokenFile, tooltip);
            }
            if (tag.click!=null) brokenFile.PointerReleased += tag.click.onClick;
            allAltEls.Add(brokenFile);
            yield return new AXAMLtag(tag.startNewLine, tag.endNewLine, brokenFile);
        }
        if (!tag.attributes.ContainsKey("src")) yield break;
        Image image = new Image{
            IsVisible = false
        };
        yield return new AXAMLtag(false, false, image);
        // continue without await because image can be loaded after page displayed to user
        loadImage(tag, image, allAltEls);
        yield break;
    }
    /// <summary>
    /// Create iframe from given <c>iframe</c> tag
    /// </summary>
    IEnumerable<AXAMLtag> createIframe(HTMLtag tag) {
        // iframe in case receiving image displays image, same with video and audio
        // so embed with our code acts same as iframe
        double width = 300;
        double height = 150;
        Uri? src = null;
        string? srcdoc = null;
        
        if (tag.attributes.TryGetValue("width", out string widthStr)) {
            if (double.TryParse(widthStr, out double width1)) width = width1;
        }
        if (tag.attributes.TryGetValue("height", out string heightStr)) {
            if (double.TryParse(heightStr, out double height1)) height = height1;
        }
        if (tag.attributes.TryGetValue("src", out string srcStr)) {
            try { src = new Uri(baseUri, srcStr); } catch{}
        }
        tag.attributes.TryGetValue("srcdoc", out srcdoc);
        // create inline panel
        InlinePanel inlinePanel = new InlinePanel(width){
            VerticalAlignment = VerticalAlignment.Top,
            Orientation = Orientation.Horizontal,
        };
        Frame newFrame = new Frame(inlinePanel);
        if (srcdoc!=null) {
            newFrame.showHtml(srcdoc);
        }
        else if (src!=null) {
            _ = newFrame.DisplayURL(new Request(src, new Uri("browser://home/"),
            HttpMethod.Get));
        }
        ScrollViewer scrollViewer = new ScrollViewer {
            Content = inlinePanel,
            Width = width,
            Height = height,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Margin = new Thickness(4)
        };
        Border border2 = new Border{
            Child = scrollViewer,
            Width = width+4,
            Height = height+4,
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(5)
        };
        yield return new AXAMLtag(tag.startNewLine, tag.endNewLine,
        border2);
        yield break;
    }
    /// <summary>
    /// Adds elements to given InlinePanel, also adds Separator of needed length
    /// respected to newLines values
    /// </summary>
    /// <param name="enumerable">Enumerable from display(HTML tag)</param>
    /// <param name="inlinePanel">InlinePanel to which tags must be added</param>
    public void addToInlinePanel(IEnumerable<AXAMLtag> enumerable,
    InlinePanel inlinePanel) {
        double separatorWidth = inlinePanel.PanelViewWidth;
        bool currentlyOnNewLine = true;
        foreach(var tag in enumerable) {
            if (tag.StartFromNewLine && !currentlyOnNewLine) {
                inlinePanel.Children.Add(new Separator{
                    Width = separatorWidth,
                    Height = 5
                });
            }
            inlinePanel.Children.Add(tag.Control);
            currentlyOnNewLine = false;
            if (tag.EndsWithNewLine) {
                inlinePanel.Children.Add(new Separator{
                    Width = separatorWidth,
                    Height = 5
                });
                currentlyOnNewLine = true;
            }
        }
    }
}
/// <summary>
/// Data structure that contains Avalonia Control
/// and new-line booleans (before and after)
/// </summary>
public class AXAMLtag {
    /// <summary>
    /// The Control must be shown on new line
    /// </summary>
    public bool StartFromNewLine = false;
    /// <summary>
    /// After Control added, next writing must be continued on new line
    /// </summary>
    public bool EndsWithNewLine = false;
    public Control Control;
    /// <summary>
    /// Data structure that contains Avalonia Control and new-line booleans (before and after)
    /// </summary>
    /// <param name="startFromNewLine">The Control must be shown on new line</param>
    /// <param name="endsWithNewLine">After Control added, next writing must be continued on new line</param>
    public AXAMLtag(bool startFromNewLine, bool endsWithNewLine, Control control) {
        StartFromNewLine = startFromNewLine;
        EndsWithNewLine = endsWithNewLine;
        Control = control;
    }
}