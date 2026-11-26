/*  Class Name      : TdsMasterMessages.cs
 *  Purpose         : Message keys for the Head Office TDS statutory masters (TDS Section,
 *                    Nature of Payments, Deductee Type, Duty Tax) not already in MessageCatalog
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Common.Messages;

public static class TdsMasterMessages
{
    // Common
    public const string TDS_MASTERS_HEAD_OFFICE_ONLY = "TDS_MASTERS_HEAD_OFFICE_ONLY";
    public const string TDS_DETAILS_REQUIRED = "TDS_DETAILS_REQUIRED";
    public const string TDS_STATUS_INVALID = "TDS_STATUS_INVALID";

    // TDS Section
    public const string TDS_SECTION_NOT_FOUND = "TDS_SECTION_NOT_FOUND";
    public const string TDS_SECTION_CODE_MAX = "TDS_SECTION_CODE_MAX";
    public const string TDS_SECTION_NAME_MAX = "TDS_SECTION_NAME_MAX";
    public const string TDS_SECTION_CODE_EXISTS = "TDS_SECTION_CODE_EXISTS";
    public const string TDS_SECTION_NAME_EXISTS = "TDS_SECTION_NAME_EXISTS";
    public const string TDS_SECTION_DELETE_SUCCESS = "TDS_SECTION_DELETE_SUCCESS";
    public const string TDS_SECTION_DELETE_FAILURE = "TDS_SECTION_DELETE_FAILURE";

    // Nature of Payments
    public const string TDS_NATURE_PAYMENT_NOT_FOUND = "TDS_NATURE_PAYMENT_NOT_FOUND";
    public const string TDS_NATURE_PAYMENT_NAME_MAX = "TDS_NATURE_PAYMENT_NAME_MAX";
    public const string TDS_NATURE_PAYMENT_CODE_MAX = "TDS_NATURE_PAYMENT_CODE_MAX";
    public const string TDS_NATURE_PAYMENT_DESCRIPTION_MAX = "TDS_NATURE_PAYMENT_DESCRIPTION_MAX";
    public const string TDS_NATURE_PAYMENT_EXISTS = "TDS_NATURE_PAYMENT_EXISTS";
    public const string TDS_NATURE_PAYMENT_SECTION_INVALID = "TDS_NATURE_PAYMENT_SECTION_INVALID";
    public const string TDS_NATURE_PAYMENT_IN_USE = "TDS_NATURE_PAYMENT_IN_USE";
    public const string TDS_NATURE_PAYMENT_SAVE_SUCCESS = "TDS_NATURE_PAYMENT_SAVE_SUCCESS";
    public const string TDS_NATURE_PAYMENT_DELETE_SUCCESS = "TDS_NATURE_PAYMENT_DELETE_SUCCESS";
    public const string TDS_NATURE_PAYMENT_DELETE_FAILURE = "TDS_NATURE_PAYMENT_DELETE_FAILURE";

    // Deductee Type
    public const string TDS_DEDUCTEE_TYPE_NOT_FOUND = "TDS_DEDUCTEE_TYPE_NOT_FOUND";
    public const string TDS_DEDUCTEE_NAME_MAX = "TDS_DEDUCTEE_NAME_MAX";
    public const string TDS_DEDUCTEE_RESIDENTIAL_STATUS_INVALID = "TDS_DEDUCTEE_RESIDENTIAL_STATUS_INVALID";
    public const string TDS_DEDUCTEE_TYPE_EXISTS = "TDS_DEDUCTEE_TYPE_EXISTS";
    public const string TDS_DEDUCTEE_TYPE_IN_USE = "TDS_DEDUCTEE_TYPE_IN_USE";
    public const string TDS_DEDUCTEE_TYPE_DELETE_SUCCESS = "TDS_DEDUCTEE_TYPE_DELETE_SUCCESS";
    public const string TDS_DEDUCTEE_TYPE_DELETE_FAILURE = "TDS_DEDUCTEE_TYPE_DELETE_FAILURE";

    // Duty Tax
    public const string TDS_DUTY_TAX_NOT_FOUND = "TDS_DUTY_TAX_NOT_FOUND";
    public const string TDS_DUTY_TAX_NAME_MAX = "TDS_DUTY_TAX_NAME_MAX";
    public const string TDS_DUTY_TAX_EXISTS = "TDS_DUTY_TAX_EXISTS";
    public const string TDS_DUTY_TAX_IN_USE = "TDS_DUTY_TAX_IN_USE";
    public const string TDS_DUTY_TAX_SYSTEM_DELETE = "TDS_DUTY_TAX_SYSTEM_DELETE";
    public const string TDS_DUTY_TAX_SAVE_SUCCESS = "TDS_DUTY_TAX_SAVE_SUCCESS";
    public const string TDS_DUTY_TAX_DELETE_SUCCESS = "TDS_DUTY_TAX_DELETE_SUCCESS";
    public const string TDS_DUTY_TAX_DELETE_FAILURE = "TDS_DUTY_TAX_DELETE_FAILURE";
}
