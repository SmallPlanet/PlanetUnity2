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
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public partial class PUText : PUTextBase {

	static public Action<string, int, PUGameObject> GlobalOnLinkClickAction;
	public Action<string, int> OnLinkClickAction;
	public Func<string, int, string> TranslateLinkAction;

	public void LinkClicked(string linkText, int linkID) {

		if (TranslateLinkAction != null) {
			linkText = TranslateLinkAction (linkText, linkID);
		}

		if (OnLinkClickAction != null) {
			OnLinkClickAction (linkText, linkID);
		}
		if (OnLinkClickAction == null && GlobalOnLinkClickAction != null) {
			GlobalOnLinkClickAction (linkText, linkID, this);
		}
		if (onLinkClick != null) {
			NotificationCenter.postNotification (Scope (), onLinkClick, NotificationCenter.Args ("link", linkText));
		}
	}

	public Text text;
	public CanvasRenderer canvasRenderer;

	public override void gaxb_init ()
	{
		gameObject = new GameObject ("<Text/>", typeof(RectTransform));

		canvasRenderer = gameObject.AddComponent<CanvasRenderer> ();
		text = gameObject.AddComponent<Text> ();

		if (title == null && value != null) {
			gameObject.name = string.Format("\"{0}\"", value);
		}

		if (fontColor != null) {
			text.color = fontColor.Value;
		}

		if (font != null) {
			text.font = PlanetUnityResourceCache.GetFont (font);
			if (text.font == null) {
				text.font = PlanetUnityResourceCache.GetFont(PlanetUnityOverride.defaultFont());
			}
		} else {
			text.font = PlanetUnityResourceCache.GetFont(PlanetUnityOverride.defaultFont());
		}

		if (fontSize != null) {
			text.fontSize = (int)fontSize;
		}

		if (lineSpacing != null) {
			text.lineSpacing = lineSpacing.Value;
		}

		if (sizeToFit) {
			text.resizeTextForBestFit = true;
		}

		if (maxFontSize != null) {
			text.resizeTextMaxSize = maxFontSize.Value;
		}

		if (minFontSize != null) {
			text.resizeTextMinSize = minFontSize.Value;
		}

		if (vOverflow) {
			text.verticalOverflow = VerticalWrapMode.Overflow;
		}

		if (hOverflow) {
			text.horizontalOverflow = HorizontalWrapMode.Overflow;
		}

		if (fontStyle != null) {
			if(fontStyle == PlanetUnity2.FontStyle.bold)
				text.fontStyle = FontStyle.Bold;
			if(fontStyle == PlanetUnity2.FontStyle.italic)
				text.fontStyle = FontStyle.Italic;
			if(fontStyle == PlanetUnity2.FontStyle.normal)
				text.fontStyle = FontStyle.Normal;
			if(fontStyle == PlanetUnity2.FontStyle.boldAndItalic)
				text.fontStyle = FontStyle.BoldAndItalic;
		}

		if (alignment != null) {
			if(alignment == PlanetUnity2.TextAlignment.lowerCenter)
				text.alignment = TextAnchor.LowerCenter;
			if(alignment == PlanetUnity2.TextAlignment.lowerLeft)
				text.alignment = TextAnchor.LowerLeft;
			if(alignment == PlanetUnity2.TextAlignment.lowerRight)
				text.alignment = TextAnchor.LowerRight;
			if(alignment == PlanetUnity2.TextAlignment.middleCenter)
				text.alignment = TextAnchor.MiddleCenter;
			if(alignment == PlanetUnity2.TextAlignment.middleLeft)
				text.alignment = TextAnchor.MiddleLeft;
			if(alignment == PlanetUnity2.TextAlignment.middleRight)
				text.alignment = TextAnchor.MiddleRight;
			if(alignment == PlanetUnity2.TextAlignment.upperCenter)
				text.alignment = TextAnchor.UpperCenter;
			if(alignment == PlanetUnity2.TextAlignment.upperLeft)
				text.alignment = TextAnchor.UpperLeft;
			if(alignment == PlanetUnity2.TextAlignment.upperRight)
				text.alignment = TextAnchor.UpperRight;

		}

		if (value != null) {
			text.text = PlanetUnityStyle.ReplaceStyleTags(value);
		}

		text.material = new Material(Shader.Find("UI/Default Font"));
	}

}
