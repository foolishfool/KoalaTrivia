using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class Question : MonoBehaviour
{
    // Start is called before the first frame update
    public int QuestID;
    public Image TimerBar;
    public Image TimerBar2;
    public GameObject PopUpInfo;
    public List<GameObject> KeepApartQuestion = new List<GameObject>();
    public Text RewardText;
    public TextMeshProUGUI QuestionText;
    public TextMeshProUGUI PopText;
    public Text OKText;
    public Text QuestionNum;
    public List<GameObject> Buttons;

    private const float QUESTIONTIME = 20f;

    private float quetionTimer;
    [HideInInspector]
    public bool isTimeAccouting;
    void Start()
    {
        DisableAnswersandTimer(false);

        quetionTimer = QUESTIONTIME;
     //   RewardText.text = "$" + GameController.Instance.Rewards[GameController.Instance.RightNum].ToString();
        RegisterDisableButtonEvent();

        StartCoroutine(StartShowTimerBehaviour());

        CenterlizeText();
        QuestionNum.text = "Q" + GameController.Instance.CurrentQuestSequence.ToString();
    }


    public void CenterlizeText()
    {
       //RewardText.alignment = TextAnchor.MiddleCenter;
       //QuestionText.alignment = TextAnchor.MiddleCenter;
       //PopText.alignment = TextAnchor.MiddleCenter;
        OKText.alignment = TextAnchor.MiddleCenter;
        for (int i = 0; i < Buttons.Count; i++)
        {
            Buttons[i].transform.GetChild(0).GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!isTimeAccouting)
        {
            return;
        }

        if (quetionTimer >0)
        {
            quetionTimer -= Time.deltaTime;
        }
      

        TimerBar.fillAmount = quetionTimer / QUESTIONTIME;
        TimerBar2.fillAmount = quetionTimer / QUESTIONTIME;

        if (quetionTimer < 0)
        {
            quetionTimer = 0;
            GameController.Instance.TimeOut();
        
        }
    }

    private void DisableAnswersandTimer(bool isshow)
    {
        for (int i = 0; i < Buttons.Count; i++)
        {
            Buttons[i].SetActive(isshow);
        }

        TimerBar.gameObject.SetActive(isshow);
        TimerBar2.gameObject.SetActive(isshow);

        isTimeAccouting = isshow;
    }

    IEnumerator StartShowTimerBehaviour()
    {
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < Buttons.Count; i++)
        {
            Buttons[i].SetActive(false);
        }
        DisableAnswersandTimer(true);

        yield break;
    }

    public void ShowPopUpInfo()
    {
        PopUpInfo.SetActive(true);
        PopUpInfo.transform.DOMoveY(PopUpInfo.transform.position.y + 20, 5.15f).OnComplete(()=> PopUpInfo.transform.Find("OKButton").gameObject.SetActive(true));

    }

    //called on ui button click event
    public void OkButtonClick()
    {
        StartCoroutine(OkButtonClickBehavior());
    }


    public IEnumerator OkButtonClickBehavior()
    {
      //  StartCoroutine(GameController.Instance.CurrentQuestion.GetComponent<Question>().FadeOut(0f));
       //wait 1 secons for fade out
        yield return new WaitForSeconds(1f);
        if (GameController.Instance.CurrentQuestSequence == GameController.Instance.TotalQuestNum)
        {
            GameController.Instance.StartCoroutine(GameController.Instance.CompleteGame());
            yield break;
        }


        //win game
        //  if (GameController.Instance.CurrentQuestSequence == GameController.Instance.TotalQuestNum)
        //  {
        //      if (GameController.Instance.RightNum == GameController.Instance.TotalQuestNum)
        //      {
        //          GameController.Instance.Blurry.PlayCelebration();
        //          StartCoroutine(GameController.Instance.CompleteGame());
        //
        //      }
        //      else GameController.Instance.GameFinish();
        //  }

        // if (UIManager.Instance.NoIcons)
        // {
        //     GameController.Instance.GameFinish();
        // }
        // else
        // {
        //     GameController.Instance.GenerateQuestion();
        // }
        GameController.Instance.GenerateQuestion();
        yield break;
    }

    private void RegisterDisableButtonEvent()
    {
        for (int i = 0; i < Buttons.Count; i++)
        {
            //*** cannot use DisableOtherButtons(i) , as i will change when the loop goes, and stops at i == 2 so it will change all the DisableOtherButtons
            int index = i;
            Buttons[i].GetComponent<Button>().onClick.AddListener(() => DisableOtherButtons(index));
        }
    }


    public void DisableOtherButtons(int buttonIndex)
    {
 
        for (int i = 0; i < Buttons.Count; i++)
        {

            Buttons[i].GetComponent<Button>().interactable = false;
            //lock the sprite state
            if (i == buttonIndex)
            {
                Sprite selectedImage = Buttons[i].GetComponent<Button>().spriteState.selectedSprite;
                Buttons[i].GetComponent<Button>().image.sprite = selectedImage;
            }
        }
    }


    public void DisableAllButtons()
    {
        for (int i = 0; i < Buttons.Count; i++)
        {

            Buttons[i].GetComponent<Button>().interactable = false;
        }
    }

    public IEnumerator FadeOut(float timer)
    {
        yield return new WaitForSeconds(timer);
        foreach (var item in GetComponentsInChildren<Text>())
        {
            item.material.DOFade(0,1f);
        }

        foreach (var item in GetComponentsInChildren<TextMeshProUGUI>())
        {
            item.material.DOFade(0, 1f);
        }


        foreach (var item in GetComponentsInChildren<Image>())
        {
            item.material.DOFade(0, 1f);
        }

        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);

        yield break;
    }


    public void FadeIn()
    {
        foreach (var item in GetComponentsInChildren<Text>())
        {
            item.material.DOFade(1, 1f);
        }

        foreach (var item in GetComponentsInChildren<Image>())
        {
            item.material.DOFade(1, 1f);
        }

        foreach (var item in GetComponentsInChildren<TextMeshProUGUI>())
        {
            item.material.DOFade(1, 1f);
        }

    }
}
