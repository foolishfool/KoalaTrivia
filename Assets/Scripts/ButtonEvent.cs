using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public int ButtonSFXID;

    public Color HoverColor;

    private Vector3 initialScale;
    // Start is called before the first frame update
    void Start()
    {
        initialScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // When highlighted with mouse.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameController.Instance.ButtonClicked)
        {
            transform.localScale = initialScale;
            //GetComponent<Image>().color = Color.white;
            return;
        }

        GetComponent<Image>().color = HoverColor;
        transform.localScale = initialScale * 1.1f;
       // if (ButtonSFXID == 0)
       // {
       //     AudioController.Instance.PlayButtonSFX(AudioController.Instance.ButtonSFX1 );
       // }
       // else AudioController.Instance.PlayButtonSFX(AudioController.Instance.ButtonSFX2);


    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameController.Instance.ButtonClicked)
        {
            transform.localScale = initialScale;
            //GetComponent<Image>().color = Color.white;
            return;
        }
        transform.localScale = initialScale;
        GetComponent<Image>().color = Color.white;
    }

}
