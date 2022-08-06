using OldMoatGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{

    static UIManager instance;

    public GameObject PausePanel;
    public GameObject StartPanel;
    public GameObject ResultPanel;
    public Button StartBg;
    public RollingNumbers RollingNumber;
    public Text CurrentQuestNum;
    public Text CurrentMoneyInGame;
    public Color QuestionColor;
    public Color AnswerTextColor;
    public TextMeshProUGUI ScreenText;
    public string AnswerText;
    public TextMeshProUGUI QNum;
    public TextMeshProUGUI QNum2;
    public TextMeshProUGUI ResultText;
    public GameObject NextButton;
    public GameObject Stargraphic;
    public GameObject Endraphic;
    public Text OptionA;
    public Text OptionB;
    public Text OptionC;
    public Text OptionD;

    public Image TimerBar;
    public Image TimerBar2;
    public Image MuteButton;
    public Text CurrentMoneyInResult;
    public Text ResultQuestDetails;
    public Transform QuestionPos;
    public RenderTexture videoRenderTexture;
    public Texture startScreenTexture;
    public bool NoIcons;
    public List<GameObject> ChangesIcons = new List<GameObject>();

    public bool Loaded;

    public List<GameObject> Buttons;

    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(UIManager)) as UIManager;
                if (instance == null)
                {
                    GameObject obj = new GameObject("UIManager");
                    instance = obj.AddComponent<UIManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }



    // Start is called before the first frame update
    void Start()
    {

        if (GlobalVariable.Instance.IsLoaded)
        {
           // ReArrangeBgResource();
        }

        UpdateUIAfterAnwser();
        Buttons[0].transform.parent.gameObject.SetActive(false);

        Invoke("Bounce", 1f);

    }

    public void Bounce()
    {
        Stargraphic.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f, 5);
        Endraphic.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f, 5);
    }

    public static List<GameObject> GetDontDestroyOnLoadObjects()
    {
        List<GameObject> result = new List<GameObject>();

        List<GameObject> rootGameObjectsExceptDontDestroyOnLoad = new List<GameObject>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            rootGameObjectsExceptDontDestroyOnLoad.AddRange(SceneManager.GetSceneAt(i).GetRootGameObjects());
        }

        List<GameObject> rootGameObjects = new List<GameObject>();
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < allTransforms.Length; i++)
        {
            Transform root = allTransforms[i].root;
            if (root.hideFlags == HideFlags.None && !rootGameObjects.Contains(root.gameObject))
            {
                rootGameObjects.Add(root.gameObject);
            }
        }

        for (int i = 0; i < rootGameObjects.Count; i++)
        {
            if (!rootGameObjectsExceptDontDestroyOnLoad.Contains(rootGameObjects[i]))
                result.Add(rootGameObjects[i]);
        }

        //foreach( GameObject obj in result )
        //    Debug.Log( obj );

        return result;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PausePanelShow()
    {
        PausePanel.SetActive(true);
    }

    public void PausePanelHide()
    {
        PausePanel.SetActive(false);
    }

    public IEnumerator StartPanelHide()
    {
        yield return new WaitForSeconds(0.5f);
        //ReArrangeBgResource();
        StartPanel.SetActive(false);
        Loaded = true;

    }

    public void ResultPanelShow()
    {
        AudioController.Instance.PlayButtonSFX(AudioController.Instance.WinSFX);
        ResultPanel.SetActive(true);
        GameController.Instance.PlayEndVideoPlayer();
        Endraphic.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f, 5);
        SetInfoForResultPanel();
        QNum2.color = UIManager.Instance.QuestionColor;
    }
    public void SetInfoForResultPanel()
    {
        CurrentMoneyInResult.text = "$" + GameController.Instance.CurrentMoney.ToString();
        ResultQuestDetails.text = "you got " + GameController.Instance.RightNum.ToString()+ "/15 correct!";
        if (GameController.Instance.RightNum == 15)
        {
            ResultText.text = "Congratulations!! You are koalafied.";
        }
        if (GameController.Instance.RightNum <= 14 && GameController.Instance.RightNum >= 12)
        {
            ResultText.text = "You are well on your way to being koalafied!";
        }

        if (GameController.Instance.RightNum <= 11 && GameController.Instance.RightNum >= 7)
        {
            ResultText.text = "Don’t give up. Try again to see if you can become koalafied!";
        }


        if (GameController.Instance.RightNum <= 6 && GameController.Instance.RightNum >= 0)
        {
            ResultText.text = "It’s great to see you’re learning. Try again.";
        }
    }

    public void ChangeLoseIcon()
    {
        for (int i = 0; i < ChangesIcons.Count; i++)
        {
            if (!ChangesIcons[i].transform.GetChild(0).gameObject.activeSelf)
            {
                ChangesIcons[i].transform.DOShakePosition(1f,5f);
                ChangesIcons[i].transform.GetChild(0).gameObject.SetActive(true);
                if (i== ChangesIcons.Count -1)
                {
                    NoIcons = true ;
                }
                return;
            }
        }

        
    }

    public void ResetLoseIcon()
    {
        NoIcons = false;
        for (int i = 0; i < ChangesIcons.Count; i++)
        {
            ChangesIcons[i].transform.GetChild(0).gameObject.SetActive(false);
        }

    }

    public void ReArrangeBgResource()
    {
         StartPanel.transform.GetChild(0).GetComponent<AnimatedGifPlayer>().enabled = false;

       // StartPanel.transform.GetChild(0).GetComponent<RawImage>().texture = videoRenderTexture;
      //  GameController.Instance.StartVideoPlay();
    }

    public void UpdateUIAfterAnwser()
    {


        CurrentQuestNum.text = GameController.Instance.CurrentQuestSequence.ToString() + "/" + GameController.Instance.TotalQuestNum.ToString();
        //    CurrentMoneyInGame.text = "$" + GameController.Instance.CurrentMoney.ToString();
        RollingNumber.StartRolling();
    }


    public void SetInitalRewardValue(RollingNumbers rollingNumber, int startValue, int endValue)
    {
        rollingNumber.initialNum = startValue;
        rollingNumber.currentValue = startValue;
        rollingNumber.barNum.text = rollingNumber.currentValue.ToString();
        rollingNumber.endNum = endValue;
    }


  
}
