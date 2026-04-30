using System.Collections.Generic;
using System.Threading.Tasks;
using Lofn.Domain.Models;
using Lofn.DTO.ProductType;

namespace Lofn.Domain.Interfaces
{
    public interface IProductTypeService
    {
        Task<ProductTypeModel> InsertAsync(ProductTypeInsertInfo info);
        Task<ProductTypeModel> UpdateAsync(ProductTypeUpdateInfo info);
        Task DeleteAsync(long productTypeId);
        Task<ProductTypeModel> GetByIdAsync(long productTypeId);
        Task<IList<ProductTypeModel>> ListAllAsync();

        Task<ProductTypeFilterModel> InsertFilterAsync(long productTypeId, ProductTypeFilterInsertInfo info);
        Task<ProductTypeFilterModel> UpdateFilterAsync(ProductTypeFilterUpdateInfo info);
        Task DeleteFilterAsync(long filterId);

        // US5 Customizations
        Task<ProductTypeCustomizationGroupModel> InsertGroupAsync(long productTypeId, CustomizationGroupInsertInfo info);
        Task<ProductTypeCustomizationGroupModel> UpdateGroupAsync(CustomizationGroupUpdateInfo info);
        Task DeleteGroupAsync(long groupId);

        Task<ProductTypeCustomizationOptionModel> InsertOptionAsync(long groupId, CustomizationOptionInsertInfo info);
        Task<ProductTypeCustomizationOptionModel> UpdateOptionAsync(CustomizationOptionUpdateInfo info);
        Task DeleteOptionAsync(long optionId);
    }
}
