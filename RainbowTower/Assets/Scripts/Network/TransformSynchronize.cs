using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


namespace LS.Game
{

    /// <summary>
    /// 传递TopPlate的Postion，Scale信息，然后更改NetTopPlate信息，
    /// 注意得处理TopPlate被销毁的情况
    /// 该脚本挂载在GameManager上
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class TransformSynchronize : MonoBehaviour, IPunObservable
    {

        #region Second Code

        #region Private Fields

        NetGameLogic logic;//Network 逻辑

        float distance;//TopPlate距离

        PhotonView photonView;

        Vector3 direction;//Top Plate位置
        Vector3 networkPosition;//网络帧的位置
        Vector3 storedPosition;//差距

        bool firstTake = false;

        bool plateActive = false;//movingPlate是否存在，netMovingPlate根据plateActive来决定是否更改位置
        bool netPlateActive = false;//movingPlate是否存在，netMovingPlate根据plateActive来决定是否更改位置
        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            logic = FindObjectOfType<NetGameLogic>();
            photonView = GetComponent<PhotonView>();

            storedPosition = logic.TopPlate.transform.position;
            networkPosition = Vector3.zero;
        }

        private void OnEnable()
        {
            firstTake = true;
        }

        private void Update()
        {
            if (!photonView.IsMine)
            {
                if (logic.NetMovingPlate == null || logic.MovingPlate == null)
                {
                    Debug.LogWarning("TransformSynchronize/Update Warning : Logic's NetMovingPlate or MovingPlate is null !");
                    return;
                }
                Debug.Log("TransformSynchronize/Update Log : NetPlateActive : " + netPlateActive);
                if (netPlateActive)
                {
                    logic.NetMovingPlate.transform.position = Vector3.MoveTowards(logic.NetMovingPlate.transform.position,
                        networkPosition, distance * (1.0f / PhotonNetwork.SerializationRate));
                }
            }
        }
        #endregion

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //传递本地的plate状态
            if (logic.MovingPlate == null)
            {
                Debug.Log("MovingPlate is null");
                plateActive = false;
            }
            else
            {
                Debug.Log("MovingPlate is exist");
                plateActive = true;
            }

            if (stream.IsWriting)
            {
                stream.SendNext(plateActive);//传递logic的Moving Active
                Debug.Log("Send plateActive :" + plateActive);
                if (plateActive)
                {
                    direction = logic.MovingPlate.transform.position - storedPosition;
                    storedPosition = logic.MovingPlate.transform.position;

                    stream.SendNext(logic.MovingPlate.transform.position);
                    stream.SendNext(direction);
                    stream.SendNext(logic.MovingPlate.transform.localScale);
                }
                else
                {
                    //if the movingPlate null,send the Vector3.zero 
                    stream.SendNext(Vector3.zero);
                    stream.SendNext(Vector3.zero);
                    stream.SendNext(Vector3.zero);
                }
            }
            if (stream.IsReading)
            {

                //get the logic movingPlate has active
                netPlateActive = (bool)stream.ReceiveNext();

                Debug.Log("Receive netPlateActive : " + netPlateActive);

                if (netPlateActive)//if active,try to calculate the net plate transform
                {

                    //calculate the position
                    networkPosition = (Vector3)stream.ReceiveNext();
                    direction = (Vector3)stream.ReceiveNext();

                    if (firstTake)
                    {
                        logic.NetMovingPlate.transform.position = networkPosition;
                        distance = 0f;
                    }
                    else
                    {
                        float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                        networkPosition += direction * lag;

                        //calculate the lerp bewteen network  moving plate and logic moving plate (0,0,20)
                        networkPosition += new Vector3(0, 0, 20);
                        distance = Vector3.Distance(logic.NetMovingPlate.position, networkPosition);
                    }

                    //set the scle
                    logic.NetMovingPlate.transform.localScale = (Vector3)stream.ReceiveNext();
                }
                if (firstTake)
                {
                    firstTake = false;
                }
            }
        }

        #endregion

    }

}