using System.Collections.Generic;
using UnityEngine;

public class ParticlePooling : Singleton<ParticlePooling>
{
    public BlastParticle particlePrefab;
    List<BlastParticle> particleList = new List<BlastParticle>();
    Transform particleParent;

    public void ActivateParticle(int cubeId, Vector3 pos)
    {
        GetParticleFromPool().InitParticle(cubeId, pos);
    }

    BlastParticle GetParticleFromPool()
    {
        for (int i = 0; i < particleList.Count; i++)
        {
            if (!particleList[i].gameObject.activeSelf)
            {
                return particleList[i];
            }
        }

        return SpawnParticle();
    }

    BlastParticle SpawnParticle()
    {
        if (particleParent == null)
            particleParent = new GameObject("Particles").transform;

        BlastParticle newParticle = Instantiate(particlePrefab, particleParent);
        particleList.Add(newParticle);
        return newParticle;
    }
}
