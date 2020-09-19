/****************************************************
	文件：EffectOfflineData.cs
	作者：NingWei
	日期：2020/09/07 11:32   	
	功能：特效离线数据
*****************************************************/
using UnityEngine;

public class EffectOfflineData : OfflineData
{
    public ParticleSystem[] m_Particle; //粒子

    public TrailRenderer[] m_TrailRenderer; //拖尾

    public override void ResetProp()
    {
        base.ResetProp();

        foreach (ParticleSystem particle in m_Particle)
        {
            particle.Clear();
            particle.Play();
        }

        foreach (TrailRenderer trail in m_TrailRenderer)
        {
            trail.Clear();
        }
    }

    public override void BindData()
    {
        base.BindData();

        m_Particle = gameObject.GetComponentsInChildren<ParticleSystem>(true);

        m_TrailRenderer = gameObject.GetComponentsInChildren<TrailRenderer>(true);
    }
}
