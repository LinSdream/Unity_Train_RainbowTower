using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS.Helper.Test
{
    /// <summary>
    /// 剔除灯光渲染
    /// 少用该脚本，可能会对fps有严重影响
    /// 有时候可行，有时候不可行，影响因素暂时不明
    /// Time:  2019.9.18
    /// </summary>
    public class ExcludeLight : MonoBehaviour
    {

        public List<Light> Lights;
        public bool CullLights = true;

        private void OnPreCull()
        {
            if (CullLights)
            {
                foreach(Light light in Lights)
                {
                    light.enabled = false;
                }
            }
        }

        private void OnPreRender()
        {
            if (CullLights)
            {
                foreach (Light light in Lights)
                {
                    light.enabled = true;
                }
            }
        }

    }

}