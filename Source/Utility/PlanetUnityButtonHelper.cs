/* A singleton to help keep button code in one place for the multiple
 * types of buttons (PUImageButton, PUTextButton, etc.) as well as keeping
 * the code behavior between the different buttons consistent.
 */ 

using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Remoting.Messaging;
using UnityEngine.EventSystems;



public partial class PlanetUnityButtonHelper {

	public static void SetNormalColor (Button btn, string ncStr) {
		if (btn != null && ncStr != null)
			SetNormalColor(btn, Color.white.PUParse(ncStr));
	}
	public static void SetNormalColor (Button btn, Color nc) {
		if (btn != null) {
			ColorBlock colors = btn.colors;
			colors.normalColor = nc;
			btn.colors = colors;
		}
	}

	public static void SetDisabledColor (Button btn, string dcStr) {
		if (btn != null && dcStr != null)
			SetDisabledColor(btn, Color.white.PUParse(dcStr));
	}
	public static void SetDisabledColor (Button btn, Color dc) {
		if (btn != null) {
			ColorBlock colors = btn.colors;
			colors.disabledColor = dc;
			btn.colors = colors;
		}
	}

	public static void SetHighlightedColor (Button btn, string hcStr) {
		if (btn != null && hcStr != null)
			SetHighlightedColor(btn, Color.white.PUParse(hcStr));
	}
	public static void SetHighlightedColor (Button btn, Color hc) {
		if (btn != null) {
			ColorBlock colors = btn.colors;
			colors.highlightedColor = hc;
			btn.colors = colors;
		}
	}

	public static void SetPressedColor (Button btn, string pcStr) {
		if (btn != null && pcStr != null)
			SetPressedColor(btn, Color.white.PUParse(pcStr));
	}
	public static void SetPressedColor (Button btn, Color pc) {
		if (btn != null) {
			ColorBlock colors = btn.colors;
			colors.pressedColor = pc;
			btn.colors = colors;
		}
	}
	
	public static void SetOnTouchUp (PUGameObject gmObj, Button btn, string newNote) {
		if (btn != null && newNote != null) {
			btn.onClick.RemoveAllListeners();

			btn.onClick.AddListener(() => {
				if (gmObj != null)
					NotificationCenter.postNotification (gmObj.Scope(), newNote, NotificationCenter.Args("sender", gmObj));
				else
					NotificationCenter.postNotification (null, newNote);
			}); 
		}
	}

	public static void SetOnTouchDown (PUGameObject gmObj, Button btn, string newNote) {
		if (btn != null && newNote != null) {
			
			// check if the ButtonOnDownBehaviour component exists
			ButtonOnDownBehaviour onDownBehavior = btn.GetComponent<ButtonOnDownBehaviour> ();

			// if we don't then add one
			if (onDownBehavior == null) {
				onDownBehavior = btn.gameObject.AddComponent<ButtonOnDownBehaviour> ();
			}

			// set the note info
			onDownBehavior.scopeObj = gmObj;
			onDownBehavior.note = newNote;
		}
	}

	private class ButtonOnDownBehaviour : MonoBehaviour, IPointerDownHandler {
		public object scopeObj = null;
		public string note = null;

		public void OnPointerDown(PointerEventData data) {
			if (scopeObj != null && scopeObj is PUGameObject)
				NotificationCenter.postNotification ((scopeObj as PUGameObject).Scope(), note, NotificationCenter.Args("sender", scopeObj));
			else
				NotificationCenter.postNotification (null, note);
		}

	}

}
