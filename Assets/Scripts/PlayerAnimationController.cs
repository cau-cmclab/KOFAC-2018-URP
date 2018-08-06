using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("SFX: Handclap")]
    [SerializeField] AudioClip[] acHandclap;
    [SerializeField] AudioClip[] acFootstep;

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
}
