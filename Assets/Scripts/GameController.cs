using Jacovone;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using LitJson;
using System;
using System.Linq;
using DG.Tweening;

public class GameController : MonoBehaviour
{


    public BlurryController Blurry;
    public PathMagic CameraPath;
    public VideoPlayer videoPlayer;
    public VideoPlayer StartVideoPlayer;
    //the number of answered right question
    [HideInInspector]
    public int RightNum;
    public int CurrentMoney;
    //the sequence index number of current question
    public int CurrentQuestSequence;
    //[HideInInspector]
    public GameObject CurrentQuestion;
    public int TotalQuestNum = 15;
    public int TotoalEasyQuestNum = 15;
    public int TotoalMediumQuestNum = 0;
    public int TotoalHardQuestNum = 0;

    public int CurrentAnswer;
    public List<int> Rewards;

    //the index of currently available number
    public List<int> currentAvailableEasyIndex = new List<int>();
    public List<int> currentAvailableMediumIndex = new List<int>();
    public List<int> currentAvailableHardIndex = new List<int>();

    public List<GameObject> EasyQuestionPool;
    public List<GameObject> MediumQuestionPool;
    public List<GameObject> HardQuestionPool;
    [HideInInspector]
    public bool ButtonClicked;

    private const float QUESTIONTIME = 20f;

    private float quetionTimer;
    private bool isMuted;
    [HideInInspector]
    public bool isTimeAccouting;
    public Dictionary<int, QuestionData> AllQuestionData = new Dictionary<int, QuestionData>();

