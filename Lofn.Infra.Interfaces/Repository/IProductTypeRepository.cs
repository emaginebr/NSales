using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.Infra.Interfaces.Repository
{
    public interface IProductTypeRepository<TProductTypeModel, TFilterModel, TGroupModel, TOptionModel>
        where TProductTypeModel : class
        where TFilterModel : class
        where TGroupModel : class
        where TOptionModel : class
    {
        Task<IEnumerable<TProductTypeModel>> ListAllAsync();
        Task<TProductTypeModel> GetByIdAsync(long productTypeId);
        Task<TProductTypeModel> InsertAsync(TProductTypeModel model);
        Task<TProductTypeModel> UpdateAsync(TProductTypeModel model);
        Task DeleteAsync(long productTypeId);
        Task<bool> ExistsAsync(long productTypeId);
        Task<bool> HasLinkedCategoriesAsync(long productTypeId);

        // Filter operations
        Task<TFilterModel> GetFilterByIdAsync(long filterId);
        Task<IEnumerable<TFilterModel>> ListFiltersByTypeAsync(long productTypeId);
        Task<TFilterModel> InsertFilterAsync(long productTypeId, TFilterModel filter);
        Task<TFilterModel> UpdateFilterAsync(TFilterModel filter);
        Task DeleteFilterAsync(long filterId);

        // Customization group operations
        Task<TGroupModel> GetGroupByIdAsync(long groupId);
        Task<IEnumerable<TGroupModel>> ListGroupsByTypeAsync(long productTypeId);
        Task<TGroupModel> InsertGroupAsync(long productTypeId, TGroupModel group);
        Task<TGroupModel> UpdateGroupAsync(TGroupModel group);
        Task DeleteGroupAsync(long groupId);

        // Customization option operations
        Task<TOptionModel> GetOptionByIdAsync(long optionId);
        Task<TOptionModel> InsertOptionAsync(long groupId, TOptionModel option);
        Task<TOptionModel> UpdateOptionAsync(TOptionModel option);
        Task DeleteOptionAsync(long optionId);
    }
}
