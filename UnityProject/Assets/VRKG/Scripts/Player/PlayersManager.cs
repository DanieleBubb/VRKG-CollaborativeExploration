using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using Photon.Pun;
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

[Serializable]
public class StartingPosRot
{
    public Vector3 Position;
    public int ActorNumber;
}

/* Handles users' spawning position */
public class PlayersManager : MonoBehaviour
{
    public FocusHandler FocusHndlr;
    public Transform AvatarPrefab;
    public List<StartingPosRot> StartingPosRots;
    public StartingPosRot BackupPosRot;

    public void OnJoinedRoom()
    {
        StartingPosRot playerStart =
            StartingPosRots.FirstOrDefault(s => s.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);
        if (playerStart == null)
        {
            Debug.LogWarning("Unable to find starting position");
            playerStart = BackupPosRot;
        }

        MixedRealityPlayspace.Transform.position = playerStart.Position;
        MixedRealityPlayspace.Transform.LookAt(FocusHndlr.FocusPoint);
        PhotonNetwork.Instantiate(AvatarPrefab.name, playerStart.Position, Quaternion.identity);
    }
}
