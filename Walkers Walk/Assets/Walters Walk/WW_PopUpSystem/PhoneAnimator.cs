using DG.Tweening;
using UnityEngine;

namespace PopUpSystem
{
    public class PhoneAnimator
    {
        public RectTransform popupTransform;
        public float animationDuration = 0.5f;
        public float visibleHeight = 0f;

        private Vector3 initialPosition;
        private Vector3 lastPosition;
        private Vector3 startDragPosition;

        public PhoneAnimator(RectTransform pT, float aD, float vH)
        {
            popupTransform = pT;
            animationDuration = aD;
            visibleHeight = vH;

            initialPosition = popupTransform.position;
            Debug.Log($"Initial position: {initialPosition}");
        }

        public void Open()
        {
            popupTransform.DOKill();
            popupTransform.DOMove(new Vector2(initialPosition.x, visibleHeight), animationDuration).SetEase(Ease.OutSine);
        }

        public void Close()
        {
            popupTransform.DOKill();
            popupTransform.DOMove(new Vector2(initialPosition.x, initialPosition.y), animationDuration).SetEase(Ease.OutSine);
        }

        public void Move(Vector2 delta)
        {
            popupTransform.DOKill();
            Vector3 newPos = lastPosition + (Vector3)delta - startDragPosition;
            if(newPos.y < visibleHeight)
            {
                popupTransform.position = lastPosition + (Vector3)delta - startDragPosition;
            }
            else
            {
                popupTransform.position = new Vector3(newPos.x, visibleHeight, newPos.z);
            }
        }

        public void SetStartDragPosition(Vector3 pos)
        {
            lastPosition = popupTransform.position;
            startDragPosition = pos;
        }
    }
}

