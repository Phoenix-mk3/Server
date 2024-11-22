using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PhoenixApi.Repositories.Base;

namespace PhoenixApi.Services
{
    public interface ILookupService<T> where T : class
    {
        Task<T> FindAsync(string key);
    }
    public class LookupService<T>(IRepository<T, int> repository): ILookupService<T> where T: class
    {
        public async Task<T> FindAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("Key cannot be null or whitespace.", nameof(key));
            }

            // Check if the key can be parsed as an integer
            if (int.TryParse(key, out int intKey))
            {
                return await GetByIdAsync(intKey) ?? throw new KeyNotFoundException($"No entity found with Id = {intKey}");
            }
            else
            {
                return await GetByNameAsync(key) ?? throw new KeyNotFoundException($"No entity found with Name or ShortName = '{key}'");
            }
        }

        private async Task<T> GetByNameAsync(string key)
        {
            var entities = await repository.GetAllAsync();
            var entity = entities.FirstOrDefault(e =>
            {
                // Use reflection to get the Name property
                var nameProperty = e.GetType().GetProperty("Name");
                var nameValue = nameProperty?.GetValue(e)?.ToString();

                // Compare the Name value with the key
                return nameValue == key;
            });

            if (entity == null)
            {
                entity = entities.FirstOrDefault(e =>
                {
                    // Use reflection to get the Name property
                    var nameProperty = e.GetType().GetProperty("ShortName");
                    var nameValue = nameProperty?.GetValue(e)?.ToString();

                    // Compare the Name value with the key
                    return nameValue == key;
                });
            }
            return entity;
        }

        private async Task<T> GetByIdAsync(int intKey)
        {
            return await repository.GetByIdAsync(intKey);
        }
    }
}
