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
using System.Collections.Generic;
using System;

/* This class is an easy entity to subclass to provide custom geomtry in your own custom PUGameObject. For instance,
 * if you wanted a to create a PUCircle which submits actual geometry for the circle, you can subclass this class
 * and simply override the correct methods to supply your circle generating geometry
 */
public partial class PUCustomGeometry : PUCustomGeometryBase {

	public class CustomGeometryShim : RawImage {

		public Action<VertexHelper> onPopulateMesh = null;

		protected override void OnPopulateMesh(VertexHelper vh) {

			if (onPopulateMesh != null) {
				onPopulateMesh (vh);
			}
		}
	}


	public CustomGeometryShim shim;
	public CanvasRenderer canvasRenderer;
	public string resourcePath;

	public override void gaxb_init ()
	{
		gameObject = new GameObject ("<Color/>", typeof(RectTransform));

		canvasRenderer = gameObject.AddComponent<CanvasRenderer> ();
		shim = gameObject.AddComponent<CustomGeometryShim> ();

		shim.onPopulateMesh = OnPopulateMesh;

		if (resourcePath != null) {
			LoadImageWithResourcePath (resourcePath);
		}
	}

	public virtual void OnPopulateMesh(VertexHelper vh) {
		vh.Clear ();

		// Simple colored square
		vh.AddVert(new Vector3(rectTransform.rect.xMin, rectTransform.rect.yMin), Color.white, new Vector2(0f, 0f));
		vh.AddVert(new Vector3(rectTransform.rect.xMin, rectTransform.rect.yMax), Color.white, new Vector2(0f, 1f));
		vh.AddVert(new Vector3(rectTransform.rect.xMax, rectTransform.rect.yMax), Color.white, new Vector2(1f, 1f));
		vh.AddVert(new Vector3(rectTransform.rect.xMax, rectTransform.rect.yMin), Color.white, new Vector2(1f, 0f));

		vh.AddTriangle (0, 1, 2);
		vh.AddTriangle (2, 3, 0);
	}

	public void LoadImageWithResourcePath(string p) {
		resourcePath = p;
		shim.texture = PlanetUnityResourceCache.GetTexture (p);
	}


	public void SetAllDirty() {
		shim.SetAllDirty ();
	}

	public void SetLayoutDirty() {
		shim.SetLayoutDirty ();
	}

	public void SetMaterialDirty() {
		shim.SetMaterialDirty ();
	}

	public void SetVerticesDirty() {
		shim.SetVerticesDirty ();
	}
}
