# Developer Documentation

# 1. General logic

This application follows the Model-View-Controller architecture and utilizes AvaloniaUI
to display the graphical user interface

The development process involved adhering to best practices, HTML 5 specification,
RFC documentation, Mozilla documentation, and common practices namely
considering how Google Chrome and Mozilla Firefox browsers
handle certain tags, attributes, and errors

Here was used AvaloniaUI 0.10.21 version and .NET 6.0 framework

## View

View is comprised of default AvaloniaUI classes and the Main Window, including:

* MainWindow.axaml.cs
* Program.cs
* App.axaml.cs
* InlinePanel.cs
* ViewLocator.cs
* and more.

### Inline Panel

This application introduces the Inline Panel - manually crated an AvaloniaUI Control
that displays content inline,
when the next element appended to the current line leads to overflow
of the PanelViewWidth (1200px), the element is moved to the next line,
and if the next element is a Separator, it is also moved to the next line.

The Inline Panel was created because neither StackPanel, WrapPanel, Panel, nor Grid
could fulfill the requirements above.
In all these Controls, their children are either all in one line or all in separate lines.

The base class used for the InlinePanel was StackPanel with horizontal orientation

### Other

The logic of the View is generally trivial, and most of the View
consists of AvaloniaUI default code. Therefore, about MainWindow.axaml.cs
and InlinePanel.cs you can read more in Chapter 3 of this document,
about other View classes you can read more in AvaloniaUI documentation

Inline Panel is not presented in AvaloniaUI documentation, since it is locally introduced
controller and MainWindow.axaml.cs mostly consists of not default code

## Controller

All classes belonged to Controller part are placed in Controller directory

Controller is comprised of:

* Frame.cs

  Displays content in the given InlinePanel. This functionality could be realised
  as a part of MainWindow.axaml.cs, but was moved to another class to support `<iframe>`

* Click.cs

  Click was designed as a base class to handle clicks on special items.
  For a current time this base class was implemented in Click class to support `<a>`
  and Summary class to support `<summary>`

* Form.cs

  Handles `<form>`'s submit and reset buttons, keeps all inputs with their
  default values, when form submitted checking for patterns and required, and
  manage other `<form>` functionality

* History.cs

  History class is a linked list of history of visited sites. Was create for
  the back button from MainWindow.axaml

* Request.cs

  Request class is a class that handles all needed request information,
  such as http request method, target url, referer, referrer policy, request data,
  enctype and so long. Generally created by Controller and pushed to Model

  Request also contains fields for response information such as response body,
  mimetype and referrer policy

  Request does not handle Cookies, handling cookies is part of Model behaviour


## Model

Model logic can be divided to three main proccesses:
1. Request data from url considering cookies (mostly HTML data)

  This classes are placed in separate subfolder "HTTP Request"

  * WebRequest.cs

    Receives Request argument, creates HTTP request using given variables from Request,
    and by loading cookies from Cookies class, generates needed Referer header based
    on Referrer-Policy, sends Request and assigns response data, metatype, Referrer-Policy
    header to same Request variable, saves received Cookies

  * Cookies.cs

    Based on Request determines what cookies can be sent according to their security rules,
    and what cookies can be added/changed according to their security rules
    In Cookies class development was implemented best practices from RFC documentation

  * COOKIE_TLDS.cs

    contains one big constant with all existed TLDs, used only by Cookie class and
    was introduced for security purposes to be sure that domains can't assign cookies
    to their TLDs like example.gov.uk can't assign cookies for .gov.uk

2. Parse HTML data into HTML tree
  * HTMLParser.cs

    Gets html code, and parse this code to HTML tree. As root was setted special
    tag `<root>` (acts as any unknown tag), singleton tags are considered

3. Recursively convert html elements from html tree into AXAML controls
    * AXAMLtags.cs

      Starting from root tag recursively assigns all style attributes to their children,
      when child occur to be endpoint (HTML tag that must be parsed as AXAML tag,
      for example any text, form, list, image, iframe, etc.) the AXAMLtag is
      generated

      Then it returns to Frame and Frame appends it to own InlinePanel

