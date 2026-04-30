using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Lofn.Domain.Interfaces;
using Lofn.Domain.Models;

namespace Lofn.Domain.Services
{
    public class ProductFilterValueResolver
    {
        private readonly ICategoryService _categoryService;

        public ProductFilterValueResolver(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public class ResolveResult
        {
            public IList<ProductFilterValueModel> Resolved { get; set; } = new List<ProductFilterValueModel>();
            public IList<long> IgnoredFilterIds { get; set; } = new List<long>();
            public IList<string> MissingRequiredLabels { get; set; } = new List<string>();
            public IList<string> InvalidValueErrors { get; set; } = new List<string>();
            public long? AppliedProductTypeId { get; set; }
        }

        public async Task<ResolveResult> ResolveAsync(long? categoryId, IList<(long FilterId, string Value)> input)
        {
            var result = new ResolveResult();
            input ??= new List<(long, string)>();

            if (categoryId == null)
            {
                result.IgnoredFilterIds = input.Select(p => p.FilterId).ToList();
                return result;
            }

            var resolution = await _categoryService.GetAppliedProductTypeAsync(categoryId.Value);
            if (resolution == null)
            {
                result.IgnoredFilterIds = input.Select(p => p.FilterId).ToList();
                return result;
            }

            var type = resolution.ProductType;
            result.AppliedProductTypeId = type.ProductTypeId;
            var filtersByid = (type.Filters ?? new List<ProductTypeFilterModel>())
                .ToDictionary(f => f.FilterId);

            var inputByFilter = new Dictionary<long, string>();
            foreach (var (filterId, value) in input)
            {
                if (!filtersByid.ContainsKey(filterId))
                {
                    result.IgnoredFilterIds.Add(filterId);
                    continue;
                }
                inputByFilter[filterId] = value;
            }

            foreach (var filter in filtersByid.Values)
            {
                if (filter.IsRequired && !inputByFilter.ContainsKey(filter.FilterId))
                {
                    result.MissingRequiredLabels.Add(filter.Label);
                }
            }

            foreach (var (filterId, value) in inputByFilter)
            {
                var filter = filtersByid[filterId];
                if (!IsValueValidForType(filter, value))
                {
                    result.InvalidValueErrors.Add($"Value '{value}' is not valid for filter '{filter.Label}' (dataType={filter.DataType})");
                    continue;
                }

                result.Resolved.Add(new ProductFilterValueModel
                {
                    FilterId = filter.FilterId,
                    FilterLabel = filter.Label,
                    DataType = filter.DataType,
                    Value = value
                });
            }

            return result;
        }

        private static bool IsValueValidForType(ProductTypeFilterModel filter, string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            switch (filter.DataType)
            {
                case "text": return true;
                case "integer": return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
                case "decimal": return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _);
                case "boolean": return value == "true" || value == "false";
                case "enum":
                    return filter.AllowedValues != null && filter.AllowedValues.Contains(value);
                default: return false;
            }
        }
    }
}
