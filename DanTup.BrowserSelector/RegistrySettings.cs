using System.Reflection;
using Microsoft.Win32;

namespace DanTup.BrowserSelector
{
	static class RegistrySettings
	{
		const string AppID = "DanTup.BrowserSelector";
		const string AppName = "DanTup's Browser Selector";
		const string AppDescription = "DanTup's Browser Selector";
		static string AppPath = Assembly.GetExecutingAssembly().Location;
		static string AppIcon = AppPath + ",0";
		static string AppOpenUrlCommand = AppPath + " %1";
		static string AppReinstallCommand = AppPath + " --register";

		internal static void RegisterBrowser()
		{
			// Register application.
			var appReg = Registry.LocalMachine.CreateSubKey(string.Format("SOFTWARE\\{0}", AppID));

			// Register capabilities.
			var capabilityReg = appReg.CreateSubKey("Capabilities");
			capabilityReg.SetValue("ApplicationName", AppName);
			capabilityReg.SetValue("ApplicationIcon", AppIcon);
			capabilityReg.SetValue("ApplicationDescription", AppDescription);

			// Set up protocols we want to handle.
			var urlAssocReg = capabilityReg.CreateSubKey("URLAssociations");
			urlAssocReg.SetValue("http", AppID + "URL");
			urlAssocReg.SetValue("https", AppID + "URL");
			urlAssocReg.SetValue("ftp", AppID + "URL");

			// Register as application.
			Registry.LocalMachine.OpenSubKey("SOFTWARE\\RegisteredApplications", true).SetValue(AppID, string.Format("SOFTWARE\\{0}\\Capabilities", AppID));

			// Set URL Handler.
			var handlerReg = Registry.LocalMachine.CreateSubKey(string.Format("SOFTWARE\\Classes\\{0}URL", AppID));
			handlerReg.SetValue("", AppName);
			handlerReg.SetValue("FriendlyTypeName", AppName);

			handlerReg.CreateSubKey(string.Format("shell\\open\\command", AppID)).SetValue("", AppOpenUrlCommand);
		}

		internal static void UnregisterBrowser()
		{
			Registry.LocalMachine.DeleteSubKeyTree(string.Format("SOFTWARE\\{0}", AppID), false);
			Registry.LocalMachine.OpenSubKey("SOFTWARE\\RegisteredApplications", true).DeleteValue(AppID);
			Registry.LocalMachine.DeleteSubKey(string.Format("SOFTWARE\\Classes\\{0}URL", AppID));
		}
	}
}
