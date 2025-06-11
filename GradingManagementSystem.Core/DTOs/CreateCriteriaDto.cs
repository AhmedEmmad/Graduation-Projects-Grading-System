using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class CreateCriteriaDto
    {
        [Required(ErrorMessage = "Criteria Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Criteria Description is required.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Maximum Grade is required.")]
        public int MaxGrade { get; set; }

        [Required(ErrorMessage = "Evaluator is required.")]
        public string Evaluator { get; set; }

        [Required(ErrorMessage = "GivenTo is required.")]
        public string GivenTo { get; set; }

        [Required(ErrorMessage = "Specialty is required.")]
        public string Specialty { get; set; }

        [Required(ErrorMessage = "Term is required.")]
        public string Term { get; set; }
    }
}
