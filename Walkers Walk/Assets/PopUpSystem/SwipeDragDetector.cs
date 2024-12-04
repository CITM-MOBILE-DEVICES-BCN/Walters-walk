using System;
using UnityEngine;

namespace PopUpSystem
{
    public class SwipeDragDetector : MonoBehaviour
    {
        public static SwipeDragDetector Instance { get; private set; }

        // Actions
        public Action<Vector2> OnDragStartAction; // Inicio del arrastre
        public Action<Vector2> OnDragAction; // Movimiento durante el arrastre
        public Action<SwipeDirection> OnSwipeAction; // Dirección del swipe

        public enum SwipeDirection
        {
            Up,
            Down,
            Left,
            Right
        }

        private Vector2 startTouchPosition; // Posición inicial del toque
        private Vector2 currentTouchPosition; // Posición actual durante el arrastre
        private bool isDragging = false;
        private float minSwipeDistance = 50f; // Distancia mínima para considerar un swipe

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            DetectTouchOrMouse();
        }

        private void DetectTouchOrMouse()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        startTouchPosition = touch.position;
                        isDragging = false;
                        OnDragStartAction?.Invoke(startTouchPosition); // Inicio del arrastre
                        break;

                    case TouchPhase.Moved:
                        currentTouchPosition = touch.position;
                        isDragging = true;
                        OnDragAction?.Invoke(currentTouchPosition); // Movimiento del arrastre
                        break;

                    case TouchPhase.Ended:
                        currentTouchPosition = touch.position;

                        if (!isDragging) // Si no hubo un arrastre prolongado, evaluamos si fue un swipe
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
                OnDragStartAction?.Invoke(startTouchPosition); // Inicio del arrastre
            }
            else if (Input.GetMouseButton(0))
            {
                currentTouchPosition = Input.mousePosition;
                isDragging = true;
                OnDragAction?.Invoke(currentTouchPosition); // Movimiento del arrastre
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
        }
    }
}
