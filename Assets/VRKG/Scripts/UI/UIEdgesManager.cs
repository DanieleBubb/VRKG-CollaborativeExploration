using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

[Serializable]
public class UIEdgeButton
{
    public GameObject Parent;
    public TextMeshProUGUI Text;
}

/* Manages the UI on the left, when a node is on focus */
public class UIEdgesManager : MonoBehaviour
{
    public Vector3 OffsetFromCamera;
    public FocusHandler FocusHndlr;
    private List<UIEdgeButton> buttons;
    private List<EdgeManager> edges;
    private List<EdgeManager> selectableEdges;

    private void Awake()
    {
        edges = new List<EdgeManager>();
        buttons = new List<UIEdgeButton>();
    }
    
    IEnumerator Start()
    {
        yield return null;
        gameObject.SetActive(false);
    }

    public void RegisterEdge(EdgeManager edge)
    {
        edges.Add(edge);
    }
    
    public void UnregisterEdge(EdgeManager edge)
    {
        edges.Remove(edge);
    }
    
    public void RegisterButton(GameObject obj, TextMeshProUGUI txt, int index)
    {
        while (index >= buttons.Count)
        {
            buttons.Add(null);
        }
        buttons[index] = new UIEdgeButton{Parent = obj, Text = txt};
    }

    public void OnJoinedRoom()
    {
        Vector3 focusPoint = Camera.main.transform.position + Camera.main.transform.forward * OffsetFromCamera.z 
                                                            + Camera.main.transform.right * OffsetFromCamera.x;
        transform.position = focusPoint;
        transform.LookAt(Camera.main.transform.position);
        transform.Rotate(0f, 180f, 0f);
    }

    public void OnNodeSelected(GameObject node)
    {
        gameObject.SetActive(true);
        selectableEdges = edges.Where(e => e.Node1 == node || e.Node2 == node).ToList();
        buttons.ForEach(b => b.Parent.SetActive(false));
        for(int i = 0; i < selectableEdges.Count; ++i)
        {
            buttons[i].Parent.SetActive(true);
            buttons[i].Text.text = selectableEdges[i].Title;
        }
    }

    public void OnNodeUnselected()
    {
        gameObject.SetActive(false);
    }

    public void OnEdgePressed(GameObject button)
    {
        UIEdgeButton uiButton = buttons.FirstOrDefault(b => b.Parent == button);
        if (uiButton != null)
        {
            int buttonIndex = buttons.IndexOf(uiButton);
            EdgeManager pressedEdge = selectableEdges[buttonIndex];
            FocusHndlr.OnEdgePressed(pressedEdge);
        }
    }
}