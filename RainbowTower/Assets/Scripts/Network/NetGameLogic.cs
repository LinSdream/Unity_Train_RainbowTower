using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.EventSystems;
using LS.Test;
using UnityEngine.UI;

namespace LS.Game
{

    public enum NetStatus
    {
        Wait,
        Playing,
        End
    }

    public class NetGameLogic :GameLogic
    {
        #region Public Fields

        public Transform NetTopPlate;
        public float NetPlateMoveSpeed;
        public Text TimeText;
        public GameObject SettlementPanel;
        
        [HideInInspector]
        public Transform NetMovingPlate;
        [HideInInspector]
        public MoveDir NetMoveDir = MoveDir.FrontBack;
        [HideInInspector]
        public NetControl Network;
        [HideInInspector]
        public NetStatus Status = NetStatus.Wait;
        [HideInInspector]
        public MoveDir LogicMoveDir => _moveDir;
        //[HideInInspector]
        public NetData OtherPlayer;
        #endregion

        #region Private Fields

        Color _netMovingPlateColor;
        int[] _netRgbIndex = { 1, 2, 3 };
        bool _netReduce = true;
        Text _settlementText;
        float _gameTime;
        #endregion

        #region MonoBehaivour Callbacks

        new void Start()
        {
            _gameTime = Global.Instance.NetworkGameTime;
            SettlementPanel.SetActive(false);
            _settlementText = SettlementPanel.GetComponentInChildren<Text>();
            Network = GetComponent<NetControl>();
            base.Start();
            NetPlateMoveSpeed = InfoSettings.PlateMoveSpeed;
            NetTopPlate.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1);
            //发送RPC信息，让联机对象的设置窗口信息
            Network.Send_ColorIndex(_rgbIndex[0], _rgbIndex[1], _rgbIndex[2], _reduce);
            Network.Send_GameReady();

            _netMovingPlateColor = new Color(1, 1, 1);

            int id = Global.Instance.MyData.Id == 1 ? 2 : 1;
            ScoreText.text = "<color=#" + ColorUtility.ToHtmlStringRGB(_movingPlateColor) + ">" + Global.Instance.MyData.Name +
                 " : " + 0 + " </color>" + "\n"
                 + "<color=#" + ColorUtility.ToHtmlStringRGB(_netMovingPlateColor) + ">" + Global.Instance.FindData(id).Name +
                 " : " + 0 + " </color>";
        }

        new void Update()
        {

           
            if (Status == NetStatus.Playing)
            {
                //当前玩家结束游戏，等待x秒后，重新开始
                if (_gameOver)
                {
                    return;
                }

                if (_movingPlate == null)
                {
                    Debug.Log("NetGameLogic/Update into the if");
                    GenerateNewPlate();
                    Network.Send_GenerateNewPlate();
                }
                MovePlate();
                PlayerOperate();
            }
        }
        #endregion

        #region New Private Methods

        new void GenerateNewPlate()
        {
            _movingPlate = Instantiate(TopPlate, TopPlate.parent);

            _movingPlate.gameObject.name = "Plate";
            _movingPlate.position = new Vector3(TopPlate.position.x, TopPlate.position.y + 0.1f, TopPlate.position.z);
            switch (_moveDir)
            {
                case MoveDir.FrontBack:
                    _movingPlate.position += new Vector3(0, 0, -InfoSettings.Border + 0.1f);

                    break;
                case MoveDir.RightLeft:
                    _movingPlate.position += new Vector3(-InfoSettings.Border + 0.1f, 0, 0);
                    break;
            }

            _effectManager.CreateNaissanceEffect(_movingPlate.gameObject, _createEffectPool);
            //Set Color
            MeshRenderer render = _movingPlate.GetComponent<MeshRenderer>();
            Material m = render.material;
            m.color = ColorGraducalChange();
        }

