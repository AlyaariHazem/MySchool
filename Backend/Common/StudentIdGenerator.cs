using System;

namespace Backend.Common;

/// <summary>
/// Structured student IDs: YYYY (4) + SS (2, school code from <see cref="SchoolID"/>) + NNNN (4, serial).
/// Stored as a single <see cref="int"/> (e.g. 2026110001).
/// </summary>
public static class StudentIdGenerator
{
    public const int SerialDigits = 4;
    public const int MinSerial = 1;
    public const int MaxSerial = 9999;

    /// <summary>Maps school primary key to a 0–99 code used in the ID (same as 2-digit school code).</summary>
    public static int ToSchoolCodeComponent(int schoolId) => Math.Abs(schoolId) % 100;

    /// <summary>6-digit prefix as integer: year * 100 + two-digit school code (e.g. 2026 and 11 → 202611).</summary>
    public static int GetPrefixPart(int year, int schoolId)
    {
        var ss = ToSchoolCodeComponent(schoolId);
        return year * 100 + ss;
    }

    public static bool TryGetRange(int year, int schoolId, out int minId, out int maxId)
    {
        minId = 0;
        maxId = 0;
        if (year < 1 || year > 9999)
            return false;

        var prefix = GetPrefixPart(year, schoolId);
        minId = prefix * 10_000 + MinSerial;
        maxId = prefix * 10_000 + MaxSerial;
        return minId > 0 && maxId >= minId && maxId <= int.MaxValue;
    }

    /// <summary>Parses the trailing 4-digit serial from a structured ID, or 0 if out of range.</summary>
    public static int GetSerialOrZero(int studentId, int year, int schoolId)
    {
        if (!TryGetRange(year, schoolId, out var minId, out var maxId))
            return 0;
        if (studentId < minId || studentId > maxId)
            return 0;
        var serial = studentId % 10_000;
        return serial is >= MinSerial and <= MaxSerial ? serial : 0;
    }

    public static int BuildStudentId(int prefixPart, int serial) => prefixPart * 10_000 + serial;

    /// <summary>Composes the numeric ID from year, school id (same as 2-digit code source), and serial — no database access.</summary>
    public static int GenerateStudentId(int year, int schoolId, int serial)
    {
        if (serial is < MinSerial or > MaxSerial)
            throw new ArgumentOutOfRangeException(nameof(serial));
        if (!TryGetRange(year, schoolId, out var minId, out var maxId))
            throw new ArgumentOutOfRangeException(nameof(year));
        var id = GetPrefixPart(year, schoolId) * 10_000 + serial;
        if (id < minId || id > maxId)
            throw new ArgumentOutOfRangeException(nameof(serial));
        return id;
    }

    /// <summary>Best-effort decode of a structured ID; returns false for values outside the expected pattern.</summary>
    public static bool TryDecodeStructured(int studentId, out int year, out int schoolCodeComponent, out int serial)
    {
        year = 0;
        schoolCodeComponent = 0;
        serial = 0;
        if (studentId <= 0)
            return false;
        var prefix = studentId / 10_000;
        year = prefix / 100;
        schoolCodeComponent = prefix % 100;
        serial = studentId % 10_000;
        if (year is < 1 or > 9999)
            return false;
        if (serial is < MinSerial or > MaxSerial)
            return false;
        if (!TryGetRange(year, schoolCodeComponent, out var minId, out var maxId))
            return false;
        return studentId >= minId && studentId <= maxId;
    }
}
