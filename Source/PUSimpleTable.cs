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
using System.Collections;
using UnityEngine.EventSystems;
using Mathos.Parser;


public class PUSimpleTableCellEditHandle : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler {

	public PUSimpleTable table;
	public PUSimpleTableCell cell;
	
	public void OnDrag (PointerEventData eventData) {
		Vector2 dragLocation;
		RectTransformUtility.ScreenPointToLocalPointInRectangle (cell.puGameObject.rectTransform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out dragLocation);

		dragLocation.x = 0;
		dragLocation.y += table.cellSize.Value.y * 0.5f;

		(cell.puGameObject.rectTransform as RectTransform).anchoredPosition = dragLocation;

		dragLocation.y -= table.cellSize.Value.y * 0.5f;

		int newIdx = table.RowIndexForPosition (dragLocation);
		int oldIdx = table.ObjectIndexOfCell (cell);
		int maxIdx = table.MaxObjectIndex ();

		// Find out current index
		if (newIdx != oldIdx && newIdx >= 0 && newIdx < maxIdx) {

			// First, remove the object from the data list
			object movedItem = null;
			int idx = 0;
			foreach (List<object> subtableObjects in table.allSegmentedObjects) {
				foreach (object item in subtableObjects.ToArray()) {
					if (idx == oldIdx) {
						movedItem = item;
						subtableObjects.Remove (item);
						Debug.Log ("removed from rowIdx: " + oldIdx);
						break;
					}
					idx++;
				}
			}

			// Next, insert the object into its new position
			if (movedItem != null) {
				if (newIdx == maxIdx - 1) {
					table.allSegmentedObjects [table.allSegmentedObjects.Count - 1].Add (movedItem);
				} else {
					idx = 0;
					foreach (List<object> subtableObjects in table.allSegmentedObjects) {
						foreach (object item in subtableObjects) {
							if (idx == newIdx) {
								subtableObjects.Insert (subtableObjects.IndexOf (item), movedItem);
								Debug.Log ("added to rowIdx: " + newIdx);
								break;
							}
							idx++;
						}
					}
				}
			}

		}
	}

	public void OnEndDrag (PointerEventData eventData) {
		table.EndEdit ();
	}

	public void OnBeginDrag (PointerEventData eventData) {
		table.BeginEditWithCell(cell);
	}
}


public class PUSimpleTableCell : PUTableCell {

	public bool isEdit;

	public PUSimpleTable simpleTable {
		get {
			return scrollRect as PUSimpleTable;
		}
	}

	public static Vector2 CellSize(RectTransform tableTransform) {
		return new Vector2 (tableTransform.rect.width, 60.0f);
	}

	public override void LoadIntoPUGameObject(PUScrollRect parent, object data) {
		base.LoadIntoPUGameObject(parent, data);

		if (puGameObject.rectTransform.rect.width == 0) {
			if (IsHeader ()) {
				puGameObject.rectTransform.sizeDelta = simpleTable.headerSize.Value;
			} else {
				puGameObject.rectTransform.sizeDelta = simpleTable.cellSize.Value;
			}
		}
	}
}


public partial class PUSimpleTable : PUSimpleTableBase {

	public PUGameObject TableFooter;
	public PUGameObject TableHeader;

	public Action OnEndEdit;
	public Action OnPullToRefresh;

	int currentScrollY = -1;
	int currentScrollHeight = -1;

	public List<List<object>> allSegmentedObjects = null;

	public void SetObjectList(List<object> objects) {
		allSegmentedObjects = new List<List<object>> ();
		allSegmentedObjects.Add(objects);
	}

	public void SetSegmentedObjectList(List<List<object>> objects) {
		allSegmentedObjects = objects;
	}

	public PUSimpleTableCell LoadCellForData(object cellData, PUSimpleTableCell reusedCell) {

		PUSimpleTableCell cell = reusedCell;
		if (reusedCell == null) {
			string className = cellData.GetType ().Name + "TableCell";
			Type cellType = Type.GetType (className, true);

			cell = (Activator.CreateInstance (cellType)) as PUSimpleTableCell;
			cell.LoadIntoPUGameObject (this, cellData);
		} else {
			cell.cellData = cellData;
			cell.puGameObject.parent = this;
			cell.puGameObject.rectTransform.SetParent (this.contentObject.transform, false);

			cell.UpdateContents ();
		}

		if (cell.IsHeader ()) {
			cell.puGameObject.rectTransform.sizeDelta = new Vector2(cell.puGameObject.rectTransform.sizeDelta.x, headerSize.Value.y);
		} else {
			cell.puGameObject.rectTransform.sizeDelta = new Vector2(cell.puGameObject.rectTransform.sizeDelta.x, cellSize.Value.y);
		}
		cell.puGameObject.rectTransform.anchorMin = new Vector2 (0, 1);
		cell.puGameObject.rectTransform.anchorMax = new Vector2 (0, 1);
		cell.puGameObject.rectTransform.pivot = new Vector2 (0, 1);

		return cell;
	}


