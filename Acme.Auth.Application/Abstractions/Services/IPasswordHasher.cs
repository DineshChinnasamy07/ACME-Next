/*  Class Name      : IPasswordHasher.cs
 *  Purpose         : EncryptValue used before AuthenticateUser compares PASSWORD
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
namespace Acme.Auth.Application.Abstractions.Services
{
    public interface IPasswordHasher
    {
        string EncryptValue(string value);
    }
}
