/*  Class Name      : CommonMember.cs
 *  Purpose         : Reusable member functions accessible to inherited class
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */

using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using Bosco.Common.CommonMemberSet;

namespace Bosco.Common
{
    public class CommonMember
    {
        private NumberSetMember numberSet = null;
        private DateSetMember dateSet = null;
        private ArraySetMember arraySet = null;
        private StringSetMember stringSet = null;
        private FileSetMember fileSet = null;
        private EnumSetMember enumSet = null;

        #region Member Group

        public NumberSetMember NumberSet
        {
            get { if (numberSet == null) numberSet = new NumberSetMember(); return numberSet; }
        }

        public DateSetMember DateSet
        {
            get { if (dateSet == null) dateSet = new DateSetMember(); return dateSet; }
        }

        public ArraySetMember ArraySet
        {
            get { if (arraySet == null) arraySet = new ArraySetMember(); return arraySet; }
        }

        public StringSetMember StringSet
        {
            get { if (stringSet == null) stringSet = new StringSetMember(); return stringSet; }
        }

        public FileSetMember FileSet
        {
            get { if (fileSet == null) fileSet = new FileSetMember(); return fileSet; }
        }

        public EnumSetMember EnumSet
        {
            get { if (enumSet == null) enumSet = new EnumSetMember(); return enumSet; }
        }

        #endregion

        public object GetDynamicInstance(string instanceType, object[] args)
        {
            Type type = Type.GetType(instanceType, false, true);
            object instance = null;

            if (type != null)
            {
                try
                {
                    if (args != null)
                    {
                        instance = System.Activator.CreateInstance(type, args);
                    }
                    else
                    {
                        instance = System.Activator.CreateInstance(type);
                    }
                }
                catch (Exception e)
                {
                    throw new ExceptionHandler(e, true);
                }
            }

            return instance;
        }
    }
}