        new void PlayerOperate()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            if (Input.GetButtonDown("Click"))
            {
                StopPlate();
                Network.Send_PlayerOperator();
            }
        }

        new void StopPlate()
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

            Network.Send_OperatorScore(1f);

            Split(ref _movingPlate, ref TopPlate, _moveDir);
            //TopPlate = _movingPlate;
            //_movingPlate = null;
            _moveDir = _moveDir == MoveDir.FrontBack ? MoveDir.RightLeft : MoveDir.FrontBack;

        }

        new IEnumerator GameOver()
        {
            _movingPlate = null;
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Network.Send_GameOver();

            Transform[] list = TopPlate.parent.GetComponentsInChildren<Transform>();
            List<Transform> childs = new List<Transform>(list);

            GameObject basePlate = null;
            for (int i = 0; i < childs.Count; i++)
            {
                //除噪
                if (childs[i].name == "Cuffcut" || childs[i].name== "BasePlate")
                {
                    if (childs[i].name == "BasePlate")
                    {
                        basePlate = childs[i].gameObject;
                    }
                    childs.Remove(childs[i]);    
                }
                    
            }

            //创建特效
            yield return StartCoroutine(_effectManager.WaitForTheBoom(childs, BoomEffect,()=> {

                TopPlate = basePlate.transform;

                if (TopPlate == null)
                {
                    Debug.LogError("NetGameLogic/GameOver Error : the top plate is null");
                    return;
                }
                TopPlate.GetComponent<MeshRenderer>().material.color = ColorGraducalChange();
            }));

            //惩罚等待时间
            yield return new WaitForSeconds(InfoSettings.DeadPunishTime);

            _gameOver = false;
            _movingPlate = null;
        }
        #endregion

        #region Private Methods
        Color ColorGraducalChange(int[] rgbIndex)
        {
            if (_reduce)
            {
                if (_rgb[rgbIndex[0]] > 0)
                    _rgb[rgbIndex[0]] -= 0.1f;
                else if (_rgb[rgbIndex[1]] > 0)
                    _rgb[rgbIndex[1]] -= 0.1f;
                else if (_rgb[rgbIndex[2]] > 0)
                    _rgb[rgbIndex[2]] -= 0.1f;
                else
                {
                    _netReduce = !_netReduce;
                    rgbIndex = RandomIndex();
                }
            }
            else
            {
                if (_rgb[rgbIndex[0]] < 1)
                    _rgb[rgbIndex[0]] += 0.1f;
                else if (_rgb[rgbIndex[1]] < 1)
                    _rgb[rgbIndex[1]] += 0.1f;
                else if (_rgb[rgbIndex[2]] < 1)
                    _rgb[rgbIndex[2]] += 0.1f;
                else
                {
                    _netReduce = !_netReduce;
                    rgbIndex = RandomIndex();
                }
            }

            _netMovingPlateColor.a = _rgb[0];
            _netMovingPlateColor.g = _rgb[1];
            _netMovingPlateColor.b = _rgb[2];

            return _netMovingPlateColor;
        }

        string GetScoreText()
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(_movingPlateColor) + ">" + Global.Instance.MyData.Name +
                 " : " + Global.Instance.MyData.Score + " </color>" + "\n"
                 + "<color=#" + ColorUtility.ToHtmlStringRGB(_netMovingPlateColor) + ">" + OtherPlayer.Name +
                 " : " + OtherPlayer.Score + " </color>";
        }

        #endregion

        #region Public Methods

        public void Net_SetScoreText()
        {
            ScoreText.text = GetScoreText();
        }

        public void Net_SetRgbIndex(int r,int g, int b,bool reduce)
        {
            _netRgbIndex[0] = r;
            _netRgbIndex[1] = g;
            _netRgbIndex[2] = b;
            _netReduce = reduce;
        }

        public void Net_GenerateNewPlate()
        {
            NetMovingPlate = Instantiate(NetTopPlate, NetTopPlate.parent);
            NetMovingPlate.gameObject.name = "Plate";
            NetMovingPlate.position = new Vector3(NetTopPlate.position.x, NetTopPlate.position.y + 0.1f, NetTopPlate.position.z);
            switch (NetMoveDir)
            {
                case MoveDir.FrontBack:
                    NetMovingPlate.position += new Vector3(0, 0, -InfoSettings.Border + 0.1f);
                    break;
                case MoveDir.RightLeft:
                    NetMovingPlate.position += new Vector3(-InfoSettings.Border + 0.1f, 0, 0);
                    break;
            }

            //_effectManager.CreateNaissanceEffect(NetMovingPlate.gameObject, _createEffectPool);
            //Set Color
            MeshRenderer render = NetMovingPlate.GetComponent<MeshRenderer>();
            Material m = render.material;
            m.color = ColorGraducalChange(_netRgbIndex);
        }

        #endregion


        #region Coroutines

        public IEnumerator WaitForGameEnd()
        {
            float i = _gameTime;
            bool onceLock = false;
            Color textColor = TimeText.color;

            Timer timer = new Timer(60f,()=> { TimeText.text = _gameTime.ToString()+"s"; },
                ()=> {
                    i--;
                    if (i <= 10&&!onceLock)
                    {
                        textColor = Color.red;
                        onceLock = true;
                    }
                    TimeText.text = i.ToString() + "s";
                },
                ()=> { 
                TimeText.text="Time up !";
                Status = NetStatus.End;
            });
            yield return StartCoroutine(timer.TimerForEveryXSecond());

            yield return new WaitForSeconds(0.5f);

            SettlementPanel.SetActive(true);
            int win = -1;
            if (Global.Instance.MyData.Score > OtherPlayer.Score) win = 0;
            else if (Global.Instance.MyData.Score < OtherPlayer.Score) win = 1;
            else win = 2;
            switch (win)
            {
                case -1:
                    Debug.LogError("NetGameLogic/IEnumerator WaitForGameEnd Error : Can't get winner !");
                    break;
                case 0:
                    _settlementText.text = "You are the winner，hero ！";
                    break;
                case 1:
                    _settlementText.text = "你丫的就是一个菜鸡";
                    break;
                case 2:
                    _settlementText.text = "英雄狭路相逢，来日再战";
                    break;
            }

            yield return new WaitForSeconds(2f);
            Network.ClearAfterGameEnd();
        }

        #endregion
    }

}