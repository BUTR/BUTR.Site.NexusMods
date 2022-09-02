using System;

namespace BUTR.Site.NexusMods.Server.Utils
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class MetadataAttribute : Attribute, IMetadataData
    {
        public string Key { get; }
        public string? Value { get; }

        public MetadataAttribute(string key)
        {
            Key = key;
            Value = null;
        }

        public MetadataAttribute(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}