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

public partial class PUImage : PUImageBase {

	public Image image;
	public CanvasRenderer canvasRenderer;

	public override void gaxb_init ()
	{
		gameObject = new GameObject ("<Image/>", typeof(RectTransform));

		canvasRenderer = gameObject.AddComponent<CanvasRenderer> ();
		image = gameObject.AddComponent<Image> ();

		if (color != null) {
			image.color = color.Value;
		}

		if (resourcePath != null) {
			LoadImageWithResourcePath(resourcePath);
		}
	}

	public virtual void LoadImageWithResourcePath(string p) {
		resourcePath = p;

		if (image == null) {
			Debug.Log(" ********** ERROR: PUImage image is null!! **********");
			return;
		}

		LoadSprite (p, PlanetUnityResourceCache.GetSprite (p, PlanetUnityOverride.ForceActualSprites));
	}

	public virtual void LoadSprite(string p, Sprite sp) {
		resourcePath = p;

		if (image == null) {
			Debug.Log(" ********** ERROR: PUImage image is null!! **********");
			return;
		}

		if (type != null) {
			if (type == PlanetUnity2.ImageType.filled) {
				image.type = Image.Type.Filled;
			}
			if (type == PlanetUnity2.ImageType.aspectFilled) {
				image.type = Image.Type.Filled;
				image.preserveAspect = true;
			}
			if (type == PlanetUnity2.ImageType.simple) {
				image.type = Image.Type.Simple;
			}
			if (type == PlanetUnity2.ImageType.sliced) {
				image.type = Image.Type.Sliced;
			}
			if (type == PlanetUnity2.ImageType.tiled) {
				image.type = Image.Type.Tiled;
			}
		}

		image.sprite = sp;
		if (image.sprite != null) {
			Vector4 border = image.sprite.border;
			if (!border.x.Equals (0) || !border.y.Equals (0) || !border.z.Equals (0) || !border.w.Equals (0)) {
				image.type = Image.Type.Sliced;
			}
		}
	}

}
