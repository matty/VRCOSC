﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.SDK.Modules.Attributes.Settings;

namespace VRCOSC.App.Pages.Modules.Settings;

public partial class DateTimeSettingPage
{
    private readonly DateTimeModuleSetting moduleSetting;

    private DateTime proxyValue;

    public DateTime ProxyValue
    {
        get => proxyValue;
        set
        {
            proxyValue = value;
            moduleSetting.Attribute.Value = new DateTimeOffset(proxyValue, TimeZoneInfo.Local.GetUtcOffset(proxyValue));
        }
    }

    public DateTimeSettingPage(DateTimeModuleSetting moduleSetting)
    {
        this.moduleSetting = moduleSetting;
        ProxyValue = moduleSetting.Attribute.Value.LocalDateTime;

        InitializeComponent();

        DataContext = this;
    }
}
