using System;
using System.Collections.Generic;
using System.Text;

namespace ZIVA_Prototype.Components.Models.Timeline
{
    public class OrphanArtifact
    {
        public DateTime Time { get; set; }

        public BrowserCookieEntry? Cookie { get; set; }

        public WebDataAutofillEntry? Autofill { get; set; }

        public BrowserExtensionEntry? Extension { get; set; }

        public UserInputEntry? Input { get; set; }
    }
}
