/*  Class Name      : LoginCipherTests.cs
 *  Purpose         : EncryptValue matches the portal password cipher
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using Bosco.Auth.Infrastructure.Security;

namespace Bosco.Domain.Tests
{
    public class LoginCipherTests
    {
        [Fact]
        public void EncryptValue_MatchesPortalAdminPassword()
        {
            string cipher = LegacyPasswordCipher.EncryptValue("acme#4t3a");

            Assert.Equal("FdmxOu7lYbMy58P1MyBUjA==", cipher);
        }
    }
}
