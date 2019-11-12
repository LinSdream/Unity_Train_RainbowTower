using LS.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace LS.Game
{
    public class EffectManage
    {

        /// <summary>
        ///落下特效
        /// </summary>
        public void CreateSpericalEffect(GameObject top,GameObjectPool pool )
        {
            Vector3 pos = top.transform.position;
            Vector3 size = top.transform.localScale;
            Color color = top.GetComponent<MeshRenderer>().material.color;
            float diagonal = Mathf.Sqrt(Mathf.Pow(size.x, 2) + Mathf.Pow(size.z, 2));

            GameObject effect = pool.GetInstanceGameObj(top.transform.position+Vector3.down*size.y/2,Quaternion.identity);

            effect.SetActive(true);

            var main = effect.GetComponent<ParticleSystem>().main;
            main.startColor = color;
            main.startSizeMultiplier = diagonal * 2;
            effect.GetComponent<ParticleSystem>().Play();
        }

        /// <summary>
        /// 创建特效
        /// </summary>
        public void CreateNaissanceEffect(GameObject move,GameObjectPool pool)
        {
            Vector3 size = move.transform.localScale;
            Vector3 pos = move.transform.position;

            float diagonal = Mathf.Sqrt(Mathf.Pow(size.x, 2) + Mathf.Pow(size.y, 2) + Mathf.Pow(size.z, 2));

            GameObject effect = pool.GetInstanceGameObj(pos,Quaternion.identity);

            effect.SetActive(true);

            var main = effect.GetComponent<ParticleSystem>().main;
            main.startSizeMultiplier = diagonal * 2;

        }

        /// <summary>
        /// AT立场特效
        /// </summary>
        public void CreateAT_Field(Transform moveCube,GameObjectPool pool,Vector3 direction,float offset,float angle,float axis)
        {
            //移动方块的信息
            Vector3 pos = moveCube.position;
            Vector3 scale = moveCube.localScale;

            //获取移动方块的前面的对角线长度,取移动到的目标的的侧面的对角线
            float diagonal = Mathf.Sqrt(Mathf.Pow(scale.y, 2) + Mathf.Pow(axis, 2));

            GameObject at_Field = pool.GetInstanceGameObj();

            at_Field.SetActive(true);

            //设置特效的生成位置，位置=方块的中心点位置+方块移动的方向*方块的该方向的一半长度
            at_Field.transform.position = pos + direction * offset / 2;

            var main = at_Field.GetComponent<ParticleSystem>().main;
            main.startRotationXMultiplier = 0;
            //AT特效的方向旋转，（把角度换算成弧度制）
            main.startRotationYMultiplier = angle * Mathf.Deg2Rad;
            main.startRotationZMultiplier = 0;

            //特效的尺寸
            main.startSizeMultiplier = diagonal * 2;

        }

        public IEnumerator WaitForTheBoom(List<Transform> list, GameObject prefab,Action action=null)
        {
            for(int i = list.Count - 1; i > 0; i--)
            {
                GameObject effect = GameObject.Instantiate(prefab);
                effect.transform.position = list[i].position;
                list[i].gameObject.SetActive(false);
                yield return new WaitForSeconds(0.05f);
            }

            action?.Invoke();
        }
    }

}