using UnityEngine;

namespace PopUpSystem
{
    public class PhoneController : MonoBehaviour
    {
        private PhoneAnimator phoneAnimator;

        private void Awake()
        {
            phoneAnimator = GetComponent<PhoneAnimator>();
        }

        private void OnEnable()
        {
            SwipeDragDetector.Instance.OnDragStartAction += StartDrag;
            SwipeDragDetector.Instance.OnDragAction += HandleDrag;
            SwipeDragDetector.Instance.OnSwipeAction += HandleSwipe;
        }

        private void OnDisable()
        {
            SwipeDragDetector.Instance.OnDragStartAction -= StartDrag;
            SwipeDragDetector.Instance.OnDragAction -= HandleDrag;
            SwipeDragDetector.Instance.OnSwipeAction -= HandleSwipe;
        }

        private void StartDrag(Vector2 position)
        {
            phoneAnimator.SetStartDragPosition(position);
        }

        private void HandleDrag(Vector2 currentPosition)
        {
            Debug.Log($"Dragging delta: {currentPosition}");

            // Mover el popup
            phoneAnimator.Move(currentPosition);
        }

        private void HandleSwipe(SwipeDragDetector.SwipeDirection direction)
        {
            // Lógica para manejar el swipe
            switch (direction)
            {
                case SwipeDragDetector.SwipeDirection.Up:
                    Debug.Log("Swipe Up detected");
                    phoneAnimator.Open();
                    break;
                case SwipeDragDetector.SwipeDirection.Down:
                    Debug.Log("Swipe Down detected");
                    phoneAnimator.Close();
                    break;
                case SwipeDragDetector.SwipeDirection.Left:
                    Debug.Log("Swipe Left detected");
                    phoneAnimator.Close();
                    break;
                case SwipeDragDetector.SwipeDirection.Right:
                    Debug.Log("Swipe Right detected");
                    phoneAnimator.Close();
                    break;
                default:
                    phoneAnimator.Close();
                    break;
            }
        }
    }
}
