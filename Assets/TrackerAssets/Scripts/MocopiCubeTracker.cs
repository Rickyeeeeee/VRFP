using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

namespace Mocopi.Receiver
{

    /// <summary>
    /// Component that receives Mocopi tracker/bone data and applies transforms to GameObjects.
    /// Can also record those transforms to JSON and play them back.
    /// </summary>
    public class MocopiCubeTracker : MocopiAvatarBase
    {
        [System.Serializable]
        public class InitializationData
        {
            public int[] boneIDs;
            public int[] paraentIDs;
        }
        #region --Fields--

        [System.Serializable]
        public class TrackerMapping
        {
            /// <summary> Mocopi bone ID (0 = root/hips, 10 = head, etc.) </summary>
            public int BoneId;

            /// <summary> GameObject to apply the transform to </summary>
            public GameObject TrackerObject;

            /// <summary> Whether to apply position (otherwise only rotation) </summary>
            public bool ApplyPosition = true;

            /// <summary> Whether to apply rotation </summary>
            public bool ApplyRotation = true;

            /// <summary> Optional offset position relative to the tracked position </summary>
            public Vector3 PositionOffset = Vector3.zero;

            /// <summary> Optional offset rotation </summary>
            public Vector3 RotationOffset = Vector3.zero;
        }

        public MocopiAvatar mocopiAvatar;
        private bool isMocopiAvatarInitialized = false;

        public enum PositionMode
        {
            /// <summary> Use positions directly as world coordinates </summary>
            WorldSpace,
            /// <summary> Make positions relative to root bone (hip) </summary>
            RootRelative,
            /// <summary> Build world positions from bone hierarchy </summary>
            Hierarchical
        }

        public enum TrackerMode
        {
            /// <summary> Live mocopi receiving & live cubes </summary>
            Live,
            /// <summary> Playback from JSON, ignore live mocopi </summary>
            Playback
        }

        [Header("Mappings & Modes")]
        [Tooltip("Map Mocopi bone IDs to your GameObjects. Bone IDs: 0=Hips, 10=Head, 12=LeftUpperArm, 16=RightUpperArm, etc.")]
        public List<TrackerMapping> TrackerMappings = new List<TrackerMapping>();

        [Tooltip("WorldSpace = Use positions directly. RootRelative = Center around root. Hierarchical = Build world positions from bone hierarchy (use this for independent GameObjects like cubes).")]
        public PositionMode PositionModeSetting = PositionMode.Hierarchical;

        [Tooltip("Live = receive Mocopi over UDP and drive cubes. Playback = ignore live data and play from JSON recording.")]
        public TrackerMode Mode = TrackerMode.Live;

        // Skeleton / bone data
        private Dictionary<int, TrackerData> boneData = new Dictionary<int, TrackerData>();
        private Dictionary<int, int> boneParents = new Dictionary<int, int>();
        private Vector3 rootBoneOffset = Vector3.zero;
        private Vector3 currentRootPosition = Vector3.zero;
        private bool isInitialized = false;
        private bool hasNewData = false;
        private FrameUpdateData latestFrameData;

        // Mode tracking
        private TrackerMode lastMode;

        #region Recording fields

        [Header("Recording (Live Mode)")]
        [Tooltip("Check to start recording in Live mode. Uncheck to stop and save JSON.")]
        public bool IsRecording = false; // Inspector checkbox

        [Tooltip("File name for recorded JSON (saved under Application.persistentDataPath).")]
        public string RecordingFolder = "C://Users//ricky//School//Extended Reality//Data";
        public string RecordingFileName = "mocopi_recording.json";

        [Tooltip("Record only bones that have TrackerMappings. If false, record all bones.")]
        public bool RecordOnlyMappedBones = true;

        [Tooltip("Optional maximum recording duration in seconds. <=0 means unlimited.")]
        public float RecordingMaxDurationSeconds = 0f;

