using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DanTup.BrowserSelector
{
	class Program
	{
		static void Main( string[] args )
		{
			if (args == null || args.Length != 1 || !HandleArg(args[0]))
				ShowHelpInfo();
		}

		static bool HandleArg( string arg )
		{
			if (string.Equals(arg, "--register", StringComparison.OrdinalIgnoreCase))
			{
				EnsureAdmin(arg);
				RegistrySettings.RegisterBrowser();
			}
			else if (string.Equals(arg, "--unregister", StringComparison.OrdinalIgnoreCase))
			{
				EnsureAdmin(arg);
				RegistrySettings.UnregisterBrowser();
			}
			else if (arg.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
				LaunchBrowser(arg);
			else
				return false;

			return true;
		}

		static void ShowHelpInfo()
		{
			MessageBox.Show(@"Usage:

	BrowserSelector.exe --register
		Register as web browser

	BrowserSelector.exe --unregister
		Unregister as web browser

	BrowserSelector.exe ""http://example.org/""
		Launch example.org

Once you have registered the app as a browser, you should use visit ""Set Default Browser"" in Windows to set this app as the default browser.");
		}

		static void EnsureAdmin( string arg )
		{
			WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
			if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = Assembly.GetExecutingAssembly().Location,
					Verb = "runas",
					Arguments = arg
				});
				Environment.Exit(0);
			}
		}


		static void LaunchBrowser( string url )
		{
			try
			{
				var urlPreferences = ConfigReader.GetUrlPreferences();
				string _url = url;
				Uri uri = new Uri(_url);

				string pattern;
				string domain = "", urlPattern;

				foreach (var preference in urlPreferences)
				{
					urlPattern = preference.UrlPattern;

					if (urlPattern.StartsWith("/") && urlPattern.EndsWith("/"))
					{
						// The domain from the INI file is a regex..
						domain = uri.Authority + uri.AbsolutePath;
						pattern = urlPattern.Substring(1, urlPattern.Length - 2);
					}
					else
					{
						// We're only checking the domain.
						domain = uri.Authority;

						// Escape the input for regex; the only special character we support is a *
						var regex = Regex.Escape(urlPattern);
						// Unescape * as a wildcard.
						pattern = string.Format("^{0}$", regex.Replace("\\*", ".*"));
					}

					if (Regex.IsMatch(domain, pattern))
					{
						string loc = preference.Browser.Location;
						if (loc.IndexOf("{url}") > -1)
						{
							loc = loc.Replace("{url}", _url);
							_url = "";
						}
						if (loc.StartsWith("\"") && loc.IndexOf('"', 2) > -1)
						{
							// Assume the quoted item is the executable, while everything
							// after (the second quote), is part of the command-line arguments.
							loc = loc.Substring(1);
							int pos = loc.IndexOf('"');
							string args = loc.Substring(pos + 1).Trim();
							loc = loc.Substring(0, pos).Trim();
							Process.Start(loc, args + " " + _url);
						}
						else
						{
							// The browser specified in the INI file is a single executable
							// without any other arguments.
							// (normal/original behavior)
							Process.Start(loc, _url);
						}
						return;
					}
				}

				MessageBox.Show(string.Format("Unable to find a suitable browser matching {0}.", url));
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format("Unable to launch browser, sorry :(\r\n\r\nPlease send a copy of this error to DanTup.\r\n\r\n{0}.", ex.ToString()));
			}
		}
	}
}
