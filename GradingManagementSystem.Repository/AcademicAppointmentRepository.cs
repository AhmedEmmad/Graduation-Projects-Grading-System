using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Repositories.Contact;
using GradingManagementSystem.Repository.Data.DbContexts;

namespace GradingManagementSystem.Repository
{
    public class AcademicAppointmentRepository : GenericRepository<AcademicAppointment>, IAcademicAppointmentRepository
    {
        private readonly GradingManagementSystemDbContext _dbContext;
        public AcademicAppointmentRepository(GradingManagementSystemDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
