namespace Domain.Shared.Security;

public enum DatabaseOperationType
{
    Select,
    Insert,
    Update,
    Delete,
    Create,
    Drop,
    Alter,
    Grant,
    Revoke,
    Execute,
    Show,
    Describe,
    Explain
}