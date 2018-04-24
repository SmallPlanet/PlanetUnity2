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
using System.Collections.Generic;
using UnityEngine.UI;
using System.Reflection;
using System;
using System.Text.RegularExpressions;
using TB;
using System.Text;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif


public class MaskGraphic : Graphic, ICanvasRaycastFilter {

	public float insetTop = 0;
	public float insetBottom = 0;
	public float insetLeft = 0;
	public float insetRight = 0;

	public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
		return true;
	}

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
		
		var vbo = new List<UIVertex>();
		_OnFillVBO(vbo);
		
		vh.Clear();

		var quad = new UIVertex[4];
		for (int i = 0; i < vbo.Count; i += 4) {
			vbo.CopyTo(i, quad, 0, 4);
			vh.AddUIVertexQuad(quad);
		}
	}
#endif

#else
	protected override void OnFillVBO (List<UIVertex> vbo) {
		_OnFillVBO(vbo);
	}
#endif

	protected void _OnFillVBO (List<UIVertex> vbo)
	{
		Vector2 corner1 = Vector2.zero;
		Vector2 corner2 = Vector2.zero;

		corner1.x = 0f;
		corner1.y = 0f;
		corner2.x = 1f;
		corner2.y = 1f;

		corner1.x -= rectTransform.pivot.x;
		corner1.y -= rectTransform.pivot.y;
		corner2.x -= rectTransform.pivot.x;
		corner2.y -= rectTransform.pivot.y;

		corner1.x *= rectTransform.rect.width;
		corner1.y *= rectTransform.rect.height;
		corner2.x *= rectTransform.rect.width;
		corner2.y *= rectTransform.rect.height;

		corner2.y -= insetTop;
		corner2.x -= insetRight;

		corner1.y -= insetBottom;
		corner1.x -= insetLeft;

		vbo.Clear();

		UIVertex vert = UIVertex.simpleVert;

		vert.position = new Vector2(corner1.x, corner1.y);
		vert.color = color;
		vbo.Add(vert);

		vert.position = new Vector2(corner1.x, corner2.y);
		vert.color = color;
		vbo.Add(vert);

		vert.position = new Vector2(corner2.x, corner2.y);
		vert.color = color;
		vbo.Add(vert);

		vert.position = new Vector2(corner2.x, corner1.y);
		vert.color = color;
		vbo.Add(vert);
	}

}



public partial class PUGameObject : PUGameObjectBase {

	public GameObject gameObject;
	public RectTransform rectTransform;

	public CanvasGroup canvasGroup = null;

	private static Dictionary<string, Vector4> stringToAnchorLookup = null;

    public string HeirarchyPath()
    {
        StringBuilder sb = new StringBuilder();
        PUGameObject t = this;
        while (t != null)
        {
            PUGameObject p = t.parent as PUGameObject;
            sb.AppendFormat("{0}.", t.GetType().Name);
            if (p != null)
            {
                sb.AppendFormat("{0}.", p.children.IndexOf(t));
            }
            t = p;
        }
        return sb.ToString();
    }

	public void SetParentGameObject(GameObject p)
	{
		gameObject.transform.SetParent (p.transform, false);
	}