        // Internal recording state
        private bool isRecordingActive = false;  // actual active recording session
        private float recordingStartTime = 0f;
        private int recordedFrameCount = 0;
        private Recording currentRecording = null;

        #endregion

        [Header("Paths (Readonly)")]
        [Tooltip("Full path where recordings are saved / loaded. Drag-drop files below in the editor window.")]
        [SerializeField, HideInInspector]
        private string fullRecordingPath;


        #region Playback fields

        [Header("Playback")]
        [Tooltip("File name to load for playback (from Application.persistentDataPath).")]
        public string PlaybackFileName = "mocopi_recording.json";

        [Tooltip("Target playback frame rate (frames per second).")]
        public float PlaybackFrameRate = 60f;

        [Tooltip("Playback speed multiplier (1 = real time, 0.5 = half speed, 2 = double speed).")]
        public float PlaybackSpeed = 1f;

        [Tooltip("Loop playback when reaching the end of the recording.")]
        public bool LoopPlayback = true;

        private bool isPlayingBack = false;
        private Recording playbackRecording = null;
        private int playbackFrameIndex = 0;
        private float playbackTimer = 0f;

        #endregion

        #endregion --Fields--

        #region --Structures--

        /// <summary> Internal bone data used at runtime </summary>
        private struct TrackerData
        {
            public Vector3 Position;
            public Quaternion Rotation;
        }

        /// <summary> Raw frame data coming from the mocopi plugin </summary>
        private struct FrameUpdateData
        {
            public int FrameId;
            public float Timestamp;
            public double UnixTime;

            public int[] BoneIds;
            public float[] RotationsX;
            public float[] RotationsY;
            public float[] RotationsZ;
            public float[] RotationsW;
            public float[] PositionsX;
            public float[] PositionsY;
            public float[] PositionsZ;
        }

        // === Serializable recording format ===

        [System.Serializable]
        private class RecordedBone
        {
            public int BoneId;
            public Vector3 Position;     // world-space position
            public Quaternion Rotation;  // world-space rotation
        }

        [System.Serializable]
        private class RecordedFrame
        {
            public int FrameId;
            public float Timestamp;
            public double UnixTime;
            public List<RecordedBone> Bones = new List<RecordedBone>();

            [NonSerialized]
            public Dictionary<int, RecordedBone> BoneMap; // built at load time for quick lookup
        }

        [System.Serializable]
        private class Recording
        {
            public string Version = "1.0";
            public float ApproxFrameRate = 0f;
            public List<RecordedFrame> Frames = new List<RecordedFrame>();
        }

        #endregion --Structures--

        #region --Unity Lifecycle--

        private void Awake()
        {
            UpdateFullPaths();
            lastMode = Mode;
        }

        #endregion --Unity Lifecycle--

        #region --Mocopi callbacks--

