using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Shapes;

public class KakuhenRect : MonoBehaviour
{
    Sequence sequence = null;
    Tween tween = null;
    [SerializeField] RegularPolygon disc;
    float alpha = 0f;
    float radius = 0f;

    public void Show()
    {
        float radiusGoal = Mathf.Floor(Random.Range(0.05f, 0.15f) * 20f) / 20f;
        radius = radiusGoal;
        alpha = 1;//0.6f;
        Color color = Color.white;
        color.a = alpha;
        disc.Color = color;
		disc.Radius = radius;
        this.transform.localScale = new Vector3(1f, 1f, 1f);
        this.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        tween = this.transform.DOLocalRotate(new Vector3(0f, 0f, -90f), 1.2f).SetEase(Ease.InOutCubic)
        .OnComplete(() =>
        {
            tween.Kill(false);
            tween = null;
            sequence.Kill(false);
            sequence = null;
            Destroy(this.gameObject);
        });

        sequence = DOTween.Sequence()
        //.Append(DOTween.To(() => radius, (x) => radius = x, radiusGoal, 1.4f).OnUpdate(() =>
        //{
        //    disc.Radius = radius;
        //}).SetEase(Ease.OutCubic))
        .Append(DOTween.To(() => alpha, (x) => alpha = x, 1f, 0.1f).OnUpdate(() =>
        {
            color.a = alpha;
            disc.Color = color;
        }))
        .Append(DOTween.To(() => alpha, (x) => alpha = x, 0f, 0.2f).SetDelay(1f).OnUpdate(() =>
        {
            color.a = alpha;
            disc.Color = color;
        }));

    }

    private void OnDestory()
    {
        sequence?.Kill();
        sequence = null;
        tween.Kill(false);
        tween = null;
    }
}