In this section was described general application logic

# 2. HTML implementation

As was mentioned before, this application implements HTML elements based on HTML 5 specification,
RFC documentation, and by determining how Google Chrome and Mozilla Firefox browsers
managed those elements

And this application does not support Javascript or CSS stylesheets

In this application was implemented all HTML 5 tags, attributes and values,
except for the following:

* All html tags, attributes, values that load, run, or change
Javascript code or CSS stylesheets
* `<video>`
* `<audio>`
* `<svg>`
* `<map>`
* `<meta>`
* any tags designed specially for Arabic, Japanese, or any other language then English
* `contenteditable` attribute
* `accesskey` attribute
* `sandbox` attribute for `<iframe>`
*  some `<input>` types: `type="file"`, `type="radio"`, `type="range"`

# 3. Detailed description

## public partial class MainWindow : Window

Main Window is the only window in this app.

### void Home (object? sender = null, RoutedEventArgs? e = null)

Reset history and displays home page (`browser://home/`)

### void Back (object? sender = null, RoutedEventArgs? e = null)

Redirects to previously visited page

### void Update (object? sender = null, RoutedEventArgs? e = null)

Reloads last request with updated cookies

### void RequestURL (object? sender, KeyEventArgs e)

Gets requested URL from top panel bar, creates GET request and displays it in the
current Frame

### public void setTitle (string title)

Sets current site page title, displays as MainWindow title

## public class InlinePanel : StackPanel

StackPanel with Horizontal orientation (so all elements append inline)
which wraps blocks to new line if PanelViewWidth can be overflowed or Separator occures

### public InlinePanel(double PanelViewWidth)

Creates InlinePanel and sets PanelViewWidth

### protected override Size ArrangeOverride(Size finalSize)

Private method which arrange children according InlinePanel logic (stated above)

### public void Clear ()

Deletes all children

### public double PanelViewWidth

PanelViewWidth sets the width for Separator children,
and sets width which should not be overflowed, if overflow can occur block
must be moved to the next line

## public class Frame

Represents HTML frame

### public InlinePanel inlinePanel

InlinePanel to which content appended

### public async Task DisplayURL(Request r)

Displays content from passed `Request`

### public void showHtml(string htmlCode)

Display html data in frame

## public class Click

Base class for clickable elements (not for `<form>`)

### public Uri? href

Href attribute, sets only for classes in which it is used

### public Uri referrer

URL address from which clickable event was initiated

### public Uri? ping

Ping attribute, sets only for classes in which it is used

### public bool download

Download attribute, sets only for classes in which it is used

### public RefPolicy referrerpolicy

Referer Policy of redirect request, sets only for classes in which
it is used

### public Frame? frame

Frame that contains this clickable element, sets only for classes in
which it is used

## public class Link : Click

Clickable item which redirects to other page

## public class Summary : Click

 Clickable element `<summary>`, closes or opens `<details>`

### TextBlock arrow

Arrow which indicates whether Summary is open or closed

### List<Control> content

Content which should appear/disappear by open/close of Summary

## public class Form

Handles `<form>` tag

### public void submit(Object? f = null, RoutedEventArgs? s = null)

Form submission: sends request to `action` URL and displays response

### public void addInput(string? name, Control? control = null Object? defValue = null, string regex = "(.*)", bool required = false, string value = "")

Adds input which belongs to `<form>`. Buttons should be added through
addButton method instead of this, even `<input type="submit">`, or 
`<input type="reset">`, etc.

### public void addButton(Button control, string? name, string value = "")

Adds button which belongs to `<form>`

### public void resetForm(object? f = null, RoutedEventArgs? s = null)

Resets form by iterating though all input fields and resetting them
to their initial values

## public class Input

Class for handling various input parameters, used primarly by Form class

## public static class History

History class memorizes visit history pages and was designed for "back button"
When user go to visit new page, the Request is pushed to History
if user wants go back, the last Request popped from History

