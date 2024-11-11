using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.Json;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.Business;
using WalliCardsNet.ClassLibrary.Card;

namespace WalliCardsNet.API.Data.Repositories
{
    public class BusinessRepository : IBusiness
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public BusinessRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task AddAsync(Business business)
        {
            if (business != null)
            {
                business.UrlToken = await GenerateUrlTokenAsync();
                await _applicationDbContext.Businesses.AddAsync(business);
            }
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task<List<Business>> GetAllAsync()
        {
            var result = await _applicationDbContext.Businesses.ToListAsync();
            if (result != null)
            {
                return result;
            }
            return null;
        }

        public async Task<Business> GetByIdAsync(Guid id)
        {
            //var result = await _applicationDbContext.Businesses.FirstOrDefaultAsync(b => b.Id == id);
            var result = await _applicationDbContext.Businesses.FindAsync(id);
            if (result != null)
            {
                return result;
            }
            return null;
        }

        //Overloaded method accepting PSP ID
        public async Task<Business> GetByIdAsync(string pspId)
        {
            var result = await _applicationDbContext.Businesses.FirstOrDefaultAsync(b => b.PspId == pspId);
            if (result != null)
            {
                return result;
            }
            return null;
        }

        public async Task<bool> BusinessWithPspIdExists(string pspId)
        {
            return await _applicationDbContext.Businesses.AnyAsync(b => b.PspId == pspId);
        }

        public async Task<Business> GetByTokenAsync(string token)
        {
            var result = await _applicationDbContext.Businesses.FirstOrDefaultAsync(x => x.UrlToken == token);
            if (result != null)
            {
                return result;
            }
            return null;
        }

        public async Task AddCardDesignFieldsToColumnPresetAsync(string designJson, Guid businessId)
        {
            var business = await GetByIdAsync(businessId);
            var columnPreset = JsonSerializer.Deserialize<ColumnPreset>(business.ColumnPresetJson) ?? new ColumnPreset();
            var design = JsonSerializer.Deserialize<List<CardField>>(designJson);

            if (design != null)
            {
                foreach (var field in design)
                {
                    if (!columnPreset.VisibleColumns.Contains(field.FieldName) && !columnPreset.HiddenColumns.Contains(field.FieldName))
                    {
                        columnPreset.HiddenColumns.Add(field.FieldName);
                    }
                }
            }

            business.ColumnPresetJson = JsonSerializer.Serialize(columnPreset);
            await UpdateAsync(business);
        }

        public async Task RemoveAsync(Guid id)
        {
            var business = await GetByIdAsync(id);
            _applicationDbContext.Businesses.Remove(business);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Business business)
        {
            _applicationDbContext.Businesses.Update(business);
            await _applicationDbContext.SaveChangesAsync();
        }

        private async Task<string> GenerateUrlTokenAsync()
        {
            string token;
            do
            {
                token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(8))
                    .Replace("+", "")
                    .Replace("/", "")
                    .Substring(0, 10);
            } while (!await IsTokenUniqueAsync(token));

            return token;
        }

        private async Task<bool> IsTokenUniqueAsync(string token)
        {
            var business = await _applicationDbContext.Businesses
                .FirstOrDefaultAsync(x => x.UrlToken == token);
            return business == null; 
        }
    }
}
