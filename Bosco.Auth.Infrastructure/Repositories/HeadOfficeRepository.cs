/*  Class Name      : HeadOfficeRepository.cs
 *  Purpose         : FetchDatabase from admin_Portal head_office
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Bosco.Auth.Application.Abstractions.Repositories;
using Bosco.Auth.Domain.Entities;
using Bosco.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bosco.Auth.Infrastructure.Repositories
{
    public class HeadOfficeRepository : IHeadOfficeRepository
    {
        private readonly AuthDbContext _context;

        public HeadOfficeRepository(AuthDbContext context)
        {
            _context = context;
        }

        public Task<HeadOffice?> FetchDatabase(string headOfficeCode, CancellationToken cancellationToken)
        {
            string code = headOfficeCode.ToLower();
            return _context.HeadOffices.AsNoTracking()
                .Where(office => office.HeadOfficeCode.ToLower() == code)
                .Select(office => new HeadOffice
                {
                    HeadOfficeId = office.HeadOfficeId,
                    HeadOfficeCode = office.HeadOfficeCode,
                    HeadOfficeName = office.HeadOfficeName,
                    HostName = office.HostName,
                    DbName = office.DbName,
                    UserName = office.UserName,
                    Password = office.Password
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
