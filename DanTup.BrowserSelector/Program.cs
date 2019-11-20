using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace DanTup.BrowserSelector
{
	class Program
	{
		static void Main(string[] args)
		{
			string arg;
			bool isOption,
				waitForClose = false;

			if (args == null || args.Length == 0)
			{
				ShowHelpInfo();
				return;
			}

			for (int i = 0; i < args.Length; i++)
			{
				arg = args[i].Trim();

				isOption = arg.StartsWith("-") || arg.StartsWith("/");
				while (arg.StartsWith("-") || arg.StartsWith("/"))
					arg = arg.Substring(1);

				if (isOption)
				{
					if (string.Equals(arg, "register", StringComparison.OrdinalIgnoreCase))
					{
						EnsureAdmin("--" + arg);
						RegistrySettings.RegisterBrowser();
						return;
					}
					else if (string.Equals(arg, "unregister", StringComparison.OrdinalIgnoreCase))
					{
						EnsureAdmin("--" + arg);
						RegistrySettings.UnregisterBrowser();
						return;
					}
					else if (string.Equals(arg, "create", StringComparison.OrdinalIgnoreCase))
					{
						CreateSampleSettings();
						return;
					}
					else if (string.Equals(arg, "wait", StringComparison.InvariantCultureIgnoreCase))
					{
						waitForClose = true;
					}
					else if (string.Equals(arg, "no-wait", StringComparison.InvariantCultureIgnoreCase))
					{
						waitForClose = false;
					}
				}
				else
				{
					if (arg.StartsWith("file://", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
					{
						LaunchBrowser(arg, waitForClose);
					}
					else if (arg.EndsWith(".url", StringComparison.InvariantCultureIgnoreCase) || arg.EndsWith(".website", StringComparison.InvariantCultureIgnoreCase))
					{
						LaunchUrlFile(arg, waitForClose);
					}
					else if (arg.EndsWith(".webloc", StringComparison.InvariantCultureIgnoreCase))
					{
						LaunchWeblocFile(arg, waitForClose);
					}
					else
					{
						ShowHelpInfo();
						return;
					}
				}
			}
		}

		static void ShowHelpInfo()
		{
			MessageBox.Show(@"Usage:

    BrowserSelector.exe --register
        Register as web browser

    BrowserSelector.exe --unregister
        Unregister as web browser

    BrowserSelector.exe --create
        Creates a default/sample settings file

Once you have registered the app as a browser, you should use visit ""Set Default Browser"" in Windows to set this app as the default browser.

    BrowserSelector.exe ""http://example.org/""
        Launch example.org

    BrowserSelector.exe [--wait] ""http://example.org/""
        Launch example.org, optionally waiting for the browser to close..

    BrowserSelector.exe ""http://example.org/"" ""http://example.com/"" [...]
        Launches multiple urls

    BrowserSelector.exe ""my bookmark file.url""
        Launches the URL specified in the .url file

    BrowserSelector.exe ""my bookmark file.webloc""
        Launches the URL specified in the .webloc (osx) file

If you use the --wait flag with multiple urls/files each will open one after the other, in order. Each waits for the previous to close before opening. Using the --wait flag is tricky, though, since many (most) browsers open new urls as a new tab in an existing instance.

To open multiple urls at the same time and wait for them, try the following:

    BrowserSelector.exe ""url-or-file"" ""url-or-file"" --wait ""url-or-file""", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Information);
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


		static void LaunchUrlFile(string file, bool waitForClose = false)
		{
			string url = "";
			string[] lines;

			if (!File.Exists(file))
			{
				MessageBox.Show("Could not find or do not have access to .url file.", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			lines = File.ReadAllLines(file);
			if (lines.Length < 2)
			{
				MessageBox.Show("Invalid .url file format.", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			foreach (string l in lines)
			{
				if (l.StartsWith("URL=", StringComparison.InvariantCulture))
				{
					url = l.Substring(4);
					break;
				}
			}

			if (url.Length > 0)
			{
				LaunchBrowser(url);
			}
			else
			{
				MessageBox.Show("Invalid shortcut file format.", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		static void LaunchWeblocFile(string file, bool waitForClose = false)
		{
			string url = "";
			XmlDocument doc;
			XmlNode node;

			if (!File.Exists(file))
			{
				MessageBox.Show("Could not find or do not have access to .url file.", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			doc = new XmlDocument();
			try
			{
				doc.Load(file);
			}
			catch (Exception)
			{
				MessageBox.Show("Could not read the .webloc file.", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			node = doc.DocumentElement.SelectSingleNode("//plist/dict/string");
			if (node == null)
			{
				MessageBox.Show("Unknown or invalid .webloc file format.", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			url = node.InnerText;
			if (url.Length > 0)
			{
				LaunchBrowser(url);
			}
		}

		static void LaunchBrowser(string url, bool waitForClose = false)
		{
			try
			{
				var urlPreferences = ConfigReader.GetUrlPreferences();
				string _url = url;
				Uri uri = new Uri(_url);
				Process p;

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
							p = Process.Start(loc, args + " " + _url);
						}
						else
						{
							// The browser specified in the INI file is a single executable
							// without any other arguments.
							// (normal/original behavior)
							p = Process.Start(loc, _url);
						}

						if (waitForClose)
						{
							p.WaitForExit();
						}

						return;
					}
				}

				MessageBox.Show(string.Format("Unable to find a suitable browser matching {0}.", url), "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format("Unable to launch browser, sorry :(\r\n\r\nPlease send a copy of this error to DanTup.\r\n\r\n{0}.", ex.ToString()), "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		static void CreateSampleSettings()
		{
			DialogResult r = DialogResult.Yes;

			if (File.Exists(ConfigReader.ConfigPath))
			{
				r = MessageBox.Show(@"The settings file already exists. Would you like to replace it with the sample file? (The existing file will be saved/renamed.)", "BrowserSelector", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
			}

			if (r == DialogResult.No)
				return;

			ConfigReader.CreateSampleIni();
		}
	}
}
