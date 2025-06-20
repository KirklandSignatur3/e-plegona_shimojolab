using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Hamon : MonoBehaviour
{
	Sequence sequence = null;
	[SerializeField] Renderer rendererCompo;
	float alpha = 0f;
    
    public void Show(Hit hit)
    {
        float radius = Mathf.Max(0f, (float)hit * 2f);
		alpha = 0.8f;
		rendererCompo.material.SetFloat("_Alpha", alpha);
		this.transform.localScale = new Vector3(1f, 1f, 1f);
		sequence = DOTween.Sequence()
		.Append(this.transform.DOScale(new Vector3(radius, radius, radius), 0.6f).SetEase(Ease.OutCubic))
		.Join(DOTween.To(() => alpha, (x) => alpha = x, 0f, 0.6f).OnUpdate(() =>
		{
			rendererCompo.material.SetFloat("_Alpha", alpha);
		}))
		.OnComplete(() =>
		{
			Destroy(this.gameObject);
		});
    }

	private void OnDestory()
	{
		sequence?.Kill();
	}
}
