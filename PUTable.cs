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
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class PUTableUpdateScript : MonoBehaviour {

	public PUScrollRect table;

	public void LateUpdate() {
		if (table != null) {
			table.LateUpdate ();
		}
	}
}

public class PUTableHeaderScript : MonoBehaviour {

	public PUScrollRect table;
	public PUTableCell tableCell;
	private float originalY;

	RectTransform cellTransform;
	RectTransform nextHeaderTransform;
	RectTransform tableTransform;
	RectTransform tableContentTransform;

	public void Start()
	{
		if (cellTransform == null) {
			cellTransform = tableCell.puGameObject.rectTransform;
		}
		if (tableTransform == null) {
			tableTransform = table.rectTransform as RectTransform;
		}
		if (tableContentTransform == null) {
			tableContentTransform = table.contentObject.transform as RectTransform;
		}
		if (nextHeaderTransform == null) {
			if ((cellTransform.GetSiblingIndex () + 1) < tableContentTransform.childCount) {
				nextHeaderTransform = tableContentTransform.GetChild (cellTransform.GetSiblingIndex () + 1) as RectTransform;
				if (nextHeaderTransform.GetComponent<PUTableHeaderScript> () == null) {
					nextHeaderTransform = null;
				}
			}
		}

		tableCell.puGameObject.CheckCanvasGroup ();

		originalY = cellTransform.anchoredPosition.y;
	}

	public Vector2 DesiredAnchorPosition() {
		float bottomOfCell = (cellTransform.anchoredPosition.y) + tableContentTransform.anchoredPosition.y - (tableContentTransform.rect.height - tableTransform.rect.height);
		float topOfCell = bottomOfCell + cellTransform.rect.height;
		float tableViewHeight = tableTransform.rect.height;

		if (topOfCell > tableViewHeight) {
			Vector2 pos = cellTransform.anchoredPosition;
			pos.y = originalY + (tableViewHeight - topOfCell);
			return pos;
		} else if(cellTransform.anchoredPosition.y.Equals(originalY) == false) {
			Vector2 pos = cellTransform.anchoredPosition;
			pos.y = originalY;
			return pos;
		}

		return cellTransform.anchoredPosition;
	}

	public void LateUpdate()
	{
		cellTransform.anchoredPosition = DesiredAnchorPosition ();

		float bottomOfCell = (cellTransform.anchoredPosition.y) + tableContentTransform.anchoredPosition.y - (tableContentTransform.rect.height - tableTransform.rect.height);
		Vector2 otherPos = cellTransform.anchoredPosition;
		float distance = 99999.0f;

		// Get my sibling's top of cell, allow it to push me out of the way
		if (nextHeaderTransform != null) {
			PUTableHeaderScript otherScript = nextHeaderTransform.GetComponent<PUTableHeaderScript> ();
			otherPos = otherScript.DesiredAnchorPosition ();
			distance = cellTransform.anchoredPosition.y - otherPos.y;
		}


		if (distance < cellTransform.rect.height) {
			cellTransform.anchoredPosition = new Vector2 (otherPos.x, otherPos.y + cellTransform.rect.height);
			tableCell.puGameObject.canvasGroup.alpha = LeanTween.easeInCubic (0, 1, distance / cellTransform.rect.height);
		} else if (bottomOfCell < 0) {
			tableCell.puGameObject.canvasGroup.alpha = LeanTween.easeInCubic (0, 1, (bottomOfCell+cellTransform.rect.height) / cellTransform.rect.height);
		} else {
			if (tableCell.puGameObject.canvasGroup.alpha.Equals (1.0f) == false) {
				tableCell.puGameObject.canvasGroup.alpha = 1.0f;
			}
		}

	}
}

public class PUTableCell {

	public PUScrollRect scrollRect = null;
	public PUGameObject puGameObject = null;
	public object cellData = null;

	public float animatedYOffset = 0;


	protected GameObject cellGameObject;
	protected RectTransform cellTransform;
	protected RectTransform tableTransform;
	protected RectTransform tableContentTransform;

	public PUTable table {
		get {
			return scrollRect as PUTable;
		}
	}

	public virtual bool IsHeader() {
		// Subclasses override this method to specify this cell should act as a section header
		return false;
	}

	public virtual string XmlPath() {
		// Subclasses override this method to supply a path to a planet unity xml for this cell
		return null;
	}

	public virtual void LateUpdate() {
		// Subclasses can override to dynamically check their current size
	}

	public virtual void UpdateContents() {
		// Subclasses can override to dynamically check their current size
	}

