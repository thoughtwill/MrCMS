using System;

namespace MrCMS.ContentTemplates.Helper;

public static class StringExtensions
{
    public static string ReplaceAt(this string str, int index, int length, string replace)
    {
        return string.Create(str.Length - length + replace.Length, (str, index, length, replace),
            (span, state) =>
            {
                state.str.AsSpan().Slice(0, state.index).CopyTo(span);
                state.replace.AsSpan().CopyTo(span.Slice(state.index));
                state.str.AsSpan().Slice(state.index + state.length)
                    .CopyTo(span.Slice(state.index + state.replace.Length));
            });
    }
}