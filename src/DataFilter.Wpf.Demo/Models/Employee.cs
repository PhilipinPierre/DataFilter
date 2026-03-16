namespace DataFilter.Wpf.Demo.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public float Salary { get; set; }
    public DateTime HireDate { get; set; }
    public TimeSpan Time { get; set; }
    public bool IsActive { get; set; }
}
