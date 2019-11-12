using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS.Common
{
    /// <summary>
    /// 物件池
    /// Create Time 2018.10.3:
    /// Lastest Time:2019.9.6
    /// </summary>
    public class GameObjectPool
    {
        #region Private Fields
        List<GameObject> _objsList;
        int _poolCount;
        GameObject _prefabObj;
        int _currentIndex;
        string _tag = "Untagged";
        #endregion

        #region Public Fields
        public string Tag
        {
            set
            {
                if (value == string.Empty || value == null || value == "")
                    _tag = "Untagged";
                else
                    _tag = value;
            }
            get
            {
                return _tag;
            }
        }
        public bool LockPool { set; get; }
        public int PoolCount
        {
            get
            {
                return _poolCount;
            }
            set
            {
                if (!LockPool)
                {
                    if (value > 0)
                        _poolCount = value;
                    else
                        _poolCount = 100;
                }
                else
                {
                    Debug.LogWarning("GameObjectPool/PoolCount Warning : the pool is locked ," +
                        " you can't change the pool. If you want to change the pool ,you need open the lock");
                }
            }
        }

        public GameObject PrefabObj
        {
            set
            {
                _prefabObj = value;
            }
        }
        #endregion

        #region Construction Methods
        public GameObjectPool()
        {
            _objsList = new List<GameObject>();
            _poolCount = 100;
            _currentIndex = 0;
            LockPool = true;
        }

        public GameObjectPool(GameObject prefab, int poolCount,string tag = "Untagged",bool isLock=true)
        {
            _objsList = new List<GameObject>();
            _prefabObj = prefab;
            _poolCount = poolCount;
            _currentIndex = 0;
            LockPool = isLock;
            Tag = tag;
        }
        #endregion

        #region Public Methods Get Instance Obj 
        public GameObject GetInstanceGameObj()
        {
            //if the list isnot full ,add a new gameObject into it and return it;
            if (_objsList.Count < _poolCount)
            {
                GameObject obj = GameObject.Instantiate(_prefabObj);
                obj.tag = Tag;
                obj.SetActive(false);
                _objsList.Add(obj);
                return obj;
            }
            else
            {
                for(int i = 0; i < _objsList.Count; i++)
                {
                    int temI = (_currentIndex + i) % _objsList.Count;
                    if (_objsList[temI] != null)//if the gameObject not be destory
                    {
                        if (!_objsList[temI].activeInHierarchy)//if the gameObject doesn't active in scene
                        {
                            Debug.Log("!!");
                            _currentIndex = (temI + 1) % _objsList.Count;
                            return _objsList[temI];//return it;
                        }
                    }
                    else
                    {
                        _objsList.Remove(_objsList[temI]);
                    }
                }
            }
            if (!LockPool)
            {
                GameObject obj = GameObject.Instantiate(_prefabObj);
                obj.tag = Tag;
                obj.SetActive(false);
                _objsList.Add(obj);
                _poolCount++;
                return obj;
            }
            Debug.LogWarning("GameObjectPool/GetInstanceGameObj Warning ； the pool fulled !");
            return null;
        }

        public GameObject GetInstanceGameObj(Transform parent)
        {
            //if the list isnot full ,add a new gameObject into it and return it;
            if (_objsList.Count < _poolCount)
            {
                GameObject obj = GameObject.Instantiate(_prefabObj,parent);
                obj.tag = Tag;
                obj.SetActive(false);
                _objsList.Add(obj);
                return obj;
            }
            else
            {
                for (int i = 0; i < _objsList.Count; i++)
                {
                    int temI = (_currentIndex + i) % _objsList.Count;
                    if (_objsList[temI] != null)//if the gameObject not be destory
                    {
                        if (!_objsList[temI].activeInHierarchy)//if the gameObject doesn't active in scene
                        {
                            _currentIndex = (temI + 1) % _objsList.Count;
                            _objsList[temI].transform.parent = parent;
                            return _objsList[temI];//return it;
                        }
                    }
                    else
                    {
                        _objsList.Remove(_objsList[temI]);
                    }
                }
            }
            if (!LockPool)
            {
                GameObject obj = GameObject.Instantiate(_prefabObj,parent);
                obj.tag = Tag;
                obj.SetActive(false);
                _objsList.Add(obj);
                _poolCount++;
                return obj;
            }
            Debug.LogWarning("GameObjectPool/GetInstanceGameObj Warning ； the pool fulled !");
            return null;
        }

        public GameObject GetInstanceGameObj(Vector3 position,Quaternion rotation)
        {
            //if the list isnot full ,add a new gameObject into it and return it;
            if (_objsList.Count < _poolCount)
            {
                GameObject obj = GameObject.Instantiate(_prefabObj, position,rotation);
                obj.tag = Tag;
                obj.SetActive(false);
                _objsList.Add(obj);
                return obj;
            }
            else
            {
                for (int i = 0; i < _objsList.Count; i++)
                {
                    int temI = (_currentIndex + i) % _objsList.Count;
                    if (_objsList[temI] != null)//if the gameObject not be destory
                    {
                        if (!_objsList[temI].activeInHierarchy)//if the gameObject doesn't active in scene
                        {
                            _currentIndex = (temI + 1) % _objsList.Count;
                            _objsList[temI].transform.position = position;
                            _objsList[temI].transform.rotation = rotation;
                            return _objsList[temI];//return it;
                        }
                    }
                    else
                    {
                        _objsList.Remove(_objsList[temI]);
                    }
                }
            }
            if (!LockPool)
            {
                GameObject obj = GameObject.Instantiate(_prefabObj, position,rotation);
                obj.tag = Tag;
                obj.SetActive(false);
                _objsList.Add(obj);
                _poolCount++;
                return obj;
            }
            Debug.LogWarning("GameObjectPool/GetInstanceGameObj Warning ； the pool fulled !");
            return null;
        }

        public GameObject GetInstanceGameObj(Vector3 position, Quaternion rotation,Transform parent)
        {
            //if the list isnot full ,add a new gameObject into it and return it;
            if (_objsList.Count < _poolCount)
            {
                GameObject obj = GameObject.Instantiate(_prefabObj, position,rotation,parent);
                obj.tag = Tag;
                obj.SetActive(false);
                _objsList.Add(obj);
                return obj;
            }
            else
            {
                for (int i = 0; i < _objsList.Count; i++)
                {
                    int temI = (_currentIndex + i) % _objsList.Count;
                    if (_objsList[temI] != null)//if the gameObject not be destory
                    {
                        if (!_objsList[temI].activeInHierarchy)//if the gameObject doesn't active in scene
                        {
                            _currentIndex = (temI + 1) % _objsList.Count;
                            _objsList[temI].transform.position = position;
                            _objsList[temI].transform.rotation = rotation;
                            _objsList[temI].transform.parent = parent;
                            return _objsList[temI];//return it;
                        }
                    }
                    else
                    {
                        _objsList.Remove(_objsList[temI]);
                    }
                }
            }
            if (!LockPool)
            {
                GameObject obj = GameObject.Instantiate(_prefabObj, position,rotation,parent);
                obj.tag = Tag;
                obj.SetActive(false);
                _objsList.Add(obj);
                _poolCount++;
                return obj;
            }
            Debug.LogWarning("GameObjectPool/GetInstanceGameObj Warning ； the pool fulled !");
            return null;
        }
        #endregion

        #region Public Methods Others
        public void DestroyPool()
        {
            if (_objsList == null)
            {
                Debug.LogError("GameObjectPool/DestroyPool Error : the pool is null !");
                return;
            }
            if (_objsList.Count == 0)
            {
                Debug.LogWarning("GameObjectPool/DestroyPool Warning : the pool is empty ");
                return;
            }
            for(int i = 0; i < _objsList.Count; i++)
            {
                if (_objsList[i] != null)
                {
                    GameObject.Destroy(_objsList[i]);
                }
            }
            _objsList.Clear();
        }
        #endregion

        #region Override Methods
        public override string ToString()
        {
            return "Prefab's name : " + _prefabObj.name + " Prefab's tag : " + _prefabObj.tag + " PoolCount : " + PoolCount;
        }
        #endregion
    }

}