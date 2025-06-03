using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Task name is required")]
        public string Name { get; set; }


        [Required(ErrorMessage = "Task description is required")]
        public string Description { get; set; }


        [Required(ErrorMessage = "Task deadline is required")]
        public DateTime Deadline { get; set; }


        [Required(ErrorMessage = "Supervisor id is required")]
        public int SupervisorId { get; set; }


        [Required(ErrorMessage = "Team id is required")]
        public int TeamId { get; set; }


        [Required(ErrorMessage = "Student ids are required")]
        public List<int> StudentIds { get; set; } = new();
    }
}
