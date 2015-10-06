

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public partial class PUClearButton : PUClearButtonBase {

	public Button button;
	public InvisibleHitGraphic graphic;
	public CanvasRenderer canvasRenderer;

	public override void gaxb_init ()
	{
		gameObject = new GameObject ("<ClearButton/>", typeof(RectTransform));

		canvasRenderer = gameObject.AddComponent<CanvasRenderer> ();
		graphic = gameObject.AddComponent<InvisibleHitGraphic> ();

#if !UNITY_4_6
		graphic.color = Color.clear;
#endif

		button = gameObject.AddComponent<Button> ();

		ColorBlock colors = button.colors;
		colors.fadeDuration = 0;
		button.colors = colors;


		if (onTouchUp != null) {

			button.onClick.AddListener(() => { 
				NotificationCenter.postNotification (Scope (), this.onTouchUp, NotificationCenter.Args("sender", this));
			}); 
		}
	}

}
