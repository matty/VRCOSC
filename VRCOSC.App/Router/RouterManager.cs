﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using VRCOSC.App.OSC.Client;
using VRCOSC.App.Profiles;
using VRCOSC.App.Router.Serialisation;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Router;

public class RouterManager
{
    private static RouterManager? instance;
    public static RouterManager GetInstance() => instance ??= new RouterManager();

    public ObservableCollection<RouterInstance> Routes { get; } = new();
    private readonly SerialisationManager serialisationManager;

    private readonly List<OscSender> senders = new();

    public RouterManager()
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new RouterManagerSerialiser(AppManager.GetInstance().Storage, this, ProfileManager.GetInstance().ActiveProfile));
    }

    public void Load()
    {
        Routes.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (RouterInstance routerInstance in e.NewItems)
                {
                    routerInstance.Name.Subscribe(_ => serialisationManager.Serialise());
                    routerInstance.Address.Subscribe(_ => serialisationManager.Serialise());
                    routerInstance.Port.Subscribe(_ => serialisationManager.Serialise());
                }
            }

            serialisationManager.Serialise();
        };

        serialisationManager.Deserialise();
    }

    public void Start()
    {
        foreach (var route in Routes)
        {
            try
            {
                var endpoint = new IPEndPoint(IPAddress.Parse(route.Address.Value), route.Port.Value);

                Logger.Log($"Starting router instance `{route.Name.Value}` on {endpoint}");

                var sender = new OscSender();
                sender.Initialise(endpoint);
                sender.Enable();

                senders.Add(sender);
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e, $"Failed to start router instance named {route.Name.Value}");
            }
        }

        AppManager.GetInstance().VRChatOscClient.OnDataReceived += onDataReceived;
    }

    public void Stop()
    {
        Logger.Log("Stopping router instances");

        AppManager.GetInstance().VRChatOscClient.OnDataReceived -= onDataReceived;

        foreach (var sender in senders)
        {
            sender.Disable();
        }

        senders.Clear();
    }

    private void onDataReceived(byte[] data)
    {
        foreach (var sender in senders)
        {
            sender.Send(data);
        }
    }
}
