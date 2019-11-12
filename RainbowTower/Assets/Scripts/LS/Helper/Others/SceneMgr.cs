using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LS.Common;
using UnityEngine.SceneManagement;

namespace LS.Helper.Others
{
    public class SceneMgr : ASingletonBasis<SceneMgr>
    {
        #region Public Fields

        /// <summary>  要异步加载的下一个场景名 </summary>
        public string AsyncLoadNextSceneName;
        /// <summary> 异步加载的过渡场景名 </summary>
        public string TransitionSceneName;
        /// <summary> 上一个场景名，不包含过渡场景</summary>
        public string PreviousSceneName;

        #endregion

        #region Public Methods

        /// <summary>
        /// 异步加载场景，含有过渡场景，如果不存在过渡场景，不要使用该方法，请使用：SceneManager.LoadSceneAsync
        /// </summary>
        public void CustomLoadSceneAsync(string name)
        {
            PreviousSceneName = SceneManager.GetActiveScene().name;
            AsyncLoadNextSceneName = name;
            SceneManager.LoadScene(TransitionSceneName);
        }

        /// <summary> 加载场景 </summary>
        public void CustomLoadScene(string name)
        {
            PreviousSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(name);
        }
        #endregion
    }

}