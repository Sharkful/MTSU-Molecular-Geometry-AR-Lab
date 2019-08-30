using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetText());
    }

   IEnumerator GetText()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://myrandaGoesToSpace.github.io");
        yield return www.SendWebRequest();

        if(www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            byte[] results = www.downloadHandler.data;
        }
    }
}
