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
using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public delegate void Task();

public class PlanetUnityOverride {

	private static Mathos.Parser.MathParser mathParser = new Mathos.Parser.MathParser();
	public static float canvasTop;
	public static float canvasBottom;
	public static float canvasLeft;
	public static float canvasRight;
	public static float canvasScale;

	public static int puCameraLayer = 30;
	public static int puEventLayer = 29;

	public static int minFPS = 10;
	public static int maxFPS = 60;

	public static Func<string, string> xmlFromPath = (path) => {
		return PlanetUnityResourceCache.GetTextFile(path);
	};

	public static Func<PUCanvas, bool> orientationDidChange = (canvas) => {
		PlanetUnityGameObject.currentGameObject.ReloadCanvas();
		return true;
	};

	public static string processString(object o, string s)
	{
		if (s == null)
			return null;

		s.Replace("@LANGUAGE", PlanetUnityLanguage.LanguageCode());

		if (s.StartsWith ("@eval(")) {

			string evalListString = s.Substring(6, s.Length-7);

			var parts = evalListString.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
			string[] results = new string[12];
			int nresults = 0;

			RectTransform rectTransform = null;

			mathParser.LocalVariables.Clear ();

			if (o is GameObject) {
				rectTransform = (o as GameObject).GetComponent<RectTransform> ();

				mathParser.LocalVariables.Add ("lastY", Convert.ToDecimal(0));
				mathParser.LocalVariables.Add ("lastX", Convert.ToDecimal(0));
			}
			else if (o is PUGameObject) {
				PUGameObject entity = (PUGameObject)o;
				rectTransform = entity.gameObject.GetComponent<RectTransform> ();

				mathParser.LocalVariables.Add ("lastY", Convert.ToDecimal(entity.lastY));
				mathParser.LocalVariables.Add ("lastX", Convert.ToDecimal(entity.lastX));
			}

			if (rectTransform) {
				mathParser.LocalVariables.Add ("scaledW", Convert.ToDecimal(rectTransform.sizeDelta.x));
				mathParser.LocalVariables.Add ("scaledH", Convert.ToDecimal(rectTransform.sizeDelta.y));

				mathParser.LocalVariables.Add ("w", Convert.ToDecimal(rectTransform.sizeDelta.x));
				mathParser.LocalVariables.Add ("h", Convert.ToDecimal(rectTransform.sizeDelta.y));
			}

			foreach (string part in parts) {
				results [nresults] = mathParser.Parse (part).ToString ();
				nresults++;
			}

			if(nresults == 4 && o is PUGameObject)
			{
				PUGameObject entity = (PUGameObject)o;
				entity.lastY = float.Parse (results [1]) + float.Parse (results [3]);
				entity.lastX = float.Parse (results [0]) + float.Parse (results [2]);
			}

			return string.Join (",", results, 0, nresults);

		} else if(s.StartsWith("@")) {

			string localizedString = PlanetUnityLanguage.Translate(s);
			if(localizedString.Equals(s) == false)
			{
				return localizedString;
			}
		}

		return s;
	}

}

public class PlanetUnityGameObject : MonoBehaviour {

	public static float desiredFPS;
	public static void RequestFPS(float f) {
		// Called by entities to request a specific fps. PlanetUnity will set the fps dynamically
		// to the highest requested fps.
		if (f > desiredFPS) {
			desiredFPS = f;
		}
	}

	public string xmlPath;
	public bool editorPreview = true;

	private GameObject planetUnityContainer;
	private PUCanvas canvas;

	private bool shouldReloadMainXML = false;

	static public PlanetUnityGameObject currentGameObject = null;

	static public Vector2 MousePosition() {
		Vector2 pos = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);

		if (currentGameObject == null || currentGameObject.canvas == null)
			return pos;

		pos.x /= Screen.width;
		pos.y /= Screen.height;

		pos.x *= PlanetUnityOverride.canvasRight - PlanetUnityOverride.canvasLeft;
		pos.y *= PlanetUnityOverride.canvasBottom - PlanetUnityOverride.canvasTop;

		pos.x += PlanetUnityOverride.canvasLeft;
		pos.y += PlanetUnityOverride.canvasTop;

