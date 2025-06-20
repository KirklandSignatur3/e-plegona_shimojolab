using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Shapes;


public class Circle : MonoBehaviour
{
    Sequence sequence = null;
    [SerializeField] Disc disc;
    float alpha = 0f;
    float radius = 0f;

    public void Show()
    {
        float radiusGoal = Mathf.Floor(Random.Range(0.2f, 0.4f) * 10f) / 10f;
        alpha = 1;//0.6f;
        Color color = Color.white;
        color.a = alpha;
        disc.Color = color;
        this.transform.localScale = new Vector3(1f, 1f, 1f);
        sequence = DOTween.Sequence()
        .Append(DOTween.To(() => radius, (x) => radius = x, radiusGoal, 3f).OnUpdate(() =>
        {
            disc.Radius = radius;
        }).SetEase(Ease.OutCubic))
        .Join(DOTween.To(() => alpha, (x) => alpha = x, 0f, 0.4f).SetDelay(3.6f).OnUpdate(() =>
        {
            color.a = alpha;
            disc.Color = color;
        }))
        .OnComplete(() =>
        {
            sequence.Kill(false);
            sequence = null;
            Destroy(this.gameObject);
        });
    }

    private void OnDestory()
    {
        sequence?.Kill();
    }
}