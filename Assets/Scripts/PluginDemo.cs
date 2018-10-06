using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PluginDemo : MonoBehaviour {
    public Text speechBubbleText;
    public GameObject speechBubbleUI;
    private Texture2D galleryTexture2D;//안드로이드 플로그인을 통해 사진어플에서 받아온 Texture2D이미지
    private Sprite gallerySprite;//SpeechBubbleImage의 source image를 할당하기위해 Teture2D에서 Sprite으로 변환 후 Sprite형식의 저장변수 
    public GameObject speechBubbleImage;
    private bool isGalleryImageLoaded = false;//사진어플에서 사진을 잘 받아왔는지 확인하는 변수
    private WWW www;//사진어플에서 받아올때의 초기 변수

    public GameObject m_player;  // malcom
    public InputField m_chatField;  // 채팅 입력창

    string Speech = "";
    // Use this for initialization
    void Start()
    {

    }

    public void SpeechDemo()
    {
        AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject ajo = new AndroidJavaObject("armessenger.choi.cmc.unityandroidplugin.UnityBinder");
        ajo.CallStatic("OpenSound", ajc.GetStatic<AndroidJavaObject>("currentActivity"), this.gameObject.name);
    }

    public void OnSpeech(string filePath)
    {
        Debug.Log(filePath);
        Speech = filePath;

        m_chatField.text = Speech;
        if (m_chatField.text.Equals("")) m_chatField.text = "No!!";

        m_player.GetComponentInChildren<SpeechBubbleControl>().InputTextChatwindow(m_chatField);
        m_chatField.text = "";
    }

    public void GalleryDemo()
    {
        isGalleryImageLoaded = false;
        AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject ajo = new AndroidJavaObject("armessenger.choi.cmc.unityandroidplugin.UnityBinder");
        ajo.CallStatic("OpenGallery", ajc.GetStatic<AndroidJavaObject>("currentActivity"), this.gameObject.name);
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
            speechBubbleText.text = "됨";
            gallerySprite = Sprite.Create(galleryTexture2D, new Rect(0.0f, 0.0f, galleryTexture2D.width, galleryTexture2D.height), new Vector2(0.5f, 0.5f));
            speechBubbleImage.GetComponent<Image>().sprite = gallerySprite;
        }

        else
        {
            speechBubbleText.text = "안됨";
        }
    }
}
