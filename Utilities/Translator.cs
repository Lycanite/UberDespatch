using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace UberDespatch
{
	public class Translator
	{
		public ConfigTranslator Config;
		public bool Available = false;


		// ========== TranslationRequest ==========
		public class TranslationRequest
		{
			public string key = "";
			public string country = "";
			public string[] data;
		}


		// ========== Config ==========
		public class ConfigTranslator : ConfigBase<ConfigTranslator>
		{
			public string URL = "";
			public string Key = "";
			public string[] Countries = new string[0];
			public bool CountryBlacklist = false;

			public static ConfigTranslator LoadFile()
			{
				return Load("config" + Path.DirectorySeparatorChar + "translator.json");
			}

			public void SaveFile()
			{
				Save("config" + Path.DirectorySeparatorChar + "translator.json");
			}
		}


		// ========== Constructor ==========
		public Translator()
		{
			this.Config = ConfigTranslator.LoadFile ();
			this.CheckAvailability ();
		}


		// ========== Constructor ==========
		public void CheckAvailability()
		{
			this.Available = this.Config.URL != null && this.Config.URL != "";
		}


		// ========== Can Translate ==========
		public bool CanTranslate(string country)
		{
			if (!this.Available)
				return false;
			if (this.Config.Countries == null || this.Config.Countries.Length == 0)
				return true;
			foreach (string countryEntry in this.Config.Countries) {
				if (countryEntry.ToUpper() == country.ToUpper())
					return !this.Config.CountryBlacklist;
			}
			return this.Config.CountryBlacklist;
		}


		// ========== Translate ==========
		public Dictionary<string, string> Translate (string country, string[] untranslated)
		{
			Dictionary<string, string> translated = new Dictionary<string, string>();
			Program.LogAlert ("Translator", "Checking for language translations...");
			WebClient web = new WebClient();
			web.Encoding = Encoding.UTF8;
			try
			{
				// Clean Untranslated:
				List<string> cleanedUntranslated = new List<string> ();
				foreach (string untranslatedEntry in untranslated) {
					if (untranslatedEntry == "")
						continue;
					if (cleanedUntranslated.Contains (untranslatedEntry))
						continue;
					cleanedUntranslated.Add (untranslatedEntry);
				}
				untranslated = cleanedUntranslated.ToArray ();

				// Create Request:
				TranslationRequest translationRequest = new TranslationRequest ();
				translationRequest.key = this.Config.Key;
				translationRequest.country = country;
				translationRequest.data = untranslated;

				JavaScriptSerializer serializer = new JavaScriptSerializer();
				string untranslatedJSON = serializer.Serialize(translationRequest);
				web.Headers[HttpRequestHeader.ContentType] = "application/json";
				web.Headers.Add("Accept-Charset", "utf-8");
				string translatedJSON = web.UploadString(new Uri(this.Config.URL), "POST", untranslatedJSON);
				string[] translatedData = serializer.Deserialize<string[]> (translatedJSON);

				// Check Translations:
				if (translatedData.Length != untranslated.Length) {
					Program.LogWarning("Translator", "Received an inconsistent number of translations, expected " + untranslated.Length + " but received " + translatedData.Length + ".");
				}

				// Manage Translations:
				bool changes = false;
				for (int i = 0; i < translatedData.Length; i++) {
					translated.Add (untranslated[i], translatedData[i]);
					if (untranslated[i] != translatedData[i])
						changes = true;
				}

				// Log Result:
				if (changes)
					Program.Log("Translator", "This order has been translated.");
				else
					Program.Log("Translator", "This order does not need to be translated or no translations are available.");

			}
			catch (Exception e)
			{
				Program.LogWarning("Translator", "There was a problem translating this order via: " + this.Config.URL);
				Program.LogException(e);
			}

			return translated;
		}
	}
}
