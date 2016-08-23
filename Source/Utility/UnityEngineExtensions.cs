
using UnityEngine;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;

public static class GameObjectExtension
{
	public static void FillParentUI(this GameObject source)
	{
		RectTransform myTransform = (RectTransform)source.transform;

		myTransform.pivot = new Vector2(0.5f,0.5f);
		myTransform.anchorMin = Vector2.zero;
		myTransform.anchorMax = Vector2.one;
		myTransform.sizeDelta = Vector2.zero;
		myTransform.anchoredPosition = Vector2.zero;
	}
}

public static class RectTransformExtension
{
	public static float GetWidth(this RectTransform myTransform)
	{
		return myTransform.rect.width;
	}
	
	public static float GetHeight(this RectTransform myTransform)
	{
		return myTransform.rect.height;
	}
	
	public static float GetMinX(this RectTransform myTransform)
	{
		RectTransform parentTransform = myTransform.parent as RectTransform;
		
		if (parentTransform == null) {
			return 0;
		}
		
		return myTransform.anchoredPosition.x - myTransform.pivot.x * myTransform.GetWidth();
	}
	
	public static float GetMaxX(this RectTransform myTransform)
	{
		return myTransform.GetMinX () + myTransform.GetWidth ();
	}
	
	public static float GetMinY(this RectTransform myTransform)
	{
		RectTransform parentTransform = myTransform.parent as RectTransform;
		
		if (parentTransform == null) {
			return 0;
		}
		
		return myTransform.anchoredPosition.y - myTransform.pivot.y * myTransform.GetHeight();
	}
	
	public static float GetMaxY(this RectTransform myTransform)
	{
		return myTransform.GetMinY () + myTransform.GetHeight ();
	}
}