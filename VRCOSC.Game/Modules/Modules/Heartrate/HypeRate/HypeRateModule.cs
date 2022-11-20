﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.Modules.Heartrate.HypeRate;

public sealed class HypeRateModule : HeartRateModule
{
    public override string Title => "HypeRate";
    public override string Description => "Connects to HypeRate.io and sends your heartrate to VRChat";

    private bool receivedHeartRate;

    protected override HeartRateProvider CreateHeartRateProvider()
    {
        var provider = new HypeRateProvider(GetSetting<string>(HypeRateSetting.Id), VRCOSCSecrets.KEYS_HYPERATE);
        provider.OnWsHeartBeat += handleWsHeartBeat;
        return provider;
    }

    protected override void CreateAttributes()
    {
        base.CreateAttributes();
        CreateSetting(HypeRateSetting.Id, "HypeRate ID", "Your HypeRate ID given on your device", string.Empty);
    }

    protected override Task OnStart(CancellationToken cancellationToken)
    {
        var hypeRateId = GetSetting<string>(HypeRateSetting.Id);

        if (string.IsNullOrEmpty(hypeRateId))
        {
            Log("Cannot connect to HypeRate. Please enter an Id");
            return Task.CompletedTask;
        }

        SendParameter(HeartrateParameter.Enabled, false);

        return base.OnStart(cancellationToken);
    }

    protected override void HandleHeartRateUpdate(int heartrate)
    {
        base.HandleHeartRateUpdate(heartrate);
        receivedHeartRate = true;
    }

    private void handleWsHeartBeat()
    {
        if (!receivedHeartRate) SendParameter(HeartrateParameter.Enabled, false);
        receivedHeartRate = false;
    }

    private enum HypeRateSetting
    {
        Id
    }
}
