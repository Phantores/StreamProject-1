using System;

namespace Audio
{
    public class Handle
    {
        Emitter emitter;
        System.Action<Handle> onComplete;

        internal Handle(Emitter emitter)
        {
            this.emitter = emitter;
        }

        #region EmitterControl

        public void Stop()
        {
            if (emitter != null && emitter.IsPlaying())
            {
                emitter.Stop();
                emitter = null;
                onComplete?.Invoke(this);
            }
        }
        public void ResetParameters()
        {
            if(emitter) emitter.SetDefaultParameters();
        }
        public void SetParameter(string name, float value)
        {
            if(emitter) emitter.SetParameter(name, value);
        }

        #endregion

        #region HandleControl

        public void ChangeEmitter(Emitter emitter)
        {
            this.emitter = emitter;
        }
        public void SetOnDone(Action<Handle> callback)
        {
            onComplete = callback;
        }

        #endregion
    }
}
