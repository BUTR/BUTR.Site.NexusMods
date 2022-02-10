using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BUTR.Site.NexusMods.Shared.Extensions
{
    public static class EnumExtension
    {
        public static string GetDisplayName<T>(this T @enum) where T : Enum
        {
            var enumStr = @enum.ToString();
            var memberInfo = typeof(T).GetMember(enumStr);
            return memberInfo.Length > 0 && memberInfo[0].GetCustomAttribute<DisplayAttribute>(false) is { } attr
                ? attr.Name ?? string.Empty
                : enumStr;
        }
        public static string GetDisplayDescription<T>(this T @enum) where T : Enum
        {
            var enumStr = @enum.ToString();
            var memberInfo = typeof(T).GetMember(enumStr);
            return memberInfo.Length > 0 && memberInfo[0].GetCustomAttribute<DisplayAttribute>(false) is { } attr
                ? attr.Description ?? string.Empty
                : enumStr;
        }
        public static string GetDisplayGroupName<T>(this T @enum) where T : Enum
        {
            var enumStr = @enum.ToString();
            var memberInfo = typeof(T).GetMember(enumStr);
            return memberInfo.Length > 0 && memberInfo[0].GetCustomAttribute<DisplayAttribute>(false) is { } attr
                ? attr.GroupName ?? string.Empty
                : enumStr;
        }
        public static string GetDisplayPrompt<T>(this T @enum) where T : Enum
        {
            var enumStr = @enum.ToString();
            var memberInfo = typeof(T).GetMember(enumStr);
            return memberInfo.Length > 0 && memberInfo[0].GetCustomAttribute<DisplayAttribute>(false) is { } attr
                ? attr.Prompt ?? string.Empty
                : enumStr;
        }
        public static string GetDisplayShortName<T>(this T @enum) where T : Enum
        {
            var enumStr = @enum.ToString();
            var memberInfo = typeof(T).GetMember(enumStr);
            return memberInfo.Length > 0 && memberInfo[0].GetCustomAttribute<DisplayAttribute>(false) is { } attr
                ? attr.ShortName ?? string.Empty
                : enumStr;
        }
    }
}