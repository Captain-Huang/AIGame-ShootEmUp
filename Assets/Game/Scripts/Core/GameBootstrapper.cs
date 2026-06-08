using UnityEngine;

namespace AIGame.ShootEmUp.Core
{
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (Object.FindObjectOfType<GameManager>() != null)
            {
                return;
            }

            var managers = new GameObject("GameManagers");
            managers.AddComponent<GameManager>();
        }
    }
}
