using UnityEngine;

namespace AIGame.ShootEmUp.Utilities
{
    public readonly struct CameraWorldBounds
    {
        public readonly float MinX;
        public readonly float MaxX;
        public readonly float MinY;
        public readonly float MaxY;

        public CameraWorldBounds(float minX, float maxX, float minY, float maxY)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
        }
    }

    public static class CameraBounds
    {
        public static CameraWorldBounds GetWorldBounds(Camera camera, float padding = 0f)
        {
            if (camera == null)
            {
                return new CameraWorldBounds(-8f, 8f, -5f, 5f);
            }

            var halfHeight = camera.orthographicSize;
            var halfWidth = halfHeight * camera.aspect;
            var center = camera.transform.position;
            return new CameraWorldBounds(
                center.x - halfWidth + padding,
                center.x + halfWidth - padding,
                center.y - halfHeight + padding,
                center.y + halfHeight - padding
            );
        }
    }
}
