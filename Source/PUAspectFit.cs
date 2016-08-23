using UnityEngine;
using UnityEngine.UI;

public partial class PUAspectFit : PUAspectFitBase {

	AspectRatioFitter fitter = null;

	public override void gaxb_init ()
	{
		gameObject = new GameObject ("<AspectFit/>", typeof(RectTransform));

		fitter = gameObject.AddComponent<AspectRatioFitter> ();
		if (mode != null) {
			switch (mode) {
			case PlanetUnity2.AspectFitMode.None:
				fitter.aspectMode = AspectRatioFitter.AspectMode.None;
				break;
			case PlanetUnity2.AspectFitMode.WidthControlsHeight:
				fitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
				break;
			case PlanetUnity2.AspectFitMode.HeightControlsWidth:
				fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
				break;
			case PlanetUnity2.AspectFitMode.FitInParent:
				fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
				break;
			case PlanetUnity2.AspectFitMode.EnvelopeParent:
				fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
				break;
			}
		}

		if (contentSize.Value.x > 0 && contentSize.Value.y > 0) {
			fitter.aspectRatio = contentSize.Value.x / contentSize.Value.y;
		}
	}		
}
