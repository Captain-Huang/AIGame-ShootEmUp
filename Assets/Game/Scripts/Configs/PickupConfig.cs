using AIGame.ShootEmUp.Core;
using UnityEngine;

namespace AIGame.ShootEmUp.Configs
{
    [CreateAssetMenu(fileName = "PickupConfig", menuName = "ShootEmUp/Configs/Pickup")]
    public class PickupConfig : ScriptableObject
    {
        public string pickupId = "Pickup_Default";
        public PickupType type = PickupType.Score;
        public GameObject prefab;
        public int value = 1;
        public float duration = 10f;
        public float moveSpeed = 1.5f;
        public Sprite icon;
    }

    [System.Serializable]
    public struct PickupDropEntry
    {
        public PickupConfig pickup;
        [Range(0f, 1f)] public float dropChance;
    }
}
