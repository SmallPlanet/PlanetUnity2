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
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using TB;

public interface IPUCode {

}

public interface IPUSingletonCode : IPUCode {
	void SingletonStart();
}

public partial class PUCode : PUCodeBase {

	MonoBehaviour controller;

	private static Hashtable singletonInstances = new Hashtable ();
	private static Hashtable normalInstances = new Hashtable ();

	private Dictionary<String,String> attributes = new Dictionary<String, String> ();

	public object GetObject()
	{
		return controller;
	}

	public void DetachObject()
	{
		controller = null;
	}

	private bool isSingleton()
	{
		return controller is IPUSingletonCode;
	}
		
	public override void unload(){

		// If we are a singleton, we need to not delete our gameobject...
		if (isSingleton()) {
			gameObject = null;
		}

		base.unload ();
	}

	public override void gaxb_load (TBXMLElement element, object _parent, Hashtable args) {
		base.gaxb_load (element, _parent, args);
		foreach (TBXMLAttribute attribute in element.attributes) {
			attributes [attribute.GetName(element.tbxml)] = attribute.GetValue(element.tbxml);
		}
	}

	public override void gaxb_unload()
	{
		base.gaxb_unload ();

		normalInstances.Remove (_class);

		if (isSingleton() == false) {
			NotificationCenter.removeObserver (controller);
		}
	}

	public override void gaxb_init()
	{
		controller = null;
		GC.Collect();
	}

	public static T GetSingletonByName<T>(){
		string name = typeof(T).FullName;
		T c = (T)singletonInstances [name];
		if (c != null) {
			return c;
		}
		return (T)normalInstances [name];
	}

	public override void gaxb_complete () {
		// If we're in live editor mode, we don't want to load controllers
		if (Application.isPlaying == false) {
			base.gaxb_complete ();
			return;
		}

		if (gameObject != null) {
			gameObject.name = _class;
		}

		// check if there is another singleton of the same class as us, if so we exit early and destroy ourself
		MonoBehaviour existingSingleton = (MonoBehaviour)singletonInstances [_class];
		if (existingSingleton != null) {
			if (!existingSingleton.Equals (this)) {
				GameObject.DestroyImmediate (this.gameObject);
				//Debug.LogFormat("existing singleton detected for {0}", _class);

				GameObject singletonGameObject = GameObject.Find (_class);
				if (singletonGameObject != null) {
					singletonGameObject.SendMessage ("MarkForCallStart");
				}

				if (_class != null) {
					AttachAllAttributes (attributes, existingSingleton.GetComponent (Type.GetType (_class, true)));
				}

				return;
			}
		}


		if (controller == null && _class != null) {
			// Attach all of the PlanetUnity objects
			try {
				controller = (MonoBehaviour)gameObject.AddComponent (Type.GetType (_class, true));

				AttachAllElements (controller, Scope ());
				AttachAllAttributes (attributes, controller);

				if (isSingleton ()) {
					//Debug.Log("Saving instance class for: "+_class);
					singletonInstances [_class] = controller;

					gameObject.transform.SetParent (null);
					UnityEngine.Object.DontDestroyOnLoad (gameObject);

					ScheduleForStart ();
				} else {
					normalInstances [_class] = controller;
				}
			} catch (Exception e) {
				UnityEngine.Debug.Log ("Controller error: " + e);
			}
		}

		if (controller != null) {
			try {
				// Attach all of the named GameObjects
				FieldInfo [] fields = controller.GetType ().GetFields ();
				foreach (FieldInfo field in fields) {
					if (field.FieldType == typeof (GameObject)) {

						GameObject [] pAllObjects = (GameObject [])Resources.FindObjectsOfTypeAll (typeof (GameObject));

						foreach (GameObject pObject in pAllObjects) {
							if (pObject.name.Equals (field.Name)) {
								field.SetValue (controller, pObject);
							}
						}
					}
				}
			} catch (Exception e) {
				UnityEngine.Debug.Log ("Controller error: " + e);
			}

			foreach (PUNotification subscribe in Notifications) {
				NotificationCenter.addObserver (controller, subscribe.name, Scope (), subscribe.name);
			}
		}

		base.gaxb_complete ();
	}

	public override void Start() {
		if (isSingleton()) {
			((IPUSingletonCode)controller).SingletonStart ();
		}
	}
	
	public static void AttachAllElements(object controller, PUObject scene) {
		if(scene != null)
		{
			FieldInfo field = controller.GetType ().GetField ("scene");
			if (field != null)
			{
				field.SetValue (controller, scene);
			}

			scene.PerformOnChildren(val =>
				{
					PUGameObject oo = val as PUGameObject;
					if(oo != null && oo.title != null)
					{
						field = controller.GetType ().GetField (oo.title);
						if (field != null)
						{
							try{
								field.SetValue (controller, oo);
							}catch(Exception e) {
								UnityEngine.Debug.Log ("Controller error: " + e);
							}
						}
					}
					return true;
				});
		}
	}

	public static void AttachAllAttributes (Dictionary<String, String> attributes, object controller) {
		foreach (string name in attributes.Keys) {
			FieldInfo field = controller.GetType ().GetField (name);
			if (field != null) {
				try {
					field.SetValue (controller, attributes[name]);
				} catch (Exception e) {
					UnityEngine.Debug.Log ("Controller error: " + e);
				}
			}
		}
	}

}