        /// <summary>
        /// Initialize skeleton - called when skeleton definition is received (likely from a non-main thread).
        /// DO NOT use UnityEngine.Time here.
        /// </summary>
        public override void InitializeSkeleton(
            int[] boneIds, int[] parentBoneIds,
            float[] rotationsX, float[] rotationsY, float[] rotationsZ, float[] rotationsW,
            float[] positionsX, float[] positionsY, float[] positionsZ)
        {
            boneParents.Clear();
            for (int i = 0; i < boneIds.Length; i++)
            {
                boneParents[boneIds[i]] = parentBoneIds[i];
            }

            boneData.Clear();
            for (int i = 0; i < boneIds.Length; i++)
            {
                Vector3 localPos = ConvertPluginDataToVector3(positionsX[i], positionsY[i], positionsZ[i]);

                if (parentBoneIds[i] < 0)
                {
                    // Root bone
                    rootBoneOffset = localPos;
                    currentRootPosition = localPos;

                    boneData[boneIds[i]] = new TrackerData
                    {
                        Position = Vector3.zero,
                        Rotation = ConvertPluginDataToQuaternion(rotationsX[i], rotationsY[i], rotationsZ[i], rotationsW[i])
                    };
                }
                else
                {
                    boneData[boneIds[i]] = new TrackerData
                    {
                        Position = localPos,
                        Rotation = ConvertPluginDataToQuaternion(rotationsX[i], rotationsY[i], rotationsZ[i], rotationsW[i])
                    };
                }
            }

            ValidateMappings();

            if (!isInitialized)
            {
                InitializationData data = new InitializationData
                {
                    boneIDs = boneIds,
                    paraentIDs = parentBoneIds
                };
                string json = JsonUtility.ToJson(data, true);
                string path = Path.Combine(RecordingFolder, "mocopi_initialization.json");

                try
                {
                    File.WriteAllText(path, json);
                    Debug.Log($"[MocopiCubeTracker] Initialization saved to: {path}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[MocopiCubeTracker] Failed to save recording: {e}");
                }
                finally
                {
                    currentRecording = null;
                }

                Debug.Log($"[MocopiCubeTracker] Skeleton initialized with {boneIds.Length} bones. Active trackers: {TrackerMappings.Count}. Root offset: {rootBoneOffset}");
            }
            isInitialized = true;
        }

        /// <summary>
        /// Update skeleton - called whenever a new Mocopi frame arrives (likely from UDP thread).
        /// This function must not touch UnityEngine APIs.
        /// </summary>
        public override void UpdateSkeleton(
            int frameId, float timestamp, double unixTime,
            int[] boneIds,
            float[] rotationsX, float[] rotationsY, float[] rotationsZ, float[] rotationsW,
            float[] positionsX, float[] positionsY, float[] positionsZ)
        {
            if (!isInitialized)
                return;

            latestFrameData = new FrameUpdateData
            {
                FrameId = frameId,
                Timestamp = timestamp,
                UnixTime = unixTime,
                BoneIds = boneIds,
                RotationsX = rotationsX,
                RotationsY = rotationsY,
                RotationsZ = rotationsZ,
                RotationsW = rotationsW,
                PositionsX = positionsX,
                PositionsY = positionsY,
                PositionsZ = positionsZ
            };

            hasNewData = true;
        }

        #endregion --Mocopi callbacks--

        #region --Unity Update--

        private void Update()
        {
            // Handle mode changes (always main thread)
            if (Mode != lastMode)
            {
                HandleModeChanged(lastMode, Mode);
                lastMode = Mode;
            }

            // === Playback mode ===
            if (Mode == TrackerMode.Playback)
            {
                if (isPlayingBack && playbackRecording != null)
                {
                    UpdatePlayback();
                }
                return; // ignore live mocopi
            }

            // === Live mode ===

            // Sync recording checkbox -> recording session (main thread only)
            if (IsRecording && !isRecordingActive)
            {
                StartRecording();
            }
            else if (!IsRecording && isRecordingActive)
            {
                StopRecordingAndSave();
            }

            if (!isInitialized || !hasNewData)
                return;

            // ----- First pass: update all bone data from latest mocopi frame -----
            Dictionary<int, TrackerData> updatedBoneData = new Dictionary<int, TrackerData>();
            int rootBoneId = GetRootBoneId();

            for (int i = 0; i < latestFrameData.BoneIds.Length; i++)
            {
                int boneId = latestFrameData.BoneIds[i];

                Vector3 localPosition = ConvertPluginDataToVector3(
                    latestFrameData.PositionsX[i],
                    latestFrameData.PositionsY[i],
                    latestFrameData.PositionsZ[i]
                );
                Quaternion rotation = ConvertPluginDataToQuaternion(
                    latestFrameData.RotationsX[i],
                    latestFrameData.RotationsY[i],
                    latestFrameData.RotationsZ[i],
                    latestFrameData.RotationsW[i]
                );

                updatedBoneData[boneId] = new TrackerData
                {
                    Position = localPosition,
                    Rotation = rotation
                };

                if (boneId == rootBoneId)
                {
                    currentRootPosition = localPosition;
                }
            }

            // ----- Second pass: compute world positions from hierarchy -----
            Dictionary<int, Vector3> worldPositions = new Dictionary<int, Vector3>();

            foreach (var kvp in updatedBoneData)
            {
                int boneId = kvp.Key;
                Vector3 worldPos = ComputeWorldPosition(boneId, updatedBoneData);
                worldPositions[boneId] = worldPos;

                boneData[boneId] = new TrackerData
                {
                    Position = worldPos,
                    Rotation = kvp.Value.Rotation
                };
            }

            // ----- Recording: write this frame if active -----
            if (isRecordingActive)
            {
                if (RecordingMaxDurationSeconds > 0f &&
                    Time.time - recordingStartTime > RecordingMaxDurationSeconds)
                {
                    Debug.Log("[MocopiCubeTracker] Max recording duration reached, stopping and saving recording.");
                    StopRecordingAndSave();
                    IsRecording = false; // update checkbox in Inspector
                }
                else
                {
                    RecordFrame(worldPositions, updatedBoneData);
                }
            }

            // ----- Third pass: apply transforms to mapped cubes -----
            foreach (var mapping in TrackerMappings)
            {
                if (mapping.TrackerObject == null)
                    continue;

                if (worldPositions.TryGetValue(mapping.BoneId, out Vector3 worldPos) &&
                    updatedBoneData.TryGetValue(mapping.BoneId, out TrackerData data))
                {
                    ApplyTransform(mapping, worldPos, data.Rotation);
                }
            }

            hasNewData = false;
        }

