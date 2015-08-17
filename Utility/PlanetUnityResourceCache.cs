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
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class PlanetUnityResourceCache
{
	static private Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
	static private Dictionary<string, string> stringFiles = new Dictionary<string, string>();
	static private Dictionary<string, Font> fonts = new Dictionary<string, Font>();

	static public Texture2D GetTexture(string s)
	{
		if (s == null) {
			return null;
		}

		TextAsset fileData = Resources.Load (s, typeof (TextAsset)) as TextAsset;
		if (fileData !=null) {
			Texture2D tex = new Texture2D (2,2,TextureFormat.ARGB32, false);
			tex.LoadImage (fileData.bytes);
			tex.filterMode = FilterMode.Bilinear;
			return tex;
		}

		Texture2D t = Resources.Load<Texture2D> (s);
		if (t == null) {
			#if (UNITY_WEBPLAYER == false && UNITY_WEBGL == false)
			if (s.EndsWith (".png") || s.EndsWith (".jpg")) {
				string filePath = Application.streamingAssetsPath + "/" + s;
				if (File.Exists(filePath))     {
					t = new Texture2D(2, 2, TextureFormat.ARGB32, false);
					t.LoadImage(File.ReadAllBytes(filePath));
				}
			}
			#endif

			if (t == null) {
				Debug.Log ("Unable to load streaming asset: " + Application.streamingAssetsPath + "/" + s);
				return null;
			}
		}
		t.filterMode = FilterMode.Bilinear;
		return t;
	}

	static public Sprite GetSprite(string s)
	{
		if (s == null) {
			return null;
		}

		string spriteName = Path.GetFileName (s);
		string spriteKey = s + spriteName;
		if (sprites.ContainsKey(spriteKey)) {
			return sprites [spriteKey];
		}

		Sprite[] allSprites = Resources.LoadAll<Sprite>(Path.GetDirectoryName(s));

		foreach(Sprite sprite in allSprites) {
			sprites [s + sprite.name] = sprite;
		}

		if (sprites.ContainsKey(spriteKey) == false) {
			// This wasn't a sprite atlas, must be an individual texture
			Texture2D texture = GetTexture(s);
			if (texture == null) {
				return null;
			}
			Sprite sprite = Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height), Vector2.zero);
			sprites [spriteKey] = sprite;
			return sprite;
		}

		return sprites [spriteKey];
	}

	static public string GetTextFile(string s)
	{
		if (s == null) {
			return null;
		}
		if (stringFiles.ContainsKey(s)) {
			return stringFiles [s];
		}

		TextAsset stringData = Resources.Load<TextAsset> (s);
		if (stringData == null) {
			return null;
		}
		string t = stringData.text;
		#if UNITY_EDITOR
		#else
		stringFiles [s] = t;
		#endif
		return t;
	}

	static public Font GetFont(string s)
	{
		if (s == null) {
			return null;
		}
		if (fonts.ContainsKey(s)) {
			return fonts [s];
		}

		Font font = null;

		if (s.Equals ("Arial")) {
			font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
		}

		if (font == null) {
			font = Resources.Load<Font> (s);
		}

		if (font == null) {
			return null;
		}
		fonts [s] = font;
		return font;
	}
}
