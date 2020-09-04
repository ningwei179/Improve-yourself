using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特效的离线数据
/// </summary>
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
