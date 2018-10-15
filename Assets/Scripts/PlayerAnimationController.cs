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
    [Space]
    [Header("SFX: Handclap")]
    [SerializeField] AudioClip[] acHandclap;
    [Space]
    [Header("SFX: Footstep")]
    [SerializeField] AudioClip[] acFootstep;
    [Space]
    [Header("SFX: Punch Whoosh")]
    [SerializeField] AudioClip[] acWhoosh;
    [Space]
    [Header("FX: Setting")]
    [SerializeField] Vector3 v3EffectHeight = new Vector3(0.0f, 2.25f, 0.0f);
    [Space]
    [Header("FX: Russell's")]
    [SerializeField] GameObject objAroused;
    [SerializeField] GameObject objExcited;
    [SerializeField] GameObject objPleasured;
    [SerializeField] GameObject objContent;
    [SerializeField] GameObject objSleepy;
    [SerializeField] GameObject objDepressed;
    [SerializeField] GameObject objMiserious;
    [SerializeField] GameObject objDistressed;
    [SerializeField] GameObject objHello;
    [SerializeField] GameObject objThanks;
    [SerializeField] GameObject objSorry;
    [SerializeField] GameObject objCongrats;
    [SerializeField] GameObject objEmbarrassed;
    [SerializeField] GameObject objNo;
    [SerializeField] GameObject objViolent;
    [SerializeField] GameObject objPoint;

    Animator animator;
    AudioSource audioSource;

    GameObject objEmotionalEffect = null;
    Camera m_camera;

	void Start ()
    {
        animator = GetComponent<Animator>();
        audioSource = gameObject.AddComponent<AudioSource>();

        m_camera = Camera.main;
    }
	
	void Update ()
    {
        // 감정표현용 게임오브젝트가 항상 카메라를 향하도록 함
		if (m_camera && objEmotionalEffect)
        {
            objEmotionalEffect.transform.LookAt(m_camera.transform);
        }
	}

    // 감정표현이 종료되었을 때
    void EmotionEnd()
    {
        // 감정표현용 게임오브젝트를 제거하기 위한 작업
        if (objEmotionalEffect)
        {
            // 감정표현용 게임오브젝트의 EmojiFadingController를 가져옴
            EmojiFadingController efc = objEmotionalEffect.GetComponent<EmojiFadingController>();

            // 만약 EFC가 있다면 이 감정표현은 이모지를 이용한 감정표현
            if (efc)
            {
                efc.FadeOutAndDestroy();
            }
            // EFC가 없다면 이 감정표현은 이펙트를 이용한 감정표현
            else
            {
                Destroy(objEmotionalEffect.gameObject);
            }
            
            objEmotionalEffect = null;
        }

        animator.SetBool("Doing Emotion", false);
    }

    // 박수를 치는 모션에서 박수소리를 재생함
    void Handclap()
    {
        int nRand = Random.Range(0, acHandclap.Length - 1);

        audioSource.PlayOneShot(acHandclap[nRand]);
    }

    // 걷는 모션에서 걷는 소리를 재생함
    void Footstep()
    {
        int nRand = Random.Range(0, acFootstep.Length - 1);

        audioSource.PlayOneShot(acFootstep[nRand]);
    }

    // 주먹을 휘두르는 모션에서 휘두르는 소리를 재생함
    void PunchWhoosh()
    {
        int nRand = Random.Range(0, acWhoosh.Length - 1);

        audioSource.PlayOneShot(acWhoosh[nRand]);
    }

    // 감정표현을 시작할 때 해당 효과음을 재생함
    void EmotionSound(int nEmotion)
    {
        switch(nEmotion)
        {
            // Russell's 8 Moods
            case 1:
                audioSource.PlayOneShot(acAroused);
                objEmotionalEffect = Instantiate(objAroused, transform.position + v3EffectHeight, Quaternion.identity, transform);
                break;
            case 2:
                audioSource.PlayOneShot(acExcited);
                objEmotionalEffect = Instantiate(objExcited, transform.position + v3EffectHeight, Quaternion.identity, transform); 
                break;
            case 3:
                audioSource.PlayOneShot(acPleasured);
                objEmotionalEffect = Instantiate(objPleasured, transform.position + v3EffectHeight, Quaternion.identity, transform); 
                break;
            case 4:
                audioSource.PlayOneShot(acContent);
                objEmotionalEffect = Instantiate(objContent, transform.position + v3EffectHeight, Quaternion.identity, transform); 
                break;
            case 5:
                audioSource.PlayOneShot(acSleepy);
                objEmotionalEffect = Instantiate(objSleepy, transform.position + v3EffectHeight, Quaternion.identity, transform); 
                break;
            case 6:
                audioSource.PlayOneShot(acDepressed);
                objEmotionalEffect = Instantiate(objDepressed, transform.position + v3EffectHeight, Quaternion.identity, transform); 
                break;
            case 7:
                audioSource.PlayOneShot(acMiserious);
                objEmotionalEffect = Instantiate(objMiserious, transform.position + v3EffectHeight, Quaternion.identity, transform);
                break;
            case 8:
                audioSource.PlayOneShot(acDistressed);
                objEmotionalEffect = Instantiate(objDistressed, transform.position + v3EffectHeight, Quaternion.identity, transform); 
                break;

            // 8 Famouse Emotions
            case 9:
                audioSource.PlayOneShot(acHello);
                objEmotionalEffect = Instantiate(objHello, transform.position + v3EffectHeight, Quaternion.identity, transform);
                break;
            case 10:
                audioSource.PlayOneShot(acThanks);
                objEmotionalEffect = Instantiate(objThanks, transform.position + v3EffectHeight, Quaternion.identity, transform);
                break;
            case 11:
                audioSource.PlayOneShot(acSorry);
                objEmotionalEffect = Instantiate(objSorry, transform.position + v3EffectHeight, Quaternion.identity, transform);
                break;
            case 12:
                audioSource.PlayOneShot(acCongrats);
                objEmotionalEffect = Instantiate(objCongrats, transform.position + v3EffectHeight, Quaternion.identity, transform);
                break;
            case 13:
                audioSource.PlayOneShot(acEmbarrassed);
                objEmotionalEffect = Instantiate(objEmbarrassed, transform.position + v3EffectHeight, Quaternion.identity, transform);
                break;
            case 14:
                audioSource.PlayOneShot(acNo);
                objEmotionalEffect = Instantiate(objNo, transform.position + v3EffectHeight, Quaternion.identity, transform);
                break;
            case 15:
                audioSource.PlayOneShot(acViolent);
                objEmotionalEffect = Instantiate(objViolent, transform.position + v3EffectHeight, Quaternion.identity, transform);
                break;
            case 16:
                audioSource.PlayOneShot(acPoint);
                objEmotionalEffect = Instantiate(objPoint, transform.position + v3EffectHeight, Quaternion.identity, transform);
                break;
        }
    }
}