	public void ClearTable() {
		for(int i = activeTableCells.Count-1; i >= 0; i--) {
			PUSimpleTableCell cell = activeTableCells [i];
			EnqueueTableCell (cell);
		}
	}

	public void EmptyTable() {
		// TODO: cancel any background loads
		//if (tableUpdateScript != null)
		//	tableUpdateScript.StopReloadTableCells();
		
		// When the table size changes, we need to not reuse table cells...
		for(int i = activeTableCells.Count-1; i >= 0; i--) {
			PUSimpleTableCell cell = activeTableCells [i];
			cell.unload();
			activeTableCells.RemoveAt(i);
		}
		
		foreach (string key in pooledTableCells.Keys) {
			foreach (PUSimpleTableCell cell in pooledTableCells[key]) {
				cell.unload ();
			}
		}
		pooledTableCells.Clear();
	}


	private List<PUSimpleTableCell> activeTableCells = new List<PUSimpleTableCell>();
	private Dictionary<string, List<PUSimpleTableCell>> pooledTableCells = new Dictionary<string, List<PUSimpleTableCell>>();

	public List<PUSimpleTableCell> allCells {
		get {
			return activeTableCells;
		}
	}

	public PUSimpleTableCell DequeueTableCell(object cellData){
		string dataKey = cellData.GetType ().Name;
		if (pooledTableCells.ContainsKey (dataKey) == false) {
			pooledTableCells [dataKey] = new List<PUSimpleTableCell> ();
		}

		PUSimpleTableCell cell = null;
		if (pooledTableCells [dataKey].Count > 0) {
			cell = pooledTableCells[dataKey][0];
			pooledTableCells [dataKey].RemoveAt (0);
		}

		cell = LoadCellForData(cellData, cell);
		cell.puGameObject.gameObject.SetActive (true);

		activeTableCells.Add (cell);

		return cell;
	}

	public void EnqueueTableCell(PUSimpleTableCell cell){
		string dataKey = cell.cellData.GetType ().Name;
		if (pooledTableCells.ContainsKey (dataKey) == false) {
			pooledTableCells [dataKey] = new List<PUSimpleTableCell> ();
		}
		pooledTableCells [dataKey].Add (cell);
		cell.puGameObject.gameObject.SetActive (false);

		activeTableCells.Remove (cell);
	}



	private int totalCellsChecked = 0;

	public void ReloadTable() {

		if (rectTransform == null) {
			return;
		}

		RectTransform contentRectTransform = contentObject.transform as RectTransform;
		contentRectTransform.sizeDelta = new Vector2(rectTransform.rect.width, 0 + _ContentOffset.y);
		//if (TableHeader != null) {
		//contentRectTransform.sizeDelta += new Vector2(0, TableHeader.rectTransform.rect.height);
		//}
		totalCellsChecked = 0;

		// Unload any cells which are not on the screen currently; store the object data for cells which are
		// still visible
		Dictionary<object,PUSimpleTableCell> visibleCells = new Dictionary<object, PUSimpleTableCell>();
		for(int i = activeTableCells.Count-1; i >= 0; i--) {
			PUSimpleTableCell cell = activeTableCells [i];
			if (cell.TestForVisibility () == false) {
				EnqueueTableCell (cell);
			} else {
				visibleCells [cell.cellData] = cell;
			}
		}


        if (TableHeader != null && TableHeader.rectTransform != null && contentObject != null) {
			TableHeader.rectTransform.SetParent(contentObject.transform, false);
			TableHeader.rectTransform.anchoredPosition = new Vector2(0,0);

			contentRectTransform.sizeDelta += new Vector2(0, TableHeader.rectTransform.rect.height);
		}

		if (allSegmentedObjects != null) {
			foreach (List<object> subtableObjects in allSegmentedObjects) {
				ReloadSubtable (subtableObjects, visibleCells);
			}
		}

		foreach (PUSimpleTableCell leftOverCell in visibleCells.Values) {
			EnqueueTableCell (leftOverCell);
		}

		if (contentRectTransform.sizeDelta.y == 0) {
			contentRectTransform.sizeDelta = new Vector2 (rectTransform.rect.width, rectTransform.rect.height + _ContentOffset.y);
		}
		//Debug.Log (totalCellsChecked + " **************");

		if (TableFooter != null) {
			TableFooter.rectTransform.SetParent(contentObject.transform, false);
			TableFooter.rectTransform.anchoredPosition = new Vector2(0,-contentRectTransform.sizeDelta.y);

			contentRectTransform.sizeDelta += new Vector2(0, TableFooter.rectTransform.rect.height);
		}
	}

