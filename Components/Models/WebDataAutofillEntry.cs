public class WebDataAutofillEntry
{
    public string Name { get; set; }          // z. B. "email", "firstname"
    public string Value { get; set; }         // z. B. "max.mustermann@gmail.com"
    public DateTime DateCreated { get; set; }
    public DateTime DateLastUsed { get; set; }
    public int Count { get; set; }            // wie oft benutzt
    public int Position { get; set; }         // für Timeline-Darstellung
}


//--------------------------------------------------------------
// Weitere Modelle für WebData-Datenbanken
// Modell für WebData - Kreditkarten

public class WebDataCreditCardEntry
{
    public string NameOnCard { get; set; }
    public string CardNumber { get; set; }            // nur unverschlüsselt
    public byte[] EncryptedCardNumber { get; set; }   // Chrome schützt Kartennummern
    public int ExpMonth { get; set; }
    public int ExpYear { get; set; }

    public DateTime DateModified { get; set; }
    public int Position { get; set; }                 // Timeline
}


// Modell für WebData - Profileinträge (Adressen, E-Mail, Telefon)

public class WebDataProfileEntry
{
    public string Guid { get; set; }
    public string FullName { get; set; }
    public string CompanyName { get; set; }
    public string StreetAddress { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
    public string CountryCode { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }

    public DateTime DateModified { get; set; }
    public int Position { get; set; } // Timeline
}
