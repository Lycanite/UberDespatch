using System;
using System.Configuration;
using System.IO;
using System.Web.Script.Serialization;

namespace UberDespatch
{
	public class ConfigBase<T> where T : new()
	{
		private const string DEFAULT_FILENAME = "config/global.json";

		protected void Save(string fileName = DEFAULT_FILENAME)
		{
			File.WriteAllText(fileName, (new JavaScriptSerializer()).Serialize(this));
		}

		protected static void Save(T pSettings, string fileName = DEFAULT_FILENAME)
		{
			File.WriteAllText(fileName, (new JavaScriptSerializer()).Serialize(pSettings));
		}

		protected static T Load(string fileName = DEFAULT_FILENAME)
		{
			if (fileName == DEFAULT_FILENAME && !Directory.Exists ("config/"))
				Directory.CreateDirectory ("config/");
			T config = new T();
			if (File.Exists (fileName))
				config = (new JavaScriptSerializer ()).Deserialize<T> (File.ReadAllText (fileName));
			else {
				File.WriteAllText (fileName, (new JavaScriptSerializer ()).Serialize (config));
				Program.LogAlert ("Config", "Unable to find config: " + fileName + " generating a new config with default settings.");
			}
			return config;
		}
	}
}

