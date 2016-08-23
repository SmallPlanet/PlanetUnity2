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

public class DetectTextClick : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler, ICanvasRaycastFilter {

	public PUText entity;


	public bool TestForHit(Vector2 screenPoint, Camera eventCamera, Action<string, int> block) {

		Text t = gameObject.GetComponent<Text> ();
		TextGenerator tGen = t.cachedTextGenerator;

		RectTransform rectTransform = gameObject.transform as RectTransform;

		Vector2 touchPos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle (rectTransform, screenPoint, eventCamera, out touchPos);

		UIVertex[] vArray = tGen.GetVerticesArray ();

		string value = t.text;

		float minDistance = 999999;
		int minChar = -1;
		int numVertices = tGen.vertexCount;
		int linkID = 0;
		int clickedLinkID = -1;

		for (int i = 0; i < tGen.characterCount; i++) {
			int idx = i * 4 + 2;
			if(idx >= numVertices)
				break;

			UIVertex c = vArray [idx];

			if (i < value.Length && value [i] == '\x0c') {
				linkID++;
			}

			float d = Vector2.Distance (touchPos, c.position);
			if (d < minDistance) {
				clickedLinkID = linkID;
				minDistance = d;
				minChar = i;
			}
		}


		if (minChar >= 0 && minDistance < 20 && minChar < value.Length) {
			// i is the index into the string which we clicked.  Determine a "link" by finding the previous '['
			// and the ending ']'
			int startIndex = -1;
			int endIndex = -1;
			for (int k = minChar; k >= 0; k--) {
				if (value [k] == '\x0b') {
					startIndex = k;
					break;
				}
				if (value [k] == '\x0c') {
					endIndex = -1;
					break;
				}
			}
			for (int k = minChar; k < value.Length; k++) {
				if (value [k] == '\x0c') {
					endIndex = k;
					break;
				}
				if (value [k] == '\x0b') {
					endIndex = -1;
					break;
				}
			}

			if (startIndex >= 0 && endIndex >= 0) {
				if (block != null) {
					string linkText = value.Substring (startIndex + 1, endIndex - startIndex - 1).Trim ();
					block (linkText, clickedLinkID);
				}
				return true;
			}
		}

		return false;
	}

	public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
		bool b = TestForHit(screenPoint, eventCamera, null);
		return b;
	}
		
	public void OnPointerClick(PointerEventData eventData) {
		TestForHit (Input.mousePosition, eventData.pressEventCamera, (linkText, clickedLinkID) => {
			if(entity != null){
				entity.LinkClicked (linkText, clickedLinkID);
			}
		});
	}

	public void OnPointerDown(PointerEventData data) {

	}

	public void OnPointerUp(PointerEventData data) {

	}
}

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

		if (onLinkClick != null || OnLinkClickAction != null || GlobalOnLinkClickAction != null) {
			gameObject.AddComponent<DetectTextClick> ();
			DetectTextClick script = gameObject.GetComponent<DetectTextClick> ();
			script.entity = this;
		}

		if (title == null && value != null) {
			gameObject.name = string.Format("\"{0}\"", value);
		}

		if (fontColor != null) {
			text.color = fontColor.Value;
		}

		if (font != null) {
			text.font = PlanetUnityResourceCache.GetFont (font);
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
