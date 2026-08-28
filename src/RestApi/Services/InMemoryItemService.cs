using System.Collections.Concurrent;
using RestApi.Models;

namespace RestApi.Services;

/// <summary>
/// In-memory store. Behind IItemService so the storage layer can be
/// swapped for a database without touching the controller.
/// </summary>
public class InMemoryItemService : IItemService
{
    private readonly ConcurrentDictionary<int, Item> _items = new();
    private int _nextId;

    public IEnumerable<Item> GetAll() => _items.Values.OrderBy(i => i.Id);

    public Item? GetById(int id) => _items.TryGetValue(id, out var item) ? item : null;

    public Item Create(ItemRequest request)
    {
        var id = Interlocked.Increment(ref _nextId);
        var item = new Item { Id = id, Name = request.Name, Description = request.Description };
        _items[id] = item;
        return item;
    }

    public bool Update(int id, ItemRequest request)
    {
        if (!_items.TryGetValue(id, out var existing)) return false;
        existing.Name = request.Name;
        existing.Description = request.Description;
        return true;
    }

    public bool Delete(int id) => _items.TryRemove(id, out _);
}
