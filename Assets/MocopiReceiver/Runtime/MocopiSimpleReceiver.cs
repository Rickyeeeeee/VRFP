/*
 * Copyright 2022 Sony Corporation
 */
using Mocopi.Receiver.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Mocopi.Receiver
{
    /// <summary>
    /// The simple component for receiving and adapting motion from UDP.
    /// Supports multiple avatars (possibly sharing the same port).
    /// </summary>
    public sealed class MocopiSimpleReceiver : MonoBehaviour
    {
        #region --Fields--
        /// <summary>
        /// Avatar list (each entry = avatar + port)
        /// </summary>
        public List<MocopiSimpleReceiverAvatarSettings> AvatarSettings = new List<MocopiSimpleReceiverAvatarSettings>();

        /// <summary>
        /// Switching variable for UDP reception start timing
        /// </summary>
        public bool IsReceivingOnEnable = true;
        #endregion --Fields--

        #region --Properties--
        /// <summary>
        /// UDP receivers grouped by port.
        /// One receiver per port; multiple avatars can subscribe to the same receiver.
        /// </summary>
        private Dictionary<int, MocopiUdpReceiver> UdpReceiversByPort { get; set; } =
            new Dictionary<int, MocopiUdpReceiver>();
        #endregion --Properties--

        #region --Methods--
        /// <summary>
        /// Perform the processing when activated
        /// </summary>
        private void OnEnable()
        {
            if (IsReceivingOnEnable)
            {
                this.UdpStart();
            }
        }

        /// <summary>
        /// OnDisable
        /// </summary>
        private void OnDisable()
        {
            if (IsReceivingOnEnable)
            {
                this.UdpStop();
            }
        }

        /// <summary>
        /// Destroy the receiver
        /// </summary>
        private void OnDestroy()
        {
            this.UdpStop();
        }

        /// <summary>
        /// Start receiving UDP
        /// </summary>
        private void UdpStart()
        {
            // Clean up any existing receivers / subscriptions first
            this.UdpStop();

            if (this.AvatarSettings == null || this.AvatarSettings.Count == 0)
            {
                return;
            }

            // Build receivers by port and subscribe avatars
            InitializeUdpReceiversAndDelegates();

            // Start all receivers
            foreach (var kv in this.UdpReceiversByPort)
            {
                kv.Value?.UdpStart();
            }
        }

        /// <summary>
        /// Stop receiving UDP
        /// </summary>
        private void UdpStop()
        {
            // Unsubscribe all delegates first
            this.UnsetUdpDelegates();

            // Stop all receivers
            foreach (var kv in this.UdpReceiversByPort)
            {
                kv.Value?.UdpStop();
            }

            // Clear dictionary
            this.UdpReceiversByPort.Clear();
        }

        /// <summary>
        /// Initialize the UDP receivers (one per port) and set delegates for all avatars.
        /// </summary>
        private void InitializeUdpReceiversAndDelegates()
        {
            if (this.AvatarSettings == null)
            {
                return;
            }

            // Create one UdpReceiver per unique port and subscribe all avatars on that port
            for (int i = 0; i < this.AvatarSettings.Count; i++)
            {
                var settings = this.AvatarSettings[i];
                if (settings == null || settings.MocopiAvatar == null)
                {
                    continue;
                }

                int port = settings.Port;

                // Get or create receiver for this port
                if (!this.UdpReceiversByPort.TryGetValue(port, out var receiver) || receiver == null)
                {
                    receiver = new MocopiUdpReceiver(port);
                    this.UdpReceiversByPort[port] = receiver;
                }

                // Subscribe avatar to this receiver
                receiver.OnReceiveSkeletonDefinition += settings.MocopiAvatar.InitializeSkeleton;
                receiver.OnReceiveFrameData += settings.MocopiAvatar.UpdateSkeleton;
            }
        }

        /// <summary>
        /// Unconfigure all UDP delegates for all avatars.
        /// </summary>
        private void UnsetUdpDelegates()
        {
            if (this.AvatarSettings == null || this.UdpReceiversByPort == null)
            {
                return;
            }

            for (int i = 0; i < this.AvatarSettings.Count; i++)
            {
                var settings = this.AvatarSettings[i];
                if (settings == null || settings.MocopiAvatar == null)
                {
                    continue;
                }

                if (this.UdpReceiversByPort.TryGetValue(settings.Port, out var receiver) && receiver != null)
                {
                    receiver.OnReceiveSkeletonDefinition -= settings.MocopiAvatar.InitializeSkeleton;
                    receiver.OnReceiveFrameData -= settings.MocopiAvatar.UpdateSkeleton;
                }
            }
        }

        /// <summary>
        /// Start receiving (manual).
        /// </summary>
        public void StartReceiving()
        {
            if (!IsReceivingOnEnable)
            {
                this.UdpStart();
            }
        }

        /// <summary>
        /// Stop receiving (manual).
        /// </summary>
        public void StopReceiving()
        {
            if (!IsReceivingOnEnable)
            {
                this.UdpStop();
            }
        }

        /// <summary>
        /// Add Avatar to AvatarSettings at runtime.
        /// </summary>
        /// <param name="mocopiAvatar">Avatar instance</param>
        /// <param name="port">Port number</param>
        public void AddAvatar(MocopiAvatar mocopiAvatar, int port)
        {
            var settings = new MocopiSimpleReceiverAvatarSettings(mocopiAvatar, port);
            AvatarSettings.Add(settings);

            // If we're already running and have a receiver for this port, hook it up immediately
            if (mocopiAvatar != null &&
                this.UdpReceiversByPort != null &&
                this.UdpReceiversByPort.TryGetValue(port, out var receiver) &&
                receiver != null)
            {
                receiver.OnReceiveSkeletonDefinition += mocopiAvatar.InitializeSkeleton;
                receiver.OnReceiveFrameData += mocopiAvatar.UpdateSkeleton;
            }
        }
        #endregion --Methods--

        #region --Classes--
        /// <summary>
        /// Hold a pair of an avatar and a port id
        /// </summary>
        [System.Serializable]
        public sealed class MocopiSimpleReceiverAvatarSettings
        {
            /// <summary>
            /// mocopi avatar
            /// </summary>
            public MocopiAvatar MocopiAvatar;

            /// <summary>
            /// Port number
            /// </summary>
            public int Port;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="mocopiAvatar">mocopi avatar</param>
            /// <param name="port">Port number</param>
            public MocopiSimpleReceiverAvatarSettings(MocopiAvatar mocopiAvatar, int port)
            {
                this.MocopiAvatar = mocopiAvatar;
                this.Port = port;
            }
        }
        #endregion --Classes--
    }
}
