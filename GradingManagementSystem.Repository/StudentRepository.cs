using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Repositories.Contact;
using GradingManagementSystem.Repository.Data.DbContexts;

namespace GradingManagementSystem.Repository
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(GradingManagementSystemDbContext dbContext) : base(dbContext)
        {
        }
    }
}
