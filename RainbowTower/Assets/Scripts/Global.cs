using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LS.Common;

namespace LS.Game
{

    [System.Serializable]
    public class NetData
    {
        public string Name;
        public int Id;
        public bool Ready;
        public float Score;

        public NetData()
        {
            Name = string.Empty;
            Id = -1;
            Ready = false;
            Score = 0f;
        }
    }

    public class Global : ASingletonBasis<Global>
    {
        #region Public Fields
        public NetData MyData;
        public List<NetData> PlayersData;
        public float NetworkGameTime;
        #endregion

        #region Public Methods
        public NetData FindData(int id)
        {
            return PlayersData.Find(value => value.Id == id);
        }

        public NetData FindData(NetData data)
        {
            return PlayersData.Find(value => value == data);
        }

        public void ClearMyData()
        {
            MyData.Ready = false;
            MyData.Id = -1;
            MyData.Score = 0f;
        }

        public void ClearReaydStatus()
        {
            foreach(NetData data in PlayersData)
            {
                data.Ready = false;
            }
        }

        public void Clear()
        {
            ClearMyData();
            PlayersData.Clear();
        }

        #endregion

        #region Override Methods
        public override void Init()
        {
            MyData = new NetData();
            PlayersData = new List<NetData>();
        }

        #endregion

    }

}