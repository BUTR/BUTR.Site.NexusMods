namespace BUTR.Site.NexusMods.Server.DynamicExpressions;

/// <summary>
/// Enum representing the different types of filter operations that can be performed.
/// </summary>
public enum FilterOperator
{
    /// <summary>
    /// Represents an equality comparison.
    /// </summary>
    Equals,

    /// <summary>
    /// Represents a non-equality comparison.
    /// </summary>
    DoesntEqual,

    /// <summary>
    /// Represents a greater than comparison.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Represents a greater than or equal to comparison.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Represents a less than comparison.
    /// </summary>
    LessThan,

    /// <summary>
    /// Represents a less than or equal to comparison.
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Represents a contains operation for collections or strings.
    /// </summary>
    Contains,

    /// <summary>
    /// Represents a not contains operation for collections or strings.
    /// </summary>
    NotContains,

    /// <summary>
    /// Represents a starts with operation for strings.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Represents an ends with operation for strings.
    /// </summary>
    EndsWith,

    /// <summary>
    /// Represents a contains operation for strings, ignoring case.
    /// </summary>
    ContainsIgnoreCase,

    /// <summary>
    /// Represents an operation to check if a collection or string is empty.
    /// </summary>
    IsEmpty,

    /// <summary>
    /// Represents an operation to check if a collection or string is not empty.
    /// </summary>
    IsNotEmpty,

    /// <summary>
    /// Represents a contains key operation for dictionaries.
    /// </summary>
    ContainsKey,

    /// <summary>
    /// Represents a not contains key operation for dictionaries.
    /// </summary>
    NotContainsKey,

    /// <summary>
    /// Represents a contains value operation for dictionaries.
    /// </summary>
    ContainsValue,

    /// <summary>
    /// Represents a not contains value operation for dictionaries.
    /// </summary>
    NotContainsValue
}