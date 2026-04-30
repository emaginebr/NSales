using Lofn.Domain.Models;
using Lofn.Infra.Context;
using Lofn.Infra.Interfaces.Repository;
using Lofn.Infra.Mappers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lofn.Infra.Repository
{
    public class ProductTypeRepository : IProductTypeRepository<
        ProductTypeModel,
        ProductTypeFilterModel,
        ProductTypeCustomizationGroupModel,
        ProductTypeCustomizationOptionModel>
    {
        private readonly LofnContext _context;

        public ProductTypeRepository(LofnContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductTypeModel>> ListAllAsync()
        {
            var rows = await _context.ProductTypes
                .Include(pt => pt.Filters).ThenInclude(f => f.AllowedValues)
                .Include(pt => pt.CustomizationGroups).ThenInclude(g => g.Options)
                .OrderBy(pt => pt.Name)
                .ToListAsync();
            return rows.Select(ProductTypeDbMapper.ToModel);
        }

        public async Task<ProductTypeModel> GetByIdAsync(long productTypeId)
        {
            var row = await _context.ProductTypes
                .Include(pt => pt.Filters).ThenInclude(f => f.AllowedValues)
                .Include(pt => pt.CustomizationGroups).ThenInclude(g => g.Options)
                .FirstOrDefaultAsync(pt => pt.ProductTypeId == productTypeId);
            return row == null ? null : ProductTypeDbMapper.ToModel(row);
        }

        public async Task<ProductTypeModel> InsertAsync(ProductTypeModel model)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            var row = new ProductType
            {
                Name = model.Name,
                Description = model.Description,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.ProductTypes.Add(row);
            await _context.SaveChangesAsync();

            await tx.CommitAsync();
            model.ProductTypeId = row.ProductTypeId;
            model.CreatedAt = row.CreatedAt;
            model.UpdatedAt = row.UpdatedAt;
            return model;
        }

        public async Task<ProductTypeModel> UpdateAsync(ProductTypeModel model)
        {
            var row = await _context.ProductTypes.FindAsync(model.ProductTypeId);
            if (row == null) return null;
            row.Name = model.Name;
            row.Description = model.Description;
            row.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            model.UpdatedAt = row.UpdatedAt;
            return model;
        }

        public async Task DeleteAsync(long productTypeId)
        {
            var row = await _context.ProductTypes.FindAsync(productTypeId);
            if (row == null) return;
            _context.ProductTypes.Remove(row);
            await _context.SaveChangesAsync();
        }

        public Task<bool> ExistsAsync(long productTypeId)
        {
            return _context.ProductTypes.AnyAsync(pt => pt.ProductTypeId == productTypeId);
        }

        public Task<bool> HasLinkedCategoriesAsync(long productTypeId)
        {
            return _context.Categories.AnyAsync(c => c.ProductTypeId == productTypeId);
        }

        public async Task<ProductTypeFilterModel> GetFilterByIdAsync(long filterId)
        {
            var row = await _context.ProductTypeFilters
                .Include(f => f.AllowedValues)
                .FirstOrDefaultAsync(f => f.FilterId == filterId);
            return row == null ? null : ProductTypeDbMapper.ToFilterModel(row);
        }

        public async Task<IEnumerable<ProductTypeFilterModel>> ListFiltersByTypeAsync(long productTypeId)
        {
            var rows = await _context.ProductTypeFilters
                .Include(f => f.AllowedValues)
                .Where(f => f.ProductTypeId == productTypeId)
                .OrderBy(f => f.DisplayOrder).ThenBy(f => f.FilterId)
                .ToListAsync();
            return rows.Select(ProductTypeDbMapper.ToFilterModel);
        }

        public async Task<ProductTypeFilterModel> InsertFilterAsync(long productTypeId, ProductTypeFilterModel filter)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            var row = new ProductTypeFilter
            {
                ProductTypeId = productTypeId,
                Label = filter.Label,
                DataType = filter.DataType,
                IsRequired = filter.IsRequired,
                DisplayOrder = filter.DisplayOrder
            };
            _context.ProductTypeFilters.Add(row);
            await _context.SaveChangesAsync();

            if (filter.AllowedValues != null)
            {
                int order = 0;
                foreach (var value in filter.AllowedValues)
                {
                    _context.ProductTypeFilterAllowedValues.Add(new ProductTypeFilterAllowedValue
                    {
                        FilterId = row.FilterId,
                        Value = value,
                        DisplayOrder = order++
                    });
                }
                await _context.SaveChangesAsync();
            }

            await tx.CommitAsync();
            filter.FilterId = row.FilterId;
            return filter;
        }

        public async Task<ProductTypeFilterModel> UpdateFilterAsync(ProductTypeFilterModel filter)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            var row = await _context.ProductTypeFilters
                .Include(f => f.AllowedValues)
                .FirstOrDefaultAsync(f => f.FilterId == filter.FilterId);
            if (row == null) return null;

            row.Label = filter.Label;
            row.IsRequired = filter.IsRequired;
            row.DisplayOrder = filter.DisplayOrder;

            _context.ProductTypeFilterAllowedValues.RemoveRange(row.AllowedValues);
            if (filter.AllowedValues != null)
            {
                int order = 0;
                foreach (var value in filter.AllowedValues)
                {
                    _context.ProductTypeFilterAllowedValues.Add(new ProductTypeFilterAllowedValue
                    {
                        FilterId = row.FilterId,
                        Value = value,
                        DisplayOrder = order++
                    });
                }
            }
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return filter;
        }

        public async Task DeleteFilterAsync(long filterId)
        {
            var row = await _context.ProductTypeFilters.FindAsync(filterId);
            if (row == null) return;
            _context.ProductTypeFilters.Remove(row);
            await _context.SaveChangesAsync();
        }

        public async Task<ProductTypeCustomizationGroupModel> GetGroupByIdAsync(long groupId)
        {
            var row = await _context.ProductTypeCustomizationGroups
                .Include(g => g.Options)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);
            return row == null ? null : ProductTypeDbMapper.ToGroupModel(row);
        }

        public async Task<IEnumerable<ProductTypeCustomizationGroupModel>> ListGroupsByTypeAsync(long productTypeId)
        {
            var rows = await _context.ProductTypeCustomizationGroups
                .Include(g => g.Options)
                .Where(g => g.ProductTypeId == productTypeId)
                .OrderBy(g => g.DisplayOrder).ThenBy(g => g.GroupId)
                .ToListAsync();
            return rows.Select(ProductTypeDbMapper.ToGroupModel);
        }

        public async Task<ProductTypeCustomizationGroupModel> InsertGroupAsync(long productTypeId, ProductTypeCustomizationGroupModel group)
        {
            var row = new ProductTypeCustomizationGroup
            {
                ProductTypeId = productTypeId,
                Label = group.Label,
                SelectionMode = group.SelectionMode,
                IsRequired = group.IsRequired,
                DisplayOrder = group.DisplayOrder
            };
            _context.ProductTypeCustomizationGroups.Add(row);
            await _context.SaveChangesAsync();
            group.GroupId = row.GroupId;
            return group;
        }

        public async Task<ProductTypeCustomizationGroupModel> UpdateGroupAsync(ProductTypeCustomizationGroupModel group)
        {
            var row = await _context.ProductTypeCustomizationGroups.FindAsync(group.GroupId);
            if (row == null) return null;
            row.Label = group.Label;
            row.SelectionMode = group.SelectionMode;
            row.IsRequired = group.IsRequired;
            row.DisplayOrder = group.DisplayOrder;
            await _context.SaveChangesAsync();
            return group;
        }

        public async Task DeleteGroupAsync(long groupId)
        {
            var row = await _context.ProductTypeCustomizationGroups.FindAsync(groupId);
            if (row == null) return;
            _context.ProductTypeCustomizationGroups.Remove(row);
            await _context.SaveChangesAsync();
        }

        public async Task<ProductTypeCustomizationOptionModel> GetOptionByIdAsync(long optionId)
        {
            var row = await _context.ProductTypeCustomizationOptions
                .FirstOrDefaultAsync(o => o.OptionId == optionId);
            return row == null ? null : ProductTypeDbMapper.ToOptionModel(row);
        }

        public async Task<ProductTypeCustomizationOptionModel> InsertOptionAsync(long groupId, ProductTypeCustomizationOptionModel option)
        {
            var row = new ProductTypeCustomizationOption
            {
                GroupId = groupId,
                Label = option.Label,
                PriceDeltaCents = option.PriceDeltaCents,
                IsDefault = option.IsDefault,
                DisplayOrder = option.DisplayOrder
            };
            _context.ProductTypeCustomizationOptions.Add(row);
            await _context.SaveChangesAsync();
            option.OptionId = row.OptionId;
            return option;
        }

        public async Task<ProductTypeCustomizationOptionModel> UpdateOptionAsync(ProductTypeCustomizationOptionModel option)
        {
            var row = await _context.ProductTypeCustomizationOptions.FindAsync(option.OptionId);
            if (row == null) return null;
            row.Label = option.Label;
            row.PriceDeltaCents = option.PriceDeltaCents;
            row.IsDefault = option.IsDefault;
            row.DisplayOrder = option.DisplayOrder;
            await _context.SaveChangesAsync();
            return option;
        }

        public async Task DeleteOptionAsync(long optionId)
        {
            var row = await _context.ProductTypeCustomizationOptions.FindAsync(optionId);
            if (row == null) return;
            _context.ProductTypeCustomizationOptions.Remove(row);
            await _context.SaveChangesAsync();
        }
    }
}
