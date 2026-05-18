using System.Text.Json.Serialization;

namespace ZIVA_Prototype.Components.Models.Timeline
{
    public class WebDataAutofillEntry
    {
        public string Name { get; set; } = string.Empty;          // z. B. "email", "firstname"
        public string Value { get; set; } = string.Empty;         // z. B. "max.mustermann@gmail.com"
        public DateTime DateCreated { get; set; }
        public DateTime DateLastUsed { get; set; }
        public int Count { get; set; }            // wie oft benutzt
        public int Position { get; set; }         // für Timeline-Darstellung

        [JsonIgnore]
        public List<ArtifactRelationEntry> Relations { get; set; } = new(); // Für Verknüpfungen zu anderen Artefakten
    }


    //--------------------------------------------------------------
    // Weitere Modelle für WebData-Datenbanken
    // Modell für WebData - Kreditkarten

    public class WebDataCreditCardEntry
    {
        public string NameOnCard { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;            // nur unverschlüsselt
        public byte[] EncryptedCardNumber { get; set; } = Array.Empty<byte>();   // Chrome schützt Kartennummern
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }

        public DateTime DateModified { get; set; }
        public int Position { get; set; }                 // Timeline
    }


    // Modell für WebData - Profileinträge (Adressen, E-Mail, Telefon)

    public class WebDataProfileEntry
    {
        public string Guid { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string StreetAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public DateTime DateModified { get; set; }
        public int Position { get; set; } // Timeline
    }
}