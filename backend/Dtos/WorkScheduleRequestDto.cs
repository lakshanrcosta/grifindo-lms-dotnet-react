public class WorkScheduleRequestDto
{
    public int EmployeeId { get; set; } // Employee ID
    public DateTime Date { get; set; } // Schedule date (YYYY-MM-DD)
    public string RoasterStartTime { get; set; } = string.Empty; // Format: "HH:mm:ss"
    public string RoasterEndTime { get; set; } = string.Empty;   // Format: "HH:mm:ss"
}
