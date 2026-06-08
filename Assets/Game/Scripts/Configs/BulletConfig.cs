using UnityEngine;

namespace AIGame.ShootEmUp.Configs
{
    [CreateAssetMenu(fileName = "BulletConfig", menuName = "ShootEmUp/Configs/Bullet")]
    public class BulletConfig : ScriptableObject
    {
        [Header("Identity")]
        public string bulletId = "Bullet_Default";

        [Header("Visual")]
        public GameObject prefab;
        public Color tint = Color.white;
        public Vector2 size = new Vector2(0.16f, 0.36f);

        [Header("Stats")]
        public float speed = 10f;
        public int damage = 1;
        public float lifetime = 3f;
        public float radius = 0.1f;
        public int pierceCount = 0;
    }
}