	public override void gaxb_load(TBXMLElement element, object _parent, Hashtable args)
	{
		if (stringToAnchorLookup == null) {
			stringToAnchorLookup = new Dictionary<string, Vector4> ();

			stringToAnchorLookup.Add ("top,left", new Vector4 (0, 1, 0, 1));
			stringToAnchorLookup.Add ("top,center", new Vector4 (0.5f, 1, 0.5f, 1));
			stringToAnchorLookup.Add ("top,right", new Vector4 (1, 1, 1, 1));
			stringToAnchorLookup.Add ("top,stretch", new Vector4 (0, 1, 1, 1));

			stringToAnchorLookup.Add ("middle,left", new Vector4 (0, 0.5f, 0, 0.5f));
			stringToAnchorLookup.Add ("middle,center", new Vector4 (0.5f, 0.5f, 0.5f, 0.5f));
			stringToAnchorLookup.Add ("middle,right", new Vector4 (1, 0.5f, 1, 0.5f));
			stringToAnchorLookup.Add ("middle,stretch", new Vector4 (0, 0.5f, 1, 0.5f));

			stringToAnchorLookup.Add ("bottom,left", new Vector4 (0, 0, 0, 0));
			stringToAnchorLookup.Add ("bottom,center", new Vector4 (0.5f, 0, 0.5f, 0));
			stringToAnchorLookup.Add ("bottom,right", new Vector4 (1, 0, 1, 0));
			stringToAnchorLookup.Add ("bottom,stretch", new Vector4 (0, 0, 1, 0));

			stringToAnchorLookup.Add ("stretch,left", new Vector4 (0, 0, 0, 1));
			stringToAnchorLookup.Add ("stretch,center", new Vector4 (0.5f, 0, 0.5f, 1));
			stringToAnchorLookup.Add ("stretch,right", new Vector4 (1, 0, 1, 1));
			stringToAnchorLookup.Add ("stretch,stretch", new Vector4 (0, 0, 1, 1));
		}

		base.gaxb_load (element, _parent, args);
	}
		
	public override void gaxb_final(TBXMLElement element, object _parent, Hashtable args)
	{
		if (gameObject == null) {
			gameObject = new GameObject ("<GameObject />", typeof(RectTransform));
		}

		gameObject.AddComponent<GameObjectRemoveFromNotificationCenter> ();

		if (title != null) {
			gameObject.name = title;
		}

		if (_parent is GameObject) {
			SetParentGameObject (_parent as GameObject);
		} else if (_parent is PUGameObject) {
			PUGameObject parentEntity = (PUGameObject)_parent;
			SetParentGameObject (parentEntity.gameObject);
		}

		if (bounds != null) {
			this.position = new Vector3 (bounds.Value.x, bounds.Value.y, 0.0f);
			this.size = new Vector2 (bounds.Value.z, bounds.Value.w);
		}

		UpdateRectTransform ();

		gameObject.layer = LayerMask.NameToLayer ("UI");

		if (rectMask2D) {
			gameObject.AddComponent<RectMask2D> ();
		}

		if (mask) {
			Mask maskComponent = gameObject.AddComponent<Mask> ();
			maskComponent.showMaskGraphic = showMaskGraphic;

			// Mask requires a Graphic; if we don't have one, add one and tell it now to draw it...
			if (gameObject.GetComponent<Graphic> () == null) {
				MaskGraphic graphic = gameObject.AddComponent<MaskGraphic> ();

				if (maskInset != null) {
					graphic.insetLeft = maskInset.Value.x;
					graphic.insetRight = maskInset.Value.y;
					graphic.insetTop = maskInset.Value.z;
					graphic.insetBottom = maskInset.Value.w;
				}
			}
		}

		if (outline) {
			gameObject.AddComponent<Outline> ();
		}

		if (shader != null) {
			Graphic graphic = gameObject.GetComponent<Graphic> ();
			if (graphic != null) {
				graphic.material = new Material (Shader.Find (shader));
			}
		} else {

			string defaultShader = PlanetUnityOverride.shaderForObject (this);
			if(defaultShader != null){
				Graphic graphic = gameObject.GetComponent<Graphic> ();
				if (graphic != null) {
					graphic.material = new Material (Shader.Find (defaultShader));
				}
			}

		}

		if (ignoreMouse) {
			IgnoreMouse (true);
		}

		if(components != null){
			string[] allComponentNames = components.Split (',');
			foreach (string componentName in allComponentNames) {
				gameObject.AddComponent (Type.GetType(componentName));
			}
		}

		gameObject.SetActive (active);
	}

	public virtual void removeChild(object child){
		children.Remove (child);
	}

