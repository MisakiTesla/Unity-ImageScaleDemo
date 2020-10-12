using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

[Header("上方边框")]
    public Transform border1;//上方边框
    [Header("上方图片")]
    public Transform pic1;//上方图片
    [Header("下方图片")]
    public Transform pic2;//下方图片
    [Header("DebugText")]
    public Text text;//下方图片

    private RectTransform pic1RectTrans;

    private bool isTwoTouch = false;

    private Vector3 firstTouch;
    private Vector3 secondTouch;

    //过去双指触控间距
    private float  doubleTouchLastDistance;
    //当前双指触控间距
    private float doubleTouchCurrentDistance;

    //"左下:"
    //"左上:"
    //"右上:"
    //"左下:"
    private Vector3[] StartCorners;//开始时的边框坐标

    private bool IsOnPic = false;//是否点击到图片

    private void Start()
    {
        //得到最初的边界坐标
        StartCorners = new Vector3[4];
        border1.GetComponent<RectTransform>().GetWorldCorners(StartCorners);
        pic1RectTrans = pic1.GetComponent<RectTransform>();
    }

    //是否在边界内
    private bool IsInEdge(Vector2 vec) {
        Vector3[] corners = new Vector3[4];
        pic1.GetComponent<RectTransform>().GetWorldCorners(corners);

        Vector3 leftDown = corners[0] +new Vector3( vec.x,vec.y,0);//左下角
        Vector3 rightUp = corners[2] + new Vector3(vec.x, vec.y, 0);//右上角


        if (leftDown.x <= StartCorners[0].x && leftDown.y <= StartCorners[0].y && rightUp.x >= StartCorners[2].x && rightUp.y >= StartCorners[2].y)
        {
            return true;
        }
        return false;
    }

    private bool IsInEdge()
    {
        Vector3[] corners = new Vector3[4];
        pic1.GetComponent<RectTransform>().GetWorldCorners(corners);

        Vector3 leftDown = corners[0];//左下角
        Vector3 rightUp = corners[2];//右上角
        if (leftDown.x <= StartCorners[0].x && leftDown.y <= StartCorners[0].y && rightUp.x >= StartCorners[2].x && rightUp.y >= StartCorners[2].y)
        {
            return true;
        }
        return false;
    }

    //调整pic1位置
    private void AdjustPosition()
    {
        Vector3[] corners = new Vector3[4];
        pic1.GetComponent<RectTransform>().GetWorldCorners(corners);

        Vector3 leftDown = corners[0];//左下角
        Vector3 leftUp = corners[1];//左上角
        Vector3 rightUp = corners[2];//右上角
        Vector3 rightDown = corners[3];//右下角
        float x = 0.5f;
        float y = 0.5f;

        if (leftDown.x >= StartCorners[0].x)//左
        {
            x = leftDown.x - StartCorners[0].x;
        }

        if (rightUp.x <= StartCorners[2].x )//右
        {
            x = rightUp.x - StartCorners[2].x;
        }
        if (rightUp.y <= StartCorners[2].y)//上
        {
            y = rightUp.y - StartCorners[2].y;
        }
        if (leftDown.y >= StartCorners[0].y)//下
        {
            y = leftDown.y - StartCorners[0].y;
        }
        pic1.localPosition -= new Vector3(x, y, 0);
    }

    private void Update()
    {

        if (GetCurrentTouch() == null || !GetCurrentTouch().CompareTag("Pic")) return;
        //获取图片四角坐标
        Vector3[] corners = new Vector3[4];
        pic1.GetComponent<RectTransform>().GetWorldCorners(corners);

        Debug.Log("左下:"+corners[0]);
        Debug.Log("左上:"+corners[1]);
        Debug.Log("右上:"+corners[2]);
        Debug.Log("左下:"+corners[3]);




        //Ray ray = new Ray();
        //ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);

        #region 缩放
        if (Input.touchCount > 1)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
            {
                Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
                transform.Translate(touchDeltaPosition.x * Time.deltaTime * 0.03f, touchDeltaPosition.y * Time.deltaTime * 0.03f, 0);
            }
            //当第二根手指按下的时候
            if (Input.GetTouch(1).phase == TouchPhase.Began)
            {
                isTwoTouch = true;
                //获取第一根手指的位置
                firstTouch = Input.touches[0].position;
                //获取第二根手指的位置
                secondTouch = Input.touches[1].position;

                doubleTouchLastDistance = Vector2.Distance(firstTouch, secondTouch);
            }

            //如果有两根手指按下
            if (isTwoTouch)
            {
                //每一帧都得到两个手指的坐标以及距离
                firstTouch = Input.touches[0].position;
                secondTouch = Input.touches[1].position;
                doubleTouchCurrentDistance = Vector2.Distance(firstTouch, secondTouch);

                //当前图片的缩放
                Vector3 curImageScale = new Vector3(pic1.localScale.x, pic1.localScale.y, 1);
                //两根手指上一帧和这帧之间的距离差
                //因为100个像素代表单位1，把距离差除以100看缩放几倍
                float changeScaleDistance = (doubleTouchCurrentDistance - doubleTouchLastDistance) / 100;
                //因为缩放 Scale 是一个Vector3，所以这个代表缩放的Vector3的值就是缩放的倍数
                Vector3 changeScale = new Vector3(changeScaleDistance, changeScaleDistance, 0);
                //图片的缩放等于当前的缩放加上 修改的缩放
                var currScale = pic1.localScale;
                pic1.localScale = curImageScale + changeScale;
                if(pic1.localScale.x < 1f)
                {
                    pic1.localScale = Vector3.one;
                    pic1.localPosition = Vector3.zero;
                    pic2.localScale = pic1.localScale;
                    pic2.localPosition = pic1.localPosition;
                    return;
                }

                //超出范围则重置缩放
                if (!IsInEdge())
                {
                    AdjustPosition();
                }
                //控制缩放级别
                pic1.localScale = new Vector3(Mathf.Clamp(pic1.localScale.x, 1f, 3f), Mathf.Clamp(pic1.localScale.y, 1f, 3f), 1);
                //这一帧结束后，当前的距离就会变成上一帧的距离了
                doubleTouchLastDistance = doubleTouchCurrentDistance;
            }

            //当第二根手指结束时（抬起）
            if (Input.GetTouch(1).phase == TouchPhase.Ended)
            {
                isTwoTouch = false;
                firstTouch = Vector3.zero;
                secondTouch = Vector3.zero;
            }


        }

        #endregion
        Debug.Log(Input.touchCount);
        string s = "";

        if (Input.touchCount == 1)
        {
            Debug.Log("Moving");
            //Vector2 currTouchPos = Input.touches[0].position;
            Vector2 deltaPos = Input.GetTouch(0).deltaPosition;
            s = s + Input.GetTouch(0).deltaPosition+ IsInEdge(deltaPos);
            if (IsInEdge(deltaPos))
            {
                pic1.position += new Vector3(deltaPos.x,deltaPos.y,0);
            }
        }
        PrintText(Time.deltaTime + Input.touchCount.ToString() + pic1.localScale.ToString() + pic2.localScale.ToString() + s);

        pic2.localScale = pic1.localScale;
        pic2.localPosition = pic1.localPosition;

    }

    private void PrintText(string s)
    {
        text.text = s;
    }

    public GameObject GetCurrentTouch()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2
            (
#if UNITY_EDITOR
            Input.mousePosition.x, Input.mousePosition.y
#elif UNITY_ANDROID || UNITY_IOS
           Input.touchCount > 0 ? Input.GetTouch(0).position.x : 0, Input.touchCount > 0 ? Input.GetTouch(0).position.y : 0
#endif 
            );
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        if (results.Count > 0)
        {
            return results[0].gameObject;
        }
        else
        {
            return null;
        }
}


}
