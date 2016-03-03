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

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Diagnostics;

public class NotificationObserver
{
	public string name;
	public string methodName;
	public object observer;
	public Action<Hashtable, string> block;

	public void callObserver(Hashtable args, string notificatioName)
	{
		if (methodName != null) {
			MethodInfo method = observer.GetType ().GetMethod (methodName);
			if (method != null) {
				if (method.GetParameters ().Length == 2) {
					method.Invoke (observer, new [] { (object)args, (object)notificatioName });
				} else if (method.GetParameters ().Length == 1) {
					method.Invoke (observer, new [] { args });
				} else {
					method.Invoke (observer, null);
				}

			} else {
				UnityEngine.Debug.Log ("Warning: NotificationCenter attempting to deliver notification, but object does not implement public method " + methodName);
			}
		} else if (block != null) {
			block (args, notificatioName);
		} 
	}
}

public class NotificationCenter
{
	private static Dictionary<object, List<NotificationObserver>> observersByScope = new Dictionary<object, List<NotificationObserver>> ();
	private static string globalScope = "GlobalScope";

	public static Hashtable Args(params object[] args){
		Hashtable hashTable = new Hashtable(args.Length/2);
		if (args.Length %2 != 0){
			return null;
		}else{
			int i = 0;
			while(i < args.Length - 1) {
				hashTable.Add(args[i], args[i+1]);
				i += 2;
			}
			return hashTable;
		}
	}

	private static void addObserverPrivate(object observer, string name, object scope, Action<Hashtable, string> block, string methodName)
	{
		if (observer == null || name == null) {
			UnityEngine.Debug.Log ("Warning: NotificationCenter.addObserver() called with null observer or name");
			return;
		}

		if (scope == null) {
			scope = globalScope;
		}

		if (name.StartsWith ("GLOBAL::", StringComparison.OrdinalIgnoreCase)) {
			scope = globalScope;
			name = name.Substring (8);
			if (methodName.StartsWith ("GLOBAL::", StringComparison.OrdinalIgnoreCase)) {
				methodName = methodName.Substring (8);
			}
		}

		NotificationObserver obv = new NotificationObserver ();
		obv.name = name;
		obv.block = block;
		obv.methodName = methodName;
		obv.observer = observer;

		List<NotificationObserver> list;
		if (!observersByScope.TryGetValue(scope, out list))
		{
			list = new List<NotificationObserver>();
			observersByScope.Add(scope, list);
		}
		list.Add(obv);
	}

	public static void addObserver(object observer, string name, object scope, Action<Hashtable, string> block)
	{
		addObserverPrivate (observer, name, scope, block, null);
	}

	public static void addObserver(object observer, string name, object scope, string methodName)
	{
		addObserverPrivate (observer, name, scope, null, methodName);
	}

	public static void addImmediateObserver(object observer, string name, object scope, Action<Hashtable, string> block) {
		removeObserver (observer, name);
		addObserverPrivate (observer, name, scope, block, null);
		block (new Hashtable(), name);
	}

	public static void postNotification(object scope, string name, Hashtable args)
	{
		// do not allow notifications from background threads
		if (PlanetUnityGameObject.IsMainThread () == false) {
			return;
		}

		if (name == null) {
			UnityEngine.Debug.Log ("Warning: NotificationCenter.postNotification() called with null notification name");
			return;
		}

		if (scope == null) {
			scope = globalScope;
		}

		if (name.StartsWith ("GLOBAL::", StringComparison.OrdinalIgnoreCase)) {
			scope = globalScope;
			name = name.Substring (8);
		}

		List<NotificationObserver> list;
		if (observersByScope.TryGetValue(scope, out list))
		{
			foreach (NotificationObserver o in new List<NotificationObserver>(list)) {
				if (o.name.Equals (name) || o.name.Equals("*")) {
					o.callObserver (args, name);
				}
			}
		}
	}

	public static void postNotification(object scope, string name)
	{
		postNotification (scope, name, null);
	}

	public static void removeObserver(object obv, string name)
	{
		foreach (List<NotificationObserver> list in observersByScope.Values) {
			list.RemoveAll(x => (x.observer == obv && x.name.Equals(name)));
		}
	}

	public static void removeObserver(object obv)
	{
		foreach (List<NotificationObserver> list in observersByScope.Values) {
			list.RemoveAll(x => x.observer == obv);
		}
	}

	public static void removeAllObservers()
	{
		observersByScope.Clear ();
	}
}
