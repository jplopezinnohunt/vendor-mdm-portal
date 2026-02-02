using System;

namespace VendorMdm.Shared.Attributes;

/// <summary>
/// Marks a property as containing Personally Identifiable Information (PII).
/// Used for GDPR compliance (Pattern 17: Data Privacy & Masking).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class PiiAttribute : Attribute
{
    public PiiLevel Level { get; set; }
    public bool RequiresEncryption { get; set; }
    public bool RequiresMasking { get; set; }

    public PiiAttribute(PiiLevel level = PiiLevel.Medium)
    {
        Level = level;
    }
}

/// <summary>
/// Classification level for PII data.
/// </summary>
public enum PiiLevel
{
    Low = 1,      // Name, general contact info
    Medium = 2,   // Email, phone number
    High = 3      // Tax ID, bank account, SSN, passport
}
