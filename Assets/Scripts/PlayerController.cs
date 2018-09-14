using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : MonoBehaviour
{
    [Header("Player Setting")]
    Animator animator;
    CharacterController charController;
    [SerializeField] float fMoveSpeed = 1.5f;
    [SerializeField] float fRunningSpeed = 0.5f;
    [SerializeField] float fRotationSpeed = 5.0f;
    
    [Header("Camera Setting")]
    [Tooltip("Camera for calculating user's front")]
    [SerializeField] Transform tfCamera;

    Vector2 v2Input = Vector2.zero;
    Vector2 v2Forward = Vector2.zero;
    float fCurrentMoveSpeed = 0.0f;
    bool bDoingEmotion = false;

    public NetworkPlayer m_netPlayer;

	void Start ()
    {
        animator = GetComponent<Animator>();
        charController = GetComponent<CharacterController>();

        // 로컬 플레이어라면 ARCamera 설정
        if (m_netPlayer.isLocalPlayer)
        {
            tfCamera = GameObject.Find("ARCamera").transform;
        }
	}
	
	void Update ()
    {
        // 리모트 플레이어라면 키입력 받지 않음.
        if (!(m_netPlayer.isLocalPlayer))
            return;

        bDoingEmotion = animator.GetBool("Doing Emotion");

        v2Input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized * fMoveSpeed * Time.deltaTime;
        v2Forward = new Vector2(tfCamera.forward.x, tfCamera.forward.z).normalized;

        fCurrentMoveSpeed = v2Input.magnitude;

        animator.SetBool("Running", Input.GetKey(KeyCode.LeftShift));

        if (!bDoingEmotion)
        {
            if (Mathf.Abs(v2Input.x) > 0.0f || Mathf.Abs(v2Input.y) > 0.0f)
            {
                float fAngleWithXAxis = Mathf.Acos(Vector2.Dot(new Vector2(0.0f, 1.0f), v2Forward.normalized)) * Mathf.Rad2Deg;
                if (v2Forward.x < 0.0f) fAngleWithXAxis *= -1.0f;

                Quaternion qRotation = Quaternion.Euler(0.0f, fAngleWithXAxis, 0.0f) * Quaternion.LookRotation(new Vector3(v2Input.x, 0.0f, v2Input.y));
                transform.rotation = Quaternion.Slerp(transform.rotation, qRotation, fRotationSpeed * Time.deltaTime);
            }

            if (animator.GetBool("Running"))
            {
                if (fCurrentMoveSpeed > 0.0f)
                    charController.Move(transform.forward * (fCurrentMoveSpeed + fRunningSpeed * Time.deltaTime));
                else
                    charController.Move(transform.forward * fCurrentMoveSpeed);
            }
            else
            {
                charController.Move(transform.forward * fCurrentMoveSpeed);
            }
        }
        
        animator.SetFloat("Move Speed", charController.velocity.magnitude);
    }

    public void DoEmotions(int nEmotionNum)
    {
        if (!bDoingEmotion && charController.velocity.magnitude < 0.01f)
        {
            animator.SetBool("Doing Emotion", true);
            animator.SetInteger("Emotion", nEmotionNum);
        }
        else
        {
            Debug.LogWarning("Emotion blocked. ");
        }
    }
}
