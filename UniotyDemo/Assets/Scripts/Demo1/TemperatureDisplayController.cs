﻿using UnityEngine;
using Unioty;

public class TemperatureDisplayController : MonoBehaviour
{
    UniotyMasterScript uniotyMaster;
    TextMesh textMesh;

    public byte DeviceID = 0x01;
    public byte ControlID_HDC1000 = 0x03;

    void Start()
    {
        uniotyMaster = FindObjectOfType<UniotyMasterScript>();
        uniotyMaster.GetDeviceControl(DeviceID, ControlID_HDC1000).DataReceived += OnHDCDataReceived;
        textMesh = GetComponent<TextMesh>();
    }

    void Update()
    {

    }

    void OnDestroy()
    {
        uniotyMaster.GetDeviceControl(DeviceID, ControlID_HDC1000).DataReceived -= OnHDCDataReceived;
    }


    void OnHDCDataReceived(object sender, DataReceivedEventArgs e)
    {
        float temperature = (float)e.Payload.Data;
        textMesh.text = string.Format("{0} C", temperature);
    }
}
