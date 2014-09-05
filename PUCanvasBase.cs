

using UnityEngine;


//
// Autogenerated by gaxb ( https://github.com/SmallPlanet/gaxb )
//

using System;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;


public partial class PUCanvas : PUCanvasBase {
	
	public PUCanvas()
	{
		string attr;

		attr = "Overlay";
		if(attr != null) { renderMode = (PlanetUnity2.CanvasRenderMode)System.Enum.Parse(typeof(PlanetUnity2.CanvasRenderMode), attr); renderModeExists = true; } 
		attr = "false";
		if(attr != null) { pixelPerfect = bool.Parse(attr); pixelPerfectExists = true; } 

	}
	
	
	public PUCanvas(
			PlanetUnity2.CanvasRenderMode renderMode,
			bool pixelPerfect ) : this()
	{
		this.renderMode = renderMode;
		this.renderModeExists = true;

		this.pixelPerfect = pixelPerfect;
		this.pixelPerfectExists = true;
	}

	
	
	public PUCanvas(
			PlanetUnity2.CanvasRenderMode renderMode,
			bool pixelPerfect,
			Vector3 position,
			Vector2 size,
			Vector2 pivot,
			Vector3 rotation,
			Vector3 scale,
			bool hidden,
			float lastY,
			float lastX,
			string title,
			string tag,
			string tag1,
			string tag2,
			string tag3,
			string tag4,
			string tag5,
			string tag6 ) : this()
	{
		this.renderMode = renderMode;
		this.renderModeExists = true;

		this.pixelPerfect = pixelPerfect;
		this.pixelPerfectExists = true;

		this.position = position;
		this.positionExists = true;

		this.size = size;
		this.sizeExists = true;

		this.pivot = pivot;
		this.pivotExists = true;

		this.rotation = rotation;
		this.rotationExists = true;

		this.scale = scale;
		this.scaleExists = true;

		this.hidden = hidden;
		this.hiddenExists = true;

		this.lastY = lastY;
		this.lastYExists = true;

		this.lastX = lastX;
		this.lastXExists = true;

		this.title = title;
		this.titleExists = true;

		this.tag = tag;
		this.tagExists = true;

		this.tag1 = tag1;
		this.tag1Exists = true;

		this.tag2 = tag2;
		this.tag2Exists = true;

		this.tag3 = tag3;
		this.tag3Exists = true;

		this.tag4 = tag4;
		this.tag4Exists = true;

		this.tag5 = tag5;
		this.tag5Exists = true;

		this.tag6 = tag6;
		this.tag6Exists = true;
	}


}




public class PUCanvasBase : PUGameObject {


	private static Type planetOverride = Type.GetType("PlanetUnityOverride");
	private static MethodInfo processStringMethod = planetOverride.GetMethod("processString", BindingFlags.Public | BindingFlags.Static);




	// XML Attributes
	public PlanetUnity2.CanvasRenderMode renderMode;
	public bool renderModeExists;

	public bool pixelPerfect;
	public bool pixelPerfectExists;




	
	public void SetRenderMode(PlanetUnity2.CanvasRenderMode v) { renderMode = v; renderModeExists = true; } 
	public void SetPixelPerfect(bool v) { pixelPerfect = v; pixelPerfectExists = true; } 


	public override void gaxb_unload()
	{
		base.gaxb_unload();

	}
	
	public void gaxb_addToParent()
	{
		if(parent != null)
		{
			FieldInfo parentField = parent.GetType().GetField("Canvas");
			List<object> parentChildren = null;
			
			if(parentField != null)
			{
				parentField.SetValue(parent, this);
				
				parentField = parent.GetType().GetField("CanvasExists");
				parentField.SetValue(parent, true);
			}
			else
			{
				parentField = parent.GetType().GetField("Canvass");
				
				if(parentField != null)
				{
					parentChildren = (List<object>)(parentField.GetValue(parent));
				}
				else
				{
					parentField = parent.GetType().GetField("GameObjects");
					if(parentField != null)
					{
						parentChildren = (List<object>)(parentField.GetValue(parent));
					}
				}
				if(parentChildren == null)
				{
					FieldInfo childrenField = parent.GetType().GetField("children");
					if(childrenField != null)
					{
						parentChildren = (List<object>)childrenField.GetValue(parent);
					}
				}
				if(parentChildren != null)
				{
					parentChildren.Add(this);
				}
				
			}
		}
	}

	public override void gaxb_load(XmlReader reader, object _parent, Hashtable args)
	{
		base.gaxb_load(reader, _parent, args);

		if(reader == null && _parent == null)
			return;
		
		parent = _parent;
		
		if(this.GetType() == typeof( PUCanvas ))
		{
			gaxb_addToParent();
		}
		
		xmlns = reader.GetAttribute("xmlns");
		

		string attr;
		attr = reader.GetAttribute("renderMode");
		if(attr != null && planetOverride != null) { attr = processStringMethod.Invoke(null, new [] {_parent, attr}).ToString(); }
		if(attr == null) { attr = "Overlay"; }
		if(attr != null) { renderMode = (PlanetUnity2.CanvasRenderMode)System.Enum.Parse(typeof(PlanetUnity2.CanvasRenderMode), attr); renderModeExists = true; } 
		
		attr = reader.GetAttribute("pixelPerfect");
		if(attr != null && planetOverride != null) { attr = processStringMethod.Invoke(null, new [] {_parent, attr}).ToString(); }
		if(attr == null) { attr = "false"; }
		if(attr != null) { pixelPerfect = bool.Parse(attr); pixelPerfectExists = true; } 
		

	}
	
	
	
	
	
	
	
	public override void gaxb_appendXMLAttributes(StringBuilder sb)
	{
		base.gaxb_appendXMLAttributes(sb);

		if(renderModeExists) { sb.AppendFormat (" {0}=\"{1}\"", "renderMode", (int)renderMode); }
		if(pixelPerfectExists) { sb.AppendFormat (" {0}=\"{1}\"", "pixelPerfect", pixelPerfect.ToString().ToLower()); }

	}
	
	public override void gaxb_appendXMLSequences(StringBuilder sb)
	{
		base.gaxb_appendXMLSequences(sb);


	}
	
	public override void gaxb_appendXML(StringBuilder sb)
	{
		if(sb.Length == 0)
		{
			sb.AppendFormat ("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
		}
		
		sb.AppendFormat ("<{0}", "Canvas");
		
		if(xmlns != null)
		{
			sb.AppendFormat (" {0}=\"{1}\"", "xmlns", xmlns);
		}
		
		gaxb_appendXMLAttributes(sb);
		
		
		StringBuilder seq = new StringBuilder();
		seq.AppendFormat(" ");
		gaxb_appendXMLSequences(seq);
		
		if(seq.Length == 1)
		{
			sb.AppendFormat (" />");
		}
		else
		{
			sb.AppendFormat (">{0}</{1}>", seq.ToString(), "Canvas");
		}
	}
}
