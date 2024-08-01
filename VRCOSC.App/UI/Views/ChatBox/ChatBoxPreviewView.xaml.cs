﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.UI.Core;

namespace VRCOSC.App.UI.Views.ChatBox;

public partial class ChatBoxPreviewView
{
    public ChatBoxPreviewView()
    {
        InitializeComponent();

        AppManager.GetInstance().VRChatOscClient.OnParameterSent += OnParameterSent;
    }

    private void OnParameterSent(VRChatOscMessage message) => Dispatcher.Invoke(() =>
    {
        if (!message.IsChatboxInput) return;

        var text = (string)message.ParameterValue;

        if (ChatBoxText.Text != string.Empty && text == string.Empty)
        {
            ChatBoxContainer.FadeOutFromOne(200, () => ChatBoxText.Text = text);
            return;
        }

        if (ChatBoxText.Text == string.Empty && text != string.Empty)
        {
            ChatBoxText.Text = text;
            ChatBoxContainer.FadeInFromZero(200);
            return;
        }

        ChatBoxText.Text = text;
    });
}
