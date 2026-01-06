// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

using System.Text.Json;
using System.Text.Json.Serialization;
using TMS.Libs.Frontend.Web.DockPanels.Components;

namespace TMS.Libs.Frontend.Web.DockPanels.Converters;

/// <summary>
/// Dock panel component converter.
/// </summary>
internal sealed class DockPanelComponentConverter : JsonConverter<List<DockPanelComponentBase>>
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override List<DockPanelComponentBase>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, List<DockPanelComponentBase> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            if (item is DockPanelContentComponent content)
            {
                writer.WriteRawValue(JsonSerializer.Serialize(content, options));
            }
            else if (item is DockPanelComponent contentItem)
            {
                writer.WriteRawValue(JsonSerializer.Serialize(contentItem, options));
            }
        }
        writer.WriteEndArray();
    }
}