	public static bool TestForVisibility(float cellPosY, float cellHeight, RectTransform tableTransform, RectTransform tableContentTransform) {

		float scrolledPos = (-cellPosY) - tableContentTransform.anchoredPosition.y;
		if (Mathf.Abs (scrolledPos - tableTransform.rect.height / 2) < (tableTransform.rect.height + cellHeight)) {
			return true;
		}
		return false;
	}

	public virtual bool TestForVisibility() {
		if (TestForVisibility (cellTransform.anchoredPosition.y, cellTransform.rect.height, tableTransform, tableContentTransform)) {
			return true;
		}
		return false;
	}

	public virtual void LoadIntoPUGameObject(PUScrollRect parent, object data) {

		scrollRect = parent;
		cellData = data;

		string xmlPath = XmlPath ();

		if (xmlPath != null) {

			puGameObject = (PUGameObject)PlanetUnity2.loadXML (PlanetUnityOverride.xmlFromPath (xmlPath), parent.contentObject, null);

			// Attach all of the PlanetUnity objects
			try {
				FieldInfo field = this.GetType ().GetField ("scene");
				if (field != null) {
					field.SetValue (this, puGameObject);
				}

				puGameObject.PerformOnChildren (val => {
					PUGameObject oo = val as PUGameObject;
					if (oo != null && oo.title != null) {
						field = this.GetType ().GetField (oo.title);
						if (field != null) {
							field.SetValue (this, oo);
						}
					}
					return true;
				});
			} catch (Exception e) {
				UnityEngine.Debug.Log ("TableCell error: " + e);
			}

			try {
				// Attach all of the named GameObjects
				FieldInfo[] fields = this.GetType ().GetFields ();
				foreach (FieldInfo field in fields) {
					if (field.FieldType == typeof(GameObject)) {

						GameObject[] pAllObjects = (GameObject[])Resources.FindObjectsOfTypeAll (typeof(GameObject));

						foreach (GameObject pObject in pAllObjects) {
							if (pObject.name.Equals (field.Name)) {
								field.SetValue (this, pObject);
							}
						}
					}
				}
			} catch (Exception e) {
				UnityEngine.Debug.Log ("TableCell error: " + e);
			}
		} else {
			puGameObject = new PUGameObject ();
			puGameObject.SetFrame (0, 0, 0, 60, 0, 0, "bottom,stretch");
			puGameObject.LoadIntoPUGameObject (parent);
			puGameObject.gameObject.transform.SetParent(parent.contentObject.transform, false);
		}

		if (IsHeader () && this is PUSimpleTableCell == false) {
			PUTableHeaderScript script = (PUTableHeaderScript)puGameObject.gameObject.AddComponent (typeof(PUTableHeaderScript));
			script.table = table;
			script.tableCell = this;
		}

		puGameObject.parent = table;

		// We want to bridge all notifications to my scope; this allows developers to handle notifications
		// at the table cell level, or at the scene controller level, with ease
		NotificationCenter.addObserver (this, "*", puGameObject, (args,name) => {
			NotificationCenter.postNotification(scrollRect.Scope(), name, args);
		});

		cellGameObject = puGameObject.gameObject;
		cellTransform = cellGameObject.transform as RectTransform;
		tableTransform = scrollRect.rectTransform;
		tableContentTransform = scrollRect.contentObject.transform as RectTransform;

		UpdateContents ();
	}

	public void unload() {
		NotificationCenter.removeObserver (this);
		puGameObject.unload ();
	}
}

public partial class PUTable : PUTableBase {

	public List<object> allObjects = null;
	public List<PUTableCell> allCells = new List<PUTableCell>();


	public void SetObjectList(List<object> objects) {
		allObjects = new List<object> (objects);
	}

	public PUTableCell TableCellForPUGameObject(PUGameObject baseObject){
		foreach(PUTableCell cell in allCells){
			if (cell.puGameObject.Equals (baseObject)) {
				return cell;
			}
		}
		return null;
	}

	public object ObjectForTableCell(PUTableCell cell){
		int idx = allCells.IndexOf (cell);
		return allObjects [(allObjects.Count-idx)-1];
	}

	public PUTableCell LoadCellForData(object cellData, PUTableCell reusedCell, float currentContentHeight) {
	
		PUTableCell cell = reusedCell;
		if (reusedCell == null) {
			string className = cellData.GetType ().Name + "TableCell";
			Type cellType = Type.GetType (className, true);

			cell = (Activator.CreateInstance (cellType)) as PUTableCell;
			cell.LoadIntoPUGameObject (this, cellData);
		} else {
			cell.cellData = cellData;
			cell.puGameObject.parent = this;
			cell.puGameObject.rectTransform.SetParent (this.contentObject.transform, false);

			cell.UpdateContents ();

			if (cell.IsHeader () == false) {
				//cell.animatedYOffset = cell.puGameObject.rectTransform.anchoredPosition.y - currentContentHeight;
			}
		}

		allCells.Add (cell);

		cell.puGameObject.rectTransform.anchoredPosition = new Vector3 (0, currentContentHeight, 0);
		cell.puGameObject.rectTransform.anchorMin = new Vector2 (0, 1);
		cell.puGameObject.rectTransform.anchorMax = new Vector2 (0, 1);
		cell.puGameObject.rectTransform.pivot = new Vector2 (0, 1);

		return cell;
	}

