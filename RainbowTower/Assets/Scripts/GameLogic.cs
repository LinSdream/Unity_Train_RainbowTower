using LS.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LS.Helper.Others;
using UnityEngine.EventSystems;

namespace LS.Game
{
    public enum MoveDir
    {
        FrontBack,
        RightLeft
    }

    public class GameLogic : MonoBehaviour
    {

        #region Public Fields
        public GameInfoSetting InfoSettings;
        public Transform TopPlate;
        public float MoveSpeed;
        public Text ScoreText;
        public GameObject PlayteEffect;
        public GameObject CreatePlateEffect;
        public GameObject AT_Effect;
        public GameObject BoomEffect;

        [HideInInspector]
        public Transform MovingPlate => _movingPlate;

        [HideInInspector]
        public Transform CutDown;//落下的plate，用于联网时的同步特效
        #endregion

        #region Protected Fields

        protected Transform _movingPlate = null;//移动盘子
        protected MoveDir _moveDir = MoveDir.FrontBack;//移动方向
        protected bool _inverseMove = false;//移动的正负
        protected bool _gameOver = false;//游戏结束
        protected CameraFollow _camera;//镜头脚本
        protected int _score = 0;//分数
        protected Color _movingPlateColor;//移动Cube的颜色
        protected float[] _rgb = { 1, 1, 1 };//rgb颜色数组
        protected int[] _rgbIndex = { 0, 1, 2 };//颜色索引数组
        protected bool _reduce = true;//颜色是否减少

        protected bool _colorGradualChange;//是否开启颜色渐变

        protected GameObjectPool _spericalEffectPool;//落下特效物件池
        protected GameObjectPool _createEffectPool;//创建特效物件池
        protected GameObjectPool _atEffectPool;//AT特效物件池
        protected GameObjectPool _CutPlatePool;//落下的Cube的物件池
        protected EffectManage _effectManager;//特效管理器

        #endregion

        #region MonoBehaviour Callback

        protected void Awake()
        {
            _spericalEffectPool = new GameObjectPool(PlayteEffect, 10);
            _createEffectPool = new GameObjectPool(CreatePlateEffect, 10);
            _atEffectPool = new GameObjectPool(AT_Effect,10);
            _effectManager = new EffectManage();
        }

        protected void Start()
        {
            //Set gameSettings
            _colorGradualChange =InfoSettings.ColorGradualChange;

            //Set camera
            _camera = Camera.main.gameObject.GetComponent<CameraFollow>();
            _camera.TargetPlate = TopPlate;
            _camera.ZoomON = InfoSettings.CameraZoomON;
            _camera.MinZoomValue = InfoSettings.MinZoomValue;

            MoveSpeed = InfoSettings.PlateMoveSpeed;

            _movingPlateColor = new Color(1, 1, 1);
            ScoreText.text = "0";
            TopPlate.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1);

            //Random the color
            _rgbIndex = RandomIndex();
        }

        // Update is called once per frame
        protected void Update()
        {
            if (_gameOver)
            {
                return;
            }
            if (_movingPlate == null)
            {
                GenerateNewPlate();
            }
            MovePlate();
            PlayerOperate();
        }

        #endregion

        #region Protected Methods

        protected void GenerateNewPlate()
        {
            _movingPlate = Instantiate(TopPlate,TopPlate.parent);
            _movingPlate.gameObject.name = "Plate";
            _movingPlate.position = new Vector3(TopPlate.position.x, TopPlate.position.y + 0.1f, TopPlate.position.z);
            switch (_moveDir)
            {
                case MoveDir.FrontBack:
                    _movingPlate.position += new Vector3(0, 0, -InfoSettings.Border+0.1f);

                    break;
                case MoveDir.RightLeft:
                    _movingPlate.position += new Vector3(-InfoSettings.Border+0.1f, 0, 0);
                    break;
            }

            _effectManager.CreateNaissanceEffect(_movingPlate.gameObject,_createEffectPool);
            //Set Color
            MeshRenderer render = _movingPlate.GetComponent<MeshRenderer>();
            Material m = render.material;
            m.color = ColorGraducalChange();

        }