### public static Request current

Returns the last `Request` added to History

### public static Request previous

Returns penult `Request` added to History

### public static Request Pop()

Remove the last added `Request` and return `Request` before the removed one

### public static void Push(Request s)

Adds new `Request` to the History

### public static void Reset()

Deletes all elements from History

## public class Request

Contains request parameters, except of cookies, also contain response body, mime-type,
and Referrer-Policy header

### public Uri url

Request URL

### public Uri referer

URL from which request was made

### public HttpMethod httpMethod

Request http method

### public HttpContent? data

Data for request body (for POST, PUT and other requests with body)

### public string mediaType

Media Type of request body if data setted

### public string acceptHeader

Value for `Accept` header

### public RefPolicy refPolicy

Current request referer policy

### public string[] mimeType

Response mime-type

### public RefPolicy? refPolicyOutHeader

Response `Referrer-Policy` header

### public byte[]? response

Response body

## public class WebRequest

Sends request with appropriate cookies, process response and assign the result to
the passed argument `Request`, or returns special value in case of
internal requests with `browser://` protocol

### public static async Task sendRequest(Request r)

Sends request with appropriate cookies

### public static async Task<string> htmlRequest(Request r)

Sends request from passed argument with appropriate cookies,
proccess response. If Response is not containing HTML, then
the function generates HTML with given data

For example if data is image, then generated HTML document with <img> tag
which contains as src local file which contains current response

### public static async Task<string?> requestFile (Request r)

Requests file and saves it to /tmp, return absolute path to the saved file

### public static RefPolicy getRefPolicy(string s)

Converts string referrer policy to RefPolicy type

## static string getReferer(Uri url, Uri referer, RefPolicy refPolicy)

Generates Referer header value based on the current URL, its referer and referrer policy

## public static class Cookies

Manages cookies respectively to their security settings

### public static IEnumerable<Cookie> getCookies(Request r)

Returns cookies that allowed to sent with given Request

### public static void addCookie(Request r, string cookie)

Creates cookie respected to security principles

### public static void SerializeCookie()

Serealize cookie and save its value in .web-browser-cookies. Must be done after
every request

## public class Cookie

Serealizable class that represents cookie and its parameters: name, value, domain,
path, expire, HttpOnly, Secure, SameSite

### public bool access(Request r)

Checks permission of passed method to access to current Cookie

## public static class COOKIE_TLDS

### public readonly static string[] tlds

List of all existed TLDs in format started with point


## public class AXAMLtags

### `public IEnumerable<AXAMLtag>` display (HTMLtag tag)

Recursively parse HTML tree into AXAML tags (Avalonia Controls).
Returns Enumerator with AXAMLtag

### public string title

Title from parsed html document

### `IEnumerable<AXAMLtag>` createEndpoint(HTMLtag tag)

Creates AXAML tags from HTMLtag. The name of the tags passed here
must be from private `endpoints` list. This method was designed to not overload `display(HTMLtag tag)`

All next function except of `addToInlinePanel(IEnumerable<AXAMLtag> enumerable, InlinePanel inlinePanel)` are designed to not overload
`createEndpoint(HTMLtag tag)`

### TextBlock createTextBlock(string? text, string color, bool bold, bool crossed, bool underline, bool italic, bool monospace, double fontsize, int[] margin, string background, bool sub, bool sup, string? title, bool pointer, Click? click)

Creates TextBlock based on given arguments

### `IEnumerable<AXAMLtag>` createMultilineText(HTMLtag tag)

Proccess `<textarea>` html tag

### `IEnumerable<AXAMLtag>` createUnorderedList(HTMLtag tag)

Proccess `<ol>` html tag

### void proccessHead (HTMLtag tag)

Proccess `<head>` tag

### `IEnumerable<AXAMLtag>` createInputCheckBox(HTMLtag tag)

Proccess `<input>` with type `<checkbox>`

### `IEnumerable<AXAMLtag>` parseText(HTMLtag tag)

