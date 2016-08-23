

using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public partial class PUGridTable : PUGridTableBase {

	public struct LORect {
		public float x, y, width, height;
		public RectTransform rectTransform;
	}


	private void SubLayoutCells(ref float maxHeight, List<PUTableCell> cellsToAdd, MaxRectsBinPack.FreeRectChoiceHeuristic heuristic) {
		RectTransform contentRectTransform = contentObject.transform as RectTransform;

		float blockHeight = 2048.0f;
		float baseY = maxHeight;

		if (cellsToAdd [0].IsHeader ()) {
			PUTableCell cell = cellsToAdd [0];
			cellsToAdd.RemoveAt (0);

			cell.puGameObject.rectTransform.anchoredPosition = new Vector2 (0, -baseY);

			baseY += cell.puGameObject.rectTransform.sizeDelta.y;
		}

		// The MaxRects packer works by being given a canvas (width/height) to fit all rectangles in
		// For us to use this and allow arbitrary height, we give it a rect the size of the visible
		// scroll area, fill it up, and then repeat until we run out of cells.
		int bail = 500;
		while (cellsToAdd.Count > 0 && bail > 0) {
			MaxRectsBinPack packer = new MaxRectsBinPack ((int)contentRectTransform.rect.width, (int)blockHeight, false);

			for (int i = cellsToAdd.Count - 1; i >= 0; i--) {
				PUTableCell cell = cellsToAdd [i];
				LORect packedRect;

				if (packer.Insert ((int)cell.puGameObject.rectTransform.sizeDelta.x, (int)cell.puGameObject.rectTransform.sizeDelta.y, heuristic, cell.puGameObject.rectTransform, out packedRect)) {
					packedRect.y += baseY;
					cell.puGameObject.rectTransform.anchoredPosition = new Vector2 (packedRect.x, -packedRect.y);
					if ((packedRect.y + packedRect.height) > maxHeight) {
						maxHeight = (packedRect.y + packedRect.height);
					}
					cellsToAdd.RemoveAt (i);
				}
			}

			if (expandToFill) {
				packer.ExpandRectsToFill ((int)contentRectTransform.rect.width, (maxHeight - baseY));
			}

			baseY = maxHeight;
			bail--;
		}

		if (bail == 0) {
			Debug.Log ("Warning: PUGridTable layout failed to place all cells");
		}


	}

	private void LayoutAllCells (MaxRectsBinPack.FreeRectChoiceHeuristic heuristic) {
		RectTransform contentRectTransform = contentObject.transform as RectTransform;

		float maxHeight = 0;

		// If we have headers, we want to layout the cells in header-to-header chuncks of cells
		if (allCells [0].IsHeader () == false) {
			SubLayoutCells (ref maxHeight, allCells.GetRange(0, allCells.Count), heuristic);
		} else {
			int lastHeaderIdx = 0;

			for (int i = 1; i < allCells.Count; i++) {
				if (allCells [i].IsHeader ()) {
					SubLayoutCells (ref maxHeight, allCells.GetRange (lastHeaderIdx, (i - lastHeaderIdx)), heuristic);
					lastHeaderIdx = i;
				}
			}
			if (lastHeaderIdx != allCells.Count - 1) {
				SubLayoutCells (ref maxHeight, allCells.GetRange (lastHeaderIdx, (allCells.Count - lastHeaderIdx)), heuristic);
			}

		}

		contentRectTransform.sizeDelta = new Vector2 (rectTransform.rect.width, maxHeight);
	}


	public override void LayoutAllCells () {
		if (heuristic == null) {
			LayoutAllCells (MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestAreaFit);
		} else {
			LayoutAllCells ((MaxRectsBinPack.FreeRectChoiceHeuristic)heuristic.Value);
		}
	}



	// Found here: http://wiki.unity3d.com/index.php?title=MaxRectsBinPack

	/*
 	Based on the Public Domain MaxRectsBinPack.cpp source by Jukka Jylänki
 	https://github.com/juj/RectangleBinPack/
 
 	Ported to C# by Sven Magnus
 	This version is also public domain - do whatever you want with it.
	*/

	public class MaxRectsBinPack {

		public int binWidth = 0;
		public int binHeight = 0;

		public List<LORect> usedRectangles = new List<LORect>();
		public List<LORect> freeRectangles = new List<LORect>();

		public enum FreeRectChoiceHeuristic {
			RectBestShortSideFit, ///< -BSSF: Positions the rectangle against the short side of a free rectangle into which it fits the best.
			RectBestLongSideFit, ///< -BLSF: Positions the rectangle against the long side of a free rectangle into which it fits the best.
			RectBestAreaFit, ///< -BAF: Positions the rectangle into the smallest free rect into which it fits.
			RectBottomLeftRule, ///< -BL: Does the Tetris placement.
			RectContactPointRule ///< -CP: Choosest the placement where the rectangle touches other rects as much as possible.
		};

		public MaxRectsBinPack(int width, int height, bool rotations = true) {
			Init(width, height, rotations);
		}

		public void Init(int width, int height, bool rotations = true) {
			binWidth = width;
			binHeight = height;

			LORect n = new LORect();
			n.x = 0;
			n.y = 0;
			n.width = width;
			n.height = height;

			usedRectangles.Clear();

			freeRectangles.Clear();
			freeRectangles.Add(n);
		}

		public bool Insert(int width, int height, FreeRectChoiceHeuristic method, RectTransform rt, out LORect packedRect) {
			LORect newNode = new LORect();
			int score1 = 0; // Unused in this function. We don't need to know the score after finding the position.
			int score2 = 0;
			switch(method) {
			case FreeRectChoiceHeuristic.RectBestShortSideFit: newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2); break;
			case FreeRectChoiceHeuristic.RectBottomLeftRule: newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2); break;
			case FreeRectChoiceHeuristic.RectContactPointRule: newNode = FindPositionForNewNodeContactPoint(width, height, ref score1); break;
			case FreeRectChoiceHeuristic.RectBestLongSideFit: newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1); break;
			case FreeRectChoiceHeuristic.RectBestAreaFit: newNode = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2); break;
			}

			newNode.rectTransform = rt;

			if (newNode.height == 0) {
				packedRect = newNode;
				return false;
			}

			int numRectanglesToProcess = freeRectangles.Count;
			for(int i = 0; i < numRectanglesToProcess; ++i) {
				if (SplitFreeNode(freeRectangles[i], ref newNode)) {
					freeRectangles.RemoveAt(i);
					--i;
					--numRectanglesToProcess;
				}
			}

			PruneFreeList();

			usedRectangles.Add(newNode);
			packedRect = newNode;
			return true;
		}

		public void Insert(List<LORect> rects, List<LORect> dst, FreeRectChoiceHeuristic method) {
			dst.Clear();

			while(rects.Count > 0) {
				int bestScore1 = int.MaxValue;
				int bestScore2 = int.MaxValue;
				int bestRectIndex = -1;
				LORect bestNode = new LORect();

				for(int i = 0; i < rects.Count; ++i) {
					int score1 = 0;
					int score2 = 0;
					LORect newNode = ScoreRect((int)rects[i].width, (int)rects[i].height, method, ref score1, ref score2);

					if (score1 < bestScore1 || (score1 == bestScore1 && score2 < bestScore2)) {
						bestScore1 = score1;
						bestScore2 = score2;
						bestNode = newNode;
						bestRectIndex = i;
					}
				}

				if (bestRectIndex == -1)
					return;

				PlaceRect(bestNode);
				rects.RemoveAt(bestRectIndex);
			}
		}

		void PlaceRect(LORect node) {
			int numRectanglesToProcess = freeRectangles.Count;
			for(int i = 0; i < numRectanglesToProcess; ++i) {
				if (SplitFreeNode(freeRectangles[i], ref node)) {
					freeRectangles.RemoveAt(i);
					--i;
					--numRectanglesToProcess;
				}
			}

			PruneFreeList();

			usedRectangles.Add(node);
		}

		LORect ScoreRect(int width, int height, FreeRectChoiceHeuristic method, ref int score1, ref int score2) {
			LORect newNode = new LORect();
			score1 = int.MaxValue;
			score2 = int.MaxValue;
			switch(method) {
			case FreeRectChoiceHeuristic.RectBestShortSideFit: newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2); break;
			case FreeRectChoiceHeuristic.RectBottomLeftRule: newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2); break;
			case FreeRectChoiceHeuristic.RectContactPointRule: newNode = FindPositionForNewNodeContactPoint(width, height, ref score1); 
				score1 = -score1; // Reverse since we are minimizing, but for contact point score bigger is better.
				break;
			case FreeRectChoiceHeuristic.RectBestLongSideFit: newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1); break;
			case FreeRectChoiceHeuristic.RectBestAreaFit: newNode = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2); break;
			}

			// Cannot fit the current rectangle.
			if (newNode.height == 0) {
				score1 = int.MaxValue;
				score2 = int.MaxValue;
			}

			return newNode;
		}

		/// Computes the ratio of used surface area.
		public float Occupancy() {
			ulong usedSurfaceArea = 0;
			for(int i = 0; i < usedRectangles.Count; ++i)
				usedSurfaceArea += (uint)usedRectangles[i].width * (uint)usedRectangles[i].height;

			return (float)usedSurfaceArea / (binWidth * binHeight);
		}

		LORect FindPositionForNewNodeBottomLeft(int width, int height, ref int bestY, ref int bestX) {
			LORect bestNode = new LORect();

			bestY = int.MaxValue;

			for(int i = 0; i < freeRectangles.Count; ++i) {
				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRectangles[i].width >= width && freeRectangles[i].height >= height) {
					int topSideY = (int)freeRectangles[i].y + height;
					if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].x < bestX)) {
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = width;
						bestNode.height = height;
						bestY = topSideY;
						bestX = (int)freeRectangles[i].x;
					}
				}
			}
			return bestNode;
		}

		LORect FindPositionForNewNodeBestShortSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)  {
			LORect bestNode = new LORect();

			bestShortSideFit = int.MaxValue;

			for(int i = 0; i < freeRectangles.Count; ++i) {
				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRectangles[i].width >= width && freeRectangles[i].height >= height) {
					int leftoverHoriz = Mathf.Abs((int)freeRectangles[i].width - width);
					int leftoverVert = Mathf.Abs((int)freeRectangles[i].height - height);
					int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);
					int longSideFit = Mathf.Max(leftoverHoriz, leftoverVert);

					if (shortSideFit < bestShortSideFit || (shortSideFit == bestShortSideFit && longSideFit < bestLongSideFit)) {
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = width;
						bestNode.height = height;
						bestShortSideFit = shortSideFit;
						bestLongSideFit = longSideFit;
					}
				}
			}
			return bestNode;
		}

		LORect FindPositionForNewNodeBestLongSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit) {
			LORect bestNode = new LORect();

			bestLongSideFit = int.MaxValue;

			for(int i = 0; i < freeRectangles.Count; ++i) {
				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRectangles[i].width >= width && freeRectangles[i].height >= height) {
					int leftoverHoriz = Mathf.Abs((int)freeRectangles[i].width - width);
					int leftoverVert = Mathf.Abs((int)freeRectangles[i].height - height);
					int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);
					int longSideFit = Mathf.Max(leftoverHoriz, leftoverVert);

					if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit)) {
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = width;
						bestNode.height = height;
						bestShortSideFit = shortSideFit;
						bestLongSideFit = longSideFit;
					}
				}
			}
			return bestNode;
		}

		LORect FindPositionForNewNodeBestAreaFit(int width, int height, ref int bestAreaFit, ref int bestShortSideFit) {
			LORect bestNode = new LORect();

			bestAreaFit = int.MaxValue;

			for(int i = 0; i < freeRectangles.Count; ++i) {
				int areaFit = (int)freeRectangles[i].width * (int)freeRectangles[i].height - width * height;

				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRectangles[i].width >= width && freeRectangles[i].height >= height) {
					int leftoverHoriz = Mathf.Abs((int)freeRectangles[i].width - width);
					int leftoverVert = Mathf.Abs((int)freeRectangles[i].height - height);
					int shortSideFit = Mathf.Min(leftoverHoriz, leftoverVert);

					if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit)) {
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = width;
						bestNode.height = height;
						bestShortSideFit = shortSideFit;
						bestAreaFit = areaFit;
					}
				}
			}
			return bestNode;
		}

		/// Returns 0 if the two intervals i1 and i2 are disjoint, or the length of their overlap otherwise.
		int CommonIntervalLength(int i1start, int i1end, int i2start, int i2end) {
			if (i1end < i2start || i2end < i1start)
				return 0;
			return Mathf.Min(i1end, i2end) - Mathf.Max(i1start, i2start);
		}

		int ContactPointScoreNode(int x, int y, int width, int height) {
			int score = 0;

			if (x == 0 || x + width == binWidth)
				score += height;
			if (y == 0 || y + height == binHeight)
				score += width;

			for(int i = 0; i < usedRectangles.Count; ++i) {
				if (usedRectangles[i].x == x + width || usedRectangles[i].x + usedRectangles[i].width == x)
					score += CommonIntervalLength((int)usedRectangles[i].y, (int)usedRectangles[i].y + (int)usedRectangles[i].height, y, y + height);
				if (usedRectangles[i].y == y + height || usedRectangles[i].y + usedRectangles[i].height == y)
					score += CommonIntervalLength((int)usedRectangles[i].x, (int)usedRectangles[i].x + (int)usedRectangles[i].width, x, x + width);
			}
			return score;
		}

		LORect FindPositionForNewNodeContactPoint(int width, int height, ref int bestContactScore) {
			LORect bestNode = new LORect();

			bestContactScore = -1;

			for(int i = 0; i < freeRectangles.Count; ++i) {
				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRectangles[i].width >= width && freeRectangles[i].height >= height) {
					int score = ContactPointScoreNode((int)freeRectangles[i].x, (int)freeRectangles[i].y, width, height);
					if (score > bestContactScore) {
						bestNode.x = (int)freeRectangles[i].x;
						bestNode.y = (int)freeRectangles[i].y;
						bestNode.width = width;
						bestNode.height = height;
						bestContactScore = score;
					}
				}
			}
			return bestNode;
		}

		bool SplitFreeNode(LORect freeNode, ref LORect usedNode) {
			// Test with SAT if the rectangles even intersect.
			if (usedNode.x >= freeNode.x + freeNode.width || usedNode.x + usedNode.width <= freeNode.x ||
				usedNode.y >= freeNode.y + freeNode.height || usedNode.y + usedNode.height <= freeNode.y)
				return false;

			if (usedNode.x < freeNode.x + freeNode.width && usedNode.x + usedNode.width > freeNode.x) {
				// New node at the top side of the used node.
				if (usedNode.y > freeNode.y && usedNode.y < freeNode.y + freeNode.height) {
					LORect newNode = freeNode;
					newNode.height = usedNode.y - newNode.y;
					freeRectangles.Add(newNode);
				}

				// New node at the bottom side of the used node.
				if (usedNode.y + usedNode.height < freeNode.y + freeNode.height) {
					LORect newNode = freeNode;
					newNode.y = usedNode.y + usedNode.height;
					newNode.height = freeNode.y + freeNode.height - (usedNode.y + usedNode.height);
					freeRectangles.Add(newNode);
				}
			}

			if (usedNode.y < freeNode.y + freeNode.height && usedNode.y + usedNode.height > freeNode.y) {
				// New node at the left side of the used node.
				if (usedNode.x > freeNode.x && usedNode.x < freeNode.x + freeNode.width) {
					LORect newNode = freeNode;
					newNode.width = usedNode.x - newNode.x;
					freeRectangles.Add(newNode);
				}

				// New node at the right side of the used node.
				if (usedNode.x + usedNode.width < freeNode.x + freeNode.width) {
					LORect newNode = freeNode;
					newNode.x = usedNode.x + usedNode.width;
					newNode.width = freeNode.x + freeNode.width - (usedNode.x + usedNode.width);
					freeRectangles.Add(newNode);
				}
			}

			return true;
		}

		bool Intersects(LORect r1, LORect r2) {
			return !(
			    r2.x >= r1.x + r1.width ||
			    r2.x + r2.width <= r1.x ||
			    r2.y + r2.height <= r1.y ||
			    r2.y >= r1.y + r1.height
			);
		}

		void PruneFreeList() {
			for(int i = 0; i < freeRectangles.Count; ++i)
				for(int j = i+1; j < freeRectangles.Count; ++j) {
					if (IsContainedIn(freeRectangles[i], freeRectangles[j])) {
						freeRectangles.RemoveAt(i);
						--i;
						break;
					}
					if (IsContainedIn(freeRectangles[j], freeRectangles[i])) {
						freeRectangles.RemoveAt(j);
						--j;
					}
				}
		}

		bool IsContainedIn(LORect a, LORect b) {
			return a.x >= b.x && a.y >= b.y 
				&& a.x+a.width <= b.x+b.width 
				&& a.y+a.height <= b.y+b.height;
		}


		public void ExpandRectsToFill(float w, float h) {

			// Run through all rects; make them the full width
			// find any other intersecting rects, mind our min X and max X
			for (int i = 0; i < usedRectangles.Count; i++) {
				LORect a = usedRectangles [i];

				float centerX = a.x + a.width * 0.5f;
				float minX = 0;
				float maxX = w;

				a.x = 0;
				a.width = w;

				for (int j = 0; j < usedRectangles.Count; j++) {
					if (i == j) {
						continue;
					}

					LORect b = usedRectangles [j];

					if (Intersects (a, b)) {

						// find a left edge which is > centerX && < maxX
						if (b.x > centerX && b.x < maxX) {
							maxX = b.x;
						}

						// find a right edge which is < centerX && > minX
						if ((b.x + b.width) < centerX && (b.x + b.width) > minX) {
							minX = (b.x + b.width);
						}
					}
				}

				a.x = minX;
				a.width = maxX - minX;
				a.rectTransform.sizeDelta = new Vector2 (a.width, a.rectTransform.sizeDelta.y);

				usedRectangles [i] = a;
			}


			// Same thing as above, but do it for the Y axis
			for (int i = 0; i < usedRectangles.Count; i++) {
				LORect a = usedRectangles [i];

				float centerY = a.y + a.height * 0.5f;
				float minY = 0;
				float maxY = h;

				a.y = 0;
				a.height = h;

				for (int j = 0; j < usedRectangles.Count; j++) {
					if (i == j) {
						continue;
					}

					LORect b = usedRectangles [j];

					if (Intersects (a, b)) {

						// find a top edge which is > centerY && < maxY
						if (b.y > centerY && b.y < maxY) {
							maxY = b.y;
						}

						// find a bottom edge which is < centerY && > minY
						if ((b.y + b.height) < centerY && (b.y + b.height) > minY) {
							minY = (b.y + b.height);
						}
					}
				}

				a.y = minY;
				a.height = maxY - minY;
				a.rectTransform.sizeDelta = new Vector2 (a.rectTransform.sizeDelta.x, a.height);

				usedRectangles [i] = a;
			}
		}

	}

}
