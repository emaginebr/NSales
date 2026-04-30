using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Lofn.Domain.Interfaces;
using Lofn.Domain.Mappers;
using Lofn.Domain.Models;
using Lofn.DTO.ProductType;
using Lofn.Infra.Interfaces.Repository;

namespace Lofn.Domain.Services
{
    public class ProductTypeService : IProductTypeService
    {
        private readonly IProductTypeRepository<
            ProductTypeModel,
            ProductTypeFilterModel,
            ProductTypeCustomizationGroupModel,
            ProductTypeCustomizationOptionModel> _repository;
        private readonly IValidator<ProductTypeInsertInfo> _insertValidator;
        private readonly IValidator<ProductTypeUpdateInfo> _updateValidator;
        private readonly IValidator<ProductTypeFilterInsertInfo> _filterInsertValidator;
        private readonly IValidator<ProductTypeFilterUpdateInfo> _filterUpdateValidator;
        private readonly IValidator<CustomizationGroupInsertInfo> _groupInsertValidator;
        private readonly IValidator<CustomizationGroupUpdateInfo> _groupUpdateValidator;
        private readonly IValidator<CustomizationOptionInsertInfo> _optionInsertValidator;
        private readonly IValidator<CustomizationOptionUpdateInfo> _optionUpdateValidator;

        public ProductTypeService(
            IProductTypeRepository<
                ProductTypeModel,
                ProductTypeFilterModel,
                ProductTypeCustomizationGroupModel,
                ProductTypeCustomizationOptionModel> repository,
            IValidator<ProductTypeInsertInfo> insertValidator,
            IValidator<ProductTypeUpdateInfo> updateValidator,
            IValidator<ProductTypeFilterInsertInfo> filterInsertValidator,
            IValidator<ProductTypeFilterUpdateInfo> filterUpdateValidator,
            IValidator<CustomizationGroupInsertInfo> groupInsertValidator = null,
            IValidator<CustomizationGroupUpdateInfo> groupUpdateValidator = null,
            IValidator<CustomizationOptionInsertInfo> optionInsertValidator = null,
            IValidator<CustomizationOptionUpdateInfo> optionUpdateValidator = null)
        {
            _repository = repository;
            _insertValidator = insertValidator;
            _updateValidator = updateValidator;
            _filterInsertValidator = filterInsertValidator;
            _filterUpdateValidator = filterUpdateValidator;
            _groupInsertValidator = groupInsertValidator;
            _groupUpdateValidator = groupUpdateValidator;
            _optionInsertValidator = optionInsertValidator;
            _optionUpdateValidator = optionUpdateValidator;
        }

        public async Task<ProductTypeModel> InsertAsync(ProductTypeInsertInfo info)
        {
            await _insertValidator.ValidateAndThrowAsync(info);

            var existingByName = await FindByNameAsync(info.Name, null);
            if (existingByName != null)
                ThrowValidation("Name", "Product type name already exists");

            var model = ProductTypeMapper.ToInsertModel(info);
            return await _repository.InsertAsync(model);
        }

        public async Task<ProductTypeModel> UpdateAsync(ProductTypeUpdateInfo info)
        {
            await _updateValidator.ValidateAndThrowAsync(info);

            var existing = await _repository.GetByIdAsync(info.ProductTypeId);
            if (existing == null)
                ThrowValidation("ProductTypeId", "Product type not found");

            var nameCollision = await FindByNameAsync(info.Name, info.ProductTypeId);
            if (nameCollision != null)
                ThrowValidation("Name", "Product type name already exists");

            ProductTypeMapper.ToUpdateModel(info, existing);
            return await _repository.UpdateAsync(existing);
        }

        public Task DeleteAsync(long productTypeId)
        {
            return _repository.DeleteAsync(productTypeId);
        }

        public Task<ProductTypeModel> GetByIdAsync(long productTypeId)
        {
            return _repository.GetByIdAsync(productTypeId);
        }

        public async Task<IList<ProductTypeModel>> ListAllAsync()
        {
            var items = await _repository.ListAllAsync();
            return items.ToList();
        }

        public async Task<ProductTypeFilterModel> InsertFilterAsync(long productTypeId, ProductTypeFilterInsertInfo info)
        {
            await _filterInsertValidator.ValidateAndThrowAsync(info);

            var type = await _repository.GetByIdAsync(productTypeId);
            if (type == null)
                ThrowValidation("ProductTypeId", "Product type not found");

            if ((type.Filters ?? new List<ProductTypeFilterModel>())
                .Any(f => string.Equals(f.Label, info.Label, System.StringComparison.OrdinalIgnoreCase)))
                ThrowValidation("Label", "Filter label must be unique within the product type");

            var model = ProductTypeMapper.ToFilterInsertModel(productTypeId, info);
            return await _repository.InsertFilterAsync(productTypeId, model);
        }

