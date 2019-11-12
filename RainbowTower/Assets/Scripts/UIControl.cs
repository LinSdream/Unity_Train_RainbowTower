using LS.Helper.Others;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS.Game
{
    public class UIControl : MonoBehaviour
    {

        public GameLogic Logic;
        public Camera UICamera;
        public GameObject PausePanel;

        #region MonoBehaviour Callbacks
        #endregion

        #region Buttons
        public void Btn_OpenThePauseWindows()
        {
            Time.timeScale = 0;
            PausePanel.SetActive(true);
        }

        public void Btn_Menu()
        {
            Time.timeScale = 1;
            PausePanel.SetActive(false);
            SceneMgr.Instance.CustomLoadSceneAsync("00_Menu");
        }

        public void Btn_Continue()
        {
            PausePanel.SetActive(false);
            Time.timeScale = 1;
        }

        public void Btn_Replay()
        {
            Time.timeScale = 1;
            PausePanel.SetActive(false);
            SceneMgr.Instance.CustomLoadScene("01_Main");

        }
        #endregion

        #region Private Methods
        public void WorldToUI(Vector3 worldPos,RectTransform uiParent,RectTransform uiTarget)
        {
            
        }
        #endregion
    }
}