        #endregion --Unity Update--

        #region --Core helpers--

        private void HandleModeChanged(TrackerMode oldMode, TrackerMode newMode)
        {
            if (newMode == TrackerMode.Playback)
            {
                // Stop any ongoing recording when entering playback mode
                if (isRecordingActive)
                {
                    StopRecordingAndSave();
                    IsRecording = false;
                }

                LoadRecordingForPlayback();
            }
            else if (newMode == TrackerMode.Live)
            {
                // Stop playback when returning to live mode
                isPlayingBack = false;
            }
        }

        private Vector3 ComputeWorldPosition(int boneId, Dictionary<int, TrackerData> boneDataDict)
        {
            if (!boneDataDict.ContainsKey(boneId))
                return Vector3.zero;

            Vector3 rawPosition = boneDataDict[boneId].Position;

            switch (PositionModeSetting)
            {
                case PositionMode.WorldSpace:
                    return rawPosition;

                case PositionMode.RootRelative:
                    return rawPosition - (currentRootPosition - rootBoneOffset);

                case PositionMode.Hierarchical:
                    return ComputeHierarchicalPosition(boneId, boneDataDict);

                default:
                    return rawPosition;
            }
        }

        private int GetRootBoneId()
        {
            foreach (var kvp in boneParents)
            {
                if (kvp.Value < 0)
                    return kvp.Key;
            }
            return -1;
        }

        private Vector3 ComputeHierarchicalPosition(int boneId, Dictionary<int, TrackerData> boneDataDict)
        {
            Vector3 worldPos = boneDataDict[boneId].Position;
            int currentBoneId = boneId;

            while (boneParents.ContainsKey(currentBoneId))
            {
                int parentId = boneParents[currentBoneId];

                if (parentId < 0)
                {
                    Vector3 rootPos = boneDataDict.ContainsKey(currentBoneId)
                        ? boneDataDict[currentBoneId].Position
                        : Vector3.zero;
                    return worldPos + rootPos - rootBoneOffset;
                }

                if (!boneDataDict.ContainsKey(parentId))
                    break;

                Quaternion parentRotation = boneDataDict[parentId].Rotation;
                Vector3 parentPos = boneDataDict[parentId].Position;
                worldPos = parentRotation * worldPos + parentPos;
                currentBoneId = parentId;
            }

            return worldPos;
        }

