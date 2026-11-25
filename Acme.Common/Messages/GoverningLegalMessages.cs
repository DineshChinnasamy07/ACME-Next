/*  Class Name      : GoverningLegalMessages.cs
 *  Purpose         : Message keys for Statutory Masters - Legal Entity, Governing Bodies, Map Governing Bodies
 *                    (English text lives in Resources/OfficeMessages.resx)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Common.Messages;

public static class GoverningLegalMessages
{
    // Shared
    public const string STATUTORY_HEAD_OFFICE_ONLY = "STATUTORY_HEAD_OFFICE_ONLY";
    public const string STATUTORY_FIELD_MAX = "STATUTORY_FIELD_MAX";

    // Legal Entity
    public const string LEGAL_ENTITY_DETAILS_REQUIRED = "LEGAL_ENTITY_DETAILS_REQUIRED";
    public const string LEGAL_ENTITY_NOT_FOUND = "LEGAL_ENTITY_NOT_FOUND";
    public const string LEGAL_ENTITY_EXISTS = "LEGAL_ENTITY_EXISTS";
    public const string LEGAL_ENTITY_COUNTRY_EMPTY = "LEGAL_ENTITY_COUNTRY_EMPTY";
    public const string LEGAL_ENTITY_STATE_EMPTY = "LEGAL_ENTITY_STATE_EMPTY";
    public const string LEGAL_ENTITY_REG_DATE_FUTURE = "LEGAL_ENTITY_REG_DATE_FUTURE";
    public const string LEGAL_ENTITY_PERMISSION_DATE_FUTURE = "LEGAL_ENTITY_PERMISSION_DATE_FUTURE";
    public const string LEGAL_ENTITY_ASSOCIATION_NATURE_EMPTY = "LEGAL_ENTITY_ASSOCIATION_NATURE_EMPTY";
    public const string LEGAL_ENTITY_ASSOCIATION_OTHER_EMPTY = "LEGAL_ENTITY_ASSOCIATION_OTHER_EMPTY";
    public const string LEGAL_ENTITY_DENOMINATION_EMPTY = "LEGAL_ENTITY_DENOMINATION_EMPTY";
    public const string LEGAL_ENTITY_DENOMINATION_OTHER_EMPTY = "LEGAL_ENTITY_DENOMINATION_OTHER_EMPTY";
    public const string LEGAL_ENTITY_EMAIL_INVALID = "LEGAL_ENTITY_EMAIL_INVALID";
    public const string LEGAL_ENTITY_SAVE_SUCCESS = "LEGAL_ENTITY_SAVE_SUCCESS";
    public const string LEGAL_ENTITY_DELETE_SUCCESS = "LEGAL_ENTITY_DELETE_SUCCESS";
    public const string LEGAL_ENTITY_DELETE_FAILURE = "LEGAL_ENTITY_DELETE_FAILURE";
    public const string LEGAL_ENTITY_USED_BY_PROJECT = "LEGAL_ENTITY_USED_BY_PROJECT";
    public const string LEGAL_ENTITY_MAPPED_TO_GOVERNING_BODY = "LEGAL_ENTITY_MAPPED_TO_GOVERNING_BODY";

    // Governing Bodies
    public const string GOVERNING_BODY_DETAILS_REQUIRED = "GOVERNING_BODY_DETAILS_REQUIRED";
    public const string GOVERNING_BODY_NOT_FOUND = "GOVERNING_BODY_NOT_FOUND";
    public const string GOVERNING_BODY_STATE_EMPTY = "GOVERNING_BODY_STATE_EMPTY";
    public const string GOVERNING_BODY_MAPPED_TO_LEGAL_ENTITY = "GOVERNING_BODY_MAPPED_TO_LEGAL_ENTITY";

    // Map Governing Bodies
    public const string MAP_GOVERNING_LEGAL_ENTITY_EMPTY = "MAP_GOVERNING_LEGAL_ENTITY_EMPTY";
    public const string MAP_GOVERNING_MEMBER_INVALID = "MAP_GOVERNING_MEMBER_INVALID";
    public const string MAP_GOVERNING_SAVE_SUCCESS = "MAP_GOVERNING_SAVE_SUCCESS";
    public const string MAP_GOVERNING_UNMAP_SUCCESS = "MAP_GOVERNING_UNMAP_SUCCESS";
}
