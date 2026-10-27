/*  Class Name      : CommonMember.cs
 *  Purpose         : Reusable member functions accessible to inherited class
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */

using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;

namespace Bosco.Common.CommonMemberSet
{
    #region Array Set

    public class ArraySetMember
    {
        public bool HasArrayValue(string[] arrayList, string findValue)
        {
            bool found = false;

            found = Array.Exists<string>(arrayList, new Predicate<string>(delegate(string arrayItem)
                    {
                        return arrayItem == findValue;
                    }));

            return found;
        }
    }

    #endregion
}
