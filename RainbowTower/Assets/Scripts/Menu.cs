using LS.Helper.Others;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS.Game
{
    public class Menu : MonoBehaviour
    {

        #region Fields
        public string TransitionScneneName;
        #endregion

        #region MonoBehaviour Callbacks
        private void Start()
        {
            SceneMgr.Instance.TransitionSceneName = TransitionScneneName;
        }
        #endregion

        #region Buttons

        public void Btn_Start()
        {
            SceneMgr.Instance.CustomLoadSceneAsync("01_Main");
        }

        public void Btn_Quit()
        {
            Application.Quit();
        }

        public void Btn_Network()
        {
            SceneMgr.Instance.CustomLoadScene("02_Launch");
        }
        #endregion
    }
}
