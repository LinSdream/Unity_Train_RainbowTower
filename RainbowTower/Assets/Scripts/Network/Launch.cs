using ExitGames.Client.Photon;
using LS.Common;
using LS.Helper.Others;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace LS.Game
{
    [RequireComponent(typeof(PhotonView))]
    public class Launch : MonoBehaviourPunCallbacks
    {

        #region Public Fields

        public Button Connect_Btn;
        public Button Start_Btn;
        public Button ReturnMenu_Btn;

        #endregion

        #region MonoBehaviouor Callbacks
        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Start()
        {
            PhotonPeer.RegisterType(typeof(NetData), (byte)'U', SerializeNetData, DeserializeNetData);
        }
        #endregion

        #region Serialize and Deserialize NetData Class
        static byte[] SerializeNetData(object obj)
        {
            int index = 0;
            NetData data = (NetData)obj;

            byte[] strBytes = Encoding.Default.GetBytes(data.Name);

            byte[] bytes = new byte[2 * 4 + 1 * 4 + strBytes.Length];

            int @bool = data.Ready ? 1 : 0;
            
            Protocol.Serialize(data.Id, bytes, ref index);
            Protocol.Serialize(@bool, bytes, ref index);
            Protocol.Serialize(data.Score, bytes, ref index);
            strBytes.CopyTo(bytes, index);

            return bytes;

        }

        static object DeserializeNetData(byte[] bytes)
        {
            NetData data = new NetData();
            int index = 0;

            int @bool;

            Protocol.Deserialize(out data.Id, bytes, ref index);
            Protocol.Deserialize(out @bool, bytes, ref index);
            Protocol.Deserialize(out data.Score, bytes, ref index);

            data.Ready = @bool == 1 ? true : false;
            data.Name = Encoding.Default.GetString(bytes, index, bytes.Length - 2 * 4 - 4);

            return data;
        }
        #endregion

        #region Private Methods
        int SetPlayerId()
        {
            List<int> ids = new List<int>();
            for (int i = 0; i < Global.Instance.PlayersData.Count; i++)
                ids.Add(Global.Instance.PlayersData[i].Id);
            ids.Sort();

            List<int> all = new List<int> { 1, 2 };
            ids = all.Except(ids).ToList();
            if (ids.Count != 0)
                return ids[0];
            Debug.LogError("NetControl/SetPlayerId Error : The PlayerId is -1 ,list of ids is " + ids.Count);
            return -1;
        }

        #endregion

        #region RPCs

        [PunRPC]
        void RPC_Master_RequestSetID(Player player)
        {
            int id = SetPlayerId();
            photonView.RPC("RPC_Client_SetId", player, id);
        }

        [PunRPC]
        void RPC_Master_RequestMasterNetData(Player player)
        {
            photonView.RPC("RPC_Client_GetMasterNetData", player, Global.Instance.MyData);
        }

        [PunRPC]
        void RPC_Client_SetId(int id)
        {
            Global.Instance.MyData.Id = id;
            Global.Instance.PlayersData.Add(Global.Instance.MyData);
            Send_WallMyData(RpcTarget.MasterClient, Global.Instance.MyData);
        }

        [PunRPC]
        void RPC_Client_GetMasterNetData(NetData data)
        {
            Global.Instance.PlayersData.Add(data);
        }

        [PunRPC]
        void RPC_Master_Ready(int id)
        {
            Global.Instance.FindData(id).Ready = true;
        }

        [PunRPC]
        void RPC_ALL_LoadScene()
        {
            //清除ready状态,为游戏期间准备，ready状态复用，减少通信的包体
            Global.Instance.ClearReaydStatus();
            Global.Instance.MyData.Ready = false;

            SceneMgr.Instance.CustomLoadScene("03_Network");
        }

        [PunRPC]
        void RPC_WallMyData(NetData info)
        {
           foreach(NetData index in Global.Instance.PlayersData)
            {
                if (index.Id == info.Id)
                {
                    index.Name = info.Name;
                    index.Ready = info.Ready;
                    return;
                }
            }
            Global.Instance.PlayersData.Add(info);
        }
        #endregion

        #region Send Message Methods
        public void Send_RequestSetId()
        {
            photonView.RPC("RPC_Master_RequestSetID", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
        }

        public void Send_RequestMasterNetData()
        {
            photonView.RPC("RPC_Master_RequestMasterNetData", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
        }

        public void Send_Ready()
        {
            photonView.RPC("RPC_Master_Ready", RpcTarget.MasterClient, Global.Instance.MyData.Id);
        }

        public void Send_LoadScene()
        {
            photonView.RPC("RPC_ALL_LoadScene", RpcTarget.All);
        }

        public void Send_WallMyData(RpcTarget who,NetData wallData)
        {
            photonView.RPC("RPC_WallMyData", who, wallData);
        }
        #endregion

        #region Buttons
        public void Btn_Connect()
        {
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.JoinRandomRoom();
            else
            {
                PhotonNetwork.GameVersion = "0.0.1";
                PhotonNetwork.ConnectUsingSettings();
            }
            Connect_Btn.interactable = false;
        }

        public void Btn_ReturnMenu()
        {
            SceneMgr.Instance.CustomLoadScene("00_Menu");
        }

        public void Btn_Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                foreach(NetData data in Global.Instance.PlayersData)
                {
                    if (!data.Ready)
                        return;
                }
                Send_LoadScene();
            }
            else
            {
                Global.Instance.MyData.Ready = true;
                Global.Instance.FindData(Global.Instance.MyData).Ready = true;
                Send_Ready();
            }
        }
        #endregion

        #region Pun2 Callbacks
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            PhotonNetwork.CreateRoom("Test", new RoomOptions { MaxPlayers = 2 });
        }

        public override void OnJoinedRoom()
        {
            ReturnMenu_Btn.gameObject.SetActive(false);

            if (!PhotonNetwork.IsMasterClient)
            {
                //向主机申请获取主机信息
                Send_RequestMasterNetData();
                //向主机申请id号
                Send_RequestSetId();
                //更改按键文本
                Start_Btn.GetComponentInChildren<Text>().text = "Reay Game";
            }
            else
            {
                //是主机，游戏准别状态改为true
                Global.Instance.MyData.Ready = true;
                //设置自己的id号
                Global.Instance.MyData.Id = SetPlayerId();
                //在本地玩家列表缓存中添加自己信息
                Global.Instance.PlayersData.Add(Global.Instance.MyData);
                //更改按键文本
                Start_Btn.GetComponentInChildren<Text>().text = "Start Game"; 
            }

            Connect_Btn.gameObject.SetActive(false);
            Start_Btn.gameObject.SetActive(true);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Connect_Btn.interactable = true;
            Connect_Btn.gameObject.SetActive(true);

            ReturnMenu_Btn.gameObject.SetActive(true);
        }

        public override void OnConnected()
        {
            Debug.Log("Connected");
            int name=UnityEngine.Random.Range(0, 10);
            Global.Instance.MyData.Name = "Name:"+name.ToString();
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinRandomRoom();
        }
        #endregion

    }

}