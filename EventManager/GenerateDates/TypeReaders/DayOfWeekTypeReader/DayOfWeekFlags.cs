using System.Diagnostics.CodeAnalysis;

namespace EventManager.GenerateDates.TypeReaders.DayOfWeekTypeReader
{
    [Flags]
    public enum DayOfWeekFlags : int
    {
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64
    }
    public static class DayOfWeekFlagsExtensions
    {
        public static DayOfWeekFlags GetFromSearchText(string searchText)
        {
            var daysOfWeek = Enum.GetValues<DayOfWeekFlags>();
            DayOfWeekFlags target = 0;
            foreach (var dayOfWeek in daysOfWeek)
            {
                if (dayOfWeek.ToString().StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    target |= dayOfWeek;
                }
            }
            return target;
        }
        public static bool Contains(this DayOfWeekFlags flags, DayOfWeek dayOfWeek)
        {
            return ((int)flags & (1 << (int)dayOfWeek)) != 0;
        }

        public static string ToPattern(this DayOfWeekFlags flags)
        {
            char[] pattern = new char[7];
            for (int i = 0; i < pattern.Length; i++)
            {
                int index = (i + 1) % 7;
                if (((int)flags & (1 << index)) != 0)
                {
                    pattern[i] = '0';
                }
                else
                {
                    pattern[i] = '_';
                }
            }
            return new string(pattern);
        }

        public static bool TryParse(string? pattern, [NotNullWhen(true)] out DayOfWeekFlags result)
        {
            result = 0;
            if (pattern == null || pattern.Length != 7)
            {
                return false;
            }
            int parsedValue = 0;
            for (int i = 0; i < pattern.Length; i++)
            {
                int index = (i + 1) % 7;
                char current = pattern[i];
                if (current != '_')
                {
                    parsedValue += (1 << index);
                }
            }
            result = (DayOfWeekFlags)parsedValue;
            return true;
        }
    }
}
