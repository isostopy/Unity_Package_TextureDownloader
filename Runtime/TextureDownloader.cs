using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class TextureDownloader : MonoBehaviour
{
    public string serverUrl;
    private string localPath;
    private List<TextureData> serverTextures;
    private List<string> localTextures;
    public Button updateButton;
    public Button deleteButton; 
    public Button clearButton; 
    public TextMeshProUGUI logText;
    private TextureSelector textureSelector;

    [System.Serializable]
    public class TextureData
    {
        public string name;
        public string url;
    }

    [System.Serializable]
    private class TextureList
    {
        public bool success;
        public List<TextureData> images;
    }

    void Start()
    {

        textureSelector = FindFirstObjectByType<TextureSelector>();

        localPath = Path.Combine(Application.persistentDataPath, "Textures");
        if (!Directory.Exists(localPath))
            Directory.CreateDirectory(localPath);

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Log("No internet connection available.");
            return;
        }

        updateButton.onClick.AddListener(DownloadTextures);
        deleteButton.onClick.AddListener(DeleteTextures);
        clearButton.onClick.AddListener(ClearLog);

        StartCoroutine(CheckForUpdates());
    }

    IEnumerator CheckForUpdates()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(serverUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Log("Error fetching texture list: " + request.error);
                yield break;
            }

            TextureList textureList = JsonUtility.FromJson<TextureList>(request.downloadHandler.text);
            serverTextures = textureList.images;
            localTextures = new List<string>(Directory.GetFiles(localPath));

            if (NeedsUpdate())
            {
                Log("New textures found. Starting download...");
                StartCoroutine(DownloadAllTextures());
            }
            else
            {
                Log("All textures are up to date.");
            }
        }
    }

    bool NeedsUpdate()
    {
        foreach (var texture in serverTextures)
        {
            string localFile = Path.Combine(localPath, texture.name);
            if (!localTextures.Contains(localFile))
            {
                return true;
            }
        }
        return false;
    }

    public void DownloadTextures()
    {
        StartCoroutine(DownloadAllTextures());
    }

    IEnumerator DownloadAllTextures()
    {
        foreach (var texture in serverTextures)
        {
            string filePath = Path.Combine(localPath, texture.name);

            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(texture.url))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Log("Failed to download " + texture.name + ": " + request.error);
                    continue;
                }

                Texture2D tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
                File.WriteAllBytes(filePath, tex.EncodeToPNG());
                Log("Downloaded: " + texture.name);
            }
        }

        textureSelector.LoadTextureList();

        Log("Textures updated!");
    }

    public void DeleteTextures()
    {
        if (Directory.Exists(localPath))
        {
            string[] files = Directory.GetFiles(localPath);
            foreach (string file in files)
            {
                File.Delete(file);
            }

            textureSelector.LoadTextureList();

            Log("All textures deleted.");
        }
        else
        {
            Log("No textures found to delete.");
        }

    }

    void Log(string message)
    {
        if (logText != null)
        {
            logText.text += message + "\n";
        }
        Debug.Log(message);
    }

    void ClearLog()
    {
        logText.text = "";
    }
}