	public void ReloadSubtable(List<object> subtableObjects, Dictionary<object,PUSimpleTableCell> visibleCells) {

		RectTransform contentRectTransform = contentObject.transform as RectTransform;

		currentScrollY = (int)contentRectTransform.anchoredPosition.y;

		// 1) Run through allObjects; instantiate a cell object based on said object class
		float currentLayoutY = 0;
		float nextY = 0;
		float x = 0;
		float offsetX = 0;


		float cellWidth = MathParser.step(contentRectTransform.sizeDelta.x, cellSize.Value.x);
		if (expandCellWidth == false) {
			cellWidth = cellSize.Value.x;
		}
		if (cellWidth <= 0) {
			cellWidth = rectTransform.rect.width;
		}

		float cellHeight = cellSize.Value.y;

		int cellsPerRow = Mathf.FloorToInt(rectTransform.rect.width / cellWidth);
		if (cellsPerRow < 1)
			cellsPerRow = 1;

		currentLayoutY = -contentRectTransform.sizeDelta.y;


		// Handle the header
		int hasHeader = 0;
		if (headerSize.Value.y > 0 && subtableObjects.Count > 0) {
			hasHeader = 1;
			//currentLayoutY -= headerSize.Value.y;
		}

		// Can I skip a known quantity in the beginning and end?
		float scrollDelta = Mathf.Max(0.0f, contentRectTransform.anchoredPosition.y);
		
		int totalVisibleCells = (Mathf.CeilToInt((rectTransform.rect.height + cellHeight) / cellHeight) * cellsPerRow) + cellsPerRow;
		int firstVisibleCell = Mathf.FloorToInt((scrollDelta + currentLayoutY - cellHeight) / cellHeight) * cellsPerRow;
		
		if (firstVisibleCell < 0) {
			totalVisibleCells += firstVisibleCell;
			firstVisibleCell = 0;
		}

		if (hasHeader == 1) {
			object myCellData = subtableObjects [0];
			PUSimpleTableCell cell = null;
			if (PUSimpleTableCell.TestForVisibility (currentLayoutY, headerSize.Value.y, rectTransform, contentRectTransform)) {
				if (visibleCells.ContainsKey (myCellData)) {
					cell = visibleCells [myCellData];
					visibleCells.Remove (myCellData);
				} else {
					cell = DequeueTableCell (myCellData);
				}
				cell.puGameObject.rectTransform.anchoredPosition = new Vector2 (x, currentLayoutY);
			}

			currentLayoutY -= headerSize.Value.y;
		}



		// Update the content size
		float subtableWidth = contentRectTransform.sizeDelta.x;
		float subtableHeight = cellHeight * Mathf.Ceil ((subtableObjects.Count-hasHeader) / (float)cellsPerRow);
		contentRectTransform.sizeDelta = new Vector2 (subtableWidth, contentRectTransform.sizeDelta.y + subtableHeight + headerSize.Value.y);

		currentLayoutY += -Mathf.FloorToInt(firstVisibleCell / cellsPerRow) * cellHeight;
		nextY = currentLayoutY - cellHeight;


		if (hasHeader == 1) {
			firstVisibleCell += 1;
		}
		
		for(int i = firstVisibleCell; i < firstVisibleCell + totalVisibleCells; i++) {
			if (i < subtableObjects.Count) {
				object myCellData = subtableObjects [i];

				PUSimpleTableCell cell = null;

				// Can I fit on the current line?
				if (x + cellWidth > (contentRectTransform.rect.width + 1)) {
					x = 0;
					currentLayoutY = nextY;
				}

				totalCellsChecked++;
				if (PUSimpleTableCell.TestForVisibility (currentLayoutY, cellHeight, rectTransform, contentRectTransform)) {
					if (visibleCells.ContainsKey (myCellData)) {
						cell = visibleCells [myCellData];
						visibleCells.Remove (myCellData);
					} else {
						cell = DequeueTableCell (myCellData);
					}

					if(cell.isEdit == false){
						cell.puGameObject.rectTransform.anchoredPosition = new Vector2 (x + offsetX, currentLayoutY);
					}
				}

				x += cellWidth;
				nextY = currentLayoutY - cellHeight;
			}
		}
	}

