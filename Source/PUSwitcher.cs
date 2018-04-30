using System;
using UnityEngine;
using UnityEngine.UI;


public partial class PUSwitcher : PUSwitcherBase {

	private Action<PUGameObject, int, int> HideAnimation;
	private Action<PUGameObject, int, int> ShowAnimation;
	private Action<PUGameObject, int, Action> CloseAnimation;

	public override void gaxb_complete() {

		HideAnimation = (x, idx, direction) => {
            x.CheckCanvasGroup();
			x.canvasGroup.alpha = 0;
			x.gameObject.SetActive (false);
		};

		ShowAnimation = (x, idx, direction) => {
            x.CheckCanvasGroup();
			x.canvasGroup.alpha = 1;
			x.gameObject.SetActive (true);
		};

		CloseAnimation = (x, idx, block) => {
            x.CheckCanvasGroup();
			x.canvasGroup.alpha = 0;
			x.gameObject.SetActive (false);
			block();
		};


		foreach (object childObj in children) {
			PUGameObject child = childObj as PUGameObject;
			child.CheckCanvasGroup ();
			child.canvasGroup.alpha = 0;
			child.gameObject.SetActive (false);
		}

		int initialIndex = currentIndex.Value;
		currentIndex = -1;
		SwitchTo (initialIndex);
	}

	public void SetAnimationBlocks(Action<PUGameObject, int, int> hide, Action<PUGameObject, int, int> show, Action<PUGameObject, int, Action> close){
		HideAnimation = hide;
		ShowAnimation = show;
		CloseAnimation = close;
	}

	public int CurrentIndex() {
		return currentIndex.Value;
	}

	public void SwitchTo(int i) {
		if (currentIndex.Value == i) {
			return;
		}

		int direction = i - currentIndex.Value;
		if (direction < -1) {
			direction = -1;
		}
		if (direction > 1) {
			direction = 1;
		}
		HideIndex (currentIndex.Value, direction);
		currentIndex = i;
		ShowIndex (currentIndex.Value, direction);
	}

	private void HideIndex(int idx, int direction) {
		if (idx >= 0 && idx < children.Count) {
			PUGameObject child = children [idx] as PUGameObject;
			HideAnimation (child, idx, direction);
		}
	}

	private void ShowIndex(int idx, int direction) {
		if (idx >= 0 && idx < children.Count) {
			PUGameObject child = children [idx] as PUGameObject;
			ShowAnimation (child, idx, direction);
		}
	}

	public void Close(Action block) {
		if (currentIndex.Value >= 0 && currentIndex.Value < children.Count) {
			PUGameObject child = children [currentIndex.Value] as PUGameObject;
			CloseAnimation (child, currentIndex.Value, block);
		} else {
			block();
		}
	}

}
