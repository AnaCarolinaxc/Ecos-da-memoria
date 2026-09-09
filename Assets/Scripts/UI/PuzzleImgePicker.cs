using UnityEngine;
using UnityEngine.UI;

public class PuzzleImagePicker : MonoBehaviour
{
    [Header("UI")]
    public Image previewImage; // caixa de "Pré-visualização"

    public void PickImage()
    {
        NativeGallery.GetImageFromGallery(
            (path) =>
            {
                if (path == null)
                {
                    return;
                }

                Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024, false);
                if (texture == null)
                {
                    Debug.Log("Falha ao carregar imagem");
                    return;
                }

                var sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                PuzzleManager.I.SetImage(sprite);

                if (previewImage != null)
                {
                    previewImage.sprite = sprite;
                    previewImage.preserveAspect = true;
                    previewImage.color = Color.white; // caso o placeholder use alpha 0
                }
            },
            "Selecionar imagem"
        );
    }
}