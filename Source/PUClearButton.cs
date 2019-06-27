

using UnityEngine;
using UnityEngine.UI;

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

		PlanetUnityButtonHelper.SetOnTouchUp(this, button, onTouchUp);
		PlanetUnityButtonHelper.SetOnTouchDown (this, button, null);
	}

}
