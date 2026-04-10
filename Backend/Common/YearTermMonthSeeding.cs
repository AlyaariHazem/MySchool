using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Common;

/// <summary>
/// Ensures <see cref="Month"/> rows exist and builds default <see cref="YearTermMonth"/> rows
/// using real MonthIDs (never assumes MonthID equals calendar month number).
/// </summary>
public static class YearTermMonthSeeding
{
    private static readonly string[] CalendarMonthNames =
    {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

    /// <summary>
    /// Ensures at least 12 month catalog rows so we can map term 1 to months 5–8 and term 2 to 9–12 in calendar order (May–Dec).
    /// </summary>
    public static async Task EnsureMonthsCatalogForGradingAsync(TenantDbContext db, CancellationToken cancellationToken = default)
    {
        var count = await db.Months.CountAsync(cancellationToken);
        if (count >= 12)
            return;

        if (count == 0)
        {
            foreach (var name in CalendarMonthNames)
                db.Months.Add(new Month { Name = name });
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        var toAdd = 12 - count;
        for (var i = 0; i < toAdd; i++)
            db.Months.Add(new Month { Name = $"Month {count + i + 1}" });

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Default: term 1 → four months (May–Aug when 12 calendar months exist), term 2 → four months (Sep–Dec).
    /// </summary>
    public static async Task<List<YearTermMonth>> CreateDefaultYearTermMonthsAsync(
        TenantDbContext db,
        int yearId,
        CancellationToken cancellationToken = default)
    {
        await EnsureMonthsCatalogForGradingAsync(db, cancellationToken);

        var orderedIds = await db.Months
            .OrderBy(m => m.MonthID)
            .Select(m => m.MonthID)
            .ToListAsync(cancellationToken);

        if (orderedIds.Count < 8)
            throw new InvalidOperationException("Months catalog could not be prepared (expected at least 8 month rows).");

        IReadOnlyList<int> term1Ids;
        IReadOnlyList<int> term2Ids;
        if (orderedIds.Count >= 12)
        {
            term1Ids = orderedIds.Skip(4).Take(4).ToList();
            term2Ids = orderedIds.Skip(8).Take(4).ToList();
        }
        else
        {
            term1Ids = orderedIds.Take(4).ToList();
            term2Ids = orderedIds.Skip(4).Take(4).ToList();
        }

        var list = new List<YearTermMonth>(8);
        foreach (var mid in term1Ids)
            list.Add(new YearTermMonth { YearID = yearId, TermID = 1, MonthID = mid });
        foreach (var mid in term2Ids)
            list.Add(new YearTermMonth { YearID = yearId, TermID = 2, MonthID = mid });
        return list;
    }

    /// <summary>
    /// Month IDs for one term when <see cref="YearTermMonths"/> has no rows for that term (same split as <see cref="CreateDefaultYearTermMonthsAsync"/>).
    /// </summary>
    public static async Task<List<int>> GetDefaultMonthIdsForTermAsync(
        TenantDbContext db,
        int termId,
        CancellationToken cancellationToken = default)
    {
        await EnsureMonthsCatalogForGradingAsync(db, cancellationToken);

        var orderedIds = await db.Months
            .OrderBy(m => m.MonthID)
            .Select(m => m.MonthID)
            .ToListAsync(cancellationToken);

        if (orderedIds.Count < 8)
            return new List<int>();

        IReadOnlyList<int> term1Ids;
        IReadOnlyList<int> term2Ids;
        if (orderedIds.Count >= 12)
        {
            term1Ids = orderedIds.Skip(4).Take(4).ToList();
            term2Ids = orderedIds.Skip(8).Take(4).ToList();
        }
        else
        {
            term1Ids = orderedIds.Take(4).ToList();
            term2Ids = orderedIds.Skip(4).Take(4).ToList();
        }

        if (termId == 1)
            return term1Ids.ToList();
        if (termId == 2)
            return term2Ids.ToList();
        return new List<int>();
    }
}
