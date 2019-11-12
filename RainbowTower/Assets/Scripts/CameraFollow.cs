using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS.Game
{
    public class CameraFollow : MonoBehaviour
    {
        #region Fields

        public float Height = 10f;
        public float FollowSpeed = 10f;
        public float RotateSpeed = 50f;
        public float ZoomSpeed = 200f;
        
        [HideInInspector]
        public Transform TargetPlate = null;
        [HideInInspector]
        public bool ZoomON;
        [HideInInspector]
        public float MinZoomValue;
        [HideInInspector]
        public float MaxZoomValue;
        [HideInInspector]
        public bool LockCamera = false;

        float _range => MaxZoomValue - MinZoomValue;
        Camera _camera;

        #endregion

        #region MonoBehaviour Callback


        private void Start()
        {
            _camera = transform.GetComponent<Camera>();
            MaxZoomValue = _camera.fieldOfView;
        }

        private void Update()
        {
            if (!LockCamera)
            {
                //the camera rotation
                Quaternion dir = Quaternion.LookRotation(TargetPlate.position - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, dir, 5);

                //thei camera position move
                Vector3 cameraPos = transform.position;
                cameraPos.y = TargetPlate.position.y + Height;

                transform.position = Vector3.Lerp(transform.position, cameraPos, Time.deltaTime * FollowSpeed);

                if (ZoomON)
                {
                    _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, CalculateZoomValue(), Time.deltaTime * ZoomSpeed);
                }

                float h = Input.GetAxis("Click");
                if (h > 0)
                {
                    transform.RotateAround(TargetPlate.position, Vector3.up, h * Time.deltaTime * RotateSpeed);
                }

                //float slider = Input.GetAxis("Mouse ScrollWheel");
                //GetComponent<Camera>().fieldOfView -= slider * Time.deltaTime * ZoomSpeed;
            }
        }

        #endregion

        #region Public Methods
        public void ResetCamera()
        {
            LockCamera = true;
            Debug.Log("ResetCamera");
            _camera.fieldOfView = MaxZoomValue;
            Debug.Log(_camera.fieldOfView);

        }
        #endregion

        #region Private Methods
        float CalculateZoomValue()
        {
            float percentage = TargetPlate.localScale.x > TargetPlate.localScale.z ? TargetPlate.localScale.z 
                : TargetPlate.localScale.x;

            return _range * percentage + MinZoomValue;

        }
        #endregion

    }
}