        private void ApplyTransform(TrackerMapping mapping, Vector3 position, Quaternion rotation)
        {
            if (mapping.TrackerObject == null)
                return;

            if (mapping.RotationOffset != Vector3.zero)
            {
                rotation = rotation * Quaternion.Euler(mapping.RotationOffset);
            }

            if (mapping.ApplyPosition)
            {
                mapping.TrackerObject.transform.position = position + mapping.PositionOffset;
            }

            if (mapping.ApplyRotation)
            {
                mapping.TrackerObject.transform.rotation = rotation;
            }
        }

        private void ValidateMappings()
        {
            for (int i = TrackerMappings.Count - 1; i >= 0; i--)
            {
                if (TrackerMappings[i].TrackerObject == null)
                {
                    Debug.LogWarning($"[MocopiCubeTracker] Removing mapping for bone ID {TrackerMappings[i].BoneId} - GameObject is null");
                    TrackerMappings.RemoveAt(i);
                }
            }
        }

        private Vector3 ConvertPluginDataToVector3(double x, double y, double z)
        {
            return new Vector3(
                -(float)x,
                (float)y,
                (float)z
            );
        }

        private Quaternion ConvertPluginDataToQuaternion(double x, double y, double z, double w)
        {
            return new Quaternion(
                -(float)x,
                (float)y,
                (float)z,
                -(float)w
            );
        }

        public void AddTrackerMapping(int boneId, GameObject trackerObject, bool applyPosition = true, bool applyRotation = true)
        {
            TrackerMappings.Add(new TrackerMapping
            {
                BoneId = boneId,
                TrackerObject = trackerObject,
                ApplyPosition = applyPosition,
                ApplyRotation = applyRotation
            });
        }

        public GameObject GetTrackerForBone(int boneId)
        {
            foreach (var mapping in TrackerMappings)
            {
                if (mapping.BoneId == boneId)
                    return mapping.TrackerObject;
            }
            return null;
        }

        #endregion --Core helpers--

        #region --Recording methods--

        private void UpdateFullPaths()
        {
            fullRecordingPath = Path.Combine(RecordingFolder, RecordingFileName);
        }

        /// <summary> Start recording (main thread only). </summary>
        public void StartRecording()
        {
            isRecordingActive = true;
            recordingStartTime = Time.time;
            recordedFrameCount = 0;
            currentRecording = new Recording();
            Debug.Log($"[MocopiCubeTracker] Recording started. Output file: {RecordingFileName}");
        }

        /// <summary> Stop recording and save JSON to persistentDataPath. </summary>
        public void StopRecordingAndSave()
        {
            if (!isRecordingActive)
                return;

            isRecordingActive = false;

            if (currentRecording == null || currentRecording.Frames == null || currentRecording.Frames.Count == 0)
            {
                Debug.LogWarning("[MocopiCubeTracker] Recording stopped but no frames were recorded.");
                currentRecording = null;
                return;
            }

            string json = JsonUtility.ToJson(currentRecording, true);
            string path = Path.Combine(RecordingFolder, RecordingFileName);

            try
            {
                File.WriteAllText(path, json);
                Debug.Log($"[MocopiCubeTracker] Recording saved to: {path} (frames: {currentRecording.Frames.Count})");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MocopiCubeTracker] Failed to save recording: {e}");
            }
            finally
            {
                currentRecording = null;
            }
        }

        /// <summary> Cancel recording and discard data. </summary>
        public void CancelRecording()
        {
            isRecordingActive = false;
            currentRecording = null;
            Debug.Log("[MocopiCubeTracker] Recording cancelled (data discarded).");
        }

