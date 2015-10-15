# Browser Selector

Small utility to launch a different browser depending on the domain of the url being launched.

### Only tested on Windows 8.1 so far :)

## Setting Up

1. Grab the [latest release](https://github.com/DanTup/BrowserSelector/releases) and extract to a folder somewhere on your PC.
2. Open the BrowserSelector.ini file and customise paths to your browsers and domain patterns (see below).
3. Run `BrowserSelector.exe --register` from this folder to register the tool in Windows as a web browser.
4. Open the "Choose a default browser" screen in Windows (you can simply search for "default browser" from the start screen).
5. Select DanTup.BrowserSelector as the default browser.

## Config

Config is a poor mans INI file:

	; Default browser is first in list
	[browsers]
	edge = edge
	chrome = C:\Program Files (x86)\Google\Chrome\Application\chrome.exe
	ff = C:\Program Files (x86)\Mozilla Firefox\firefox.exe
	ie = iexplore.exe

	; Url preferences.
	; Only * is treated as a special character (wildcard).
	; Matches are domain-only. Protocols and paths are ignored.
	; Use "*.blah.com" for subdomains, not "*blah.com" as that would also match "abcblah.com".
	[urls]
	microsoft.com = ie
	*.microsoft.com = ie
	google.com = chrome
	visualstudio.com = edge

Notes:

- Browser paths must be exact paths to exes with no arguments (or in `PATH`). Values do not need to be quoted. For Microsoft Edge just use "edge".
- Only * is treated as a special character in URL patterns, and matches any characters.
- Only the domain part (or IP address) of a URL is checked.
- There is no implied wildcard at the start or end, so you must include these if you need them, but be aware that "microsoft.*" will not only match "microsoft.com" and "microsoft.co.uk" but also "microsoft.somethingelse.com".
