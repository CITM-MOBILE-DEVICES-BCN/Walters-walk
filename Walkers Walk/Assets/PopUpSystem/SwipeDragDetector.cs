using System;
using UnityEngine;

namespace PopUpSystem
{
    public class SwipeDragDetector
    {
        public Action<Vector2> OnDragStartAction;
        public Action<Vector2> OnDragAction;
        public Action<SwipeDirection> OnSwipeAction;
        public Action OnTouchAction;

        public enum SwipeDirection
        {
            None,
            Up,
            Down,
            Left,
            Right
        }

        private Vector2 startTouchPosition;
        private Vector2 currentTouchPosition;
        private bool isDragging = false;
        private float minSwipeDistance = 50f;

        public void DetectTouchOrMouse()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        startTouchPosition = touch.position;
                        isDragging = false;
                        OnDragStartAction?.Invoke(startTouchPosition);
                        break;

                    case TouchPhase.Moved:
                        currentTouchPosition = touch.position;
                        isDragging = true;
                        OnDragAction?.Invoke(currentTouchPosition);
                        break;

                    case TouchPhase.Ended:
                        currentTouchPosition = touch.position;

                        if (!isDragging)
                        {
                            HandleSwipe();
                        }

                        isDragging = false;
                        break;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                currentTouchPosition = Input.mousePosition;

                HandleSwipe();

                isDragging = false;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                startTouchPosition = Input.mousePosition;
                isDragging = false;
                OnDragStartAction?.Invoke(startTouchPosition);
            }
            else if (Input.GetMouseButton(0))
            {
                currentTouchPosition = Input.mousePosition;
                isDragging = true;
                OnDragAction?.Invoke(currentTouchPosition);
            }
        }

        private void HandleSwipe()
        {
            float swipeDistance = Vector2.Distance(startTouchPosition, currentTouchPosition);

            if (swipeDistance >= minSwipeDistance)
            {
                Vector2 swipeVector = (currentTouchPosition - startTouchPosition).normalized;

                if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
                {
                    if (swipeVector.x > 0)
                    {
                        Debug.Log("Swipe detected: Right");
                        OnSwipeAction?.Invoke(SwipeDirection.Right);
                    }
                    else
                    {
                        Debug.Log("Swipe detected: Left");
                        OnSwipeAction?.Invoke(SwipeDirection.Left);
                    }
                }
                else
                {
                    if (swipeVector.y > 0)
                    {
                        Debug.Log("Swipe detected: Up");
                        OnSwipeAction?.Invoke(SwipeDirection.Up);
                    }
                    else
                    {
                        Debug.Log("Swipe detected: Down");
                        OnSwipeAction?.Invoke(SwipeDirection.Down);
                    }
                }
            }
            else
            {
                OnTouchAction?.Invoke();
            }
        }
    }
}
