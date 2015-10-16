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
        static readonly Dictionary<string, string> SpecialCommands = new Dictionary<string, string>
	    {
	        {"edge", "microsoft-edge:{0}"}
	    };

        static void Main(string[] args)
		{
			if (args == null || args.Length != 1 || !HandleArg(args[0]))
				ShowHelpInfo();
		}

		static bool HandleArg(string arg)
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

		static void EnsureAdmin(string arg)
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


		static void LaunchBrowser(string url)
		{
			try
			{
				var urlPreferences = ConfigReader.GetUrlPreferences();

				foreach (var preference in urlPreferences)
				{
					// Escape the input for regex; the only special character we support is a *
					var regex = Regex.Escape(preference.UrlPattern);
					// Unescape * as a wildcard.
					var pattern = string.Format("^{0}$", regex.Replace("\\*", ".*"));

					// We're only checking the domain.
					var domain = new Uri(url).Authority;

					if (Regex.IsMatch(domain, pattern))
					{
					    if (SpecialCommands.ContainsKey(preference.Browser.Name))
					        Process.Start(string.Format(SpecialCommands[preference.Browser.Name], url));
					    else
					        Process.Start(preference.Browser.Location, url);
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
