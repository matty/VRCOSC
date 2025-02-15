// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules.Attributes.Settings;

/// <summary>
/// For use with value types
/// </summary>
public abstract class ValueModuleSetting<T> : ModuleSetting where T : notnull
{
    public Observable<T> Attribute { get; }

    protected ValueModuleSetting(string title, string description, Type viewType, T defaultValue)
        : base(title, description, viewType)
    {
        Attribute = new Observable<T>(defaultValue);
        Attribute.Subscribe(_ => OnSettingChange?.Invoke());
    }

    internal override object Serialise() => Attribute.Value;

    internal override bool IsDefault() => Attribute.IsDefault;
}

public class BoolModuleSetting : ValueModuleSetting<bool>
{
    public BoolModuleSetting(string title, string description, Type viewType, bool defaultValue)
        : base(title, description, viewType, defaultValue)
    {
    }

    internal override bool Deserialise(object? ingestValue)
    {
        if (ingestValue is not bool boolValue) return false;

        Attribute.Value = boolValue;
        return true;
    }

    public override bool GetValue<T>(out T returnValue)
    {
        if (typeof(T) == typeof(bool))
        {
            returnValue = (T)Convert.ChangeType(Attribute.Value, typeof(T));
            return true;
        }

        returnValue = (T)Convert.ChangeType(false, typeof(T));
        return false;
    }
}

public class StringModuleSetting : ValueModuleSetting<string>
{
    public StringModuleSetting(string title, string description, Type viewType, string defaultValue)
        : base(title, description, viewType, defaultValue)
    {
    }

    internal override bool Deserialise(object? ingestValue)
    {
        if (ingestValue is not string stringValue) return false;

        Attribute.Value = stringValue;
        return true;
    }

    public override bool GetValue<T>(out T returnValue)
    {
        if (typeof(T) == typeof(string))
        {
            returnValue = (T)Convert.ChangeType(Attribute.Value, typeof(T));
            return true;
        }

        returnValue = (T)Convert.ChangeType(string.Empty, typeof(T));
        return false;
    }
}

public class IntModuleSetting : ValueModuleSetting<int>
{
    public IntModuleSetting(string title, string description, Type viewType, int defaultValue)
        : base(title, description, viewType, defaultValue)
    {
    }

    internal override bool Deserialise(object? ingestValue)
    {
        // json stores as long
        if (ingestValue is not long longValue) return false;

        Attribute.Value = (int)longValue;
        return true;
    }

    public override bool GetValue<T>(out T returnValue)
    {
        if (typeof(T) == typeof(int))
        {
            returnValue = (T)Convert.ChangeType(Attribute.Value, typeof(T));
            return true;
        }

        returnValue = (T)Convert.ChangeType(0, typeof(T));
        return false;
    }
}

public class FloatModuleSetting : ValueModuleSetting<float>
{
    public FloatModuleSetting(string title, string description, Type viewType, float defaultValue)
        : base(title, description, viewType, defaultValue)
    {
    }

    internal override bool Deserialise(object? ingestValue)
    {
        // json stores as double
        if (ingestValue is not double doubleValue) return false;

        Attribute.Value = (float)doubleValue;
        return true;
    }

    public override bool GetValue<T>(out T returnValue)
    {
        if (typeof(T) == typeof(float))
        {
            returnValue = (T)Convert.ChangeType(Attribute.Value, typeof(T));
            return true;
        }

        returnValue = (T)Convert.ChangeType(0f, typeof(T));
        return false;
    }
}

public class EnumModuleSetting : IntModuleSetting
{
    internal readonly Type EnumType;

    public EnumModuleSetting(string title, string description, Type viewType, int defaultValue, Type enumType)
        : base(title, description, viewType, defaultValue)
    {
        EnumType = enumType;
    }

    public override bool GetValue<T>(out T returnValue)
    {
        if (typeof(T) == EnumType)
        {
            returnValue = (T)Enum.ToObject(EnumType, Attribute.Value);
            return true;
        }

        returnValue = (T)Enum.ToObject(typeof(T), 0);
        return false;
    }
}

