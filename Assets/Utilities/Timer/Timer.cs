using System.Collections.Generic;
using UnityEngine;
namespace Utilities {
    public class Timer : Singleton<Timer> {
        public static List<TimerInstance> timers =  new List<TimerInstance>();
        void Update() {
            RemoveCompletedTimers(ref timers);
            foreach (var timer in timers) {
                if (!timer.IsPaused) timer.Tick(Time.deltaTime);
            }
        }
        /// <summary>Creates a countdown Timer. Triggers callback when timer is up.</summary>
        ///<param name = "duration"> Duration in seconds of the timer.</param>
        ///<param name = "delay"> Delay in seconds of the timer (default 0).</param>
        public static TimerInstance Countdown (float duration, float delay = 0) => new TimerInstance(duration, delay);
        public static void CancelAllTimers() {
            foreach (var timer in timers) {
                timer.Cancel();
            };
        }
        public static void ForceCompleteAllTimers() {
            foreach (var timer in timers) {
                timer.ForceComplete();
            };
        }
        void RemoveCompletedTimers (ref List<TimerInstance> timerList) {
            if (timerList == null || timerList.Count == 0) return;
            var tempList = new List<TimerInstance>();
            foreach (var timer in timers) {
                if (timer.TimeLeft > 0) tempList.Add(timer);
            }
            timerList = tempList;
        }
        public static bool IsRunning(TimerInstance timer) => timers.Contains(timer);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void StartUp() => Instance.Initialize();
        void Initialize(){}
    }
}
