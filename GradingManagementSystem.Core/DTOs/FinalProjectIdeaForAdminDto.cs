using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingManagementSystem.Core.DTOs
{
    public class FinalProjectIdeaForAdminDto
    {
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? ProjectDescription { get; set; }
        public int? SupervisorId { get; set; }
        public string? SupervisorName { get; set; }
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public string PostedBy { get; set; }
        public List<TeamMemberDto>? TeamMembers { get; set; } = new();
    }
}
