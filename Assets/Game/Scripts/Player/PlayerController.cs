using AIGame.ShootEmUp.Core;
using AIGame.ShootEmUp.Utilities;
using UnityEngine;

namespace AIGame.ShootEmUp.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private Vector2 edgePadding = new Vector2(0.45f, 0.45f);

        public void Configure(float speed)
        {
            moveSpeed = Mathf.Max(0.5f, speed);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            var nextPosition = transform.position + (Vector3)(input * (moveSpeed * Time.deltaTime));
            var bounds = CameraBounds.GetWorldBounds(Camera.main);
            nextPosition.x = Mathf.Clamp(nextPosition.x, bounds.MinX + edgePadding.x, bounds.MaxX - edgePadding.x);
            nextPosition.y = Mathf.Clamp(nextPosition.y, bounds.MinY + edgePadding.y, bounds.MaxY - edgePadding.y);
            transform.position = nextPosition;
        }
    }
}