	public virtual void unload(){

		PUGameObject p = parent as PUGameObject;
		if (p != null) {
			p.removeChild (this);
		}

		this.PerformOnChildren (val => {
			MethodInfo method = val.GetType ().GetMethod ("gaxb_unload");
			if (method != null) {
				method.Invoke (val, null);
			}
			return true;
		});

		unloadAllChildren ();

		MethodInfo method2 = this.GetType ().GetMethod ("gaxb_unload");
		if (method2 != null) {
			method2.Invoke (this, null);
		}

		NotificationCenter.removeCompletely (this);

		if (gameObject != null) {
			gameObject.SetActive (false);
			GameObject.Destroy (gameObject);
			gameObject = null;
		}
		rectTransform = null;
	}

	public virtual void unloadAllChildren(){
		for(int i = children.Count-1; i >= 0; i--) {
			PUGameObject p = children [i] as PUGameObject;
			p.unload ();
		}
		children.Clear ();
	}

	public override void gaxb_private_complete() {
		SetAnchor (anchor);
	}

	public void UpdateRectTransform() {

		rectTransform = gameObject.GetComponent<RectTransform> ();
		if (rectTransform != null) {
			rectTransform.pivot = pivot.Value;

			rectTransform.localPosition = new Vector3 (0, 0, position.Value.z);
			rectTransform.anchoredPosition = position.Value;
			rectTransform.localScale = scale.Value;
			rectTransform.localEulerAngles = rotation.Value;

			RectTransform parentTransform = gameObject.transform.parent as RectTransform;
			float parentW = Screen.width;
			float parentH = Screen.height;

			if (parentTransform != null) {
				Canvas rootCanvas = parentTransform.GetComponent<Canvas> ();

				if (rootCanvas == null || rootCanvas.isRootCanvas == false) {
					// Work around for unity issue where the rect transform of a sub canvas does not update soon enough
					if (rootCanvas != null && ((int)parentTransform.rect.width == 100 && (int)parentTransform.rect.height == 100) ) {

					} else {
						parentW = parentTransform.rect.width;
						parentH = parentTransform.rect.height;
					}
				}
			}

			if ((int)size.Value.x == 0) {
				size = new Vector2 (parentW, size.Value.y);
			}
			if ((int)size.Value.y == 0) {
				size = new Vector2 (size.Value.x, parentH);
			}

			rectTransform.sizeDelta = size.Value;
		}
	}

	public void SetAnchor(string newAnchor) {
		// Delay setting of stretch anchors until after loading has happened
		if (gameObject == null) {
			return;
		}
		
		RectTransform parentTransform = gameObject.transform.parent as RectTransform;
		float parentW = Screen.width;
		float parentH = Screen.height;
		
		if (parentTransform != null) {
			Canvas rootCanvas = parentTransform.GetComponent<Canvas> ();
			
			if (rootCanvas == null || rootCanvas.isRootCanvas == false) {
				// Work around for unity issue where the rect transform of a sub canvas does not update soon enough
				if (rootCanvas != null && ((int)parentTransform.rect.width == 100 && (int)parentTransform.rect.height == 100) ) {
					
				} else {
					parentW = parentTransform.rect.width;
					parentH = parentTransform.rect.height;
				}
			}
		}

		if (newAnchor != null) {
			anchor = newAnchor;
			anchor = Regex.Replace(anchor, @"\s+", ""); // remove whitespace
			int numCommas = anchor.NumberOfOccurancesOfChar (',');
			Vector4 values = new Vector4 ();
			
			if (numCommas == 1) {
				// english representation
				values = stringToAnchorLookup [anchor];
			}
			
			if (numCommas == 3) {
				// math representation
				values.PUParse (anchor);
			}
			
			rectTransform.anchorMin = new Vector2 (values.x, values.y);
			rectTransform.anchorMax = new Vector2 (values.z, values.w);
			
			// the sizeDelta is the amount left over after the anchors are calculated; therefore,
			// if we have set the anchors we need to adjust the sizeDelta
			float anchorDeltaX = rectTransform.anchorMax.x - rectTransform.anchorMin.x;
			float anchorDeltaY = rectTransform.anchorMax.y - rectTransform.anchorMin.y;
			
			float mySizeDeltaX = rectTransform.sizeDelta.x;
			float mySizeDeltaY = rectTransform.sizeDelta.y;
			
			mySizeDeltaX -= (parentW * anchorDeltaX);
			mySizeDeltaY -= (parentH * anchorDeltaY);
			
			rectTransform.sizeDelta = new Vector2 (mySizeDeltaX, mySizeDeltaY);
		}
	}