        protected void MovePlate()
        {
            Vector3 move = Vector3.zero;
            Vector3 size = _movingPlate.localScale;
            switch (_moveDir)
            {
                case MoveDir.FrontBack:
                    move = new Vector3(0, 0, MoveSpeed);
                    if (Mathf.Abs(_movingPlate.position.z - TopPlate.position.z) >= InfoSettings.Border+0.1f)
                    {
                        //if (_movingPlate.position.z > TopPlate.position.z)
                        //    _effectManager.CreateAT_Field(_movingPlate, _atEffectPool, Vector3.forward, size.z, 0, size.x);
                        //else
                        //    _effectManager.CreateAT_Field(_movingPlate, _atEffectPool, Vector3.back, size.z, 0, size.x);
                        _inverseMove = !_inverseMove;
                    }
                    break;

                case MoveDir.RightLeft:
                    move = new Vector3(MoveSpeed, 0, 0);
                    if (Mathf.Abs(_movingPlate.position.x - TopPlate.position.x) >= InfoSettings.Border+0.1f)
                    {
                        //if (_movingPlate.position.z > TopPlate.position.z)
                        //    _effectManager.CreateAT_Field(_movingPlate, _atEffectPool, Vector3.forward, size.x, 90, size.z);
                        //else
                        //    _effectManager.CreateAT_Field(_movingPlate, _atEffectPool, Vector3.back, size.x, 90, size.z);
                        _inverseMove = !_inverseMove;
                    }
                    break;
            }
            if (_inverseMove)
            {
                move = -move;
            }
            _movingPlate.Translate(move * Time.deltaTime);
        }

        protected void PlayerOperate()
        {
            //点击UI与点击屏幕分离
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            if (Input.GetButtonDown("Click"))
            {
                StopPlate();
            }
        }

        protected void StopPlate()
        {
            switch (_moveDir)
            {
                case MoveDir.FrontBack:
                    if (Mathf.Abs(_movingPlate.position.z - TopPlate.position.z) > _movingPlate.localScale.z)
                        _gameOver = true;
                    break;

                case MoveDir.RightLeft:
                    if (Mathf.Abs(_movingPlate.position.x - TopPlate.position.x) > _movingPlate.localScale.x)
                        _gameOver = true;
                    break;
            }
            if (_gameOver)
            {
                StartCoroutine(GameOver());
                return;
            }

            _score++;
            ScoreText.text = _score.ToString();
            ScoreText.color = new Color(_rgb[0], _rgb[1], _rgb[2]);


            Split(ref _movingPlate,ref TopPlate,_moveDir);

            //TopPlate = _movingPlate;
            //_movingPlate = null;
            _moveDir = _moveDir == MoveDir.FrontBack ? MoveDir.RightLeft : MoveDir.FrontBack;

        }

        protected void Split(ref Transform moveCube, ref Transform topCube, MoveDir dir)
        {
            Transform stayPlate = null;
            Transform cutPlate = null;

            float cutFront, cutBack, stayFront, stayBack;
            float cutRight, cutLeft, stayRight, stayLeft;

            if (dir == MoveDir.FrontBack)
            {
                float moveFront = moveCube.position.z + moveCube.localScale.z / 2;
                float moveBack = moveCube.position.z - moveCube.localScale.z / 2;

                float topFront = topCube.position.z + topCube.localScale.z / 2;
                float topBack = topCube.position.z - topCube.localScale.z / 2;

                if (moveCube.position.z > topCube.position.z)
                {
                    cutFront = moveFront;
                    cutBack = topFront;

                    stayFront = topFront;
                    stayBack = moveBack;

                }
                else
                {
                    cutFront = topBack;
                    cutBack = moveBack;

                    stayFront = moveFront;
                    stayBack = topBack;
                }

                Destroy(moveCube.gameObject);

                stayPlate = Instantiate(moveCube, topCube.parent);
                stayPlate.position = new Vector3(stayPlate.position.x, stayPlate.position.y, (stayFront + stayBack) / 2);

                stayPlate.localScale = new Vector3(stayPlate.localScale.x, stayPlate.localScale.y, (stayFront - stayBack));

                if (stayPlate.localScale != topCube.localScale)
                {
                    cutPlate = Instantiate(moveCube, topCube.parent);
                    cutPlate.position = new Vector3(cutPlate.position.x, cutPlate.position.y, (cutFront + cutBack) / 2);

                    cutPlate.localScale = new Vector3(cutPlate.localScale.x, cutPlate.localScale.y, (cutFront - cutBack));
                }
            }
            else
            {
                float moveRight = moveCube.position.x + moveCube.localScale.x / 2;
                float moveLeft = moveCube.position.x - moveCube.localScale.x / 2;

                float topRight = topCube.position.x + topCube.localScale.x / 2;
                float topLeft = topCube.position.x - topCube.localScale.x / 2;

                if (moveCube.position.x > topCube.position.x)
                {
                    cutRight = moveRight;
                    cutLeft = topLeft;

                    stayRight = topRight;
                    stayLeft = moveLeft;

                }
                else
                {
                    cutRight = topLeft;
                    cutLeft = moveLeft;

                    stayRight = moveRight;
                    stayLeft = topLeft;
                }

                Destroy(moveCube.gameObject);

                stayPlate = Instantiate(moveCube, topCube.parent);
                stayPlate.position = new Vector3((stayRight + stayLeft) / 2, stayPlate.position.y, stayPlate.position.z);

                stayPlate.localScale = new Vector3((stayRight - stayLeft), stayPlate.localScale.y, stayPlate.localScale.z);

                if (stayPlate.localScale != topCube.localScale)
                {
                    cutPlate = Instantiate(moveCube, topCube.parent);
                    cutPlate.position = new Vector3((cutRight + cutLeft) / 2, cutPlate.position.y, cutPlate.position.z);

                    cutPlate.localScale = new Vector3((cutRight - cutLeft), cutPlate.localScale.y, cutPlate.localScale.z);
                }
            }

            if (cutPlate != null)
            {
                cutPlate.gameObject.AddComponent<CuffcutScript>();
            }

            if (stayPlate == null)
            {
                topCube = moveCube;
                _camera.TargetPlate = topCube;
            }
            else
            {
                topCube = stayPlate;
                _camera.TargetPlate = topCube;
            }
            _effectManager.CreateSpericalEffect(topCube.gameObject, _spericalEffectPool);

            moveCube = null;
            CutDown = cutPlate;

            

        }

