using System;
namespace Utilities {
    public class TimerInstance {
        event Action OnTimeUp;
        event Action<float> OnTick;
        public float TimeLeft {get; private set;}
        public float Delay {get; private set;}
        public bool IsPaused {get; private set;}
        public TimerInstance (float duration, float delay = 0) {
            TimeLeft = duration;
            Delay = delay;
            Timer.timers.Add(this);
        }
        public void Tick(float time) {
            TimeLeft -= time;
            if (TimeLeft <= 0 && TimeLeft > -1) {
                OnTimeUp?.Invoke();
            } else if(OnTick != null && TimeLeft > 0) OnTick(TimeLeft);
        }
        public TimerInstance SetOnTimeUp (Action OnTimeUp) {
            this.OnTimeUp = OnTimeUp;
            return this;
        }
        public TimerInstance SetOnTick (Action<float> OnTick) {
            this.OnTick = OnTick;
            return this;
        }
        public void Cancel () => TimeLeft = -2;
        public void ForceComplete () {
            TimeLeft = -0.1f;
            OnTimeUp?.Invoke();
        }
        public void Pause() => IsPaused = true;
        public void Resume() => IsPaused = false;
        
    }
}