        public async Task<ProductTypeFilterModel> UpdateFilterAsync(ProductTypeFilterUpdateInfo info)
        {
            await _filterUpdateValidator.ValidateAndThrowAsync(info);

            var existing = await _repository.GetFilterByIdAsync(info.FilterId);
            if (existing == null)
                ThrowValidation("FilterId", "Filter not found");

            var type = await _repository.GetByIdAsync(existing.ProductTypeId);
            if (type != null && (type.Filters ?? new List<ProductTypeFilterModel>())
                .Any(f => f.FilterId != info.FilterId
                    && string.Equals(f.Label, info.Label, System.StringComparison.OrdinalIgnoreCase)))
                ThrowValidation("Label", "Filter label must be unique within the product type");

            ProductTypeMapper.ToFilterUpdateModel(info, existing);
            return await _repository.UpdateFilterAsync(existing);
        }

        public Task DeleteFilterAsync(long filterId)
        {
            return _repository.DeleteFilterAsync(filterId);
        }

        // ----- US5 Customizations -----

        public async Task<ProductTypeCustomizationGroupModel> InsertGroupAsync(long productTypeId, CustomizationGroupInsertInfo info)
        {
            if (_groupInsertValidator != null)
                await _groupInsertValidator.ValidateAndThrowAsync(info);

            if (!await _repository.ExistsAsync(productTypeId))
                ThrowValidation("ProductTypeId", "Product type not found");

            var model = ProductTypeMapper.ToInsertGroupModel(productTypeId, info);
            return await _repository.InsertGroupAsync(productTypeId, model);
        }

        public async Task<ProductTypeCustomizationGroupModel> UpdateGroupAsync(CustomizationGroupUpdateInfo info)
        {
            if (_groupUpdateValidator != null)
                await _groupUpdateValidator.ValidateAndThrowAsync(info);

            var existing = await _repository.GetGroupByIdAsync(info.GroupId);
            if (existing == null)
                ThrowValidation("GroupId", "Customization group not found");

            // Constraint: changing multi → single must not leave > 1 default option.
            if (existing.SelectionMode == "multi" && info.SelectionMode == "single")
            {
                var defaults = (existing.Options ?? new System.Collections.Generic.List<ProductTypeCustomizationOptionModel>())
                    .Count(o => o.IsDefault);
                if (defaults > 1)
                    ThrowValidation("SelectionMode", "Cannot switch to single — group has multiple default options");
            }

            ProductTypeMapper.ToUpdateGroupModel(info, existing);
            return await _repository.UpdateGroupAsync(existing);
        }

        public Task DeleteGroupAsync(long groupId) => _repository.DeleteGroupAsync(groupId);

        public async Task<ProductTypeCustomizationOptionModel> InsertOptionAsync(long groupId, CustomizationOptionInsertInfo info)
        {
            if (_optionInsertValidator != null)
                await _optionInsertValidator.ValidateAndThrowAsync(info);

            var group = await _repository.GetGroupByIdAsync(groupId);
            if (group == null)
                ThrowValidation("GroupId", "Customization group not found");

            // Constraint: single-select group can have at most 1 default option.
            if (info.IsDefault && group.SelectionMode == "single"
                && (group.Options ?? new System.Collections.Generic.List<ProductTypeCustomizationOptionModel>()).Any(o => o.IsDefault))
            {
                ThrowValidation("IsDefault", "single-select group already has a default option");
            }

            var model = ProductTypeMapper.ToInsertOptionModel(groupId, info);
            return await _repository.InsertOptionAsync(groupId, model);
        }

        public async Task<ProductTypeCustomizationOptionModel> UpdateOptionAsync(CustomizationOptionUpdateInfo info)
        {
            if (_optionUpdateValidator != null)
                await _optionUpdateValidator.ValidateAndThrowAsync(info);

            var existing = await _repository.GetOptionByIdAsync(info.OptionId);
            if (existing == null)
                ThrowValidation("OptionId", "Customization option not found");

            if (info.IsDefault)
            {
                var group = await _repository.GetGroupByIdAsync(existing.GroupId);
                if (group != null && group.SelectionMode == "single"
                    && (group.Options ?? new System.Collections.Generic.List<ProductTypeCustomizationOptionModel>())
                        .Any(o => o.OptionId != info.OptionId && o.IsDefault))
                {
                    ThrowValidation("IsDefault", "single-select group already has a default option");
                }
            }

            ProductTypeMapper.ToUpdateOptionModel(info, existing);
            return await _repository.UpdateOptionAsync(existing);
        }

        public Task DeleteOptionAsync(long optionId) => _repository.DeleteOptionAsync(optionId);

        private async Task<ProductTypeModel> FindByNameAsync(string name, long? excludeId)
        {
            var items = await _repository.ListAllAsync();
            return items.FirstOrDefault(t =>
                string.Equals(t.Name, name, System.StringComparison.OrdinalIgnoreCase)
                && (excludeId == null || t.ProductTypeId != excludeId.Value));
        }

        private static void ThrowValidation(string property, string message)
        {
            throw new ValidationException(new[] { new ValidationFailure(property, message) });
        }
    }
}
