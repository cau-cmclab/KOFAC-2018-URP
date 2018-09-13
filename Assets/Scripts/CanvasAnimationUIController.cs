using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasAnimationUIController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] Button[] btnEmotion = new Button[16];
    [Header("Player Controller")]
    [SerializeField] PlayerController playerController;

    void Start ()
    {
        /*
        if (!playerController)
        {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }*/

        // Basic Emotions
        btnEmotion[0].onClick.AddListener(delegate { playerController.DoEmotions(1); });
        btnEmotion[1].onClick.AddListener(delegate { playerController.DoEmotions(2); });
        btnEmotion[2].onClick.AddListener(delegate { playerController.DoEmotions(3); });
        btnEmotion[3].onClick.AddListener(delegate { playerController.DoEmotions(4); });
        btnEmotion[4].onClick.AddListener(delegate { playerController.DoEmotions(5); });
        btnEmotion[5].onClick.AddListener(delegate { playerController.DoEmotions(6); });
        btnEmotion[6].onClick.AddListener(delegate { playerController.DoEmotions(7); });
        btnEmotion[7].onClick.AddListener(delegate { playerController.DoEmotions(8); });

        // Popular Emotions
        btnEmotion[8].onClick.AddListener(delegate  { playerController.DoEmotions(9); });
        btnEmotion[9].onClick.AddListener(delegate  { playerController.DoEmotions(10); });
        btnEmotion[10].onClick.AddListener(delegate { playerController.DoEmotions(11); });
        btnEmotion[11].onClick.AddListener(delegate { playerController.DoEmotions(12); });
        btnEmotion[12].onClick.AddListener(delegate { playerController.DoEmotions(13); });
        btnEmotion[13].onClick.AddListener(delegate { playerController.DoEmotions(14); });
        btnEmotion[14].onClick.AddListener(delegate { playerController.DoEmotions(15); });
        btnEmotion[15].onClick.AddListener(delegate { playerController.DoEmotions(16); });
    }
}
