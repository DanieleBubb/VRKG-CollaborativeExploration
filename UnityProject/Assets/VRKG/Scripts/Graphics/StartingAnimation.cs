using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities;
using Photon.Pun;
using TMPro;
using UnityEngine;

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

/* Manages the animation of skybox and graph right when it's created */
public class StartingAnimation : MonoBehaviour
{
    public GraphicsProfileManager ProfilesManager;
    public Material Skybox;
    public string DissolveVarName;
    public float MinValue;
    public float MaxValue;
    public float LerpTime;

    public ClippingPlane ClipPlane;
    public float MinYClipPlane;
    public float MaxYClipPlane;
    public float TitleAnimDuration;
    private List<TextMeshPro> NodeTitles;
    private Color TitleColorStart;
    private Color TitleColorEnd;
    private List<TextMeshPro> EdgeTitles;
    private Color EdgeTitleColorStart;
    private Color EdgeTitleColorEnd;

    public ParticleSystem Particle;

    private void Start()
    {
        NodeTitles = new List<TextMeshPro>();
        EdgeTitles = new List<TextMeshPro>();
        RenderSettings.skybox = Skybox;
        Skybox.SetFloat(DissolveVarName, MinValue);
    }

    public void OnGraphCreated()
    {
        Skybox = ProfilesManager.CurrentProfile.SkyboxMaterial;
        TitleColorStart = ProfilesManager.CurrentProfile.NodeTitleColorTransparent;
        TitleColorEnd = ProfilesManager.CurrentProfile.NodeTitleColorOpaque;
        EdgeTitleColorStart = ProfilesManager.CurrentProfile.EdgeColorTransparent;
        EdgeTitleColorEnd = ProfilesManager.CurrentProfile.EdgeColorOpaque;
        StartCoroutine(DissolveCoroutine());
    }

    public void OnJoinedRoom()
    {
        if(!PhotonNetwork.IsMasterClient)
            StartCoroutine(DissolveCoroutine());
    }

    public void OnNodeRegistered(GameObject node)
    {
        ClipPlane.AddRenderer(node.GetComponent<Renderer>());
        NodeTitles.Add(node.GetComponent<NodeContent>().Title);
    }
    
    public void OnEdgeRegistered(GameObject edge)
    {
        EdgeContent edgeCnt = edge.GetComponent<EdgeContent>();
        ClipPlane.AddRenderer(edgeCnt.MainRenderer);
        EdgeTitles.Add(edgeCnt.TitleUi);
    }

    IEnumerator TextColorFade(TextMeshPro text, Color start, Color end, float duration)
    {
        float startTime = Time.time;
        while (Time.time <= startTime + duration)
        {
            float lerpFactor = (Time.time - startTime) / duration;
            Color newColor = Color.Lerp(start, end, lerpFactor);
            text.color = newColor;
            yield return null;
        }
        text.outlineColor = ProfilesManager.CurrentProfile.NodeTitleOutlineColor;
    }

    IEnumerator DissolveCoroutine()
    {
        RenderSettings.skybox = Skybox;
        Skybox.SetFloat(DissolveVarName, MinValue);
        yield return null;
        Particle.Play();
        ParticleSystem.MainModule particleMain = Particle.main;
        particleMain.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, 0f));
        
        float startTime = Time.time;
        ClipPlane.transform.position = new Vector3(0f, MinYClipPlane, 0f);
        List<bool> titlesAnimStarted = new List<bool>(NodeTitles.Count);
        for (int i = 0; i < NodeTitles.Count; ++i)
        {
            NodeTitles[i].color = TitleColorStart;
        }
        
        List<bool> edgeAnimStarted = new List<bool>(EdgeTitles.Count);
        for (int i = 0; i < EdgeTitles.Count; ++i)
        {
            EdgeTitles[i].color = EdgeTitleColorStart;
        }
        while (Time.time <= startTime + LerpTime)
        {
            float lerpFactor = (Time.time - startTime) / LerpTime;
            float newVal = Mathf.Lerp(MinValue, MaxValue, lerpFactor);
            Skybox.SetFloat(DissolveVarName, newVal);
            
            particleMain.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, lerpFactor));

            float newClipY = Mathf.Lerp(MinYClipPlane, MaxYClipPlane, lerpFactor);
            ClipPlane.transform.position = new Vector3(0f, newClipY, 0f);

            for (int i = 0; i < NodeTitles.Count; ++i)
            {
                while (titlesAnimStarted.Count <= i)
                {
                    titlesAnimStarted.Add(false);
                }
                if (!titlesAnimStarted[i])
                {
                    if (NodeTitles[i].transform.position.y < newClipY)
                    {
                        StartCoroutine(TextColorFade(NodeTitles[i], TitleColorStart, TitleColorEnd, TitleAnimDuration));
                        NodeTitles[i].outlineColor = ProfilesManager.CurrentProfile.NodeTitleOutlineColor;
                        titlesAnimStarted[i] = true;
                    }
                }
            }
            
            for (int i = 0; i < EdgeTitles.Count; ++i)
            {
                while (edgeAnimStarted.Count <= i)
                {
                    edgeAnimStarted.Add(false);
                }
                if (!edgeAnimStarted[i])
                {
                    if (EdgeTitles[i].transform.position.y < newClipY)
                    {
                        StartCoroutine(TextColorFade(EdgeTitles[i], EdgeTitleColorStart, EdgeTitleColorEnd, TitleAnimDuration));
                        EdgeTitles[i].outlineColor = ProfilesManager.CurrentProfile.NodeTitleOutlineColor;
                        edgeAnimStarted[i] = true;
                    }
                }
            }
            yield return null;
        }
        ClipPlane.transform.position = new Vector3(0f, 5000, 0f);
        
    }
}
