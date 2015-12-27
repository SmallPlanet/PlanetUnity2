
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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections;

public partial class PUInputField : PUInputFieldBase {


	public GameObject textGameObject;

	public InputField field;
	public PUText placeholderText;

	private string regexValidation = null;

	public string GetValue() {
		if (field.text.Length > 0) {
			return field.text;
		}
		if (placeholderText != null && placeholderText.text.text.Length > 0) {
			return placeholderText.text.text;
		}
		return "";
	}
	
	public override void gaxb_final(XmlReader reader, object _parent, Hashtable args) {
		base.gaxb_final (reader, _parent, args);
		
		string attrib;
		
		if (reader != null) {
			attrib = reader.GetAttribute ("regexValidation");
			if (attrib != null) {
				regexValidation = attrib;
			}
		}
		
		ScheduleForStart ();
		ScheduleForUpdate ();
	}

	public override void gaxb_init ()
	{
		// We call Text's gaxb_init, which creates the appropriate text component on gameObject
		base.gaxb_init ();

		textGameObject = gameObject;

		// Next, we create a new gameObject, and put the Text-created gameObject inside me
		gameObject = new GameObject ("<InputField/>", typeof(RectTransform));

		canvasRenderer = gameObject.AddComponent<CanvasRenderer> ();
		field = gameObject.AddComponent<InputField> ();

		// Move the text to be the child of the input field
		textGameObject.transform.SetParent (gameObject.transform, false);
		textGameObject.FillParentUI ();

		text.supportRichText = false;
		text.alignment = TextAnchor.UpperLeft;

		field.transition = Selectable.Transition.None;

		field.textComponent = text;

		if (contentType == PlanetUnity2.InputFieldContentType.standard) {
			field.contentType = InputField.ContentType.Standard;
		} else if (contentType == PlanetUnity2.InputFieldContentType.autocorrected) {
			field.contentType = InputField.ContentType.Autocorrected;
		} else if (contentType == PlanetUnity2.InputFieldContentType.integer) {
			field.contentType = InputField.ContentType.IntegerNumber;
		} else if (contentType == PlanetUnity2.InputFieldContentType.number) {
			field.contentType = InputField.ContentType.DecimalNumber;
		} else if (contentType == PlanetUnity2.InputFieldContentType.alphanumeric) {
			field.contentType = InputField.ContentType.Alphanumeric;
		} else if (contentType == PlanetUnity2.InputFieldContentType.name) {
			field.contentType = InputField.ContentType.Name;
		} else if (contentType == PlanetUnity2.InputFieldContentType.email) {
			field.contentType = InputField.ContentType.EmailAddress;
		} else if (contentType == PlanetUnity2.InputFieldContentType.password) {
			field.contentType = InputField.ContentType.Password;
		} else if (contentType == PlanetUnity2.InputFieldContentType.pin) {
			field.contentType = InputField.ContentType.Pin;
		} else if (contentType == PlanetUnity2.InputFieldContentType.custom) {
			field.contentType = InputField.ContentType.Custom;
			
			field.onValidateInput += ValidateInput;
		}

		if (lineType == PlanetUnity2.InputFieldLineType.single) {
			field.lineType = InputField.LineType.SingleLine;
		} else if (lineType == PlanetUnity2.InputFieldLineType.multiSubmit) {
			field.lineType = InputField.LineType.MultiLineSubmit;
		} else if (lineType == PlanetUnity2.InputFieldLineType.multiNewline) {
			field.lineType = InputField.LineType.MultiLineNewline;
		}

		if (placeholder != null) {
			placeholderText = new PUText ();
			placeholderText.value = this.placeholder;
			placeholderText.LoadIntoPUGameObject (this);

			placeholderText.text.horizontalOverflow = this.text.horizontalOverflow;
			placeholderText.text.verticalOverflow = this.text.verticalOverflow;

			placeholderText.text.alignment = this.text.alignment;
			placeholderText.text.font = this.text.font;
			placeholderText.text.fontSize = this.text.fontSize;
			placeholderText.text.fontStyle = this.text.fontStyle;
			placeholderText.text.color = this.text.color - new Color(0,0,0,0.5f);
			placeholderText.text.lineSpacing = this.text.lineSpacing;

			placeholderText.gameObject.FillParentUI ();

			field.placeholder = placeholderText.text;
		}

		if (limit != null) {
			field.characterLimit = (int)limit;
		}

		if (selectionColor != null) {
			field.selectionColor = selectionColor.Value;
		}

		// This is probably not the best way to do this, but 4.60.f1 removed the onSubmit event
		field.onEndEdit.AddListener ((value) => {
			if(onValueChanged != null){
				NotificationCenter.postNotification (Scope (), this.onValueChanged, NotificationCenter.Args("sender", this));
			}
		});
			
		foreach (Object obj in gameObject.GetComponentsInChildren<DetectTextClick>()) {
			GameObject.Destroy (obj);
		}

		field.Rebuild (CanvasUpdate.LatePreRender);

		field.text = this.value;
	}

	// only allow what FanDuel server supports
	private char ValidateInput(string text, int charIndex, char addedChar)
	{
		// if we don't have a regex then we'll allow any char
		// if we do then check the added char against the specified regex
		if (regexValidation == null || Regex.IsMatch(""+addedChar, regexValidation))
			return addedChar;

		return '\0';
	}

}
