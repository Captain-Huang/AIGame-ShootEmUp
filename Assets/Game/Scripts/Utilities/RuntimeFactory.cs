using UnityEngine;

namespace AIGame.ShootEmUp.Utilities
{
    public static class RuntimeFactory
    {
        private static Sprite _squareSprite;

        public static GameObject CreateActor(string name, Color color, Vector2 size, int sortingOrder, string layerName = null)
        {
            var go = new GameObject(name);
            if (!string.IsNullOrWhiteSpace(layerName))
            {
                var layer = LayerMask.NameToLayer(layerName);
                if (layer >= 0)
                {
                    go.layer = layer;
                }
            }

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSquareSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            return go;
        }

        private static Sprite GetSquareSprite()
        {
            if (_squareSprite != null)
            {
                return _squareSprite;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            _squareSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f
            );
            _squareSprite.name = "RuntimeSquare";
            return _squareSprite;
        }
    }
}
