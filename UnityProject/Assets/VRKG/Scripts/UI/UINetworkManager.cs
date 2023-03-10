using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/*
 * MIT License

Copyright (c) 2023 Alberto Accardo, Daniele Monaco, Maria Angela Pellegrino, Vittorio Scarano, Carmine Spagnuolo

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

 */

[Serializable]
public class UINetworkButton
{
    public GameObject Parent;
    public TextMeshProUGUI Text;
    public int Index;
}

/* Manages the UI which lets pick a query */
public class UINetworkManager : MonoBehaviour
{
    public Transform QueryButtonPrefab;
    public GameObject QueryButtonsParent;
    public Vector3 OffsetFromCamera;
    public TextMeshProUGUI NetworkStatusTextUI;
    public NetworkManager NetworkMan;
    public FileServerStorage Storage;
    public GraphicsProfileManager ProfilesManager;
    private List<QueryEntry> entries;
    private bool cacheRefreshStarted;

    private void Start()
    {
        Vector3 focusPoint = Camera.main.transform.position + Camera.main.transform.forward * OffsetFromCamera.z 
                                                            + Camera.main.transform.up * OffsetFromCamera.y 
                                                            + Camera.main.transform.right * OffsetFromCamera.x;
        transform.position = focusPoint;
    }
    
    public void OnButtonClick(int index)
    {
        NetworkMan.JoinQueryRoom(entries[index]);
        ProfilesManager.OnNewGraph(entries[index]);
    }
    
    void OnQueriesUpdatedCallback(List<QueryEntry> newEntries)
    {
        entries = newEntries;
        for (int i = 0; i < entries.Count; ++i)
        {
            GameObject newButtonUI = Instantiate(QueryButtonPrefab).gameObject;
            newButtonUI.transform.parent = QueryButtonsParent.transform;
            newButtonUI.transform.localScale = new Vector3(1f, 1f, 1f);
            newButtonUI.transform.localPosition = new Vector3(0f, 0f,0f);
            newButtonUI.transform.localEulerAngles = new Vector3(0f, 0f,0f);
            UINetworkQueryButtonController buttonCtrl = newButtonUI.GetComponent<UINetworkQueryButtonController>();
            buttonCtrl.DescriptionText.text = entries[i].Name + "\n" + entries[i].Preview;
            int x = i;
            buttonCtrl.Button.onClick.AddListener(() => OnButtonClick(x));
        }
        
    }

    /* handles Photon network connection, and queries retrieval, disappears on query loaded */
    private void Update()
    {
        NetworkStatusTextUI.text = PhotonNetwork.NetworkClientState + "\n" + Camera.main.transform.position;
        if (PhotonNetwork.InRoom)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            switch (PhotonNetwork.NetworkClientState)
            {
                case ClientState.PeerCreated:
                    NetworkMan.ConnectToServer();
                    cacheRefreshStarted = false;
                    break;
                case ClientState.ConnectedToMasterServer:
                    if (!cacheRefreshStarted)
                    {
                        cacheRefreshStarted = true;
                        StartCoroutine(Storage.GetCachedEntries(OnQueriesUpdatedCallback));
                    }
                    break;
            }
        }
    }
}
