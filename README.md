# User documentation - Web Browser

Web Browser is an app for accessing websites and showing their content using HTML.
The Web Browser supports only HTML; Javascript and CSS are not supported.

HTML 5 is the only supported HTML specification, however, some HTML 5 features are
not supported: ```<video>```, ```<audio>```, ```<svg>```, and
```<map>``` tags; contenteditable, autofocus, accesskey and sandbox attributes;
file type input field; HTML tags for formatting Arabian or Japanese languages

You can visit any web page by typing your URL into the top bar and pressing Enter.
This action requests data using a GET request. In the address bar, you can specify URL with parameters
and fragment. All parts of URL are optional except for the domain. If the protocol is not specified,
then https will be used. This allows to request "example.com" in the bar,
while the actual request will be made to ```https://example.com/```

Empty request leads to redirect to ```browser://home/```

All URLs with the protocol "browser" are internal app URLs. There are four supported
internal URLs:
1. ```browser://home/``` - Home page
2. ```browser://error/Wrong-protocol``` - shows message about wrong protocol
3. ```browser://error/Wrong-url-format``` - shows message about incorrect URL format
4. ```browser://error/Unknown-error``` - shows message about unknown error

All other links with browser protocol also show messages about unknown error

You can repeat the last request and reload site content by pressing Update button
in the top panel. It will send request with the same method, data, and encoding as before,
only cookies are updated

You can navigate to the previous page by pressing Back button. You can erase back-button
history by pressing Home button

Home button displays home page at ```browser://home/```

You can find Developer documentation at [README_dev.md](README_dev.md)