Parse text html tags (HTMLtag with null name and non-null content)

### `IEnumerable<AXAMLtag>` createSelect(HTMLtag tag)

Proccess `<select>` and `<datalist>` tag

### `IEnumerable<AXAMLtag>` createForm(HTMLtag tag)

Proccess `<form>` tag

### `IEnumerable<AXAMLtag>` createInputNumber(HTMLtag tag)

Proccess `<input>` tag with type number from tag

### `IEnumerable<AXAMLtag>` createMeter(HTMLtag tag)

Parse `<meter>` and `<progress>` html tags

### `IEnumerable<AXAMLtag>` createNumeratedList(HTMLtag tag)

Proccess `<ol>` tag

### `IEnumerable<AXAMLtag>` createNumeratedList(HTMLtag tag)

Proccess `<ol>` tag

### `IEnumerable<AXAMLtag>` createTable(HTMLtag tag)

Creates table from provided `<table>` tag

### `IEnumerable<AXAMLtag>` createDetails(HTMLtag tag)

Parse provided `<details>` tag

### Button createButton(HTMLtag tag, bool valueInsteadContent = false, string defValue = "", Control? buttonContent = null)

Generates button content based on given arguments, where
`valueInsteadContent` means whether the method must show value as
a button content instead of button tag children;
`defValue` is a text content that must be shown in case if button is
empty; and `buttonContent` is an opportunity to set already prepared
content for the button

### async Task loadImage(HTMLtag tag, Image image, List<Control> allAltEls)

Requests image and waits until image loads, then replace alt content with loaded image. This function is not called by another Thread because
in Avalonia only main thread has permission to change Window content

### TextBox createTextBox(HTMLtag tag)

Creates TextBox Avalonia control from `<input>` tag

### `IEnumerable<AXAMLtag>` createImage(HTMLtag tag)

Genereate Image Control from `<img>` html tag

### `IEnumerable<AXAMLtag>` createIframe(HTMLtag tag)

Create iframe from given `<iframe>` tag

###  public void addToInlinePanel(`IEnumerable<AXAMLtag>` enumerable, InlinePanel inlinePanel)

Adds elements to given InlinePanel, also adds Separator of needed length respected to newLines values

## AXAMLtag

Data structure that contains Avalonia Control names `Control`,
and `StartFromNewLine` which states whether the `Control` must be
wrote from new line, and `EndsWithNewLine` which states whether
the next writing (appending next element) after `Control` must start
from the new line 

## public static class HTMLParser

### public static HTMLtag Parse (string content)

Parses HTML document to HTML tree with `<root>` as its root, was designed for the
case when html document contains more than one main tags

In case of setting `<html>` as a root tag we can't parse document if it doesn't contain
`<html>` tag and we can't display tags that accidentally moved outside `<html>` tag

All unknown tags are considered the same as `<div>` tag, they must be closed,
and they are displayed as a block, not inline

Comments, DOCTYPE and any other tag which starts from `!` must be ignored,
and will not be included in the final HTML tree

## public class HTMLtag

HTMLtag is a node in HTML tree that creates by HTMLParser.Parse(string content)

Every tag has name and always empty content. The only exception is text which have name=null
and content which contains this text. For example `<p>hi</p>` will be parsed as HTMLtag
with name p and empty content which has child with name null and content Hi

HTMLtag contains name, content, parent, children and attributes

In addition to those mentioned, it includes style tags which are used by AXAMLtags
to inherit style to children. For example: `<p><b><i>bold-and-cursive</i></b></p>`
p is block tag so all data here will start from new line and end with new line,
tag b inherit this property it will start and ends from new line (because it is the only child),
but it also bold, i tag will inherit newLines properties and bold, while it is cursive,
finally text will inherit all these properties from i and we receive bolad and cursive paragraph

### public void inheritStyleToChildren(bool inheritNewLine = true)

Inherit style values to children

## public class Other

All other classes are part of AvaloniaUI and described in AvaloniaUI documentation