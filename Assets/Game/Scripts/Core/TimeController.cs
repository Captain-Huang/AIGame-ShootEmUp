using UnityEngine;

namespace AIGame.ShootEmUp.Core
{
    public static class TimeController
    {
        public static void Pause()
        {
            Time.timeScale = 0f;
        }

        public static void Resume()
        {
            Time.timeScale = 1f;
        }
    }
}
