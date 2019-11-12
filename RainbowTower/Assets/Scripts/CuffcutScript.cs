using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS.Game
{
    public class CuffcutScript : MonoBehaviour
    {
        #region Fields
        // Start is called before the first frame update
        Color _a;

        #endregion

        #region MonoBehaviors Callbacks
        void Start()
        {
            gameObject.name = "Cuffcut";
            StartCoroutine(DestoryMyself());
            _a = GetComponent<MeshRenderer>().material.color;
        }

        // Update is called once per frame
        void Update()
        {
            transform.Translate(Vector3.down * Time.deltaTime);

            _a.a -= Time.deltaTime * 0.5f;

            GetComponent<MeshRenderer>().material.color = _a;

        }

        #endregion

        #region Coroutines
        IEnumerator DestoryMyself()
        {
            yield return new WaitUntil(() =>
            {
                if (_a.a < 0f)
                    return true;
                return false;
            });
            Destroy(gameObject);
        }
    }
    #endregion 
}