        private void RecordFrame(Dictionary<int, Vector3> worldPositions, Dictionary<int, TrackerData> updatedBoneData)
        {
            if (currentRecording == null)
                return;

            var frame = new RecordedFrame
            {
                FrameId = latestFrameData.FrameId,
                Timestamp = latestFrameData.Timestamp,
                UnixTime = latestFrameData.UnixTime
            };

            if (RecordOnlyMappedBones)
            {
                foreach (var mapping in TrackerMappings)
                {
                    if (worldPositions.TryGetValue(mapping.BoneId, out Vector3 pos) &&
                        updatedBoneData.TryGetValue(mapping.BoneId, out TrackerData data))
                    {
                        frame.Bones.Add(new RecordedBone
                        {
                            BoneId = mapping.BoneId,
                            Position = pos,
                            Rotation = data.Rotation
                        });
                    }
                }
            }
            else
            {
                foreach (var kvp in worldPositions)
                {
                    int boneId = kvp.Key;
                    if (!updatedBoneData.TryGetValue(boneId, out TrackerData data))
                        continue;

                    frame.Bones.Add(new RecordedBone
                    {
                        BoneId = boneId,
                        Position = kvp.Value,
                        Rotation = data.Rotation
                    });
                }
            }

            currentRecording.Frames.Add(frame);
            recordedFrameCount++;

            float elapsed = Mathf.Max(Time.time - recordingStartTime, 0.0001f);
            currentRecording.ApproxFrameRate = recordedFrameCount / elapsed;
        }

        #endregion --Recording methods--

        #region --Playback methods--

