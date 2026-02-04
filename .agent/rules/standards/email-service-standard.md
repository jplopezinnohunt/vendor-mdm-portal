# Email Service Standard

**Category**: Integration & Infrastructure
**Pattern #**: 16
**Status**: MANDATORY

---

## Definition

Email sending MUST use `IEmailService` interface with template-based rendering.

---

## Rules

1. **ALWAYS** use `IEmailService` interface
2. **ALWAYS** use templates for email content
3. **ALWAYS** implement simulation mode for local dev
4. **NEVER** hardcode email content in code

---

## Implementation

### Interface

```csharp
public interface IEmailService
{
    Task<Result> SendAsync(EmailMessage message);
    Task<Result> SendTemplatedAsync(string templateName, object model, string recipient);
}

public class EmailMessage
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsHtml { get; set; } = true;
    public List<string>? Cc { get; set; }
    public List<EmailAttachment>? Attachments { get; set; }
}
```

### Azure Communication Services Implementation

```csharp
public class AzureEmailService : IEmailService
{
    private readonly EmailClient _emailClient;

    public async Task<Result> SendAsync(EmailMessage message)
    {
        var emailMessage = new EmailMessage(
            senderAddress: _config.SenderAddress,
            content: new EmailContent(message.Subject)
            {
                Html = message.Body
            },
            recipients: new EmailRecipients(new[] {
                new EmailAddress(message.To)
            }));

        await _emailClient.SendAsync(WaitUntil.Completed, emailMessage);
        return Result.Success();
    }
}
```

### Simulation Implementation

```csharp
public class SimulatedEmailService : IEmailService
{
    private readonly IStructuredLogger _logger;

    public Task<Result> SendAsync(EmailMessage message)
    {
        _logger.LogInformation("[SIMULATION MODE] Email sent", new {
            to = message.To,
            subject = message.Subject,
            bodyLength = message.Body.Length
        });

        // Optionally write to console or file
        Console.WriteLine($"📧 EMAIL TO: {message.To}");
        Console.WriteLine($"   SUBJECT: {message.Subject}");

        return Task.FromResult(Result.Success());
    }
}
```

### Email Templates

```csharp
// Templates stored in: Resources/EmailTemplates/
public class EmailTemplateService
{
    public async Task<string> RenderAsync(string templateName, object model)
    {
        var templatePath = $"Resources/EmailTemplates/{templateName}.html";
        var template = await File.ReadAllTextAsync(templatePath);

        // Simple token replacement
        foreach (var prop in model.GetType().GetProperties())
        {
            var value = prop.GetValue(model)?.ToString() ?? "";
            template = template.Replace($"{{{{{prop.Name}}}}}", value);
        }

        return template;
    }
}
```

### Registration

```csharp
if (builder.Configuration.GetValue<bool>("Email:UseMock"))
{
    builder.Services.AddSingleton<IEmailService, SimulatedEmailService>();
}
else
{
    builder.Services.AddSingleton<IEmailService, AzureEmailService>();
}
```

---

## Reference

- **Interface**: `Core.Framework/Email/IEmailService.cs`
- **Templates**: `Resources/EmailTemplates/`
- **Golden Rules**: Section 10.4 Pattern 16
