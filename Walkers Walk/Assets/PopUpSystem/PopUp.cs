using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PopUpSystem
{
    public class PopUp : MonoBehaviour
    {
        [SerializeField] private RectTransform popupTransform; // Asigna el RectTransform del popup
        [SerializeField] private float animationDuration = 0.5f; // Duración de la animación
        [SerializeField] private float visibleHeight = 0f; // Altura visible (posición final del popup en pantalla)

        void Start()
        {
            Spawn();
        }

        private void Spawn()
        {
            popupTransform.DOMoveY(visibleHeight, animationDuration).SetEase(Ease.InOutSine);
        }

        public void Close()
        {
            popupTransform.DOMoveY(-visibleHeight, animationDuration)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}
