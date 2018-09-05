using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * 작성자: Simple
 * 설  명: 이모지가 출력될 때 페이드 인, 사라지기 직전에 페이드 아웃?
 */

public class EmojiFadingController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float fDuration = 0.5f;

    Color color;

    Color colorBeforeFadeIn = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    Color colorBeforeFadeOut = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    /// <summary>
    /// 이모지가 표시될 때 알파를 증가시켜 페이드인 효과를 나타냄
    /// </summary>
    IEnumerator Start ()
    {
        color = colorBeforeFadeIn;

        while (color.a < 1.0f)
        {
            color.a += (1.0f / fDuration * Time.deltaTime);
            GetComponent<SpriteRenderer>().color = color;

            yield return new WaitForEndOfFrame();
        }
	}
	
    /// <summary>
    /// 감정표현이 종료될 때 알파를 감소시켜 페이드아웃 효과를 나타냅니다. 
    /// 이후 게임오브젝트를 제거합니다. Destroy() 중복해서 쓰지 마라는 뜻입니다. 
    /// </summary>
	public void FadeOutAndDestroy()
    {
        StartCoroutine(_FadeOutAndDestroy());
    }

    /// <summary>
    /// 감정표현이 종료될 때 알파를 감소시켜 페이드아웃 효과를 나타냄
    /// </summary>
    IEnumerator _FadeOutAndDestroy()
    {
        color = colorBeforeFadeOut;

        while (color.a > 0.0f)
        {
            color.a -= (1.0f / fDuration * Time.deltaTime);
            GetComponent<SpriteRenderer>().color = color;

            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }
}
