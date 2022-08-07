using Cinemachine;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A class used to store gyro properties per camera
    /// </summary>
    [Serializable]
    public class MMGyroCam
    {
        /// the bound cinemachine camera
        public CinemachineVirtualCamera Cam;
        /// the transform this camera should rotate around
        public Transform RotationCenter;
        /// the minimum rotation to apply to this camera (in degrees)
        public Vector2 MinRotation = new Vector2(-2f, -2f);
        /// the maximum rotation to apply to this camera (in degrees)
        public Vector2 MaxRotation = new Vector2(2f, 2f);
        /// the camera's initial angles
        [MMReadOnly]
        public Vector3 InitialAngles;
        /// the camera's initial position
        [MMReadOnly]
        public Vector3 InitialPosition;
    }

    /// <summary>
    /// Add this class to a camera rig (an empty object), bind some Cinemachine virtual cameras to it, and they'll move around the specified object as your gyro powered device moves
    /// </summary>
    public class MMGyroParallax : MMGyroscope
    {

        [Header("Cameras")]
        /// the list of cameras to move as the gyro moves
        public List<MMGyroCam> Cams;
        
        protected Vector3 _newAngles;

        /// <summary>
        /// On start we initialize our rig
        /// </summary>
        protected override void Start()
        {
            base.Start();
            Initialization();
        }

        /// <summary>
        /// Grabs the cameras and stores their position
        /// </summary>
        public virtual void Initialization()
        {
            foreach (MMGyroCam cam in Cams)
            {
                cam.InitialAngles = cam.Cam.transform.localEulerAngles;
                cam.InitialPosition = cam.Cam.transform.position;
            }
        }

        /// <summary>
        /// On Update we move our cameras
        /// </summary>
        protected override void Update()
        {
            base.Update();
            MoveCameras();
        }

        /// <summary>
        /// Moves cameras around based on gyro input
        /// </summary>
        protected virtual void MoveCameras()
        {            
            foreach (MMGyroCam cam in Cams)
            {
                float newX = MMMaths.Remap(LerpedCalibratedGyroscopeGravity.x, 0.5f, -0.5f, cam.MinRotation.x, cam.MaxRotation.x);
                float newY = MMMaths.Remap(LerpedCalibratedGyroscopeGravity.y, 0.5f, -0.5f, cam.MinRotation.y, cam.MaxRotation.y);

                _newAngles = cam.InitialAngles;
                _newAngles.x += newX;
                _newAngles.z += newY;

                cam.Cam.transform.position = cam.InitialPosition;
                cam.Cam.transform.localEulerAngles = cam.InitialAngles;
                cam.Cam.transform.RotateAround(cam.RotationCenter.transform.position, cam.RotationCenter.transform.up, _newAngles.x);
                cam.Cam.transform.RotateAround(cam.RotationCenter.transform.position, cam.RotationCenter.transform.right, _newAngles.z);
                cam.Cam.transform.LookAt(cam.RotationCenter.transform);
            }
        }
    }
}