	public int VerticalScrollOffset() {
		return currentScrollY;
	}














	public int RowIndexForPosition(Vector2 pos) {

		RectTransform contentRectTransform = contentObject.transform as RectTransform;

		float currentLayoutY = 0;

		float cellWidth = rectTransform.rect.width;
		if(rectTransform.rect.width > cellSize.Value.x)
			cellWidth = Mathf.Floor (rectTransform.rect.width / Mathf.Floor (rectTransform.rect.width / cellSize.Value.x));
		
		float cellHeight = cellSize.Value.y;
		
		int cellsPerRow = Mathf.FloorToInt(rectTransform.rect.width / cellWidth);
		if (cellsPerRow < 1)
			cellsPerRow = 1;

		currentLayoutY -= pos.y;

		return (Mathf.FloorToInt((Mathf.Abs (contentRectTransform.anchoredPosition.y) + currentLayoutY - cellHeight) / cellHeight) * cellsPerRow) + 1;
	}

	private bool didPullToRefresh = false;
	private float lastTableHeight = 0;

	public override void LateUpdate() {

		RectTransform tableContentTransform = contentObject.transform as RectTransform;

		if (OnPullToRefresh != null) {

			// dont refresh check if the bounds of the table are animating
			if(lastTableHeight == rectTransform.rect.height){
				float refreshHeight = rectTransform.rect.height * 0.2f;
				if(refreshHeight > cellSize.Value.y){
					refreshHeight = cellSize.Value.y;
				}

				// check to see if we've pulled enough in the top direction to desire a pull to refresh
				if (didPullToRefresh == false) {
					if (tableContentTransform.anchoredPosition.y < -refreshHeight) {
						didPullToRefresh = true;
						OnPullToRefresh ();
					}
				} else {
					if (tableContentTransform.anchoredPosition.y > -3.0f) {
						didPullToRefresh = false;
					}
				}
			}

			lastTableHeight = rectTransform.rect.height;
		}

		// If we've scrolled, retest cells to see who needs to load/unload

		// TODO: are we currently reloading in the background?
		if (currentScrollY != (int)tableContentTransform.anchoredPosition.y ||
		    currentScrollHeight != (int)rectTransform.rect.height ||
		    isEdit == true) {

			LayoutChildren();
		}
	}

	public void LayoutChildren() {
		RectTransform tableContentTransform = contentObject.transform as RectTransform;

		ReloadTable ();

		currentScrollY = (int)tableContentTransform.anchoredPosition.y;
		currentScrollHeight = (int)rectTransform.rect.height;
	}
	
	public override void gaxb_complete() {

		base.gaxb_complete ();

		NotificationCenter.addObserver (this, "OnAspectChanged", null, (args, name) => {

			EmptyTable();

			ReloadTable();
		});

		ScheduleForLateUpdate ();
	}

	public override void unload() {

		// TODO: cancel any background loads
		//if (tableUpdateScript != null)
		//	tableUpdateScript.StopReloadTableCells();

		foreach (PUSimpleTableCell cell in activeTableCells) {
			cell.unload ();
		}
		foreach (string key in pooledTableCells.Keys) {
			foreach (PUSimpleTableCell cell in pooledTableCells[key]) {
				cell.unload ();
			}
		}


		base.unload ();
	}


	public bool isEdit = false;

	public void BeginEditWithCell(PUSimpleTableCell cell){
		isEdit = true;
		cell.isEdit = true;

		cell.puGameObject.rectTransform.SetParent (contentObject.transform, false);
	}

	public void EndEdit(){
		isEdit = false;
		foreach (PUSimpleTableCell cell in allCells) {
			cell.isEdit = false;
		}

		LayoutChildren ();

		if (OnEndEdit != null) {
			OnEndEdit();
		}
	}

	public int ObjectIndexOfCell(PUSimpleTableCell cell){
		int idx = 0;
		foreach (List<object> subtableObjects in allSegmentedObjects) {
			foreach(object item in subtableObjects){
				if(cell.cellData == item){
					return idx;
				}
				idx++;
			}
		}
		return -1;
	}

	public int MaxObjectIndex(){
		int idx = 0;
		foreach (List<object> subtableObjects in allSegmentedObjects) {
			idx += subtableObjects.Count;
		}
		return idx;
	}

}
