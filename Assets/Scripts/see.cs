using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class see : MonoBehaviour {

    public float ViewAngle;    //시야각
    public float ViewDistance; //시야거리

    public LayerMask TargetMask;    //Enemy 레이어마스크 지정을 위한 변수
    public LayerMask ObstacleMask;  //Obstacle 레이어마스크 지정 위한 변수

    private Transform _transform;
    private GameObject arcameracollider;
    private GameObject chatbubble;

    void Awake()
    {
        _transform = GetComponent<Transform>();
        arcameracollider = GameObject.Find("ARcollider");
        chatbubble = GameObject.Find("ChatBubble");
    }

    void Update()
    {
        DrawView();             //Scene뷰에 시야범위 그리기
        FindVisibleTargets();   //Enemy인지 Obstacle인지 판별
    }

    public Vector3 DirFromAngle(float angleInDegrees)
    {
        //탱크의 좌우 회전값 갱신
        angleInDegrees += transform.eulerAngles.y;
        //경계 벡터값 반환
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public void DrawView()
    {
        Vector3 leftBoundary = DirFromAngle(-ViewAngle / 2);
        Vector3 rightBoundary = DirFromAngle(ViewAngle / 2);
        Debug.DrawLine(_transform.position, _transform.position + leftBoundary * ViewDistance, Color.blue);
        Debug.DrawLine(_transform.position, _transform.position + rightBoundary * ViewDistance, Color.blue);
        Debug.DrawLine(_transform.position, _transform.forward* ViewDistance, Color.blue);
    }

    public void FindVisibleTargets()
    {
        //시야거리 내에 존재하는 모든 컬라이더 받아오기
        Collider[] targets = Physics.OverlapSphere(_transform.position, ViewDistance, TargetMask);

        for (int i = 0; i < targets.Length; i++)
        {
            Transform target = targets[i].transform;

            //탱크로부터 타겟까지의 단위벡터
            Vector3 dirToTarget = (target.position - _transform.position).normalized;

            //_transform.forward와 dirToTarget은 모두 단위벡터이므로 내적값은 두 벡터가 이루는 각의 Cos값과 같다.
            //내적값이 시야각/2의 Cos값보다 크면 시야에 들어온 것이다.
            if (Vector3.Dot(_transform.forward, dirToTarget) > Mathf.Cos((ViewAngle / 2) * Mathf.Deg2Rad))
            //if (Vector3.Angle(_transform.forward, dirToTarget) < ViewAngle/2)
            {
                float distToTarget = Vector3.Distance(_transform.position, target.position);

                if (!Physics.Raycast(_transform.position, dirToTarget, distToTarget, ObstacleMask))
                {
                    Debug.DrawLine(_transform.position, target.position, Color.red);
                    chatbubble.transform.LookAt(arcameracollider.transform);
                }
            }
        }
    }
}
