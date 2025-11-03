using System.Text.Json.Serialization;
using System.ComponentModel;

namespace HedgeConfig.Models
{
    public class Option : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        public string Name 
        { 
            get => _name;
            set 
            { 
                if (_name != value)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }
        
        private string _description = string.Empty;
        public string Description 
        { 
            get => _description;
            set 
            { 
                if (_description != value)
                {
                    _description = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
                }
            }
        }
        
        public List<Choice> Choices { get; set; } = new();
        public ConfigElement? OriginalJsonElement { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class Choice : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        public string Name 
        { 
            get => _name;
            set 
            { 
                if (_name != value)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }
        
        private string _folder = string.Empty;
        public string Folder 
        { 
            get => _folder;
            set 
            { 
                if (_folder != value)
                {
                    _folder = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Folder)));
                }
            }
        }
        
        private string _description = string.Empty;
        public string Description 
        { 
            get => _description;
            set 
            { 
                if (_description != value)
                {
                    _description = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
                }
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class JsonEnumEntry
    {
        [JsonPropertyName("DisplayName")]
        public string DisplayName { get; set; } = string.Empty;
        
        [JsonPropertyName("Value")]
        public string Value { get; set; } = string.Empty;
        
        [JsonPropertyName("Description")]
        public List<string> Description { get; set; } = new();
    }

    public class RootObject
    {
        [JsonPropertyName("Groups")]
        public List<ConfigGroup> Groups { get; set; } = new();
        
        [JsonPropertyName("Enums")]
        public Dictionary<string, List<JsonEnumEntry>> Enums { get; set; } = new();
        
        [JsonPropertyName("IniFile")]
        public string? IniFile { get; set; }
    }

    public class ConfigGroup
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("DisplayName")]
        public string DisplayName { get; set; } = string.Empty;
        
        [JsonPropertyName("Elements")]
        public List<ConfigElement> Elements { get; set; } = new();
    }

    public class ConfigElement
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("DisplayName")]
        public string DisplayName { get; set; } = string.Empty;
        
        [JsonPropertyName("Type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("DefaultValue")]
        public string? DefaultValue { get; set; }
        
        [JsonPropertyName("Description")]
        public List<string> Description { get; set; } = new();
        
        [JsonPropertyName("EnumName")]
        public string? EnumName { get; set; }
        
        [JsonPropertyName("MinValue")]
        public string? MinValue { get; set; }
        
        [JsonPropertyName("MaxValue")]
        public string? MaxValue { get; set; }
    }
}
