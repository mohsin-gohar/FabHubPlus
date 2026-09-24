namespace FanHubPlus.Models.Enums;

// Approval flow for fan submissions (admin decides)
public enum SubmissionStatus
{
    Pending,  // waiting for admin review
    Approved, // published
    Rejected  // declined
}
