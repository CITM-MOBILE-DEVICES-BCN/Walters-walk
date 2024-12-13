using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TweeningAlert : MonoBehaviour
{
    [Header("Tweening Settings")]
    public float duration = 1f;
    public float scale = 1.2f;
    public Ease easeType = Ease.InOutSine;

    void Start()
    {
        transform.DOScale(scale, duration).SetEase(easeType).SetLoops(-1, LoopType.Yoyo);
    }
}