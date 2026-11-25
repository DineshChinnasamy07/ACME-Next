/*  Class Name      : PasswordHasher.cs
 *  Purpose         : EncryptValue for AuthenticateUser
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Acme.Auth.Application.Abstractions.Services;

namespace Acme.Auth.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        public string EncryptValue(string value)
        {
            return LegacyPasswordCipher.EncryptValue(value);
        }
    }
}