    static GameController instance;
    public static GameController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(GameController)) as GameController;
                if (instance == null)
                {
                    GameObject obj = new GameObject("GameController");
                    instance = obj.AddComponent<GameController>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    public class QuestionData
    {
        public int QuestionID;
        public string QuestionText;
        public string AnswerText;
        public string OptionA;
        public string OptionB;
        public string OptionC;
        public string OptionD;
        public int Answer;

    }

    private void Awake()
    {
        // IntializeVideoPlayer();




    }


    public void ReadAllQuestionData()
    {
        TextAsset asset = Resources.Load("Json/QuestionData") as TextAsset;
        if (!asset)
        {
            Debug.Log("QuestionData JSON doesn't exist");
            return;
        }

        string strScenarioData = asset.text;

        JSONReadFormatConvert();


        QuestionData[] leveldataArray = JsonMapper.ToObject<QuestionData[]>(strScenarioData);

        foreach (var item in leveldataArray)
        {
            if (!AllQuestionData.ContainsKey(item.QuestionID))
            {
                AllQuestionData.Add(item.QuestionID, item);
            }

        }
    }



    public void JSONReadFormatConvert()
    {
        JsonMapper.RegisterImporter<int, string>((int value) =>
        {
            return value.ToString();
        });

        JsonMapper.RegisterImporter<double, string>((double value) =>
        {
            return value.ToString();
        });


        JsonMapper.RegisterImporter<bool, string>((bool value) =>
        {
            return value.ToString();
        });
        JsonMapper.RegisterImporter<string, int>((string value) =>
        {
            if (value == "")
            {
                value = "0";
            }
            return Convert.ToInt32(value);
        });

        JsonMapper.RegisterImporter<double, int>((double value) =>
        {
            return Convert.ToInt32(value);
        });
    }
    // Start is called before the first frame update
    void Start()
    {
        //initilaize initalIndexList

        // GetCurrentAvailableIndex(EasyQuestionPool, currentAvailableEasyIndex);
        //GetCurrentAvailableIndex(MediumQuestionPool, currentAvailableMediumIndex);
        //GetCurrentAvailableIndex(HardQuestionPool, currentAvailableHardIndex);
        // StartVideoPlayer.Play();
        //  IntializeVideoPlayer();

        //used for wb start video play automatically
        // UIManager.Instance.StartBg.onClick.Invoke();
        ReadAllQuestionData();
        RegisterDisableButtonEvent();
    }

    private void Update()
    {
        if (!isTimeAccouting)
        {
            return;
        }

        if (quetionTimer > 0)
        {
            quetionTimer -= Time.deltaTime;
        }


        UIManager.Instance.TimerBar.fillAmount = quetionTimer / QUESTIONTIME;
        UIManager.Instance.TimerBar2.fillAmount = quetionTimer / QUESTIONTIME;

        if (quetionTimer < 0)
        {
            quetionTimer = 0;
            //TimeOut();

        }
    }
    public void GenerateQuestion()
    {

        if (CurrentQuestSequence == TotalQuestNum )
        {
            StartCoroutine(CompleteGame());
            return;
        }
        Blurry.PlayQuestion();
        AudioController.Instance.PlayButtonSFX(AudioController.Instance.StartAndRestartButtonSFX);
        quetionTimer = QUESTIONTIME;
        ButtonClicked = false;
        UIManager.Instance.NextButton.SetActive(false);
        UIManager.Instance.Buttons[0].transform.parent.gameObject.SetActive(true); //Button UI show
        CurrentQuestSequence++;
        int i = UnityEngine.Random.Range(0, AllQuestionData.Count);
        // Debug.Log("i:" +i);
        int k = AllQuestionData.ElementAt(i).Key;
        // Debug.Log( "k:" + k);
        UIManager.Instance.ScreenText.text = AllQuestionData[k].QuestionText;
        UIManager.Instance.ScreenText.color = UIManager.Instance.QuestionColor;
        UIManager.Instance.AnswerText = AllQuestionData[k].AnswerText;
        UIManager.Instance.OptionA.text = AllQuestionData[k].OptionA;
        UIManager.Instance.OptionB.text = AllQuestionData[k].OptionB;
        UIManager.Instance.OptionC.text = AllQuestionData[k].OptionC;
        UIManager.Instance.OptionD.text = AllQuestionData[k].OptionD;

        UIManager.Instance.QNum.transform.parent.gameObject.SetActive(true);
        UIManager.Instance.QNum.gameObject.SetActive(true);
        UIManager.Instance.QNum2.gameObject.SetActive(true);
        UIManager.Instance.QNum.text = "Q" + CurrentQuestSequence.ToString();
        UIManager.Instance.QNum2.text =  CurrentQuestSequence.ToString() + "/15";

        if (UIManager.Instance.OptionC.text == "")
            UIManager.Instance.OptionC.transform.parent.gameObject.SetActive(false);
        else UIManager.Instance.OptionC.transform.parent.gameObject.SetActive(true);
        if (UIManager.Instance.OptionD.text == "")
            UIManager.Instance.OptionD.transform.parent.gameObject.SetActive(false);
        else UIManager.Instance.OptionD.transform.parent.gameObject.SetActive(true);
        CurrentAnswer = AllQuestionData[k].Answer;

        UIManager.Instance.TimerBar.fillAmount = 1;
        UIManager.Instance.TimerBar2.fillAmount = 1;

        UIManager.Instance.ScreenText.gameObject.SetActive(true);

        AudioController.Instance.PlayEventSFX(AudioController.Instance.NewQuestSFX);
        DisableAnswersandTimer(false);
        InitializeAllButtons();
        StartCoroutine(StartShowTimerBehaviour());
        AllQuestionData.Remove(k);
    }

    public void AnswerQuestion(int index)
    {
        Vector3 initialScale = UIManager.Instance.ScreenText.transform.localScale;

        UIManager.Instance.ScreenText.text = UIManager.Instance.AnswerText;
        UIManager.Instance.ScreenText.color = UIManager.Instance.AnswerTextColor;
        UIManager.Instance.ScreenText.transform.DOPunchScale(new Vector3(0.05f, 0.05f, 0.05f), 0.5f, 5);
        UIManager.Instance.NextButton.SetActive(true);
        if (index == CurrentAnswer)
        {
            AnsweRight();
        }
        else AnswerWrong();
    }

    public void GetCurrentAvailableIndex(List<GameObject> questPool, List<int> currentAvailableIndex)
    {
        for (int i = 0; i < questPool.Count; i++)
        {


            NewRandom:

            int randomIndex = UnityEngine.Random.Range(0, questPool.Count);

            if (!currentAvailableIndex.Contains(randomIndex))
            {
                currentAvailableIndex.Add(randomIndex);
            }
            else goto NewRandom;



        }

        //get read of keep apart ones;

        for (int i = 0; i < currentAvailableIndex.Count; i++)
        {
            if (questPool[currentAvailableIndex[i]].GetComponent<Question>().KeepApartQuestion.Count >= 1)
            {
                //Debug.Log(questPool[currentAvailableIndex[i]].name + "  ddd");
                for (int j = 0; j < questPool[currentAvailableIndex[i]].GetComponent<Question>().KeepApartQuestion.Count; j++)
                {
                    if (currentAvailableIndex.Contains(questPool.IndexOf(questPool[currentAvailableIndex[i]].GetComponent<Question>().KeepApartQuestion[j])))
                    {
                        int currentCheckIndex = currentAvailableIndex[i];
                        //   Debug.Log(currentCheckIndex);
                        GameObject currentApartObj = questPool[currentCheckIndex].GetComponent<Question>().KeepApartQuestion[j];
                        //   Debug.Log(currentApartObj.name);
                        //    Debug.Log(questPool.IndexOf(currentApartObj));
                        currentAvailableIndex.Remove(questPool.IndexOf(questPool[currentAvailableIndex[i]].GetComponent<Question>().KeepApartQuestion[j]));
                    }
                }
            }
        }



    }
    private void IntializeVideoPlayer()
    {
        videoPlayer.url = Path.Combine(Application.streamingAssetsPath, "Video Screen Background.mp4");
        StartVideoPlayer.url = Path.Combine(Application.streamingAssetsPath, "ATS Start Screen.mp4");
        StartVideoPlayer.playOnAwake = true;
        StartVideoPlayer.isLooping = true;
        StartVideoPlayer.Play();

    }

    public void StartVideoPlay()
    {
        StartVideoPlayer.Play();
    }
    public void GamePause()
    {
        Time.timeScale = 0;
        UIManager.Instance.PausePanelShow();
    }

    public void GameStart()
    {
        UIManager.Instance.Stargraphic.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f, 5);
        StartCoroutine(GameStartBehavior());
        GlobalVariable.Instance.IsLoaded = true;
        AudioController.Instance.PlayButtonSFX(AudioController.Instance.StartAndRestartButtonSFX);
    }
    public IEnumerator GameStartBehavior()
    {
        StartCoroutine(UIManager.Instance.StartPanelHide());
        //videoPlayer.Play();
        //UIManager.Instance.ReArrangeBgResource();
        yield return new WaitUntil(() => UIManager.Instance.StartPanel.activeSelf == false);
        CameraPath.Rewind();
        CameraPath.Play();
        Blurry.PlayIntro();
        //called in magicpath
        // GenerateQuestion();
        AudioController.Instance.PlayEventSFX(AudioController.Instance.IntroSFX);
        // Invoke("BackGroundSoundPlay",4.5f);

    }

    public void BackGroundSoundPlay()
    {
        AudioController.Instance.BackgourndFXAudioSource.Play();
    }

    public void GameResume()
    {
        Time.timeScale = 1;
        UIManager.Instance.PausePanelHide();
    }

    public void GameRestart()
    {
        UIManager.Instance.Endraphic.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f, 5).OnComplete(() => UIManager.Instance.ResultPanel.SetActive(false));
       
        AudioController.Instance.PlayButtonSFX(AudioController.Instance.StartAndRestartButtonSFX);
        AllQuestionData.Clear();
        ReadAllQuestionData();
        UIManager.Instance.ScreenText.gameObject.SetActive(false);
        DisableAnswersandTimer(false);
        StartCoroutine(GameStartBehavior());
        RightNum = 0;
        UIManager.Instance.NextButton.SetActive(false);
        UIManager.Instance.ResetLoseIcon();
        CurrentQuestSequence = 0;
        UIManager.Instance.QNum.transform.parent.gameObject.SetActive(false);
        Time.timeScale = 1;
        UIManager.Instance.QNum2.gameObject.SetActive(false);
        UIManager.Instance.QNum2.text = CurrentQuestSequence.ToString() + "1/15";
        UIManager.Instance.QNum2.color = Color.white;
        // Scene scene = SceneManager.GetActiveScene();
        // SceneManager.LoadScene(scene.name);
    }


    public void TimeOut()
    {
        TimeOutWrong();
    }

    public void GameFinish()
    {

        Invoke("AnswerLastWrongQuestion", 0.5f);
    }

    public void GenerateQuestion1()
    {
        ButtonClicked = false;

        if (CurrentQuestSequence < 15)
        {
            GenerateQuestFromPool(EasyQuestionPool, currentAvailableEasyIndex);
        }

        // else
        // if (CurrentQuestSequence > 5 && CurrentQuestSequence <= 10)
        // {
        // 
        //     GenerateQuestFromPool(MediumQuestionPool, currentAvailableMediumIndex);
        // }
        // else
        // if (CurrentQuestSequence > 10 && CurrentQuestSequence < 15)
        // {
        //     GenerateQuestFromPool(HardQuestionPool, currentAvailableHardIndex);
        // }
        else
            GameFinish();
    }

    public void GenerateQuestFromPool(List<GameObject> QuestionPool, List<int> currentRamdomQuestList)
    {
        if (QuestionPool.Count >= 1)
        {
            // Debug.Log(QuestionPool[currentRamdomQuestList[0]].name);
            QuestionPool[currentRamdomQuestList[0]].GetComponent<Question>().FadeIn();
            QuestionPool[currentRamdomQuestList[0]].SetActive(true);
            CurrentQuestion = QuestionPool[currentRamdomQuestList[0]];
            // Debug.Log(QuestionPool[currentRamdomQuestList[0]]);
            currentRamdomQuestList.Remove(currentRamdomQuestList[0]);
            CurrentQuestSequence++;
            AudioController.Instance.PlayEventSFX(AudioController.Instance.NewQuestSFX);
            Blurry.PlayQuestion();
        }
    }

    public void AnsweRight()
    {
        if (ButtonClicked)
        {
            return;
        }
        ButtonClicked = true;
        DisableTimer();
        UIManager.Instance.SetInitalRewardValue(UIManager.Instance.RollingNumber, CurrentMoney, CurrentMoney + Rewards[RightNum]);
        CurrentMoney += Rewards[RightNum];
        RightNum++;
        Blurry.PlayYes();

        UIManager.Instance.UpdateUIAfterAnwser();
        AudioController.Instance.PlayButtonSFX(AudioController.Instance.RightSFX);
        // if (CurrentQuestion.GetComponent<Question>().PopUpInfo)
        // {
        //     CurrentQuestion.GetComponent<Question>().ShowPopUpInfo();
        //     return;
        // }
        // else
        //
        // {
        //
        //win game
        if (CurrentQuestSequence == TotalQuestNum)
        {
            if (RightNum == TotalQuestNum)
            {
                //  Blurry.PlayCelebration();

            }

           // StartCoroutine(CompleteGame());
        }

        // else
        // {
        //     Invoke("GenerateQuestion", 7.15f);
        //    // StartCoroutine(CurrentQuestion.GetComponent<Question>().FadeOut(6.15f));
        // }



        // }

    }


    public void AnswerWrong()
    {
        if (ButtonClicked)
        {
            return;
        }
        ButtonClicked = true;
        DisableTimer();
        UIManager.Instance.ChangeLoseIcon();
        UIManager.Instance.UpdateUIAfterAnwser();
        Blurry.PlayNo();
        AudioController.Instance.PlayButtonSFX(AudioController.Instance.WrongSFX);

        // if (CurrentQuestion.GetComponent<Question>().PopUpInfo)
        // {
        //     CurrentQuestion.GetComponent<Question>().ShowPopUpInfo();
        //     return;
        // }

        //if (UIManager.Instance.NoIcons)
        //{
        //    UIManager.Instance.NextButton.SetActive(false);
        //    GameFinish();
        //}
        //  else
        //  {
        //      Invoke("GenerateQuestion", 5.5f);
        //    //  StartCoroutine(CurrentQuestion.GetComponent<Question>().FadeOut(4.5f));
        //  }



    }

    public void TimeOutWrong()
    {
        DisableAllButtons();
        DisableTimer();
        UIManager.Instance.ChangeLoseIcon();
        UIManager.Instance.UpdateUIAfterAnwser();
        Blurry.PlayNo();
        AudioController.Instance.PlayButtonSFX(AudioController.Instance.WrongSFX);
        Invoke("GenerateQuestion", 5.5f);

       // if (UIManager.Instance.NoIcons)
       // {
       //     GameFinish();
       // }
       //
       // else
       // {
       //     Invoke("GenerateQuestion", 5.5f);
       //     //UIManager.Instance.ScreenText.gameObject.SetActive(false);
       //     // StartCoroutine(CurrentQuestion.GetComponent<Question>().FadeOut(4.5f));
       // }
       //


    }
    public void AnswerLastWrongQuestion()
    {
        //  CurrentQuestion.SetActive(false);

        {
            AudioController.Instance.PlayEventSFX(AudioController.Instance.LoseSFX);
            UIManager.Instance.ResultPanelShow();
        }


    }

    public IEnumerator CompleteGame()
    {
        AudioController.Instance.FadeOut(AudioController.Instance.BackgourndFXAudioSource, 2f);
        //if (RightNum == TotalQuestNum)
        //{
        //    AudioController.Instance.PlayEventSFX(AudioController.Instance.WinSFX);
        //    yield return new WaitForSeconds(4f);
        //
        //}

        UIManager.Instance.ResultPanelShow();

        yield break;
    }

    public void DisableTimer()
    {

        isTimeAccouting = false;

    }



    private void DisableAnswersandTimer(bool isshow)
    {
        for (int i = 0; i < UIManager.Instance.Buttons.Count; i++)
        {
            if (isshow)
            {
                UIManager.Instance.Buttons[i].transform.localScale = new Vector3(0.4282626f, 0.4282626f, 0.4282626f);
            }
            else UIManager.Instance.Buttons[i].transform.localScale = Vector3.zero;


        }

        UIManager.Instance.TimerBar.gameObject.SetActive(isshow);
        UIManager.Instance.TimerBar2.gameObject.SetActive(isshow);

        isTimeAccouting = isshow;
    }

    private void RegisterDisableButtonEvent()
    {
        for (int i = 0; i < UIManager.Instance.Buttons.Count; i++)
        {
            //*** cannot use DisableOtherButtons(i) , as i will change when the loop goes, and stops at i == 2 so it will change all the DisableOtherButtons
            int index = i;
            UIManager.Instance.Buttons[i].GetComponent<Button>().onClick.AddListener(() => DisableOtherButtonAndShowSprite(index));
        }
    }

    public void DisableOtherButtonAndShowSprite(int buttonIndex)
    {

        for (int i = 0; i < UIManager.Instance.Buttons.Count; i++)
        {

            UIManager.Instance.Buttons[i].GetComponent<Button>().interactable = false;
            //lock the sprite state
            if (i == buttonIndex)
            {
                Sprite selectedImage = UIManager.Instance.Buttons[i].GetComponent<Button>().spriteState.selectedSprite;
                // wright 
                if (CurrentAnswer != i + 1)
                {
                    UIManager.Instance.Buttons[i].GetComponent<Button>().image.sprite = selectedImage;
                    UIManager.Instance.Buttons[i].GetComponent<Button>().image.color = new Color(Color.red.r, Color.red.g, Color.red.b, 0.5f);
                    UIManager.Instance.Buttons[i].GetComponentInChildren<Text>().color = new Color(Color.white.r, Color.white.g, Color.white.b, 0.5f);
                }
                // else
                // {
                //     UIManager.Instance.Buttons[i].GetComponent<Button>().image.sprite = (Sprite)Resources.Load("Sprites/Button Correct", typeof(Sprite));
                // }

            }
            else
            {
                UIManager.Instance.Buttons[i].GetComponent<Button>().image.color = new Color(Color.white.r, Color.white.g, Color.white.b, 0.5f);
                UIManager.Instance.Buttons[i].GetComponentInChildren<Text>().color = UIManager.Instance.QuestionColor;
            }

        }

        UIManager.Instance.Buttons[CurrentAnswer - 1].GetComponent<Button>().image.sprite = (Sprite)Resources.Load("Sprites/Button Correct", typeof(Sprite));
        UIManager.Instance.Buttons[CurrentAnswer - 1].GetComponentInChildren<Text>().color = Color.white;
        // UIManager.Instance.Buttons[CurrentAnswer - 1].GetComponent<Button>().image.color = new Color(Color.white.r, Color.white.g, Color.white.b, 1f);
    }


    public void DisableAllButtons()
    {
        for (int i = 0; i < UIManager.Instance.Buttons.Count; i++)
        {

            UIManager.Instance.Buttons[i].GetComponent<Button>().interactable = false;
        }
    }

    public void InitializeAllButtons()
    {
        for (int i = 0; i < UIManager.Instance.Buttons.Count; i++)
        {
            UIManager.Instance.Buttons[i].transform.localScale = new Vector3(0.4282626f, 0.4282626f, 0.4282626f);
            UIManager.Instance.Buttons[i].GetComponent<Button>().interactable = true;
            UIManager.Instance.Buttons[i].GetComponent<Image>().sprite = (Sprite)Resources.Load("Sprites/Button", typeof(Sprite));
            UIManager.Instance.Buttons[i].GetComponent<Button>().image.color = new Color(Color.white.r, Color.white.g, Color.white.b, 1f);
            UIManager.Instance.Buttons[i].GetComponentInChildren<Text>().color = UIManager.Instance.QuestionColor;
        }
    }
    IEnumerator StartShowTimerBehaviour()
    {
        // yield return new WaitForSeconds(2f);

        //  for (int i = 0; i < UIManager.Instance.Buttons.Count; i++)
        //  {
        //      UIManager.Instance.Buttons[i].SetActive(false);
        //  }
        DisableAnswersandTimer(true);

        yield break;
    }

    public void NextButtonClick()
    {
        GenerateQuestion();
       // if (UIManager.Instance.NoIcons)
       // {
       //     GameFinish();
       // }
       // else
       // {
       //     GenerateQuestion();
       // }

    }

    public void MuteChange()

    {
        isMuted = !isMuted;
        if (isMuted)
        {
            UIManager.Instance.MuteButton.sprite = (Sprite)Resources.Load("Sprites/AudioOff", typeof(Sprite));
            AudioListener.volume = 0;
        }
        else
        {
            AudioListener.volume = 1;
            UIManager.Instance.MuteButton.sprite = (Sprite)Resources.Load("Sprites/AudioOn", typeof(Sprite));
        }

    }

}