﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace DanTup.BrowserSelector
{
	static class ConfigReader
	{
		public const string t = "t";
		/// <summary>
		/// Config lives in the same folder as the EXE, name "BrowserSelector.ini".
		/// </summary>
		public static string ConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BrowserSelector.ini");

		static internal IEnumerable<UrlPreference> GetUrlPreferences()
		{
			if (!File.Exists(ConfigPath))
				throw new InvalidOperationException(string.Format("The config file was not found:\r\n{0}\r\n", ConfigPath));

			// Poor mans INI file reading... Skip comment lines (TODO: support comments on other lines).
			var configLines =
				File.ReadAllLines(ConfigPath)
				.Select(l => l.Trim())
				.Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith(";") && !l.StartsWith("#"));

			// Read the browsers section into a dictionary.
			var browsers = GetConfig(configLines, "browsers")
				.Select(SplitConfig)
				.Select(kvp => new Browser { Name = kvp.Key, Location = kvp.Value })
				.ToDictionary(b => b.Name);

			// If there weren't any at all, force IE in there (nobody should create a config file like this!).
			if (!browsers.Any())
				browsers.Add("ie", new Browser { Name = "ie", Location = @"iexplore.exe ""{0}""" });

			// Read the url preferences, and add a catchall ("*") for the first browser.
			var urls = GetConfig(configLines, "urls")
				.Select(SplitConfig)
				.Select(kvp => new UrlPreference { UrlPattern = kvp.Key, Browser = browsers.ContainsKey(kvp.Value) ? browsers[kvp.Value] : null })
				.Union(new[] { new UrlPreference { UrlPattern = "*", Browser = browsers.FirstOrDefault().Value } }) // Add in a catchall that uses the first browser
				.Where(up => up.Browser != null);

			return urls;
		}

		static IEnumerable<string> GetConfig(IEnumerable<string> configLines, string configName)
		{
			// Read everything from [configName] up to the next [section].
			return configLines
				.SkipWhile(l => !l.StartsWith(string.Format("[{0}]", configName), StringComparison.OrdinalIgnoreCase))
				.Skip(1)
				.TakeWhile(l => !l.StartsWith("[", StringComparison.OrdinalIgnoreCase))
				.Where(l => l.Contains('='));
		}

		/// <summary>
		/// Splits a line on the first '=' (poor INI parsing).
		/// </summary>
		static KeyValuePair<string, string> SplitConfig(string configLine)
		{
			var parts = configLine.Split(new[] { '=' }, 2);
			return new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim());
		}

		public static void CreateSampleIni()
		{
			Assembly assembly;
			Stream stream;
			StringBuilder result;

			assembly = Assembly.GetExecutingAssembly();
			//stream = assembly.GetManifestResourceStream("DanTup.BrowserSelector.BrowserSelector.ini");
			stream = assembly.GetManifestResourceStream(assembly.GetManifestResourceNames()[0]);
			if (stream == null)
			{
				return;
			}

			result = new StringBuilder();

			using (StreamReader reader = new StreamReader(stream))
			{
				result.Append(reader.ReadToEnd());
				reader.Close();
			}

			if (result.Length > 0)
			{
				if (File.Exists(ConfigPath))
				{
					string newName = GetBackupFileName(ConfigPath);
					File.Move(ConfigPath, newName);
				}

				File.WriteAllText(ConfigPath, result.ToString());
			}
		}

		static string GetBackupFileName(string fileName)
		{
			string newName;
			string fname;
			string fext;
			int index = 0;

			fname = Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName);
			fext = Path.GetExtension(fileName);

			do
			{
				newName = string.Format("{0}.{1:0000}{2}", fname, ++index, fext);
			} while (File.Exists(newName));

			return newName;
		}
	}

	class Browser
	{
		public string Name { get; set; }
		public string Location { get; set; }
	}

	class UrlPreference
	{
		public string UrlPattern { get; set; }
		public Browser Browser { get; set; }
	}
}
