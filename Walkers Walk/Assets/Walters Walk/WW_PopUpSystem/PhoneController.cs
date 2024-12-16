using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace PopUpSystem
{
    public class PhoneController : MonoBehaviour
    {
        SwipeDragDetector swipeDragDetector;
        private PhoneState phoneState = PhoneState.Closed;

        #region Animation Settings
        private PhoneAnimator phoneAnimator;

        [Header("Phone Animation Settings")]
        [SerializeField] private RectTransform popupTransform;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private float visibleHeight = 400f;
        #endregion

        #region Interactions
        [Header("Interactions")]
        private Interactions interactions;
        public UnityEvent OnInteractionCompleted;

        public enum InteractionType
        {
            None,
            SwipeUp,
            Touch,
            DoubleTouch
        }
        #endregion

        #region Touch
        [Header("Touch")]
        [SerializeField] private float doubleTouchThreshold = 0.1f;
        private float lastTouchTime = 0f;
        private bool isWaitingForSecondTouch = false;
        public UnityEvent<PhoneState> OnDoubleTouchAction;
        public UnityEvent<PhoneState> OnSingleTouchAction;
        #endregion

        public enum PhoneState
        {
            Open,
            Closed
        }

        private void Awake()
        {
            phoneAnimator = new PhoneAnimator(popupTransform, animationDuration, visibleHeight);
            swipeDragDetector = new SwipeDragDetector();
            interactions = GetComponent<Interactions>();
        }

        private void OnEnable()
        {
            swipeDragDetector.OnDragStartAction += StartDrag;
            swipeDragDetector.OnDragAction += HandleDrag;
            swipeDragDetector.OnSwipeAction += HandleSwipe;
            swipeDragDetector.OnTouchAction += HandleTouch;
        }

        private void OnDisable()
        {
            swipeDragDetector.OnDragStartAction -= StartDrag;
            swipeDragDetector.OnDragAction -= HandleDrag;
            swipeDragDetector.OnSwipeAction -= HandleSwipe;
            swipeDragDetector.OnTouchAction -= HandleTouch;
        }

        private void Update()
        {
            swipeDragDetector.DetectTouchOrMouse();
        }

        public void ClosePhone()
        {
            phoneAnimator.Close();
            phoneState = PhoneState.Closed;
        }

        private void StartDrag(Vector2 position)
        {
            phoneAnimator.SetStartDragPosition(position);
        }

        private void HandleDrag(Vector2 currentPosition)
        {

            phoneAnimator.Move(currentPosition);
        }

        private void HandleSwipe(SwipeDragDetector.SwipeDirection direction)
        {
            switch (direction)
            {
                case SwipeDragDetector.SwipeDirection.Up:
                    Debug.Log("Swipe Up detected");
                    if(phoneState == PhoneState.Open)
                    {
                        HandleInteraction(InteractionType.SwipeUp);
                    }
                    else
                    {
                        phoneAnimator.Open();
                        phoneState = PhoneState.Open;
                        interactions.SetActiveInteraction();
                    }
                    break;
                case SwipeDragDetector.SwipeDirection.Down:
                    Debug.Log("Swipe Down detected");
                    phoneAnimator.Close();
                    phoneState = PhoneState.Closed;
                    break;
                case SwipeDragDetector.SwipeDirection.Left:
                    Debug.Log("Swipe Left detected");
                    phoneAnimator.Close();
                    phoneState = PhoneState.Closed;
                    break;
                case SwipeDragDetector.SwipeDirection.Right:
                    Debug.Log("Swipe Right detected");
                    phoneAnimator.Close();
                    phoneState = PhoneState.Closed;
                    break;
                default:
                    phoneAnimator.Close();
                    phoneState = PhoneState.Closed;
                    break;
            }
        }
        private void HandleTouch()
        {
            if (phoneState == PhoneState.Closed)
            {
                HandleSingleTouch();
                return;
            }

            float currentTouchTime = Time.time;

            if (isWaitingForSecondTouch)
            {
                if (currentTouchTime - lastTouchTime <= doubleTouchThreshold)
                {
                    Debug.Log("Double Touch Detected!");
                    HandleDoubleTouch();
                    isWaitingForSecondTouch = false;
                }
            }
            else
            {
                isWaitingForSecondTouch = true;
                lastTouchTime = currentTouchTime;

                StartCoroutine(WaitForSingleTouch());
            }
        }

        private IEnumerator WaitForSingleTouch()
        {
            yield return new WaitForSeconds(doubleTouchThreshold);

            if (isWaitingForSecondTouch)
            {
                Debug.Log("Single Touch Detected!");
                HandleSingleTouch();
                isWaitingForSecondTouch = false;
            }
        }

        private void HandleSingleTouch()
        {
            Debug.Log("Handling Single Touch Logic");
            OnSingleTouchAction?.Invoke(phoneState);

            HandleInteraction(InteractionType.Touch);
        }

        private void HandleDoubleTouch()
        {
            Debug.Log("Handling Double Touch Logic");
            OnDoubleTouchAction?.Invoke(phoneState);

            HandleInteraction(InteractionType.DoubleTouch);
        }

        private void HandleInteraction(InteractionType type)
        {
            if (phoneState == PhoneState.Closed)
            {
                return;
            }

            if(type == interactions.GetActiveInteractionType())
            {
                OnInteractionCompleted?.Invoke();
                ClosePhone();
            }
        }
    }
}
