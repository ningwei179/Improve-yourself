using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 离线数据
/// </summary>
public class OfflineData : MonoBehaviour
{
    public Rigidbody m_Rigidbody;

    public Collider m_Collider;

    public Transform[] m_AllPoint;          //所有的节点

    public int[] m_AllPointChildCount;      //每个节点的个数

    public bool[] m_AllPointActive;         //每个节点的激活信息

    public Vector3[] m_Pos;                 //每个节点的坐标

    public Vector3[] m_Scale;               //每个节点的缩放

    public Quaternion[] m_Rot;              //每个节点的旋转

    /// <summary>
    /// 还原属性
    /// </summary>
    public virtual void ResetProp()
    {
        int allPointCount = m_AllPoint.Length;
        for (int i = 0; i < allPointCount; i++)
        {
            Transform tempTrs = m_AllPoint[i];
            if (tempTrs != null)
            {
                tempTrs.localPosition = m_Pos[i];
                tempTrs.localRotation = m_Rot[i];
                tempTrs.localScale = m_Scale[i];

                if (m_AllPointActive[i])
                {
                    if (!tempTrs.gameObject.activeSelf)
                    {
                        tempTrs.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (tempTrs.gameObject.activeSelf)
                    {
                        tempTrs.gameObject.SetActive(false);
                    }
                }

                //多了一个节点，新添加的节点
                if (tempTrs.childCount > m_AllPointChildCount[i])
                {
                    int childCount = tempTrs.childCount;
                    for (int j = m_AllPointChildCount[i]; j < childCount; j++)
                    {
                        GameObject tempObj = tempTrs.GetChild(j).gameObject;
                        //回收多出的节点
                        if (!ObjectManager.Instance.IsObjectManagerCreate(tempObj))
                        {
                            GameObject.Destroy(tempObj);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 编辑器下保存初始数据
    /// </summary>
    public virtual void BindData()
    {
        m_Rigidbody = gameObject.GetComponentInChildren<Rigidbody>(true);
        m_Collider = gameObject.GetComponentInChildren<Collider>(true);
        m_AllPoint = gameObject.GetComponentsInChildren<Transform>(true);
        int allPointCount = m_AllPoint.Length;
        m_AllPointChildCount = new int[allPointCount];

        m_AllPointActive = new bool[allPointCount];

        m_Pos = new Vector3[allPointCount];

        m_Scale = new Vector3[allPointCount];

        m_Rot = new Quaternion[allPointCount];

        for (int i = 0; i < allPointCount; i++)
        {
            Transform temp = m_AllPoint[i] as Transform;

            m_AllPointChildCount[i] = temp.childCount;

            m_AllPointActive[i] = temp.gameObject.activeSelf;

            m_Pos[i] = temp.localPosition;

            m_Scale[i] = temp.localScale;

            m_Rot[i] = temp.localRotation;
        }
    }
}
