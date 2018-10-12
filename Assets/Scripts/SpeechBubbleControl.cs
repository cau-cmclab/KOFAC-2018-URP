using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBubbleControl : MonoBehaviour {

    //추가적으로 주석이 달리지 않은 변수는 오브젝트이름과 동일하게 구성, 변수와 오브젝트가 연결되어있다고 생각하면 된다.
    
    
    private Texture2D galleryTexture2D;//안드로이드 플로그인을 통해 사진어플에서 받아온 Texture2D이미지
    private Sprite gallerySprite;//SpeechBubbleImage의 source image를 할당하기위해 Teture2D에서 Sprite으로 변환 후 Sprite형식의 저장변수 
    
    private bool isGalleryImageLoaded = false;//사진어플에서 사진을 잘 받아왔는지 확인하는 변수
    private WWW www;//사진어플에서 받아올때의 초기 변수

    [SerializeField]
    private GameObject[] Emotion_Button = new GameObject[16];

    public GameObject UpButton;
    public GameObject DownButton;

    private int emotional_state = 1;

    public Text speechBubbleText;
    public GameObject speechBubbleUI;
    public GameObject speechBubbleImage;

    private GameObject arCamera;
    private float timer;
    private float waitingTime;

    public NetworkPlayer m_player;

    // Use this for initialization
    void Start () {
       arCamera = GameObject.Find("ARCamera");

        timer = 0;
        waitingTime = 5f;
    }

    // Update is called once per frame
    void Update() {
        speechBubbleUI.transform.LookAt(arCamera.transform);


        speechBubbleText.text = m_player.m_text;

        if(m_player.m_text == "")
            speechBubbleUI.SetActive(false);
        else{
            speechBubbleUI.SetActive(true);
            timer += Time.deltaTime;  
            if(timer > waitingTime){
                m_player.CmdShareBubble("");
                //speechBubbleText.text = "";
                timer = 0.0f;
            }
        }

        if(!m_player.isLocalPlayer)
            return;

        switch(emotional_state)
        {
            case 1:
                UpButton.SetActive(false);
                DownButton.SetActive(true);

                Emotion_Button[0].SetActive(true);
                Emotion_Button[1].SetActive(true);
                Emotion_Button[2].SetActive(true);
                Emotion_Button[3].SetActive(true);

                Emotion_Button[4].SetActive(false);
                Emotion_Button[5].SetActive(false);
                Emotion_Button[6].SetActive(false);
                Emotion_Button[7].SetActive(false);

                Emotion_Button[8].SetActive(false);
                Emotion_Button[9].SetActive(false);
                Emotion_Button[10].SetActive(false);
                Emotion_Button[11].SetActive(false);

                Emotion_Button[12].SetActive(false);
                Emotion_Button[13].SetActive(false);
                Emotion_Button[14].SetActive(false);
                Emotion_Button[15].SetActive(false);
                break;

            case 2:
                UpButton.SetActive(true);
                DownButton.SetActive(true);

                Emotion_Button[0].SetActive(false);
                Emotion_Button[1].SetActive(false);
                Emotion_Button[2].SetActive(false);
                Emotion_Button[3].SetActive(false);

                Emotion_Button[4].SetActive(true);
                Emotion_Button[5].SetActive(true);
                Emotion_Button[6].SetActive(true);
                Emotion_Button[7].SetActive(true);

                Emotion_Button[8].SetActive(false);
                Emotion_Button[9].SetActive(false);
                Emotion_Button[10].SetActive(false);
                Emotion_Button[11].SetActive(false);

                Emotion_Button[12].SetActive(false);
                Emotion_Button[13].SetActive(false);
                Emotion_Button[14].SetActive(false);
                Emotion_Button[15].SetActive(false);
                break;

            case 3:
                UpButton.SetActive(true);
                DownButton.SetActive(true);

                Emotion_Button[0].SetActive(false);
                Emotion_Button[1].SetActive(false);
                Emotion_Button[2].SetActive(false);
                Emotion_Button[3].SetActive(false);

                Emotion_Button[4].SetActive(false);
                Emotion_Button[5].SetActive(false);
                Emotion_Button[6].SetActive(false);
                Emotion_Button[7].SetActive(false);

                Emotion_Button[8].SetActive(false);
                Emotion_Button[9].SetActive(false);
                Emotion_Button[10].SetActive(true);
                Emotion_Button[11].SetActive(false);

                Emotion_Button[12].SetActive(true);
                Emotion_Button[13].SetActive(true);
                Emotion_Button[14].SetActive(true);
                Emotion_Button[15].SetActive(false);
                break;

            case 4:
                UpButton.SetActive(true);
                DownButton.SetActive(false);

                Emotion_Button[0].SetActive(false);
                Emotion_Button[1].SetActive(false);
                Emotion_Button[2].SetActive(false);
                Emotion_Button[3].SetActive(false);

                Emotion_Button[4].SetActive(false);
                Emotion_Button[5].SetActive(false);
                Emotion_Button[6].SetActive(false);
                Emotion_Button[7].SetActive(false);

                Emotion_Button[8].SetActive(true);
                Emotion_Button[9].SetActive(true);
                Emotion_Button[10].SetActive(false);
                Emotion_Button[11].SetActive(true);

                Emotion_Button[12].SetActive(false);
                Emotion_Button[13].SetActive(false);
                Emotion_Button[14].SetActive(false);
                Emotion_Button[15].SetActive(true);
                break;
        }

        

    }

    
    public void OnPhotoPick(string filePath)
    {
        Debug.Log(filePath);
        www = new WWW("file://" + filePath);
        if (www != null && www.isDone)
        {
            galleryTexture2D = new Texture2D(www.texture.width, www.texture.height);
            galleryTexture2D.SetPixels32(www.texture.GetPixels32());
            galleryTexture2D.Apply();
            www = null;
            isGalleryImageLoaded = true;
        }
        if (isGalleryImageLoaded)
        {
           // speechBubbleText.text = "됨";
            gallerySprite = Sprite.Create(galleryTexture2D, new Rect(0.0f, 0.0f, galleryTexture2D.width, galleryTexture2D.height), new Vector2(0.5f, 0.5f));
            //speechBubbleImage.GetComponent<Image>().sprite = gallerySprite;
        }
    }

    public void OnClickGalleryButton()
    {
        isGalleryImageLoaded = false;
        AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject ajo = new AndroidJavaObject("armessenger.choi.cmc.unityandroidplugin.UnityBinder");
        ajo.CallStatic("OpenGallery", ajc.GetStatic<AndroidJavaObject>("currentActivity"));
    }


    public void InitBubbleTimer()
    {
            //speechBubbleText.text = ip.text;
           // CmdShareBubble(ip.text);
            // 몇 초뒤에 말풍선 초기화
            //StartCoroutine(MyWaitForSeconds(5f));
            timer = 0.0f;
    }


    public void Emotion_UpButton()
    {
        emotional_state--;
    }

    public void Emotion_DownButton()
    {
        emotional_state++;
    }
    /*
    IEnumerator MyWaitForSeconds(float scnds)
    {
        yield return new WaitForSeconds(scnds);
        speechBubbleText.text = "";
    }*/

}
