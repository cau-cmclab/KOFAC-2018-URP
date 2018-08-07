using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("SFX: Basic Emotions")]
    [SerializeField] AudioClip acAroused;
    [SerializeField] AudioClip acExcited;
    [SerializeField] AudioClip acPleasured;
    [SerializeField] AudioClip acContent;
    [SerializeField] AudioClip acSleepy;
    [SerializeField] AudioClip acDepressed;
    [SerializeField] AudioClip acMiserious;
    [SerializeField] AudioClip acDistressed;
    [SerializeField] AudioClip acHello;
    [SerializeField] AudioClip acThanks;
    [SerializeField] AudioClip acSorry;
    [SerializeField] AudioClip acCongrats;
    [SerializeField] AudioClip acEmbarrassed;
    [SerializeField] AudioClip acNo;
    [SerializeField] AudioClip acViolent;
    [SerializeField] AudioClip acPoint;
    [Header("SFX: Handclap")]
    [SerializeField] AudioClip[] acHandclap;
    [Header("SFX: Footstep")]
    [SerializeField] AudioClip[] acFootstep;
    [Header("SFX: Punch Whoosh")]
    [SerializeField] AudioClip[] acWhoosh;


    Animator animator;
    AudioSource audioSource;

	void Start ()
    {
        animator = GetComponent<Animator>();

        audioSource = gameObject.AddComponent<AudioSource>();
    }
	
	void Update ()
    {
		
	}

    void EmotionEnd()
    {
        animator.SetBool("Doing Emotion", false);
    }

    void Handclap()
    {
        int nRand = Random.Range(0, acHandclap.Length - 1);

        audioSource.PlayOneShot(acHandclap[nRand]);
    }

    void Footstep()
    {
        int nRand = Random.Range(0, acFootstep.Length - 1);

        audioSource.PlayOneShot(acFootstep[nRand]);
    }

    void PunchWhoosh()
    {
        int nRand = Random.Range(0, acWhoosh.Length - 1);

        audioSource.PlayOneShot(acWhoosh[nRand]);
    }

    void EmotionSound(int nEmotion)
    {
        switch(nEmotion)
        {
            case 1: audioSource.PlayOneShot(acAroused); break;
            case 2: audioSource.PlayOneShot(acExcited); break;
            case 3: audioSource.PlayOneShot(acPleasured); break;
            case 4: audioSource.PlayOneShot(acContent); break;
            case 5: audioSource.PlayOneShot(acSleepy); break;
            case 6: audioSource.PlayOneShot(acDepressed); break;
            case 7: audioSource.PlayOneShot(acMiserious); break;
            case 8: audioSource.PlayOneShot(acDistressed); break;
            case 9: audioSource.PlayOneShot(acHello); break;
            case 10: audioSource.PlayOneShot(acThanks); break;
            case 11: audioSource.PlayOneShot(acSorry); break;
            case 12: audioSource.PlayOneShot(acCongrats); break;
            case 13: audioSource.PlayOneShot(acEmbarrassed); break;
            case 14: audioSource.PlayOneShot(acNo); break;
            case 15: audioSource.PlayOneShot(acViolent); break;
            case 16: audioSource.PlayOneShot(acPoint); break;
        }
    }
}
