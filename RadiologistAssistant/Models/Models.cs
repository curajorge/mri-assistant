namespace RadiologistAssistant.Models
{
    public class Finding
    {
        public string? Description { get; set; }
        public double ConfidenceLevel { get; set; }
    }

    public class Analysis
    {
        public List<Finding>? Findings { get; set; }
        public string? Summary { get; set; }
    }

    public class Conclusion
    {
        public string? OverallSummary { get; set; }
        public List<string>? Recommendations { get; set; }
    }
}
