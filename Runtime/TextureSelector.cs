using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class TextureSelector : MonoBehaviour
{
    private string textureFolder;
    public Renderer targetRenderer; // Asigna el objeto que recibirá la textura
    public TMP_Dropdown textureDropdown; // Dropdown para elegir texturas
    private List<string> textureFiles = new List<string>();

    void Start()
    {
        textureFolder = Path.Combine(Application.persistentDataPath, "Textures");
    }

    public void LoadTextureList()
    {
        if (!Directory.Exists(textureFolder))
        {
            Debug.LogError("Texture folder not found!");
            return;
        }

        textureFiles.Clear();
        textureFiles.AddRange(Directory.GetFiles(textureFolder, "*.png"));
        textureFiles.AddRange(Directory.GetFiles(textureFolder, "*.jpg"));

        textureDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (var file in textureFiles)
        {
            options.Add(Path.GetFileName(file));
        }
        textureDropdown.AddOptions(options);

        textureDropdown.onValueChanged.AddListener(delegate { ApplySelectedTexture(); });
    }

    void ApplySelectedTexture()
    {
        if (textureFiles.Count == 0 || targetRenderer == null)
            return;

        string selectedTexturePath = textureFiles[textureDropdown.value];
        StartCoroutine(LoadTexture(selectedTexturePath));
    }

    IEnumerator LoadTexture(string path)
    {
        byte[] textureBytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(textureBytes);
        targetRenderer.material.mainTexture = texture;
        yield return null;
    }
}
