> NOTE: This software is not being actively developed or maintained.

# Browser Selector [![Build status](https://ci.appveyor.com/api/projects/status/github/DanTup/BrowserSelector?svg=true)](https://ci.appveyor.com/project/DanTup/browserselector)

Small utility to launch a different browser depending on the domain of the url being launched.

> **[Read the blog post about this here](http://blog.dantup.com/2015/09/simple-windows-browser-selector/)**.

## Setting Up

1. Grab the [latest release](https://github.com/DanTup/BrowserSelector/releases) and extract to a folder somewhere on your PC.
2. Open the BrowserSelector.ini file and customise paths to your browsers and domain patterns (see below).
3. Run `BrowserSelector.exe --register` from this folder to register the tool in Windows as a web browser.
4. Open the "Choose a default browser" screen in Windows (you can simply search for "default browser" from the start screen).
5. Select BrowserSelector as the default browser.

So far, it has been tested on the following:

- Windows 8.1
- Windows 10 Pro

## Usage

    BrowserSelector.exe --register
        Register as web browser

    BrowserSelector.exe --unregister
        Unregister as web browser

    BrowserSelector.exe --create
        Creates a default/sample settings file

    BrowserSelector.exe "http://example.org/"
        Launch example.org

    BrowserSelector.exe [--wait] "http://example.org/"
        Launch example.org, optionally waiting for the browser to close..

    BrowserSelector.exe "http://example.org/" "http://example.com/" [...]
        Launches multiple urls

    BrowserSelector.exe "my bookmark file.url"
        Launches the URL specified in the .url file.

    BrowserSelector.exe "my bookmark file.webloc"
        Launches the URL specified in the .webloc (osx) file.

If you use the --wait flag with multiple urls/files each will open one after the other, in order. Each waits for the previous to close before opening. Using the --wait flag is tricky, though, since many (most) browsers open new urls as a new tab in an existing instance.

To open multiple urls at the same time and wait for them, try the following:

    BrowserSelector.exe "url-or-file" "url-or-file" --wait "url-or-file"

## Config

Config is a poor mans INI file:

```ini
; Default browser is first in list
; Use `{url}` to specify UWP app browser details
[browsers]
chrome = C:\Program Files (x86)\Google\Chrome\Application\chrome.exe
ff = C:\Program Files (x86)\Mozilla Firefox\firefox.exe
edge = microsoft-edge:{url}
ie = iexplore.exe
chrome_prof8 = "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" --profile-directory="Profile 8"

; Url preferences.
; Only * is treated as a special character (wildcard).
; Matches are domain-only. Protocols and paths are ignored.
; Use "*.blah.com" for subdomains, not "*blah.com" as that would also match "abcblah.com".
[urls]
microsoft.com = ie
*.microsoft.com = ie

; Use my project-based Chrome profile
myproject.live = chrome_prof8
myproject.local = chrome_prof8

; if the key is wrapped in /'s, it is treated as a regex.
/sites\.google\.com/a/myproject.live\.com/ = chrome_prof8

google.com = chrome
visualstudio.com = edge
```

### Browsers

    chrome = C:\Program Files (x86)\Google\Chrome\Application\chrome.exe
    chrome_prof8 = "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" --profile-directory="Profile 8"

- Browser exes must be exact paths to the browser executable.
- Arguments are optional. However, if you provide arguments the exe _must_ be enclosed in quotes.
- If there are no arguments, then the exe paths do not need to be quoted.

**Special cases:**

    edge = microsoft-edge:{url}

- For special browsers, you can include the `{url}` flag. This allows better control over the browser command-line arguments.
- This is required when specifying UWP app's such as Microsoft Edge.
- By default, the url is used as an argument when launching the exe. If the `{url}` flag is specified, it will not be added to the arguments. (In other words, it _won't_ be added twice..)

### Urls

There are two ways to specify an Url. You can use simple wildcards or full regular expressions.

**Simple wildcards:**

    microsoft.com = ie
    *.microsoft.com = ie

- Only `*` is treated as a special character in URL patterns, and matches any characters (equivalent to the `.*` regex syntax).
- Only the domain part (or IP address) of a URL is checked.
- There is no implied wildcard at the start or end, so you must include these if you need them, but be aware that "microsoft.\*" will not only match "microsoft.com" and "microsoft.co.uk" but also "microsoft.somethingelse.com".

**Full regular expressions:**

```regex
	/sites\.google\.com/a/myproject.live\.com/ = chrome_prof8
```

- Full regular expressions are specified by wrapping it in /'s.
- The domain _and_ path are used in the Url comparison.
- The regular expression syntax is based on the Microsoft .NET implementation.
