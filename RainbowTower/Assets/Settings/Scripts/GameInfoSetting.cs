using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS.Game
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "CustomizeSettings/GameInfoSettings")]
    public class GameInfoSetting : ScriptableObject
    {
        /// <summary> The plate MoveSpeed  </summary>
        [Tooltip("The plate MoveSpeed")]
        public float PlateMoveSpeed = 3f;

        /// <summary>  The plate can move boundary ,the value base the plate below the moving plate </summary>
        [Tooltip("The plate can move boundary")]
        public float Border = 2f;
        
        /// <summary> Whether to turn on camera zoom by the plate changed </summary>
        [Tooltip("Whether to turn on camera zoom by the plate changed")]
        public bool CameraZoomON = true;

        /// <summary> The Camera can Zoom the Min value</summary>
        [Tooltip("The Camera can Zoom the Min value")]
        public float MinZoomValue = 10f;

        /// <summary> Whether open the color gradual change</summary>
        [Tooltip("Whether open the color gradual change")]
        public bool ColorGradualChange = true;

        /// <summary> When player death in the net modle </summary>
        [Tooltip("When player death in the net modle")]
        public float DeadPunishTime = 1f;
    }

}