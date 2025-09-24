using System;
using System.Runtime.InteropServices;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Audio
{
    public class Emitter : MonoBehaviour
    {
        public EventInstance eventInstance { get; private set; }

        #region Fields

        //Property fields
        Event audioEvent;
        Transform followTarget;
        Handle handle;

        //Technical fields
        GCHandle fmodHandle;
        bool hasEnded = false;
        volatile int finalizeRequested = 0;

        static readonly FMOD.Studio.EVENT_CALLBACK stopCallback = OnEventCallback;

        #endregion

        public bool IsLooping => audioEvent != null && audioEvent.IsLooping;
        public bool IsPlaying()
        {
            if (eventInstance.isValid())
            {
                eventInstance.getPlaybackState(out var state);
                return state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING;
            }
            return false;
        }

        public void Initialize(Event audioEvent, Transform followTarget = null, Handle handle = null)
        {
            if (eventInstance.isValid())
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                eventInstance.release();
            }
            if (fmodHandle.IsAllocated) fmodHandle.Free();
            hasEnded = false;

            this.audioEvent = audioEvent;
            this.followTarget = followTarget;
            this.handle = handle;
            eventInstance = RuntimeManager.CreateInstance(audioEvent.eventReference);

            if (audioEvent.Is3D) RuntimeManager.AttachInstanceToGameObject(eventInstance, gameObject);
        }

        #region Playback
        public void Play()
        {
            fmodHandle = GCHandle.Alloc(this);
            eventInstance.setUserData(GCHandle.ToIntPtr(fmodHandle));
            eventInstance.setCallback(stopCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.STOPPED);
            eventInstance.start();
        }
        public void Stop()
        {
            if (eventInstance.isValid())
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
        #endregion

        #region Parameters
        public void SetParameter(string name, float value)
        {
            if (audioEvent == null) return;
            float? defaultValue = audioEvent.GetDefaultParamValue(name);
            if (defaultValue == null) return;
            eventInstance.setParameterByName(name, value);
        }
        public void SetDefaultParameters()
        {
            foreach (var param in audioEvent.DefaultParameters)
            {
                SetParameter(param.Name, param.Value);
            }
        }
        #endregion

        #region Runtime

        void LateUpdate()
        {
            if (followTarget != null) transform.position = followTarget.position;
            if (System.Threading.Interlocked.Exchange(ref finalizeRequested, 0) == 1)
            {
                eventInstance.setCallback(null, 0);
                eventInstance.setUserData(IntPtr.Zero);

                if (eventInstance.isValid())
                    eventInstance.release();

                if (fmodHandle.IsAllocated)
                    fmodHandle.Free();

                End();
            }
        }

        private void OnDisable()
        {
            if (eventInstance.isValid())
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                eventInstance.release();
            }
            if (fmodHandle.IsAllocated) fmodHandle.Free();
        }

        #endregion

        // check the cleanup plz
        #region Cleanup

        internal void End()
        {
            if (hasEnded) return;
            hasEnded = true;

            if (eventInstance.isValid())
            {
                RuntimeManager.DetachInstanceFromGameObject(eventInstance);
            }
            if (handle is ChainedHandle chainedHandle)
            {
                AudioManager.Instance.PlayNext(chainedHandle);
            }

            PoolManager.Instance.ReturnEmitter(audioEvent, this);
            AudioManager.Instance.UntrackEmitter(audioEvent, this);
        }

        //Check this later
        static FMOD.RESULT OnEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
        {
            if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.STOPPED)
            {
                EventInstance instance = new EventInstance(instancePtr);
                instance.getUserData(out IntPtr userData);

                GCHandle handle = default;

                try
                {
                    if (userData != IntPtr.Zero)
                    {
                        handle = GCHandle.FromIntPtr(userData);
                        if (handle.Target is Emitter emitter)
                        {
                            System.Threading.Interlocked.Exchange(ref emitter.finalizeRequested, 1);
                        }
                    }
                }
                finally
                {
                    if (handle.IsAllocated) handle.Free();
                }
            }
            return FMOD.RESULT.OK;
        }

        #endregion
    }
}
