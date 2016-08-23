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
using System.Xml;
using System.Collections;
using UnityEngine.UI;

public partial class PUMovie : PUMovieBase {

	public RawImage image;
	public CanvasRenderer canvasRenderer;

	public override void gaxb_init ()
	{
		gameObject = new GameObject ("<Movie/>", typeof(RectTransform));

		canvasRenderer = gameObject.AddComponent<CanvasRenderer> ();
		image = gameObject.AddComponent<RawImage> ();

		if (color != null) {
			image.color = color.Value;
		}

		if (resourcePath != null) {

			// Why, oh why are movie textures not supported in iOS?
			#if (UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL || UNITY_TVOS)

			#else
			// Set texture
			MovieTexture tex = (MovieTexture)PlanetUnityOverride.LoadResource(typeof(MovieTexture), resourcePath);
		    if (tex != null) {
				image.texture = tex;

				tex.Stop ();
				tex.Play ();
				tex.loop = looping;
			}
			#endif

		}
	}
}
