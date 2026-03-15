namespace notifications.Models;

public class EmailMessage
{
    public required string To { get; set; }
    public required string ToName { get; set; }
    public required string Subject { get; set; }
    public required string HtmlBody { get; set; }
}
