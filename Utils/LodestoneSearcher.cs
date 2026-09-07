using System.Collections.Immutable;
using System.Globalization;
using System.Net.Http;
using AngleSharp.Html.Parser;

namespace OmenTools.Utils;

public sealed class LodestoneSearcher(HttpClient client, CancellationToken cancellationToken)
{
    private ImmutableDictionary<(string Name, string World), (Task<string?> Task, DateTimeOffset Expires)> queries =
        ImmutableDictionary<(string, string), (Task<string?>, DateTimeOffset)>.Empty;

    public async Task<string?> GetCharacterIDAsync(string name, string world)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(world);
        cancellationToken.ThrowIfCancellationRequested();

        var key = (name.ToUpperInvariant(), world.ToUpperInvariant());
        var completion = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

        while (true)
        {
            var snapshot = Volatile.Read(ref queries);
            if (snapshot.TryGetValue(key, out var existing) &&
                (!existing.Task.IsCompleted || existing.Expires > DateTimeOffset.UtcNow))
                return await existing.Task.ConfigureAwait(false);

            var now = DateTimeOffset.UtcNow;
            var updated = snapshot.RemoveRange(snapshot.Where(x => x.Value.Task.IsCompleted && x.Value.Expires <= now).Select(x => x.Key))
                                  .SetItem(key, (completion.Task, now.AddMinutes(5)));
            if (ReferenceEquals(Interlocked.CompareExchange(ref queries, updated, snapshot), snapshot))
                break;
        }

        string? characterID = null;

        try
        {
            characterID = await SearchCharacterIDAsync(name, world).ConfigureAwait(false);
            completion.SetResult(characterID);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            completion.SetCanceled(cancellationToken);
        }
        catch (Exception exception)
        {
            completion.SetException(exception);
        }
        finally
        {
            if (characterID == null)
                ImmutableInterlocked.Update
                (
                    ref queries,
                    current => current.TryGetValue(key, out var entry) && ReferenceEquals(entry.Task, completion.Task)
                                   ? current.Remove(key)
                                   : current
                );
        }

        return await completion.Task.ConfigureAwait(false);
    }

    private async Task<string?> SearchCharacterIDAsync(string name, string world)
    {
        var searchURL = $"https://na.finalfantasyxiv.com/lodestone/character/?q={Uri.EscapeDataString(name)}&worldname={Uri.EscapeDataString(world)}";

        for (var page = 1; ; page++)
        {
            var html = await client.GetStringAsync($"{searchURL}&page={page}", cancellationToken).ConfigureAwait(false);
            using var document = await new HtmlParser().ParseDocumentAsync(html, cancellationToken).ConfigureAwait(false);

            if (document.QuerySelector(".parts__total") == null)
                throw new InvalidDataException("Lodestone returned an unexpected character search page.");

            foreach (var entry in document.QuerySelectorAll(".entry > a.entry__link"))
            {
                var characterName = entry.QuerySelector(".entry__name")?.TextContent.Trim();
                var characterWorld = entry.QuerySelector(".entry__world")?.TextContent.Split('[')[0].Trim();
                if (!string.Equals(characterName, name, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(characterWorld, world, StringComparison.OrdinalIgnoreCase))
                    continue;

                var path = entry.GetAttribute("href")?.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (path is not ["lodestone", "character", var id] ||
                    !uint.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out var numericID) || numericID == 0)
                    throw new InvalidDataException("Lodestone returned an invalid character ID.");

                return id;
            }

            if (document.QuerySelector("a.btn__pager__next:not(.btn__pager__no)") == null)
                return null;

            await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        }
    }
}