		return pos;
	}

	// Use this for initialization
	void Start () {

		currentGameObject = this;

		ReloadCanvas ();

		#if UNITY_EDITOR
		NotificationCenter.addObserver(this, PlanetUnity2.EDITORFILEDIDCHANGE, null, (args,name) => {
			string assetPath = args ["path"].ToString();
			if( assetPath.Contains(xmlPath+".xml") ||
				assetPath.EndsWith(".strings"))
			{
				PlanetUnityLanguage.ReloadAllLanguages();
				ReloadCanvas ();
			}
		});
		#endif
	}

	void OnDestroy () {

		NotificationCenter.removeObserver (this);

		#if !UNITY_EDITOR
		RemoveCanvas ();
		#else
		EditorReloadCanvas ();
		#endif
	}

	void Update () {

		if (shouldReloadMainXML) {
			shouldReloadMainXML = false;
			#if UNITY_EDITOR
			ReloadCanvas ();
			#endif
		}

		lock (_queueLock)
		{
			if (TaskQueue.Count > 0)
				TaskQueue.Dequeue()();
		}
	}

	public void RemoveCanvas () {
		if (canvas != null) {
			canvas.PerformOnChildren (val => {
				MethodInfo method = val.GetType ().GetMethod ("gaxb_unload");
				if (method != null) {
					method.Invoke (val, null);
				}
				return true;
			});
				
			canvas.gaxb_unload ();

			DestroyImmediate (canvas.gameObject);
			canvas = null;
		}

		SafeRemoveAllChildren ();
	}

	public PUCanvas Canvas() {
		return canvas;
	}

	public void LoadCanvasXML (string xml) {

		if (xmlPath == null || PlanetUnityOverride.xmlFromPath (xmlPath) == null) {
			return;
		}

		RemoveCanvas ();

		Stopwatch sw = Stopwatch.StartNew ();

		planetUnityContainer = GameObject.Find ("PlanetUnityContainer");
		if (planetUnityContainer == null) {
			planetUnityContainer = new GameObject ("PlanetUnityContainer");
		}
			
		canvas = (PUCanvas)PlanetUnity2.loadXML (xml, planetUnityContainer, null);

		#if UNITY_EDITOR
		foreach (Transform t in planetUnityContainer.GetComponentsInChildren<Transform>()) {
			t.gameObject.hideFlags = HideFlags.DontSave;
		}
		#endif

		sw.Stop ();

		#if !UNITY_EDITOR
		UnityEngine.Debug.Log ("[" + sw.Elapsed.TotalMilliseconds + "ms] Loading canvas " + xmlPath + ".xml");
		#endif

		//Profile.PrintResults ();
		//Profile.Reset ();
	}

	public void ReloadCanvas () {

		LoadCanvasXML (PlanetUnityOverride.xmlFromPath (xmlPath));
	}

	public void SafeRemoveAllChildren() {
		// This gets hokey, but the editor complains if the components are not removed in a specific order
		// before the game object itself is destroyed...
		planetUnityContainer = GameObject.Find ("PlanetUnityContainer");
		if (planetUnityContainer != null) {
			for (int i = planetUnityContainer.transform.childCount - 1; i >= 0; i--) {
				Transform canvasObject = planetUnityContainer.transform.GetChild (i);

				// Remove all components...
				DestroyImmediate (canvasObject.GetComponent<GraphicRaycaster> ());
				DestroyImmediate (canvasObject.GetComponent<ReferenceResolution> ());
				DestroyImmediate (canvasObject.GetComponent<Canvas> ());

				DestroyImmediate (canvasObject.gameObject);
			}

			DestroyImmediate (planetUnityContainer);
		}
	}

	public void EditorReloadCanvas () {
	
		SafeRemoveAllChildren ();

		if (editorPreview) {
			ReloadCanvas ();
		}
	}

	private Queue<Task> TaskQueue = new Queue<Task>();
	private object _queueLock = new object();

	public void PrivateScheduleTask(Task newTask) {
		lock (_queueLock)
		{
			if (TaskQueue.Count < 100) {
				TaskQueue.Enqueue (newTask);
			}
		}
	}

	public bool PrivateHasTasks()
	{
		return (TaskQueue.Count > 0);
	}

	public static void ScheduleTask(Task newTask)
	{
		if (System.Object.ReferenceEquals(currentGameObject, null)) {
			return;
		}
		currentGameObject.PrivateScheduleTask(newTask);
	}

	public static bool HasTasks()
	{
		if (System.Object.ReferenceEquals(currentGameObject, null)) {
			return false;
		}
		return currentGameObject.PrivateHasTasks ();
	}
}

#if UNITY_EDITOR

[InitializeOnLoad]
public class Autorun
{
	static Autorun()
	{
		EditorApplication.update += RunOnce;
	}

	static void RunOnce()
	{
		// If we're the editor, and we're in edit mode, and live preview is set...
		GameObject puObject = GameObject.Find ("PlanetUnity");
		if (puObject == null)
			return;
		PlanetUnityGameObject script = puObject.GetComponent<PlanetUnityGameObject> ();
		if (script == null)
			return;

		if (Application.isPlaying == false) {
			script.EditorReloadCanvas ();
		}

		EditorApplication.update -= RunOnce;
	}
}

[ExecuteInEditMode]
public class CustomPostprocessor : AssetPostprocessor
{
	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
	{
		foreach(string asset in importedAssets)
		{
			NotificationCenter.postNotification(null, PlanetUnity2.EDITORFILEDIDCHANGE, NotificationCenter.Args("path", asset));
		}

		// If we're the edit, and we're in edit mode, and live preview is set...
		GameObject puObject = GameObject.Find ("PlanetUnity");
		if (puObject == null)
			return;
		PlanetUnityGameObject script = puObject.GetComponent<PlanetUnityGameObject> ();
		if (script == null)
			return;

		script.EditorReloadCanvas ();
	}
}
	
#endif

