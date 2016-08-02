/* Copyright (c) 2012 Small Planet Digital, LLC
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
 * (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
 * publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
 * FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Text;


public class PlanetUnityStyle
{
	private static Dictionary<string,Dictionary<string,string>> allLanguages = new Dictionary<string,Dictionary<string,string>>();
	private static Dictionary<string, string> languageToCode = null;
	private static string languageCode = null;

	public static int GlobalFontSize = 24;

	static public string LanguageCode()
	{
		if(languageCode == null) {
			languageCode = Application.systemLanguage.ToString ();
		}

		CreateLanguageCodeMapping ();

		try {
			languageCode = languageToCode [languageCode];
		} catch {}

		return languageCode;
	}

	static public void CreateLanguageCodeMapping()
	{
		if(languageToCode == null)
		{
			languageToCode = new Dictionary<string, string>();

			languageToCode.Add(SystemLanguage.English.ToString(), "en");
			languageToCode.Add(SystemLanguage.Afrikaans.ToString(), "af");
			languageToCode.Add(SystemLanguage.Arabic.ToString(), "ar");
			languageToCode.Add(SystemLanguage.Basque.ToString(), "eu");
			languageToCode.Add(SystemLanguage.Belarusian.ToString(), "be");
			languageToCode.Add(SystemLanguage.Bulgarian.ToString(), "bg");
			languageToCode.Add(SystemLanguage.Catalan.ToString(), "ca");
			languageToCode.Add(SystemLanguage.Chinese.ToString(), "zh");
			languageToCode.Add(SystemLanguage.Czech.ToString(), "cs");
			languageToCode.Add(SystemLanguage.Danish.ToString(), "da");
			languageToCode.Add(SystemLanguage.Dutch.ToString(), "nl");
			languageToCode.Add(SystemLanguage.Estonian.ToString(), "et");
			languageToCode.Add(SystemLanguage.Faroese.ToString(), "fo");
			languageToCode.Add(SystemLanguage.Finnish.ToString(), "fi");
			languageToCode.Add(SystemLanguage.French.ToString(), "fr");
			languageToCode.Add(SystemLanguage.German.ToString(), "de");
			languageToCode.Add(SystemLanguage.Greek.ToString(), "el");
			languageToCode.Add(SystemLanguage.Hebrew.ToString(), "he");
			languageToCode.Add(SystemLanguage.Icelandic.ToString(), "is");
			languageToCode.Add(SystemLanguage.Indonesian.ToString(), "id");
			languageToCode.Add(SystemLanguage.Italian.ToString(), "it");
			languageToCode.Add(SystemLanguage.Japanese.ToString(), "ja");
			languageToCode.Add(SystemLanguage.Korean.ToString(), "ko");
			languageToCode.Add(SystemLanguage.Latvian.ToString(), "lv");
			languageToCode.Add(SystemLanguage.Lithuanian.ToString(), "lt");
			languageToCode.Add(SystemLanguage.Norwegian.ToString(), "no");
			languageToCode.Add(SystemLanguage.Polish.ToString(), "pl");
			languageToCode.Add(SystemLanguage.Portuguese.ToString(), "pt");
			languageToCode.Add(SystemLanguage.Romanian.ToString(), "ro");
			languageToCode.Add(SystemLanguage.Russian.ToString(), "ru");
			languageToCode.Add(SystemLanguage.SerboCroatian.ToString(), "hr");
			languageToCode.Add(SystemLanguage.Slovak.ToString(), "sk");
			languageToCode.Add(SystemLanguage.Slovenian.ToString(), "sl");
			languageToCode.Add(SystemLanguage.Spanish.ToString(), "es");
			languageToCode.Add(SystemLanguage.Swedish.ToString(), "sv");
			languageToCode.Add(SystemLanguage.Thai.ToString(), "th");
			languageToCode.Add(SystemLanguage.Turkish.ToString(), "tr");
			languageToCode.Add(SystemLanguage.Ukrainian.ToString(), "uk");
			languageToCode.Add(SystemLanguage.Vietnamese.ToString(), "vi");
			languageToCode.Add(SystemLanguage.Hungarian.ToString(), "hu");
			languageToCode.Add(SystemLanguage.Unknown.ToString(), "en");
		}
	}

	static public void ReloadAllLanguages()
	{
		allLanguages.Clear ();
	}

	static public void VerifyLanguageCode(string code)
	{
		Dictionary<string,string> languageDict;

		CreateLanguageCodeMapping ();

		try {
			code = languageToCode [code];
		} catch {}
			
		if (!allLanguages.TryGetValue (code, out languageDict)) {
			string stringsFile = PlanetUnityResourceCache.GetTextFile ("Languages/" + code + "/Localizable.styles");
			if (stringsFile == null) {
				// default to English in cases where
				stringsFile = PlanetUnityResourceCache.GetTextFile ("Languages/en/Localizable.styles");
			}

			if (stringsFile != null) {
				Dictionary<string,string> currentLanguage = new Dictionary<string,string> ();
				MatchCollection matches = Regex.Matches (stringsFile, "\"([^\"]+)\"\\s*=\\s*\"([^\"]+)\"");
				foreach (Match match in matches) {
					currentLanguage.Add (match.Groups [1].Value, match.Groups [2].Value);
				}
				allLanguages.Add (code, currentLanguage);
			}
		}
	}

	static public string ReplaceStyleTags(string baseString, string code)
	{
		#if PLANET_UNITY_DISABLE_STYLES
		return baseString;
		#endif


		VerifyLanguageCode (code);

		Dictionary<string,string> languageDict;
		if(!allLanguages.TryGetValue(code, out languageDict))
		{
			if(code.Equals("en"))
				return baseString;
			return ReplaceStyleTags(baseString, "en");
		}

		StringBuilder sb = new StringBuilder (baseString);
		foreach (String key in languageDict.Keys) {
			sb.Replace (key, languageDict [key]);
		}
		sb.Replace ("\\n", "\n");
		sb.Replace ("\\x0c", "\x0c");
		sb.Replace ("\\x0b", "\x0b");

		sb.Replace ("FONTSIZE_MINUS_8", (GlobalFontSize - 8).ToString());
		sb.Replace ("FONTSIZE_MINUS_6", (GlobalFontSize - 6).ToString());
		sb.Replace ("FONTSIZE_MINUS_4", (GlobalFontSize - 4).ToString());
		sb.Replace ("FONTSIZE_MINUS_2", (GlobalFontSize - 2).ToString());
		sb.Replace ("FONTSIZE", GlobalFontSize.ToString());

		return sb.ToString ();
	}

	static public string ReplaceStyleTags(string key)
	{
		if (languageCode == null) {
			LanguageCode ();
		}
		return ReplaceStyleTags (key, languageCode);
	}
}
