﻿using System.Collections;
using System.Collections.Generic;

namespace Unioty
{
    /// <summary>
    /// Wrapper of UniotyServer that uses thread-safe event-based callbacks 
    /// </summary>
    public class UniotyServerUpdater : IUniotyServer, IUpdate
    {
        const int MAX_EVENTS_PER_UPDATE = 20;

        UniotyServer server;
        Dictionary<int, DeviceControl> controlMap = new Dictionary<int, DeviceControl>();
        Queue deviceDataQueue = Queue.Synchronized(new Queue()); // Queue used to read data from device

        public int Port
        {
            get { return server.Port; }
        }

        public UniotyServerUpdater(int port, WriteLogDelegate writeLogFunc)
        {
            server = new UniotyServer(port, writeLogFunc, OnDataReceived);
        }

        public void Start()
        {
            server.Start();
        }

        public void Stop()
        {
            server.Stop();
        }

        public void Update()
        {
            for (int i = 0; i < MAX_EVENTS_PER_UPDATE; i++)
            {
                if (deviceDataQueue.Count <= 0) break;
                var e = deviceDataQueue.Dequeue();
                if (e != null)
                {
                    // Raise the received event for this control
                    var args = (DataReceivedEventArgs)e;
                    if (controlMap.ContainsKey(args.ControlCode))
                    {
                        controlMap[args.ControlCode].RaiseDataReceivedEvent(args);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the device control in the control map. If it's not in the map already, it'll be created.
        /// </summary>
        /// <param name="devID"></param>
        /// <param name="ctrlID"></param>
        /// <returns></returns>
        public DeviceControl GetDeviceControl(byte devID, byte ctrlID)
        {
            // Check if control is already in the hash map
            var controlCode = DeviceControl.GetControlCode(devID, ctrlID);
            if (!controlMap.ContainsKey(controlCode))
            {
                controlMap.Add(controlCode, new DeviceControl(controlCode));
            }
            return controlMap[controlCode];
        }

        void OnDataReceived(byte devID, byte ctrlID, Payload payload)
        {
            var args = new DataReceivedEventArgs(devID, ctrlID, payload);
            deviceDataQueue.Enqueue(args);
        }
    }
}
