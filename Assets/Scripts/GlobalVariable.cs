using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariable : MonoBehaviour
{

    static GlobalVariable instance;
    public static GlobalVariable Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(GlobalVariable)) as GlobalVariable;
                if (instance == null)
                {
                    GameObject obj = new GameObject("GlobalVariable");
                    instance = obj.AddComponent<GlobalVariable>();

                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    //to prove that the game has alredy be loaded , used for restart then screen can directly load video
    public bool IsLoaded;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
