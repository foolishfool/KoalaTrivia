using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonEvent : MonoBehaviour, IPointerEnterHandler
{

    public int ButtonSFXID;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // When highlighted with mouse.
    public void OnPointerEnter(PointerEventData eventData)
    {
        return; 
        if (GameController.Instance.ButtonClicked && gameObject.name != "NextButton")
        {
            return;
        }
        if (ButtonSFXID == 0)
        {
            AudioController.Instance.PlayButtonSFX(AudioController.Instance.ButtonSFX1 );
        }
        else AudioController.Instance.PlayButtonSFX(AudioController.Instance.ButtonSFX2);

 
    }


}
