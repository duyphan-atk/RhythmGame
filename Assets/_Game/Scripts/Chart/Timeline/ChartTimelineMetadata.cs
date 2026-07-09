using System;
using System.Globalization;
using System.Linq;

public static class ChartTimelineMetadata
{
    private const char SectionSeparator = '|';
    private const char ValueSeparator = ',';

    public static string Encode(NoteData note)
    {
        string flick = note.flickDirection.ToString();
        string slidePath = note.slidePath == null || note.slidePath.Length == 0
            ? string.Empty
            : string.Join(ValueSeparator.ToString(), note.slidePath);

        return string.Join(
            SectionSeparator.ToString(),
            flick,
            slidePath);
    }

    public static void Decode(string value, NoteData note)
    {
        if (note == null || string.IsNullOrEmpty(value))
            return;

        string[] sections = value.Split(SectionSeparator);

        if (sections.Length > 0 &&
            Enum.TryParse(sections[0], out FlickDirection flickDirection))
        {
            note.flickDirection = flickDirection;
        }

        if (sections.Length <= 1 || string.IsNullOrWhiteSpace(sections[1]))
            return;

        note.slidePath = sections[1]
            .Split(ValueSeparator)
            .Select(ParseLane)
            .ToArray();
    }

    private static int ParseLane(string value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int lane)
            ? lane
            : 0;
    }
}