	public void SetFrame(float x, float y, float w, float h, float pivotX, float pivotY, string anchor) {
		position = new Vector3 (x, y, 0);
		size = new Vector2 (w, h);
		pivot = new Vector2 (pivotX, pivotY);
		this.anchor = anchor;
	}

	public void LoadIntoPUGameObject(PUGameObject _parent, Action doChildren = null) {
		LoadIntoPUGameObject (_parent, -1, doChildren);
	}

	public void LoadIntoPUGameObject(PUGameObject _parent, int atIndex, Action doChildren = null)
	{
		// Sanity check to make sure this wasn't called multiple times
		if (gameObject == null) {
			gaxb_load (null, null, null);
			gaxb_init ();
			gaxb_final (null, _parent, null);
		}

		parent = _parent;

		if (_parent != null) {
			if (_parent is PUScrollRect) {
				gameObject.transform.SetParent ((_parent as PUScrollRect).contentObject.transform, false);
			} else {
				gameObject.transform.SetParent (_parent.gameObject.transform, false);
			}

			if (atIndex < 0 || atIndex >= _parent.children.Count) {
				_parent.children.Add (this);
			} else {
				_parent.children.Insert (atIndex, this);
				this.rectTransform.SetSiblingIndex (atIndex);
			}
		}

		if (doChildren != null) {
			doChildren();
		}

		gaxb_complete ();
		gaxb_private_complete ();
	}

	public void LoadIntoGameObject(GameObject _parent)
	{
		// Sanity check to make sure this wasn't called multiple times
		if (gameObject == null) {
			gaxb_load (null, null, null);
			gaxb_init ();
			gaxb_final (null, null, null);
		}

		parent = _parent;

		if (_parent != null) {
			gameObject.transform.SetParent (_parent.transform, false);
		}

		gaxb_complete ();
		gaxb_private_complete ();
	}

	public void SetStretchStretch(float x, float y) {
		if (rectTransform == null) {
			Debug.Log("SetStretchStretch() must be called after the LoadIntoPUGameObject");
			return;
		}
		rectTransform.sizeDelta = new Vector2 (x * -2.0f, y * -2.0f);
		rectTransform.anchoredPosition = new Vector2 (x, y);
	}
	
	public void SetStretchStretch(float top, float left, float bottom, float right) {
		if (rectTransform == null) {
			Debug.Log("SetStretchStretch() must be called after the LoadIntoPUGameObject");
			return;
		}
		rectTransform.sizeDelta = new Vector2 (-(left+right), -(top+bottom));
		rectTransform.anchoredPosition = new Vector2 (left, bottom);
	}

	public void CheckCanvasGroup () {
		if (canvasGroup == null && gameObject != null) {
			canvasGroup = gameObject.AddComponent<CanvasGroup> ();
		}
	}

	public void IgnoreMouse(bool i) {
		CheckCanvasGroup ();
		canvasGroup.blocksRaycasts = !i;
		canvasGroup.interactable = !i;
	}



	public void UnscheduleForUpdates() {
		GameObjectLateUpdateScript script1 = gameObject.GetComponent<GameObjectLateUpdateScript> ();
		if (script1 != null) {
			script1.enabled = false;
		}

		GameObjectUpdateScript script2 = gameObject.GetComponent<GameObjectUpdateScript> ();
		if (script2 != null) {
			script2.enabled = false;
		}
	}

	public void ScheduleForLateUpdate() {
		if (gameObject != null) {
			GameObjectLateUpdateScript script = gameObject.AddComponent<GameObjectLateUpdateScript> ();
			script.entity = this;
		}
	}

	public void ScheduleForUpdate() {
		if (gameObject != null) {
			GameObjectUpdateScript script = gameObject.AddComponent<GameObjectUpdateScript> ();
			script.entity = this;
		}
	}

