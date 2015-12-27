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

public interface IPUCode {

}

public interface IPUSingletonCode : IPUCode {
	void SingletonStart();
}

public partial class PUCode : PUCodeBase {

	IPUCode controller;

	private static Hashtable instances = new Hashtable ();
	private static Hashtable normalInstances = new Hashtable ();

	public object GetObject()
	{
		return controller;
	}

	public void DetachObject()
	{
		controller = null;
	}
		
	public override void unload(){

		// If we are a singleton, we need to not delete our gameobject...
		if (singleton) {
			gameObject = null;
		}

		base.unload ();
	}

	public override void gaxb_unload()
	{
		base.gaxb_unload ();

		normalInstances.Remove (_class);

		if (singleton == false) {
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
		T c = (T)instances [name];
		if (c != null) {
			return c;
		}
		return (T)normalInstances [name];
	}

	public override void gaxb_complete()
	{
		ScheduleForStart ();

		bool shouldCallSingletonStart = false;
		// If we're in live editor mode, we don't want to load controllers
		if (Application.isPlaying == false) {
			base.gaxb_complete ();
			return;
		}

		if (gameObject != null) {
			gameObject.name = _class;
		}

		if (singleton) {
			if (instances [_class] != null && instances [_class] != this) {
				GameObject.DestroyImmediate (this.gameObject);
				controller = (IPUCode)instances[_class];
				shouldCallSingletonStart = true;
			} else {
				MonoBehaviour.DontDestroyOnLoad(this.gameObject);
				this.gameObject.transform.SetParent (null);
			}
		}


		if (controller == null && _class != null) {
			// Attach all of the PlanetUnity objects
			try {
				controller = (IPUCode)gameObject.AddComponent(Type.GetType (_class, true));

				PUGameObject scene = Scope() as PUGameObject;
				if(scene != null)
				{
					FieldInfo field = controller.GetType ().GetField ("scene");
					if (field != null)
					{
						field.SetValue (controller, scene);
					}

					field = controller.GetType ().GetField ("puGameObject");
					if (field != null)
					{
						field.SetValue (controller, this);
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

				if(singleton){
					Debug.Log("Saving instance class for: "+_class);
					instances[_class] = controller;
				}else{
					normalInstances[_class] = controller;
				}
			}
			catch(Exception e) {
				UnityEngine.Debug.Log ("Controller error: " + e);
			}
		}

		if (controller != null) {
			try {
				// Attach all of the named GameObjects
				FieldInfo[] fields = controller.GetType ().GetFields ();
				foreach (FieldInfo field in fields) {
					if (field.FieldType == typeof(GameObject)) {

						GameObject[] pAllObjects = (GameObject[])Resources.FindObjectsOfTypeAll (typeof(GameObject));

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

		if (shouldCallSingletonStart) {
			GameObject singletonGameObject = GameObject.Find (_class);
			if (singletonGameObject != null) {
				singletonGameObject.SendMessage ("MarkForCallStart");
			}
		}

		base.gaxb_complete ();
	}

	public override void Start() {
		if (controller is IPUSingletonCode) {
			((IPUSingletonCode)controller).SingletonStart ();
		}
	}

}
