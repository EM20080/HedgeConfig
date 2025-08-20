using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HedgeConfig
{
    public class Option
    {
        public string Name { get; set; } = string.Empty;
        public List<Choice> Choices { get; set; } = new List<Choice>();
        public JsonElement? OriginalJsonElement { get; set; }
    }

    public class Choice
    {
        public string Name { get; set; } = string.Empty;
        public string Folder { get; set; } = string.Empty;
    }

    public class RootObject
    {
        public List<Group>? Groups { get; set; }
        public Dictionary<string, List<JsonEnumEntry>>? Enums { get; set; }
        public string? IniFile { get; set; }
    }

    public class Group
    {
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public List<JsonElement>? Elements { get; set; }
    }

    public class JsonElement
    {
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public List<string>? Description { get; set; }
        public string? Type { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? EnumName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? MinValue { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? MaxValue { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? DefaultValue { get; set; }
    }

    public class JsonEnumEntry
    {
        public string? DisplayName { get; set; }
        public string? Value { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Description { get; set; }
    }

    public static class Functions
    {
        public static void UpdateEnums(List<Option> options, Dictionary<string, List<JsonEnumEntry>> enums)
        {
            enums.Clear();
            foreach (var option in options)
            {
                if (option.Choices.Any())
                {
                    string enumKeyName = option.Name.Replace(" ", "");
                    var enumEntries = new List<JsonEnumEntry>();
                    foreach (var choice in option.Choices)
                    {
                        string folderVal = choice.Folder;
                        if (!string.IsNullOrEmpty(folderVal))
                        {
                            if (!folderVal.EndsWith("/") && !folderVal.EndsWith("\\"))
                                folderVal += "/";
                            folderVal = $"./{folderVal}";
                        }
                        else
                        {
                            folderVal = "./";
                        }
                        enumEntries.Add(new JsonEnumEntry
                        {
                            DisplayName = choice.Name,
                            Value = folderVal,
                            Description = new List<string> { "" }
                        });
                    }
                    enums[enumKeyName] = enumEntries;
                }
            }
        }
    }
}
