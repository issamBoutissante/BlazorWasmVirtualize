using Blazored.LocalStorage;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static VirtualizeGrid.Client.Pages.Home;

namespace VirtualizeGrid.Client;

public class LocalDatabaseService
{
    private readonly ILocalStorageService _localStorage;
    private const string PartsStorageKey = "CachedPartsData";

    public LocalDatabaseService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    // Method to cache parts data in IndexedDB
    public async Task CachePartsDataAsync(IEnumerable<PartDto> parts)
    {

        return;
        const int chunkSize = 10_000;
        var partList = parts.ToList();

        // Remove any previous chunks
        int existingChunkCount = await _localStorage.GetItemAsync<int>("PartsChunksCount");
        for (int i = 0; i < existingChunkCount; i++)
        {
            await _localStorage.RemoveItemAsync($"PartsStorageKey_{i}");
        }

        // Save parts in chunks
        for (int i = 0; i < partList.Count; i += chunkSize)
        {
            var chunk = partList.Skip(i).Take(chunkSize).ToList();
            await _localStorage.SetItemAsync($"PartsStorageKey_{i / chunkSize}", chunk);
        }

        // Save metadata about the number of chunks
        await _localStorage.SetItemAsync("PartsChunksCount", (partList.Count + chunkSize - 1) / chunkSize);
    }


    // Method to load cached parts data from IndexedDB
    public async Task<List<PartDto>> LoadCachedPartsDataAsync()
    {
        var allParts = new List<PartDto>();

        // Get the number of saved chunks
        int chunkCount = await _localStorage.GetItemAsync<int>("PartsChunksCount");

        // Load each chunk and combine them into the final list
        for (int i = 0; i < chunkCount; i++)
        {
            var chunk = await _localStorage.GetItemAsync<List<PartDto>>($"PartsStorageKey_{i}");
            if (chunk != null)
            {
                allParts.AddRange(chunk);
            }
        }

        Debug.WriteLine($"Rows fetched: {allParts.Count}");
        return allParts;
    }

}
