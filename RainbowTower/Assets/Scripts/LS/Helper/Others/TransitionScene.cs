using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LS.Helper.Others
{
    public class TransitionScene : MonoBehaviour
    {

        #region Fields
        public Slider LoadingSlider = null;

        float _loadingSpeed = 1f;
        float _targetValue;
        AsyncOperation _operation;
        bool _haveSlider = false;
        #endregion

        #region MonoBehaviour Callbacks
        private void Start()
        {
            if (LoadingSlider != null)
            {
                _haveSlider = true;
                LoadingSlider.value = 0.0f;
            }
            if (SceneManager.GetActiveScene().name == SceneMgr.Instance.TransitionSceneName)
            {
                StartCoroutine(AsyncLoading());
            }
        }

        private void Update()
        {
            _targetValue = _operation.progress;
            if (_operation.progress >= 0.9f)
            {
                _targetValue = 1.0f;
            }
            if (_haveSlider)
            {
                if (_targetValue != LoadingSlider.value)
                {
                    //插值运算
                    LoadingSlider.value = Mathf.Lerp(LoadingSlider.value, _targetValue, Time.deltaTime);
                    if (Mathf.Abs(LoadingSlider.value - _targetValue) < 0.01f)
                    {
                        LoadingSlider.value = _targetValue;
                    }
                }

                if ((int)LoadingSlider.value * 100 == 100)
                {
                    _operation.allowSceneActivation = true;
                }
            }
            
        }

        #endregion

        #region Coroutines

        IEnumerator AsyncLoading()
        {
            _operation = SceneManager.LoadSceneAsync(SceneMgr.Instance.AsyncLoadNextSceneName);
            if(_haveSlider)
                _operation.allowSceneActivation = false;
            yield return _operation;
        }

        #endregion

    }

}