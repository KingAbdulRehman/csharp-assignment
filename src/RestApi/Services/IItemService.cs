using RestApi.Models;

namespace RestApi.Services;

public interface IItemService
{
    IEnumerable<Item> GetAll();
    Item? GetById(int id);
    Item Create(ItemRequest request);
    bool Update(int id, ItemRequest request);
    bool Delete(int id);
}
