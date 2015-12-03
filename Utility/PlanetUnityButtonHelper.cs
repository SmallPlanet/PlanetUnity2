/* A singleton to help keep button code in one place for the multiple
 * types of buttons (PUImageButton, PUTextButton, etc.) as well as keeping
 * the code behavior between the different buttons consistent.
 */ 

using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Remoting.Messaging;
using UnityEngine.EventSystems;



public partial class PlanetUnityButtonHelper {

	private UnityEngine.Events.UnityAction currentOnTouchUpAction = null;

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

}