        /// <summary>
        /// Load recording from PlaybackFileName (under persistentDataPath) and prepare for playback.
        /// Called automatically when switching Mode to Playback.
        /// </summary>
        private void LoadRecordingForPlayback()
        {
            string path = Path.Combine(RecordingFolder, PlaybackFileName);

            if (!File.Exists(path))
            {
                Debug.LogError($"[MocopiCubeTracker] Playback file not found: {path}");
                isPlayingBack = false;
                playbackRecording = null;
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                playbackRecording = JsonUtility.FromJson<Recording>(json);

                if (playbackRecording == null || playbackRecording.Frames == null || playbackRecording.Frames.Count == 0)
                {
                    Debug.LogError("[MocopiCubeTracker] Failed to load recording or recording is empty.");
                    isPlayingBack = false;
                    playbackRecording = null;
                    return;
                }

                BuildPlaybackBoneMaps(playbackRecording);

                playbackFrameIndex = 0;
                playbackTimer = 0f;
                isPlayingBack = true;

                Debug.Log($"[MocopiCubeTracker] Loaded recording for playback: {path} (frames: {playbackRecording.Frames.Count})");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MocopiCubeTracker] Error loading recording: {e}");
                isPlayingBack = false;
                playbackRecording = null;
            }
        }

        private void BuildPlaybackBoneMaps(Recording recording)
        {
            if (recording == null || recording.Frames == null)
                return;

            foreach (var frame in recording.Frames)
            {
                if (frame.Bones == null)
                    continue;

                frame.BoneMap = new Dictionary<int, RecordedBone>(frame.Bones.Count);
                foreach (var bone in frame.Bones)
                {
                    frame.BoneMap[bone.BoneId] = bone;
                }
            }
        }

        private void UpdatePlayback()
        {
            if (playbackRecording == null || playbackRecording.Frames == null || playbackRecording.Frames.Count == 0)
                return;

            float frameDuration = (PlaybackFrameRate > 0f) ? 1f / PlaybackFrameRate : Time.deltaTime;
            playbackTimer += Time.deltaTime * PlaybackSpeed;

            while (playbackTimer >= frameDuration)
            {
                playbackTimer -= frameDuration;

                ApplyPlaybackFrame();

                playbackFrameIndex++;
                if (playbackFrameIndex >= playbackRecording.Frames.Count)
                {
                    if (LoopPlayback)
                    {
                        playbackFrameIndex = 0;
                    }
                    else
                    {
                        isPlayingBack = false;
                        break;
                    }
                }
            }
        }

        private void ApplyPlaybackFrame()
        {
            if (playbackRecording == null ||
                playbackRecording.Frames == null ||
                playbackRecording.Frames.Count == 0 ||
                playbackFrameIndex < 0 ||
                playbackFrameIndex >= playbackRecording.Frames.Count)
            {
                return;
            }

            var frame = playbackRecording.Frames[playbackFrameIndex];


            if (mocopiAvatar)
            {
                int[] boneIDs = new int[27];
                float[] rotationsX = new float[27];
                float[] rotationsY = new float[27];
                float[] rotationsZ = new float[27];
                float[] rotationsW = new float[27];
                float[] positionsX = new float[27];
                float[] positionsY = new float[27];
                float[] positionsZ = new float[27];
                if (!isMocopiAvatarInitialized)
                {
                    string path = Path.Combine(RecordingFolder, "mocopi_initialization.json");
                    if (!File.Exists(path))
                    {
                        Debug.LogWarning("No save file found!");
                    }

                    string json = File.ReadAllText(path);
                    InitializationData initData = JsonUtility.FromJson<InitializationData>(json);
                    for (int i = 0; i < 27; i++)
                    {
                        rotationsX[i] = 0;
                        rotationsY[i] = 0;
                        rotationsZ[i] = 0;
                        rotationsW[i] = 0;
                        positionsX[i] = 0;
                        positionsY[i] = 0;
                        positionsZ[i] = 0;
                    }
                    mocopiAvatar.InitializeSkeleton(
                        boneIds: initData.boneIDs,
                        parentBoneIds: initData.paraentIDs,
                        rotationsX: rotationsX,
                        rotationsY: rotationsY,
                        rotationsZ: rotationsZ,
                        rotationsW: rotationsW,
                        positionsX: positionsX,
                        positionsY: positionsY,
                        positionsZ: positionsZ
                    );
                    isMocopiAvatarInitialized = true;
                }
                for (int i = 0; i < 27; i++)
                {
                    boneIDs[i] = frame.Bones[i].BoneId;
                    rotationsX[i] = frame.Bones[i].Rotation.x;
                    rotationsY[i] = frame.Bones[i].Rotation.y;
                    rotationsZ[i] = frame.Bones[i].Rotation.z;
                    rotationsW[i] = frame.Bones[i].Rotation.w;
                    positionsX[i] = frame.Bones[i].Position.x;
                    positionsY[i] = frame.Bones[i].Position.y;
                    positionsZ[i] = frame.Bones[i].Position.z;
                }
                mocopiAvatar.UpdateSkeleton(
                    frameId: frame.FrameId,
                    timestamp: frame.Timestamp,
                    unixTime: frame.UnixTime,
                    boneIds: boneIDs,
                    rotationsX: rotationsX,
                    rotationsY: rotationsY,
                    rotationsZ: rotationsZ,
                    rotationsW: rotationsW,
                    positionsX: positionsX,
                    positionsY: positionsY,
                    positionsZ: positionsZ
                );
            }


            foreach (var mapping in TrackerMappings)
            {
                if (mapping.TrackerObject == null)
                    continue;

                RecordedBone bone = null;

                if (frame.BoneMap != null)
                {
                    frame.BoneMap.TryGetValue(mapping.BoneId, out bone);
                }
                else if (frame.Bones != null)
                {
                    for (int i = 0; i < frame.Bones.Count; i++)
                    {
                        if (frame.Bones[i].BoneId == mapping.BoneId)
                        {
                            bone = frame.Bones[i];
                            break;
                        }
                    }
                }

                if (bone != null)
                {
                    ApplyTransform(mapping, bone.Position, bone.Rotation);
                }
            }
        }

        /// <summary> Stop playback (does not change Mode). </summary>
        public void StopPlayback()
        {
            isPlayingBack = false;
        }

        #endregion --Playback methods--

        private void OnValidate()
        {
            UpdateFullPaths();
        }

        public string GetFullRecordingPath()
        {
            return fullRecordingPath;
        }


    }
}
