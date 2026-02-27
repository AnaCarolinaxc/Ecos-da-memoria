using UnityEngine;

public class ImagePickerPC : MonoBehaviour
{
    public GalleryUI gallery;

    public void PickImage()
    {
        NativeGallery.GetImageFromGallery(
            (path) =>
            {
                Debug.Log("ANDROID: Callback chamado");

                if (path == null)
                {
                    Debug.Log("ANDROID: Usuário cancelou");
                    return;
                }

                Texture2D texture = NativeGallery.LoadImageAtPath(
                    path,
                    1024,
                    false
                );

                if (texture == null)
                {
                    Debug.Log("Falha ao carregar imagem");
                    return;
                }

                Debug.Log("ANDROID: Texture carregada");
                gallery.AddImage(texture);
            },
            "Selecione uma imagem"
        );
    }
}