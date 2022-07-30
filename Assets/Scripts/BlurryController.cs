using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlurryController : MonoBehaviour
{

    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        PlayIdle();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayIdle()
    {
        animator.Play("Idle");
    }

    public void PlayIntro()
    {
        animator.Play("Intro");
    }


    public void PlayYes()
    {
        animator.Play("Yes");
    }

    public void PlayNo()
    {
        animator.Play("No");
    }

    public void PlayCelebration()
    {
        animator.Play("Celebration");
    }


    public void PlayQuestion()
    {
        animator.Play("Question");
    }
}
