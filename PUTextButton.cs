
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
using System.Collections;
using System.Xml;

public partial class PUTextButton : PUTextButtonBase {

	public Button button;
	public Color? normalColor;
	public Color? disabledColor;
	public Color? highlightedColor;
	public Color? pressedColor;

	private UnityEngine.Events.UnityAction currentOnTouchUpAction = null;

	public override void gaxb_init ()
	{
		base.gaxb_init ();

		if (title == null) {
			gameObject.name = "<TextButton/>";
		}

		button = gameObject.AddComponent<Button> ();

		ColorBlock colors = button.colors;
		colors.fadeDuration = 0;
		if (normalColor != null)
			colors.normalColor = normalColor.Value;
		if (disabledColor != null)
			colors.disabledColor = disabledColor.Value;
		if (highlightedColor != null)
			colors.highlightedColor = highlightedColor.Value;
		if (pressedColor != null)
			colors.pressedColor = pressedColor.Value;
		button.colors = colors;

		if (onTouchUp != null) {
			currentOnTouchUpAction = () => { 
				NotificationCenter.postNotification (Scope (), this.onTouchUp, NotificationCenter.Args("sender", this));
			};

			button.onClick.AddListener(currentOnTouchUpAction); 
		}
	}

	public override void gaxb_final(XmlReader reader, object _parent, Hashtable args) {
		base.gaxb_final (reader, _parent, args);
		
		string attrib;

		if (reader != null) {
			attrib = reader.GetAttribute ("normalColor");
			if (attrib != null) {
				normalColor = Color.white.PUParse(attrib);
				if (button != null) {
					ColorBlock colors = button.colors;
					colors.normalColor = normalColor.Value;
					button.colors = colors;
				}
			}
		}
		if (reader != null) {
			attrib = reader.GetAttribute ("disabledColor");
			if (attrib != null) {
				disabledColor = Color.white.PUParse(attrib);
				if (button != null) {
					ColorBlock colors = button.colors;
					colors.disabledColor = disabledColor.Value;
					button.colors = colors;
				}
			}
		}
		if (reader != null) {
			attrib = reader.GetAttribute ("highlightedColor");
			if (attrib != null) {
				highlightedColor = Color.white.PUParse(attrib);
				if (button != null) {
					ColorBlock colors = button.colors;
					colors.highlightedColor = highlightedColor.Value;
					button.colors = colors;
				}
			}
		}
		if (reader != null) {
			attrib = reader.GetAttribute ("pressedColor");
			if (attrib != null) {
				pressedColor = Color.white.PUParse(attrib);
				if (button != null) {
					ColorBlock colors = button.colors;
					colors.pressedColor = pressedColor.Value;
					button.colors = colors;
				}
			}
		}
	}

	public void SetOnTouchUp (string newNote) {
		if (newNote != null) {

			if (currentOnTouchUpAction != null) {
				button.onClick.RemoveListener(currentOnTouchUpAction);
			}

			this.onTouchUp = newNote;

			currentOnTouchUpAction = () => { 
				NotificationCenter.postNotification (Scope (), this.onTouchUp, NotificationCenter.Args("sender", this));
			};
			
			button.onClick.AddListener(currentOnTouchUpAction); 
		}
	}

}
