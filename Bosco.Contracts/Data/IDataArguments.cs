/* Class        : IDataArguments.cs
 * Purpose      : Interface to DataArguments class
 * Author       : Dinesh C
 * Created on   : 16-Oct-2026
 */

using System;

namespace Bosco.Contracts.Data
{
    public interface IDataArguments
    {
        string FieldName { get; set; }
        int FieldSize { get; set; }
        object FieldValue { get; set; }
        DataType FieldType { get; set; }
        bool IsRowUniqueId { get; set; }
        SQLParameterType ParameterType { get; set; }
    }
}