public class DropdownListModuleSetting : StringModuleSetting
{
    private readonly Type itemType;

    public string TitlePath { get; }
    public string ValuePath { get; }

    public IEnumerable<object> Items { get; }

    public DropdownListModuleSetting(string title, string description, Type viewType, IEnumerable<object> items, string defaultValue, string titlePath, string valuePath)
        : base(title, description, viewType, defaultValue)
    {
        itemType = items.GetType().GenericTypeArguments[0];

        TitlePath = titlePath;
        ValuePath = valuePath;

        // take a copy to stop developers holding a reference
        Items = items.ToList().AsReadOnly();
    }

    public override bool GetValue<T>(out T returnValue)
    {
        if (typeof(T) == itemType)
        {
            var valueProperty = itemType.GetProperty(ValuePath);
            returnValue = (T)Items.First(item => valueProperty!.GetValue(item)!.ToString()! == Attribute.Value);
            return true;
        }

        returnValue = (T)new object();
        return false;
    }
}

public class SliderModuleSetting : ValueModuleSetting<float>
{
    public Type ValueType { get; }
    public float MinValue { get; }
    public float MaxValue { get; }
    public float TickFrequency { get; }

    public SliderModuleSetting(string title, string description, Type viewType, float defaultValue, float minValue, float maxValue, float tickFrequency)
        : base(title, description, viewType, defaultValue)
    {
        ValueType = typeof(float);

        MinValue = minValue;
        MaxValue = maxValue;
        TickFrequency = tickFrequency;
    }

    public SliderModuleSetting(string title, string description, Type viewType, int defaultValue, int minValue, int maxValue, int tickFrequency)
        : base(title, description, viewType, defaultValue)
    {
        ValueType = typeof(int);

        MinValue = minValue;
        MaxValue = maxValue;
        TickFrequency = tickFrequency;
    }

    internal override bool Deserialise(object? ingestValue)
    {
        // json stores as double
        if (ingestValue is not double doubleValue) return false;

        var floatValue = (float)doubleValue;
        Attribute.Value = Math.Clamp(floatValue, MinValue, MaxValue);

        return true;
    }

    public override bool GetValue<T>(out T returnValue)
    {
        if (typeof(T) == typeof(float) && ValueType == typeof(float))
        {
            returnValue = (T)Convert.ChangeType(Attribute.Value, typeof(T));
            return true;
        }

        if (typeof(T) == typeof(int) && ValueType == typeof(int))
        {
            returnValue = (T)Convert.ChangeType(Attribute.Value, typeof(T));
            return true;
        }

        returnValue = (T)Convert.ChangeType(0f, typeof(T));
        return false;
    }
}

/// <summary>
/// Serialises the time directly in UTC and converts to a local timezone's DateTimeOffset on deserialise
/// </summary>
public class DateTimeModuleSetting : ValueModuleSetting<DateTimeOffset>
{
    public DateTimeModuleSetting(string title, string description, Type viewType, DateTimeOffset defaultValue)
        : base(title, description, viewType, defaultValue)
    {
    }

    internal override object Serialise() => Attribute.Value.UtcTicks;

    internal override bool Deserialise(object? ingestValue)
    {
        if (ingestValue is not long ingestUtcTicks) return false;

        // Since we're storing the UTC ticks we have to do some conversions to adjust it to local time on load
        // This allows people to share configs and have it automatically adjust to their timezones

        var utcDateTime = new DateTime(ingestUtcTicks, DateTimeKind.Utc);
        var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.Local);
        var dateTimeOffset = new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime));

        Attribute.Value = dateTimeOffset;

        return true;
    }

    public override bool GetValue<T>(out T returnValue)
    {
        if (typeof(T) == typeof(DateTimeOffset))
        {
            returnValue = (T)Convert.ChangeType(Attribute.Value, typeof(T));
            return true;
        }

        returnValue = (T)Convert.ChangeType(DateTimeOffset.FromUnixTimeSeconds(0), typeof(T));
        return false;
    }
}