using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PopUpSystem.PhoneController;

namespace PopUpSystem
{
    public struct Interaction
    {
        public Interaction(GameObject go, InteractionType iType)
        {
            this.gameObject = go;
            interactionType = iType;
        }

        public GameObject gameObject;
        public InteractionType interactionType;
    }

    public class Interactions : MonoBehaviour
    {
        [SerializeField] private List<GameObject> interactionsGO;
        private List<Interaction> _interactions = new List<Interaction>();
        private Interaction activeInteraction;

        private void Awake()
        {
            InitializeInteractions();
        }

        private void InitializeInteractions()
        {
            for (int i = 0; i < interactionsGO.Count; i++)
            {
                InteractionType type = InteractionType.None;

                if (interactionsGO[i].name.Contains("SwipeUp"))
                {
                    Debug.Log("Interaction type is SwipeUp");
                    type = InteractionType.SwipeUp;
                }
                else if (interactionsGO[i].name.Contains("DoubleTouch"))
                {
                    Debug.Log("Interaction type is DoubleTouch");
                    type = InteractionType.DoubleTouch;
                }
                else if (interactionsGO[i].name.Contains("Touch"))
                {
                    Debug.Log("Interaction type is Touch");
                    type = InteractionType.Touch;
                }
                else
                {
                    Debug.Log("Interaction type is None");
                    type = InteractionType.None;
                }

                _interactions.Add(new Interaction(interactionsGO[i], type));

                interactionsGO[i].SetActive(false);

            }
            interactionsGO = null;
        }

        public InteractionType GetActiveInteractionType()
        {
            return activeInteraction.interactionType;
        }

        public void SetActiveInteraction()
        {
            if (activeInteraction.gameObject != null)
            {
                activeInteraction.gameObject.SetActive(false);
            }

            activeInteraction = _interactions[Random.Range(0, _interactions.Count)];
            activeInteraction.gameObject.SetActive(true);
        }
    }
}
