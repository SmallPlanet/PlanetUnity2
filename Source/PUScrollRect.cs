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
using UnityEngine.EventSystems;

public class InvisibleHitGraphic : Graphic, ICanvasRaycastFilter {
	
#if !UNITY_4_6 && !UNITY_5_0 && !UNITY_5_1

#if UNITY_5_2_0 || UNITY_5_2_1
	protected override void OnPopulateMesh(Mesh m) {
		// This is glue that uses <=5.1 OnFillVBO code with >=5.2 OnPopulatedMesh
		// http://docs.unity3d.com/ScriptReference/UI.Graphic.OnPopulateMesh.html
		
		var vbo = new List<UIVertex>();
		_OnFillVBO(vbo);
		using (var vh = new VertexHelper()) {
			var quad = new UIVertex[4];
			for (int i = 0; i < vbo.Count; i += 4) {
				vbo.CopyTo(i, quad, 0, 4);
				vh.AddUIVertexQuad(quad);
			}
			vh.FillMesh(m);
		}
	}
#else
	protected override void OnPopulateMesh(VertexHelper vh) {
		
		// This is glue that uses <=5.1 OnFillVBO code with >=5.2 OnPopulatedMesh
		// http://docs.unity3d.com/ScriptReference/UI.Graphic.OnPopulateMesh.html
		
		List<UIVertex> vbo = new List<UIVertex> ();
		_OnFillVBO (vbo);
		
		vh.Clear ();

		// any new stuff here
	}
#endif

#else
	protected override void OnFillVBO (List<UIVertex> vbo) {
		_OnFillVBO(vbo);
	}
#endif

	protected void _OnFillVBO (List<UIVertex> vbo) {

	}

	public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
		return true;
	}
}

public partial class PUScrollRect : PUScrollRectBase {

	private Vector2 _contentMin = Vector2.zero;
	public Vector2 ContentMin {
		get {
			return _contentMin;
		}
	}
	private Vector2 _contentMax = Vector2.zero;
	public Vector2 ContentMax {
		get {
			return _contentMax;
		}
	}

	public Vector2 _ContentOffset = Vector2.zero;
	public Vector2 ContentOffset {
		get {
			return _ContentOffset;
		}
		
		set {
			Vector2 diff = value - _ContentOffset;
			_ContentOffset = value;
			CalculateContentSize();

			RectTransform contentRectTransform = (RectTransform)contentObject.transform;
			contentRectTransform.anchoredPosition -= diff / 8;
		}
	}


	public GameObject contentObject;
	public ScrollRect scroll;
	public CanvasRenderer canvasRenderer;

	public override void gaxb_init ()
	{
		gameObject = new GameObject ("<ScrollRect/>", typeof(RectTransform));

		canvasRenderer = gameObject.AddComponent<CanvasRenderer> ();
		scroll = gameObject.AddComponent<ScrollRect> ();

		scroll.inertia = inertia;
		scroll.horizontal = horizontal;
		scroll.vertical = vertical;

		if (scrollWheelSensitivity != null) {
			scroll.scrollSensitivity = (float)scrollWheelSensitivity;
		}
	}

	public void CalculateContentSize()
	{
		RectTransform myRectTransform = (RectTransform)contentObject.transform;

		if (contentObject.transform.childCount == 0) {
			myRectTransform.sizeDelta = new Vector2((gameObject.transform as RectTransform).rect.width, 0);
			return;
		}

		// if contentSize does not exist, run through planet children and calculate a content size
		float minX = 999999, maxX = -999999;
		float minY = 999999, maxY = -999999;

		foreach (RectTransform t in contentObject.transform) {
			if(t.gameObject.activeSelf == false) {
				continue;
			}

			float tMinX = t.GetMinX ();
			float tMaxX = t.GetMaxX ();
			float tMinY = t.GetMinY ();
			float tMaxY = t.GetMaxY ();
			
			if (tMinX < minX)
				minX = tMinX;
			if (tMinY < minY)
				minY = tMinY;

			if (tMaxX > maxX)
				maxX = tMaxX;
			if (tMaxY > maxY)
				maxY = tMaxY;
		}

		// If the scroller is locked on an axis, use the parents size for that axis
		if (scroll.horizontal == false) {
			minX = 0;
			maxX = ((RectTransform)myRectTransform.parent).rect.width;
			//maxY = 0;
		}

		if (scroll.vertical == false) {
			minY = 0;
			maxY = ((RectTransform)myRectTransform.parent).rect.height;
			//maxX = 0;
		}

		_contentMin = new Vector2 (minX, minY);
		_contentMax = new Vector2 (maxX, maxY);

		myRectTransform.sizeDelta = new Vector2 (Mathf.Abs(maxX - minX) + _ContentOffset.x, Mathf.Abs(maxY - minY) + _ContentOffset.y);
	}

	public Vector2 SetContentSize(float w, float h)
	{
		// returns the ideal scroll position after content size has changed
		RectTransform myRectTransform = (RectTransform)contentObject.transform;
		Vector2 oldSize = myRectTransform.sizeDelta;

		myRectTransform.sizeDelta = new Vector2 (w + _ContentOffset.x, h + _ContentOffset.y);

		// do we need to adjust the scroll to take this into account??
		RectTransform contentRectTransform = (RectTransform)contentObject.transform;
		return contentRectTransform.anchoredPosition + (oldSize - myRectTransform.sizeDelta);
	}
		
	public override void gaxb_complete()
	{
		// 0) create a content game object to place all of our children into
		contentObject = new GameObject ("ScrollRectContent", typeof(RectTransform));

		for(int i = gameObject.transform.childCount-1; i >= 0; i--){
			Transform t = gameObject.transform.GetChild (i);
			t.SetParent (contentObject.transform, false);
		}
		contentObject.transform.SetParent (gameObject.transform, false);

		contentObject.layer = LayerMask.NameToLayer ("UI");

		// 1) point the scroll rect at our content object
		scroll.content = (RectTransform)contentObject.transform;

		// 2) set the size of the contentObject to the largest rect containing all of our children
		CalculateContentSize ();

		// 3) fix the position of the contentObject so it the scroll is at the top...
		RectTransform myRectTransform = (RectTransform)contentObject.transform;
		myRectTransform.pivot = new Vector2 (0, 1);
		myRectTransform.anchorMin = myRectTransform.anchorMax = new Vector2 (0, 1);
		myRectTransform.anchoredPosition = Vector2.zero;

		if (gameObject.GetComponent<Graphic> () == null) {
			InvisibleHitGraphic invisibleHitGraphic = gameObject.AddComponent<InvisibleHitGraphic> ();
#if !UNITY_4_6
			invisibleHitGraphic.color = Color.clear;
#endif
		}

		base.gaxb_complete ();
	}

}
