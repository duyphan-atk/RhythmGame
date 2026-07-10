using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

public static class ChartTimelineMetadata
{
    private const char SectionSeparator = '|';
    private const char ValueSeparator = ',';

    public static string Encode(NoteData note)
    {
        string type = note.type.ToString();
        string duration = note.duration.ToString(CultureInfo.InvariantCulture);
        string flick = note.flickDirection.ToString();
        string slidePath = note.slidePath == null || note.slidePath.Length == 0
            ? string.Empty
            : string.Join(ValueSeparator.ToString(), note.slidePath);

        return string.Join(
            SectionSeparator.ToString(),
            type,
            duration,
            flick,
            slidePath);
    }

    public static void Decode(string value, NoteData note)
    {
        if (note == null || string.IsNullOrEmpty(value))
            return;

        string[] sections = value.Split(SectionSeparator);
        int index = 0;

        if (sections.Length > index &&
            Enum.TryParse(sections[index], out NoteType noteType))
        {
            note.type = noteType;
            index++;
        }

        if (sections.Length > index &&
            float.TryParse(sections[index], NumberStyles.Float, CultureInfo.InvariantCulture, out float duration))
        {
            if (note.duration <= 0f)
                note.duration = Mathf.Max(0f, duration);

            index++;
        }

        if (sections.Length > index &&
            Enum.TryParse(sections[index], out FlickDirection flickDirection))
        {
            note.flickDirection = flickDirection;
            index++;
        }

        if (sections.Length <= index || string.IsNullOrWhiteSpace(sections[index]))
            return;

        note.slidePath = sections[index]
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
