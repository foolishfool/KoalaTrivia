using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RollingNumbers : MonoBehaviour
{

    [HideInInspector]
    public int endNum;
    //used to caculate difference
    [HideInInspector]
    public int initialNum;
    [HideInInspector]
    public int currentValue;

    [SerializeField]
    private float ChangeTime; //rolling time

    private int change_number;
    private int difference;
    [HideInInspector]
    public Text barNum;

    [HideInInspector]
    public bool isLevelUp;
    [HideInInspector]
    //when levelup, the remain number needs rolling
    public int levelup_reamain_num;
    //*** must in awake, as if in start(), in LevelFinisheUIEvent Start()->  LoadRewardNUserData(); here some RollingNumbers' Start() hasnot been called
    //*** as a result it will barNum will be null
    void Awake()
    {
        barNum = GetComponent<Text>();
        //*** currentValue cannot be assigned here, it will be the num in the UI, not realy data
        //currentValue is applied outside the class
        // currentValue = initialNum;
    }

    //from big to small number or small to big number
    private bool isAdd()
    {
        if (endNum - initialNum >= 0)
        {
            return true;
        }
        else
            return false;
    }

    public void StartRolling()
    {
        //initialNum is assigned outside the class
        difference = endNum - initialNum;
        //0.05seond rolls one time
        change_number = (int)Mathf.Ceil(difference / (ChangeTime * (1 / 0.05f)));
        StartCoroutine(Change());
    }

    public IEnumerator Change()
    {
        if (isAdd())
        {
            if (change_number > 0)
            {
                currentValue += change_number;

                barNum.text = "$" + currentValue.ToString();
                yield return new WaitForSeconds(0.05f);     // add by every 0.05s

                while (currentValue < endNum)
                {
                    currentValue += change_number;
                    barNum.text = "$" + currentValue.ToString();
                    yield return new WaitForSeconds(0.05f);     // add by every 0.05s
                }
            }
        }
        else
        {
            if (change_number < 0)
            {
                while (endNum < currentValue)
                {
                    currentValue += change_number;
                    barNum.text = "$" + currentValue.ToString();
                    yield return new WaitForSeconds(0.05f);     // add by every 0.05s
                }
            }
        }

        barNum.text = "$"+ endNum.ToString();

        StopCoroutine(Change());

    }


}