        protected Color ColorGraducalChange()
        {
            if (!_colorGradualChange)
            {
                _movingPlateColor.a = 1;
                _movingPlateColor.g = 1;
                _movingPlateColor.b = 1;
                return _movingPlateColor;
            }

            if (_reduce)
            {
                if (_rgb[_rgbIndex[0]] > 0)
                    _rgb[_rgbIndex[0]] -= 0.1f;
                else if (_rgb[_rgbIndex[1]] > 0)
                    _rgb[_rgbIndex[1]] -= 0.1f;
                else if (_rgb[_rgbIndex[2]] > 0)
                    _rgb[_rgbIndex[2]] -= 0.1f;
                else
                {
                    _reduce = !_reduce;
                    _rgbIndex = RandomIndex();
                }
            }
            else
            {
                if (_rgb[_rgbIndex[0]] <1)
                    _rgb[_rgbIndex[0]] += 0.1f;
                else if (_rgb[_rgbIndex[1]] <1)
                    _rgb[_rgbIndex[1]] += 0.1f;
                else if (_rgb[_rgbIndex[2]]<1)
                    _rgb[_rgbIndex[2]] += 0.1f;
                else
                {
                    _reduce = !_reduce;
                    _rgbIndex = RandomIndex();
                }
            }
            _movingPlateColor.r = _rgb[0];
            _movingPlateColor.g = _rgb[1];
            _movingPlateColor.b = _rgb[2];
            return _movingPlateColor;
        }

        protected int[] RandomIndex()
        {
            List<int> list1 = new List<int>();
            for (int i = 0; i < _rgbIndex.Length; i++)
            {
                list1.Add(_rgbIndex[i]);
            }

            //洗牌算法
            for(int i = list1.Count-1; i > 0; i--)
            {
                int index = Random.Range(0, i);
                int tmp = list1[index];
                list1[index] = list1[i];
                list1[i] = tmp;
            }

            return list1.ToArray();
        }
        #endregion

        #region Coroutines
        protected IEnumerator GameOver() 
        {
            //Reset Camera
            _camera.ResetCamera();
            Transform[] list = TopPlate.parent.GetComponentsInChildren<Transform>();
            List<Transform> childs = new List<Transform>(list);
            for(int i = 0; i < childs.Count; i++)
            {
                //除噪
                if (childs[i].name == "Cuffcut")
                    childs.Remove(childs[i]);
            }
            Debug.Log(childs.Count);
            //Create Boom SE
            StartCoroutine(_effectManager.WaitForTheBoom(childs, BoomEffect,
                () => { SceneMgr.Instance.CustomLoadScene("01_Main"); }));
            yield return null;
        }
        #endregion

        #region Public Methods
        public void LoadGameScene()
        {
            SceneMgr.Instance.CustomLoadScene("01_Main");
        }
        #endregion

    }

}