	public void ScheduleForFixedUpdate() {
		if (gameObject != null) {
			GameObjectFixedUpdateScript script = gameObject.AddComponent<GameObjectFixedUpdateScript> ();
			script.entity = this;
		}
	}

	public void ScheduleForStart() {
		if (gameObject != null) {
			GameObjectStartScript script = gameObject.AddComponent<GameObjectStartScript> ();
			script.entity = this;
		}
	}

	public void ScheduleForOnLevelWasLoaded() {
		if (gameObject != null) {
			GameObjectOnLevelWasLoadedScript script = gameObject.AddComponent<GameObjectOnLevelWasLoadedScript> ();
			script.entity = this;
		}
	}

	public void ScheduleForOnEnable() {
		if (gameObject != null) {
			GameObjectOnEnableScript script = gameObject.AddComponent<GameObjectOnEnableScript> ();
			script.entity = this;
		}
	}

	public virtual void Start() {

	}

	public virtual void Update() {

	}

	public virtual void FixedUpdate() {

	}

	public virtual void LateUpdate() {

	}

	public virtual void OnLevelWasLoaded(int i) {

	}

	public virtual void OnEnable() {

	}

	public virtual void OnDisable() {

	}

	#region Performance Wrapped Setters For RectTransform
	public void RectTransformSafeSetLocalScale(Vector2 t){
		if (rectTransform != null && Vector2.Distance(rectTransform.localScale, t) > 0.001f) {
			rectTransform.localScale = t;
		}
	}

	public void RectTransformSafeSetPivot(Vector2 t){
		if (rectTransform != null && Vector2.Distance(rectTransform.pivot, t) > 0.001f) {
			rectTransform.pivot = t;
		}
	}

	public void RectTransformSafeSetAnchoredPosition(Vector2 t){
		if (rectTransform != null && Vector2.Distance(rectTransform.anchoredPosition, t) > 0.001f) {
			rectTransform.anchoredPosition = t;
		}
	}

	public void RectTransformSafeSetLocalEulerAngles(Vector3 t){
		if (rectTransform != null && Vector2.Distance(rectTransform.localEulerAngles, t) > 0.001f) {
			rectTransform.localEulerAngles = t;
		}
	}
	#endregion
}

public class GameObjectRemoveFromNotificationCenter : MonoBehaviour {
	void OnDestroy() {
		NotificationCenter.removeCompletely (this);
	}
}

public class GameObjectLateUpdateScript : MonoBehaviour {
	public PUGameObject entity;

	public void LateUpdate() {
		if (entity != null) {
			entity.LateUpdate ();
		}
	}
}

public class GameObjectUpdateScript : MonoBehaviour {
	public PUGameObject entity;

	public void Update() {
		if (entity != null) {
			UnityEngine.Profiling.Profiler.BeginSample((entity.title != null ? entity.title : entity.ToString()));
			entity.Update ();
			UnityEngine.Profiling.Profiler.EndSample();
		}
	}
}

public class GameObjectFixedUpdateScript : MonoBehaviour {
	public PUGameObject entity;

	public void FixedUpdate() {
		if (entity != null) {
			entity.FixedUpdate ();
		}
	}
}

public class GameObjectStartScript : MonoBehaviour {
	public PUGameObject entity;
	private bool shouldRecallStart = false;

	public void MarkForCallStart() {
		shouldRecallStart = true;
	}

	public void Update() {
		if (shouldRecallStart) {
			if (entity != null) {
				entity.Start ();
			}
			shouldRecallStart = false;
		}
	}

	public void Start() {
		if (entity != null) {
			entity.Start ();
		}
	}
}

public class GameObjectOnLevelWasLoadedScript : MonoBehaviour {
	public PUGameObject entity;

	public void OnLevelWasLoaded(int i) {
		if (entity != null) {
			entity.OnLevelWasLoaded (i);
		}
	}
}


public class GameObjectOnEnableScript : MonoBehaviour {
	public PUGameObject entity;

	public void OnEnable() {
		if (entity != null) {
			entity.OnEnable ();
		}
	}

	public void OnDisable() {
		if (entity != null) {
			entity.OnDisable ();
		}
	}
}