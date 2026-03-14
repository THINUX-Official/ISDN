using System;
using System.Collections.Generic;

namespace ISDN.ViewModels
{
    public class OrderLifecycleViewModel
    {
        public int OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }

        public List<Milestone> Milestones { get; set; } = new List<Milestone>();

        public class Milestone
        {
            public string Name { get; set; } = string.Empty;
            public string Status { get; set; } = "Future"; // Completed, InProgress, Future, Issue
            public DateTime? Timestamp { get; set; }
            public string Icon { get; set; } = "fa-circle";
            public string? Description { get; set; }
            public string? DetailsHtml { get; set; }
        }
    }
}
