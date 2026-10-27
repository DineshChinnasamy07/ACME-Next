/*  Class Name      : EnumTypeSchema.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System.Data;

namespace Bosco.Common.CommonMemberSet
{
    public partial class EnumTypeSchema : DataSet
    {
        public class EnumTypeDataTable : DataTable
        {
            public DataColumn IdColumn { get; private set; }
            public DataColumn NameColumn { get; private set; }

            public EnumTypeDataTable()
                : base("EnumType")
            {
                IdColumn = new DataColumn("Id", typeof(int));
                NameColumn = new DataColumn("Name", typeof(string));
                Columns.Add(IdColumn);
                Columns.Add(NameColumn);
            }
        }
    }
}
