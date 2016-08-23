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
using UnityEngine.UI;

public partial class PUGridLayoutGroup : PUGridLayoutGroupBase {

	public GridLayoutGroup layout;

	public override void gaxb_init ()
	{
		gameObject = new GameObject ("<GridLayoutGroup/>", typeof(RectTransform));

		layout = gameObject.AddComponent<GridLayoutGroup> ();

		if (cellSize != null) {
			layout.cellSize = cellSize.Value;
		}

		if (spacing != null) {
			layout.spacing = spacing.Value;
		}

		if (fixedRows > 0) {
			layout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
			layout.constraintCount = (int)fixedRows;
		} else if (fixedColumns > 0) {
			layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			layout.constraintCount = (int)fixedColumns;
		} else {
			layout.constraint = GridLayoutGroup.Constraint.Flexible;
		}


		if (startAxis != null) {
			if (startAxis == PlanetUnity2.GridLayoutStartAxis.horizontal)
				layout.startAxis = GridLayoutGroup.Axis.Horizontal;
			if (startAxis == PlanetUnity2.GridLayoutStartAxis.vertical)
				layout.startAxis = GridLayoutGroup.Axis.Vertical;
		}

		if (startCorner != null) {
			if (startCorner == PlanetUnity2.GridLayoutStartCorner.lowerLeft)
				layout.startCorner = GridLayoutGroup.Corner.LowerLeft;
			if (startCorner == PlanetUnity2.GridLayoutStartCorner.lowerRight)
				layout.startCorner = GridLayoutGroup.Corner.LowerRight;
			if (startCorner == PlanetUnity2.GridLayoutStartCorner.upperLeft)
				layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
			if (startCorner == PlanetUnity2.GridLayoutStartCorner.upperRight)
				layout.startCorner = GridLayoutGroup.Corner.UpperRight;
		}

		if(childAlignment != null) {
			if (childAlignment == PlanetUnity2.GridLayoutChildAlignment.upperLeft)
				layout.childAlignment = TextAnchor.UpperLeft;
			if (childAlignment == PlanetUnity2.GridLayoutChildAlignment.upperCenter)
				layout.childAlignment = TextAnchor.UpperCenter;
			if (childAlignment == PlanetUnity2.GridLayoutChildAlignment.upperRight)
				layout.childAlignment = TextAnchor.UpperRight;

			if (childAlignment == PlanetUnity2.GridLayoutChildAlignment.middleLeft)
				layout.childAlignment = TextAnchor.MiddleLeft;
			if (childAlignment == PlanetUnity2.GridLayoutChildAlignment.middleCenter)
				layout.childAlignment = TextAnchor.MiddleCenter;
			if (childAlignment == PlanetUnity2.GridLayoutChildAlignment.middleRight)
				layout.childAlignment = TextAnchor.MiddleRight;

			if (childAlignment == PlanetUnity2.GridLayoutChildAlignment.lowerLeft)
				layout.childAlignment = TextAnchor.LowerLeft;
			if (childAlignment == PlanetUnity2.GridLayoutChildAlignment.lowerCenter)
				layout.childAlignment = TextAnchor.LowerCenter;
			if (childAlignment == PlanetUnity2.GridLayoutChildAlignment.lowerRight)
				layout.childAlignment = TextAnchor.LowerRight;
		}
	}

}
