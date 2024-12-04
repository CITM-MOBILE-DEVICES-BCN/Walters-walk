using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PopUpSystem.PhoneController;

namespace PopUpSystem
{
    struct Interaction
    {
        public Interaction(GameObject go, InteractionType iType)
        {
            this.gameObject = go;
            interactionType = iType;
        }

        public GameObject gameObject;
        public InteractionType interactionType;
    }

    public class Interactions
    {
        private List<Interaction> _interactions;
        private Interaction activeInteraction;

        public Interactions(ref List<GameObject> interactionsGO)
        {
            for(int i = 0; i < interactionsGO.Count; i++)
            {
                InteractionType type = InteractionType.None;
                if (interactionsGO[i].name.Contains("SwipeUp"))
                {
                    Debug.Log("Interaction type is SwipeUp");
                    type = InteractionType.SwipeUp;
                }
                else if (interactionsGO[i].name.Contains("Touch"))
                {
                    Debug.Log("Interaction type is Touch");
                    type = InteractionType.Touch;
                }
                else if (interactionsGO[i].name.Contains("DoubleTouch"))
                {
                    Debug.Log("Interaction type is DoubleTouch");
                    type = InteractionType.DoubleTouch;
                }
                else
                {
                    Debug.Log("Interaction type is None");
                    type = InteractionType.None;
                }

                _interactions.Add(new Interaction(interactionsGO[i], type));

                _interactions[i].gameObject.SetActive(false);
            }
        }

        public InteractionType GetActiveInteractionType()
        {
            return activeInteraction.interactionType;
        }

        public void SetActiveInteraction()
        {
            activeInteraction.gameObject?.SetActive(false);

            activeInteraction = _interactions[Random.Range(0, _interactions.Count - 1)];
            activeInteraction.gameObject.SetActive(true);
        }
    }
}
