﻿using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.BusinessProfile;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IBusinessProfile
    {
        public Task AddAsync(BusinessProfile businessProfile);

        public Task<List<BusinessProfile>> GetAllAsync(Guid businessId);

        public Task<BusinessProfile> GetByIdAsync(Guid id);

        public Task<BusinessProfile> GetByBusinessIdAsync(Guid businessId);
        public Task RemoveAsync(int id);

        public Task UpdateAsync(BusinessProfileRequestDTO businessProfile, Guid businessId);
        public Task SetActiveAsync(Guid id);
    }
}
