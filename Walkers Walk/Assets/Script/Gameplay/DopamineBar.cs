using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DopamineBar : MonoBehaviour
{
    private Slider dopamineBar;
    [SerializeField] private float dopamineDecreaseRate = 0.1f;
    [SerializeField] private float dopamineIncreaseRate = 10f;
    [SerializeField] private UnityEvent OnDeath;

    private void Start()
    {
        dopamineBar = GetComponent<Slider>();
        dopamineBar.value = 100;
    }

    private void Update()
    {
        dopamineBar.value -= dopamineDecreaseRate * Time.deltaTime;
        if(dopamineBar.value <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void DopaminePlus()
    {
        dopamineBar.value += dopamineIncreaseRate;
    }
}