	public void ReloadTable(bool reuseCells = true) {
		RectTransform contentRectTransform = contentObject.transform as RectTransform;


		if(gameObject.GetComponent<PUTableUpdateScript>() == null){
			PUTableUpdateScript script = (PUTableUpdateScript)gameObject.AddComponent (typeof(PUTableUpdateScript));
			script.table = this;
		}

		// -2) Calculate the old height, so we can subtract it from the new height and adjust the scrolling appropriately
		float oldHeight = contentRectTransform.rect.height;


		// -1) Save previous cells for reuse if we can...
		List<PUTableCell> savedCells = new List<PUTableCell>(allCells);
		
		allCells.Clear ();

		if (allObjects == null || allObjects.Count == 0) {
			return;
		}

		// 0) Remove all of the cells from the list transform temporarily
		foreach (PUTableCell cell in savedCells) {
			cell.puGameObject.rectTransform.SetParent (null, false);
		}

		// 1) Run through allObjects; instantiate a cell object based on said object class
		float currentContentHeight = 0;
		for(int i = 0; i < allObjects.Count; i++) {
			object myCellData = allObjects [i];

			// Can we reuse an existing cell?
			PUTableCell savedCell = null;
			if (reuseCells) {
				foreach (PUTableCell otherCell in savedCells) {
					if (otherCell.IsHeader() == false && otherCell.cellData.Equals (myCellData)) {
						savedCell = otherCell;
						savedCells.Remove (otherCell);
						break;
					}
				}
			}

			PUTableCell newCell = LoadCellForData(myCellData, savedCell, currentContentHeight);

			currentContentHeight += newCell.puGameObject.rectTransform.rect.height;
		}

		for(int i = 0; i < allObjects.Count; i++) {
			PUTableCell cell = allCells [i];
			if (cell.IsHeader ()) {
				// TODO: Move me to the end of the stuff
				cell.puGameObject.parent = this;
				cell.puGameObject.gameObject.transform.SetParent (cell.puGameObject.gameObject.transform.parent, false);

				PUTableHeaderScript headerScript = cell.puGameObject.rectTransform.GetComponent<PUTableHeaderScript> ();
				headerScript.Start ();
			}
		}
			
		// This will layout our cells
		LateUpdate ();

		// 3) offset the scroll based upon the change in table height
		float newHeight = contentRectTransform.rect.height;

		if (oldHeight > 1) {
			Vector2 scroll = contentRectTransform.anchoredPosition;
			scroll.y += newHeight - oldHeight;
			contentRectTransform.anchoredPosition = scroll;
		}

		// 2) Remove all previous content which have not reused
		foreach (PUTableCell cell in savedCells) {
			cell.unload ();
		}
	}

	public override void LateUpdate() {
		RectTransform contentRectTransform = contentObject.transform as RectTransform;

		float y = 0;
		float nextY = 0;
		float x = 0;

		foreach (PUTableCell cell in allCells) {
			cell.LateUpdate ();

			// Can I fit on the current line?
			if(	x + cell.puGameObject.rectTransform.rect.width > (contentRectTransform.rect.width+1) ||
			   	cell.IsHeader()){
				x = 0;
				y = nextY;
			}

			//cell.animatedYOffset += (0.0f - cell.animatedYOffset) * 0.12346f;
			cell.puGameObject.rectTransform.anchoredPosition = new Vector2 (x, y);


			x += cell.puGameObject.rectTransform.rect.width;

			float ny = y - cell.puGameObject.rectTransform.rect.height;
			if (ny < nextY) {
				nextY = ny;
			}

			cell.puGameObject.gameObject.SetActive (cell.TestForVisibility ());
		}

		contentRectTransform.sizeDelta = new Vector2 (rectTransform.rect.width, Mathf.Abs (nextY));
	}

	public override void gaxb_complete() {

		base.gaxb_complete ();

		NotificationCenter.addObserver (this, "OnAspectChanged", null, (args, name) => {
			ReloadTable(false);
		});

	}

	public override void unload() {

		foreach (PUTableCell cell in allCells) {
			cell.unload ();
		}
		base.unload ();
	}

}
