using System;
using System.Collections.Generic;
using System.Text;

namespace ZIVA_Prototype.Components.Models
{
    public class UserInputEntry
    {
        public DateTime Time { get; set; }
        public string Value { get; set; } = "";
        public UserInputType Type { get; set; }
        public int Position { get; set; }
    }

    public enum UserInputType
    {
        SearchQuery,
        Autofill,
        Favicon,
    }
}
