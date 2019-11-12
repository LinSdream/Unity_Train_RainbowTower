using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectCallback : MonoBehaviour
{
    private void OnParticleSystemStopped()
    {
        gameObject.SetActive(false);
    }
}
