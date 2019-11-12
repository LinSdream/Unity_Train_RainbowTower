using ExitGames.Client.Photon;
using LS.Helper.Others;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS.Game
{
    [RequireComponent(typeof(PhotonView))]
    public class NetControl : MonoBehaviourPunCallbacks
    {
        #region Fields
        [HideInInspector]
        public NetGameLogic Logic;

        private EffectManage _effectManager;
        #endregion

        #region Serialize and Deserialize NetData Class

        static byte[] SerializeMoveDir(object obj)
        {
            int index = 0;
            MoveDir data = (MoveDir)obj;

            int tmp;

            switch (data)
            {
                case MoveDir.FrontBack:
                    tmp = 0;
                    break;
                case MoveDir.RightLeft:
                    tmp = 1;
                    break;
                default:
                    tmp = -1;
                    break;
            }

            byte[] bytes = new byte[4];
            Protocol.Serialize(tmp, bytes, ref index);
            return bytes;

        }

        static object DeserializeMoveDir(byte[] bytes)
        {
            MoveDir dir;

            int index = 0;
            int tmp;
            Protocol.Deserialize(out tmp, bytes, ref index);
            if (tmp == 0)
            {
                dir = MoveDir.FrontBack;
            }else if (tmp == 1)
            {
                dir = MoveDir.RightLeft;
            }
            else
            {
                Debug.LogError("NetControl/DeserialNetData Error : the data has error!");
                dir = MoveDir.FrontBack;
            }
            return dir;
        }
        #endregion

        #region MonoBehaviour Callbacks
        private void Start()
        {

            _effectManager = new EffectManage();

            PhotonPeer.RegisterType(typeof(NetData), (byte)'M', SerializeMoveDir, DeserializeMoveDir);

            Logic = GetComponent<NetGameLogic>();
            Global.Instance.MyData.Ready = true;
            Global.Instance.FindData(Global.Instance.MyData).Ready = true;

            PhotonNetwork.Instantiate("NetTransform",Vector3.zero,Quaternion.identity);

            if (PhotonNetwork.IsMasterClient)
            {
                photonView.StartCoroutine(WaitForOtherPlayersStart());
            }
        }
        #endregion

        #region Send Message
        public void Send_ColorIndex(int r,int g,int b,bool reduce)
        {
            photonView.RPC("RPC_Others_ColorIndex", RpcTarget.Others, r, g, b, reduce);
        }

        public void Send_PlayerOperator()
        {
            if (Logic.TopPlate == null) return;
            Vector3 position = Logic.TopPlate.position+ new Vector3(0, 0, 20);
            Vector3 downPos = Vector3.zero;
            Vector3 downScale = Vector3.zero;
            if (Logic.CutDown != null)
            {
                downPos = Logic.CutDown.position;
                downPos += new Vector3(0, 0, 20);

                downScale = Logic.CutDown.localScale;
            }
            photonView.RPC("RPC_Others_PlayerOperator",RpcTarget.Others,
                position, Logic.TopPlate.localScale,
                downPos,downScale);
        }

        public void Send_GenerateNewPlate()
        {
            photonView.RPC("RPC_Others_GenerateNewPlate", RpcTarget.Others);
        }

        public void Send_GameReady()
        {
            photonView.RPC("RPC_Others_GameReady",RpcTarget.Others,Global.Instance.MyData.Id);
        }

        public void Send_GameStart()
        {
            photonView.RPC("RPC_ALL_GameStart", RpcTarget.All);
        }

        public void Send_OperatorScore(float score)
        {
            //更改List表
            Global.Instance.FindData(Global.Instance.MyData).Score += score;
            Logic.Net_SetScoreText();
            photonView.RPC("RPC_Others_OperatorScore", RpcTarget.Others, Global.Instance.MyData.Id, score);
        }

        public void Send_GameOver()
        {
            photonView.RPC("RPC_Others_GameOver", RpcTarget.Others);
        }

        public void ClearAfterGameEnd()
        {
            PhotonNetwork.LeaveRoom();
        }

        #endregion

        #region RPCs

        /// <summary>
        /// 设置联机对象的  _rgbIndex 与联机对象的颜色是否减少
        /// </summary>
        [PunRPC]
        void RPC_Others_ColorIndex(int r,int g,int b,bool reduce)
        {
            Logic.Net_SetRgbIndex(r, g, b, reduce);
        }

        [PunRPC]
        void RPC_Others_PlayerOperator(Vector3 position,Vector3 scale,Vector3 downPos,Vector3 downScale)
        {
            if (Logic.NetMovingPlate == null || Logic.NetTopPlate == null)
            {
                Debug.LogError("NetControl/RPC_Others_PlayerOpertor Error : Can't find the NetMovingPlate or NetTopPlate , " +
                    "NetGameLogic's NetMovingPlate or NetTopPlate maybe have somethings errors !");
                return;
            }

            if (Logic.NetMovingPlate != null)
            {
                Destroy(Logic.NetMovingPlate.gameObject);

                Transform stay = Instantiate(Logic.NetMovingPlate, Logic.NetMovingPlate.parent);
                stay.transform.position = position;
                stay.localScale = scale;
                Logic.NetTopPlate = stay;

                Transform down = Instantiate(Logic.NetMovingPlate, Logic.NetMovingPlate.parent);
                down.position = downPos;
                down.localScale = downScale;

                down.gameObject.AddComponent<CuffcutScript>();
            }
            Logic.NetMoveDir = (Logic.NetMoveDir == MoveDir.FrontBack) ? MoveDir.RightLeft : MoveDir.FrontBack;
        }

        [PunRPC]
        void RPC_Others_GenerateNewPlate()
        {
            Logic.Net_GenerateNewPlate();
        }

        [PunRPC]
        void RPC_Others_GameReady(int id)
        {
            Global.Instance.FindData(id).Ready = true;
            Logic.OtherPlayer = Global.Instance.FindData(id);
        }

        [PunRPC]
        void RPC_ALL_GameStart()
        {
            Logic.Status = NetStatus.Playing;
            StartCoroutine(Logic.WaitForGameEnd());
        }

        [PunRPC]
        void RPC_Others_OperatorScore(int id,float score)
        {
            Global.Instance.FindData(id).Score += score;
            Logic.Net_SetScoreText();
        }

        [PunRPC]
        void RPC_Others_GameOver()
        {
            StartCoroutine(OthersPlayerGameOver());
        }

        #endregion

        #region Pun Callbacks
        public override void OnLeftRoom()
        {
            Global.Instance.Clear();
            SceneMgr.Instance.CustomLoadScene(SceneMgr.Instance.PreviousSceneName);
        }
        #endregion

        #region Coroutines
        IEnumerator WaitForOtherPlayersStart()
        {
            yield return new WaitUntil(() =>
            {
                foreach (NetData data in Global.Instance.PlayersData)
                {
                    if (!data.Ready)
                        return false;
                }
                Debug.Log("NetControl/WaitForOtherPlayerStart : All Ready");
                return true;
            });

            Send_GameStart();
        }

        IEnumerator OthersPlayerGameOver()
        {
            Transform[] list = Logic.NetTopPlate.parent.GetComponentsInChildren<Transform>();
            GameObject basePlate = null;

            List<Transform> childs = new List<Transform>(list);

            for (int i = 0; i < childs.Count; i++)
            {
                //除噪
                if (childs[i].name == "Cuffcut" || childs[i].name == "NetBasePlate")
                {
                    if (childs[i].name == "NetBasePlate")
                    {
                        basePlate = childs[i].gameObject;
                    }
                    childs.Remove(childs[i]);
                }
            }
            //创建特效
            yield return StartCoroutine(_effectManager.WaitForTheBoom(childs, Logic.BoomEffect, () => {

                Logic.NetTopPlate = basePlate.transform;

                Logic.NetMoveDir = MoveDir.FrontBack;

                if (Logic.NetTopPlate == null)
                {
                    Debug.LogError("NetGameLogic/GameOver Error : the top plate is null");
                    return;
                }
            }));
        }
        #endregion

    }

}