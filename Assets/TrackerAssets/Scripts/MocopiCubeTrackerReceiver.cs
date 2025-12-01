/*
 * Simple receiver component for MocopiCubeTracker
 * Alternative to MocopiSimpleReceiver that works with MocopiCubeTracker
 */
using Mocopi.Receiver.Core;
using UnityEngine;

namespace Mocopi.Receiver
{
    /// <summary>
    /// Simple component for receiving Mocopi data and applying it to MocopiCubeTracker
    /// </summary>
    public class MocopiCubeTrackerReceiver : MonoBehaviour
    {
        #region --Fields--
        /// <summary>
        /// The cube tracker component to update
        /// </summary>
        [Tooltip("The MocopiCubeTracker component that will receive the motion data")]
        public MocopiCubeTracker CubeTracker;

        /// <summary>
        /// UDP port to listen on
        /// </summary>
        [Tooltip("UDP port number to receive Mocopi data from")]
        public int Port = 12351;

        /// <summary>
        /// Automatically start receiving when enabled
        /// </summary>
        [Tooltip("Start receiving UDP data automatically when component is enabled")]
        public bool StartOnEnable = true;

        /// <summary>
        /// UDP receiver instance
        /// </summary>
        private MocopiUdpReceiver udpReceiver;
        #endregion --Fields--

        #region --Methods--
        /// <summary>
        /// OnEnable
        /// </summary>
        private void OnEnable()
        {
            if (StartOnEnable)
            {
                StartReceiving();
            }
        }

        /// <summary>
        /// OnDisable
        /// </summary>
        private void OnDisable()
        {
            if (StartOnEnable)
            {
                StopReceiving();
            }
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        private void OnDestroy()
        {
            UnsubscribeFromReceiver();
        }

        /// <summary>
        /// Start receiving UDP data
        /// </summary>
        public void StartReceiving()
        {
            StopReceiving();

            if (CubeTracker == null)
            {
                Debug.LogError("[MocopiCubeTrackerReceiver] CubeTracker is not assigned!");
                return;
            }

            // Create UDP receiver if needed
            if (udpReceiver == null)
            {
                udpReceiver = new MocopiUdpReceiver(Port);
            }

            // Subscribe to events
            udpReceiver.OnReceiveSkeletonDefinition += CubeTracker.InitializeSkeleton;
            udpReceiver.OnReceiveFrameData += CubeTracker.UpdateSkeleton;

            // Error handlers (optional - for debugging)
            udpReceiver.OnUdpStartFailed += OnUdpStartFailed;
            udpReceiver.OnUdpReceiveFailed += OnUdpReceiveFailed;

            // Start receiving
            udpReceiver.UdpStart();
            Debug.Log($"[MocopiCubeTrackerReceiver] Started receiving on port {Port}");
        }

        /// <summary>
        /// Stop receiving UDP data
        /// </summary>
        public void StopReceiving()
        {
            UnsubscribeFromReceiver();

            if (udpReceiver != null)
            {
                udpReceiver.UdpStop();
            }
        }

        /// <summary>
        /// Unsubscribe from receiver events
        /// </summary>
        private void UnsubscribeFromReceiver()
        {
            if (udpReceiver == null || CubeTracker == null)
            {
                return;
            }

            udpReceiver.OnReceiveSkeletonDefinition -= CubeTracker.InitializeSkeleton;
            udpReceiver.OnReceiveFrameData -= CubeTracker.UpdateSkeleton;
            udpReceiver.OnUdpStartFailed -= OnUdpStartFailed;
            udpReceiver.OnUdpReceiveFailed -= OnUdpReceiveFailed;
        }

        /// <summary>
        /// UDP start failed handler
        /// </summary>
        private void OnUdpStartFailed(System.Exception e)
        {
            Debug.LogError($"[MocopiCubeTrackerReceiver] UDP start failed: {e.Message}");
        }

        /// <summary>
        /// UDP receive failed handler
        /// </summary>
        private void OnUdpReceiveFailed(System.Exception e)
        {
            Debug.LogError($"[MocopiCubeTrackerReceiver] UDP receive failed: {e.Message}");
        }
        #endregion --Methods--
    }
}

