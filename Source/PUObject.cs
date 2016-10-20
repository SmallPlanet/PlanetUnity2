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

using System.Reflection;
using System;
using System.Xml;
using System.Collections;
using TBSharpXML;

public partial class PUObject : PUObjectBase {

	public object UserData;
	public object UserData1;
	public object UserData2;

	public float UserFloat1;
	public float UserFloat2;
	public float UserFloat3;


	public override void gaxb_load(TBXMLElement element, object _parent, Hashtable args)
	{
		base.gaxb_load(element, _parent, args);
	}

	public override void gaxb_unload()
	{
		NotificationCenter.removeObserver (this);
	}

	public virtual void gaxb_complete()
	{

	}

	public virtual void gaxb_init()
	{

	}

	public virtual void gaxb_final(TBXMLElement element, object _parent, Hashtable args)
	{

	}

	public virtual void gaxb_private_complete() {

	}

	public T GetChildOfType<T>(){
		object child = null;
		this.PerformOnChildren (val => {
			if(val is T){
				child = val;
				return false;
			}
			return true;
		});
		return (T)child;
	}

	public T GetChildWithTitle<T>(string childTitle){
		object child = null;
		this.PerformOnChildren (val => {
			PUObject obj = (val as PUObject);
			if(childTitle.Equals(obj.title)){
				child = val;
				return false;
			}
			return true;
		});
		return (T)child;
	}

	public bool PerformOnChildren(Func<object, bool> block)
	{
		for (int i = children.Count - 1; i >= 0; i--) {
			object child = children[i];

			if (!block (child)) {
				return false;
			}

			MethodInfo method = child.GetType().GetMethod ("PerformOnChildren");
			if (method != null) {
				bool shouldContinue = Convert.ToBoolean(method.Invoke (child, new[] { block }));
				if (!shouldContinue) {
					return false;
				}
			}
		}

		return true;
	}

	public bool PerformOnChildrenForward(Func<object, bool> block)
	{
		for (int i = 0; i < children.Count; i++) {
			object child = children[i];

			if (!block (child)) {
				return false;
			}

			MethodInfo method = child.GetType().GetMethod ("PerformOnChildrenForward");
			if (method != null) {
				bool shouldContinue = Convert.ToBoolean(method.Invoke (child, new[] { block }));
				if (!shouldContinue) {
					return false;
				}
			}
		}

		return true;
	}

	public PUObject Scope()
	{
		if (IsScopeContainer ())
			return this;
		if (parent == null)
			return this;
		if ((parent is PUObject) == false)
			return this;
		return (parent as PUObject).Scope();
	}

	public virtual bool IsScopeContainer()
	{
		return false;
	}

}
