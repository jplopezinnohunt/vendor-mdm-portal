using Newtonsoft.Json;

namespace VendorMdm.Api.Models;

// Existing entities...

/// <summary>
/// Cosmos DB artifact for invitation payloads
/// Stores complete invitation data for audit trail
/// </summary>
public class InvitationArtifact
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString(); // Matches SQL VendorInvitation.Id

    [JsonProperty("invitationId")]
    public string InvitationId { get; set; } = string.Empty; // Partition Key

    [JsonProperty("vendorLegalName")]
    public string VendorLegalName { get; set; } = string.Empty;

    [JsonProperty("primaryContactEmail")]
    public string PrimaryContactEmail { get; set; } = string.Empty;

    [JsonProperty("invitedBy")]
    public string InvitedBy { get; set; } = string.Empty;

    [JsonProperty("invitedByName")]
    public string InvitedByName { get; set; } = string.Empty;

    [JsonProperty("token")]
    public string Token { get; set; } = string.Empty;

    [JsonProperty("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    [JsonProperty("notes")]
    public string? Notes { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("fullPayload")]
    public object? FullPayload { get; set; } // Complete invitation request data

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Cosmos DB artifact for completed vendor applications via invitation
/// </summary>
public class InvitationCompletionArtifact
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("invitationId")]
    public string InvitationId { get; set; } = string.Empty; // Partition Key

    [JsonProperty("vendorApplicationId")]
    public string VendorApplicationId { get; set; } = string.Empty;

    [JsonProperty("submittedData")]
    public object SubmittedData { get; set; } = new object();

    [JsonProperty("completedAt")]
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
