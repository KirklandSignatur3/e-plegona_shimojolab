using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class ButtonsView : IView
{
	[SerializeField] List<Pad> buttons;
	[SerializeField] CanvasGroup group;
	Tweener tweener = null;
	bool isLaneThree = false;
	public bool IsLaneThree
	{
		get { return isLaneThree; }
		set { isLaneThree = value; }
	}

	public void Active()
	{
		foreach (var item in buttons) item.Active();
	}

	public void Negative()
	{
		foreach (var item in buttons) item.Negative();
	}

	public override void Init()
	{
		gameObject.SetActive(false);
		group.alpha = 0;
		foreach (var item in buttons)
		{
			item.Negative();
			item.anim.Hide();
		}
		isShow = false;
	}

	public async override Task Show(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		isShow = true;
		gameObject.SetActive(true);
		group.alpha = 0;

		if (isLaneThree)
		{
			buttons[0].anim.Show();
			buttons[3].anim.Show();
		}
		else
		{
			buttons[0].anim.Hide();
			buttons[3].anim.Hide();
		}

		buttons[1].anim.Show();
		buttons[2].anim.Show();
		buttons[4].anim.Show();
		buttons[5].anim.Show();

		var ts = new TaskCompletionSource<bool>();
		tweener = group.DOFade(1, 0.5f).OnComplete(() =>
		{
			if (isLaneThree)
			{
				buttons[0].Active();
				buttons[3].Active();
			}
			else
			{
				buttons[0].Negative();
				buttons[3].Negative();
			}

			buttons[1].Active();
			buttons[2].Active();
			buttons[4].Active();
			buttons[5].Active();

			tweener.Kill(false);
			tweener = null;
			ts.SetResult(true);
		});

		token.Register(() =>
		{
			tweener?.Kill(false);
			tweener = null;
			ts.TrySetCanceled();
		});
		
		await ts.Task;
	}

	public async override Task Hide(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		var ts = new TaskCompletionSource<bool>();

		if (!isShow)
		{
			group.gameObject.SetActive(false);
			group.alpha = 0;
			foreach (var item in buttons) item.anim.Hide();
			ts.SetResult(true);
			await ts.Task;
			return;
		}

		Negative();

		tweener = group.DOFade(0, 0.5f).OnComplete(() =>
		{
			tweener.Kill(false);
			tweener = null;
			group.alpha = 0;
			foreach (var item in buttons) item.anim.Hide();
			group.gameObject.SetActive(false);
			isShow = false;
			ts.SetResult(true);
		});

		token.Register(() =>
		{
			tweener?.Kill(false);
			tweener = null;
			ts.TrySetCanceled();
		});

		await ts.Task;
	}

	public async Task Tutorial1Start(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		var arrayTask = new List<Task>();
		arrayTask.Add(buttons[1].anim.Excuse(token));
		arrayTask.Add(buttons[2].anim.Excuse(token));
		arrayTask.Add(buttons[4].anim.Excuse(token));
		arrayTask.Add(buttons[5].anim.Excuse(token));

		await Task.WhenAll(arrayTask);
	}

	public async Task Tutorial3Start(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		var arrayTask = new List<Task>();
		arrayTask.Add(buttons[1].anim.ExcuseHold(token));
		arrayTask.Add(buttons[5].anim.ExcuseHold(token));

		await Task.WhenAll(arrayTask);
	}
}
