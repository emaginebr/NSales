using Lofn.Infra.Interfaces.Repository;
using Lofn.Domain.Core;
using Lofn.Domain.Mappers;
using Lofn.Domain.Models;
using Lofn.Domain.Interfaces;
using Lofn.DTO.Category;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Lofn.Domain.Services
{
    public class CategoryService : ICategoryService
    {
        private const int MaxDepth = 5;

        private readonly ISlugGenerator _slugGenerator;
        private readonly ICategoryRepository<CategoryModel> _categoryRepository;
        private readonly IStoreRepository<StoreModel> _storeRepository;
        private readonly IValidator<CategoryInsertInfo> _insertValidator;
        private readonly IValidator<CategoryUpdateInfo> _updateValidator;
        private readonly IValidator<CategoryGlobalInsertInfo> _globalInsertValidator;
        private readonly IValidator<CategoryGlobalUpdateInfo> _globalUpdateValidator;

        public CategoryService(
            ISlugGenerator slugGenerator,
            ICategoryRepository<CategoryModel> categoryRepository,
            IStoreRepository<StoreModel> storeRepository,
            IValidator<CategoryInsertInfo> insertValidator,
            IValidator<CategoryUpdateInfo> updateValidator,
            IValidator<CategoryGlobalInsertInfo> globalInsertValidator,
            IValidator<CategoryGlobalUpdateInfo> globalUpdateValidator
        )
        {
            _slugGenerator = slugGenerator;
            _categoryRepository = categoryRepository;
            _storeRepository = storeRepository;
            _insertValidator = insertValidator;
            _updateValidator = updateValidator;
            _globalInsertValidator = globalInsertValidator;
            _globalUpdateValidator = globalUpdateValidator;
        }

        public async Task<IList<CategoryInfo>> ListAllAsync()
        {
            var items = await _categoryRepository.ListAllAsync();
            return items.Select(CategoryMapper.ToInfo).ToList();
        }

        public async Task<IList<CategoryInfo>> ListByStoreAsync(long storeId)
        {
            var items = await _categoryRepository.ListByStoreAsync(storeId);
            return items.Select(CategoryMapper.ToInfo).ToList();
        }

        public async Task<IList<CategoryInfo>> ListGlobalAsync()
        {
            var items = await _categoryRepository.ListGlobalAsync();
            return items.Select(CategoryMapper.ToInfo).ToList();
        }

        public async Task<IList<CategoryInfo>> ListActiveByStoreSlugAsync(string storeSlug)
        {
            var store = await _storeRepository.GetBySlugAsync(storeSlug);
            if (store == null)
                throw new Exception("Store not found");

            var items = await _categoryRepository.ListByStoreAsync(store.StoreId);
            var counts = await _categoryRepository.CountActiveProductsByStoreAsync(store.StoreId);

            return items
                .Where(x => counts.ContainsKey(x.CategoryId) && counts[x.CategoryId] > 0)
                .Select(x =>
                {
                    var info = CategoryMapper.ToInfo(x);
                    info.ProductCount = counts[x.CategoryId];
                    return info;
                }).ToList();
        }

        public async Task<CategoryInfo> GetBySlugAndStoreSlugAsync(string storeSlug, string categorySlug)
        {
            var store = await _storeRepository.GetBySlugAsync(storeSlug);
            if (store == null)
                throw new Exception("Store not found");

            var model = await _categoryRepository.GetBySlugAndStoreAsync(store.StoreId, categorySlug);
            if (model == null)
                return null;

            return CategoryMapper.ToInfo(model);
        }

        public async Task<IList<CategoryInfo>> ListWithProductCountAsync()
        {
            var items = await _categoryRepository.ListAllAsync();
            var counts = await _categoryRepository.CountProductsByCategoryAsync();
            return items.Select(x =>
            {
                var info = CategoryMapper.ToInfo(x);
                info.ProductCount = counts.ContainsKey(x.CategoryId) ? counts[x.CategoryId] : 0;
                return info;
            }).ToList();
        }

        private async Task ValidateStoreOwnerAsync(long storeId, long userId)
        {
            if (storeId <= 0)
                throw new Exception("StoreId is required");

            var store = await _storeRepository.GetByIdAsync(storeId);
            if (store == null)
                throw new Exception("Store not found");

            if (store.OwnerId != userId)
                throw new UnauthorizedAccessException("Access denied: user is not the owner of this store");
        }

        public async Task<CategoryModel> GetByIdAsync(long categoryId, long storeId, long userId)
        {
            await ValidateStoreOwnerAsync(storeId, userId);

            var model = await _categoryRepository.GetByIdAsync(categoryId);
            if (model == null)
                return null;

            if (model.StoreId != storeId)
                throw new UnauthorizedAccessException("Access denied: category does not belong to this store");

            return model;
        }

        // ----- Parent / hierarchy helpers (002-category-subcategories) -----

        private async Task<CategoryModel> AssertParentExistsAsync(long parentId, long? expectedStoreId)
        {
            var parent = await _categoryRepository.GetByIdAsync(parentId);
            if (parent == null)
                throw BuildValidationException($"Parent category {parentId} not found");

            if (parent.StoreId != expectedStoreId)
                throw BuildValidationException("Parent and child must share the same scope");

            return parent;
        }

        private async Task AssertNoCycleAsync(long? movingCategoryId, long prospectiveParentId)
        {
            if (!movingCategoryId.HasValue) return;

            // Walk ancestors of prospective parent. If we hit movingCategoryId, the move would create a cycle.
            var chain = await _categoryRepository.GetAncestorChainAsync(prospectiveParentId);
            if (chain.Any(c => c.CategoryId == movingCategoryId.Value))
                throw BuildValidationException($"Setting parent {prospectiveParentId} would create a cycle");
        }

        private async Task AssertDepthOkAsync(long? movingCategoryId, long prospectiveParentId)
        {
            var chain = await _categoryRepository.GetAncestorChainAsync(prospectiveParentId);
            // chain includes the parent itself plus its ancestors. New child depth = chain.Count + 1.
            // Plus, if we are moving a node that has descendants, we must also account for the deepest descendant.
            var childDepth = chain.Count + 1;

            if (movingCategoryId.HasValue)
            {
                var descendants = await _categoryRepository.GetDescendantsAsync(movingCategoryId.Value);
                var maxDescendantDepthFromMovingNode = descendants.Count == 0
                    ? 0
                    : await ComputeMaxDescendantHopsAsync(movingCategoryId.Value);
                childDepth += maxDescendantDepthFromMovingNode;
            }

            if (childDepth > MaxDepth)
                throw BuildValidationException($"Maximum nesting depth ({MaxDepth}) would be exceeded");
        }

        private async Task<int> ComputeMaxDescendantHopsAsync(long categoryId)
        {
            // Returns the deepest descendant chain length below `categoryId` (0 = no descendants).
            var hops = 0;
            var frontier = new List<long> { categoryId };
            while (frontier.Count > 0)
            {
                var nextLevel = new List<long>();
                foreach (var id in frontier)
                {
                    var children = await _categoryRepository.GetDescendantsAsync(id)
                                   ?? new List<CategoryModel>();
                    nextLevel.AddRange(children
                        .Where(c => c.ParentId == id)
                        .Select(c => c.CategoryId));
                }
                if (nextLevel.Count == 0) break;
                hops++;
                frontier = nextLevel;
            }
            return hops;
        }

        private async Task AssertSiblingNameAvailableAsync(long? parentId, long? storeId, string name, long? excludeCategoryId)
        {
            var clash = await _categoryRepository.ExistSiblingNameAsync(parentId, storeId, name, excludeCategoryId);
            if (clash)
                throw BuildValidationException($"A category named \"{name}\" already exists under this parent");
        }

        private async Task<string> ComputeFullSlugAsync(long? parentId, string name, long? exceptCategoryId)
        {
            var segment = _slugGenerator.Generate(name);
            if (!parentId.HasValue)
            {
                return await ResolveUniqueSlugAsync(exceptCategoryId, segment);
            }

            var parent = await _categoryRepository.GetByIdAsync(parentId.Value)
                ?? throw BuildValidationException($"Parent category {parentId.Value} not found");

            var basePath = $"{parent.Slug}/{segment}";
            return await ResolveUniqueSlugAsync(exceptCategoryId, basePath);
        }

        private async Task<string> ResolveUniqueSlugAsync(long? exceptCategoryId, string baseSlug)
        {
            var candidate = baseSlug;
            var counter = 1;
            while (await _categoryRepository.ExistSlugInTenantAsync(exceptCategoryId, candidate))
            {
                candidate = $"{baseSlug}-{counter}";
                counter++;
            }
            return candidate;
        }

        // Legacy helper retained for backwards compat (used by older insert flow before parent support).
        private async Task<string> GenerateSlugAsync(long? exceptCategoryId, string name)
        {
            return await ComputeFullSlugAsync(null, name, exceptCategoryId);
        }

        // ----- Mutating operations: store-scoped -----

        public async Task<CategoryModel> InsertAsync(CategoryInsertInfo category, long storeId, long userId)
        {
            await _insertValidator.ValidateAndThrowAsync(category);
            await ValidateStoreOwnerAsync(storeId, userId);

            long? parentId = null;
            if (category.ParentCategoryId.HasValue)
            {
                var parent = await AssertParentExistsAsync(category.ParentCategoryId.Value, storeId);
                await AssertDepthOkAsync(null, parent.CategoryId);
                parentId = parent.CategoryId;
            }

            await AssertSiblingNameAvailableAsync(parentId, storeId, category.Name, null);

            var model = new CategoryModel
            {
                Name = category.Name,
                StoreId = storeId,
                ParentId = parentId
            };
            model.Slug = await ComputeFullSlugAsync(parentId, category.Name, null);

            return await _categoryRepository.InsertAsync(model);
        }

        public async Task<CategoryModel> UpdateAsync(CategoryUpdateInfo category, long storeId, long userId)
        {
            await _updateValidator.ValidateAndThrowAsync(category);
            await ValidateStoreOwnerAsync(storeId, userId);

            var existing = await _categoryRepository.GetByIdAsync(category.CategoryId)
                ?? throw new Exception("Category not found");

            if (existing.StoreId != storeId)
                throw new UnauthorizedAccessException("Access denied: category does not belong to this store");

            long? newParentId = category.ParentCategoryId;
            if (newParentId.HasValue)
            {
                var parent = await AssertParentExistsAsync(newParentId.Value, storeId);
                await AssertNoCycleAsync(existing.CategoryId, parent.CategoryId);
                await AssertDepthOkAsync(existing.CategoryId, parent.CategoryId);
                newParentId = parent.CategoryId;
            }

            var nameChanged = !string.Equals(existing.Name, category.Name, StringComparison.Ordinal);
            var parentChanged = existing.ParentId != newParentId;

            if (nameChanged || parentChanged)
            {
                await AssertSiblingNameAvailableAsync(newParentId, storeId, category.Name, existing.CategoryId);
            }

            existing.Name = category.Name;
            existing.ParentId = newParentId;
            existing.Slug = await ComputeFullSlugAsync(newParentId, category.Name, existing.CategoryId);

            var updated = await _categoryRepository.UpdateAsync(existing);

            if (parentChanged || nameChanged)
            {
                await RecomputeSlugCascadeAsync(existing);
            }

            return updated;
        }

        public async Task DeleteAsync(long categoryId, long storeId, long userId)
        {
            await ValidateStoreOwnerAsync(storeId, userId);

            var model = await _categoryRepository.GetByIdAsync(categoryId)
                ?? throw new Exception("Category not found");

            if (model.StoreId != storeId)
                throw new UnauthorizedAccessException("Access denied: category does not belong to this store");

            if (await _categoryRepository.HasChildrenAsync(categoryId))
                throw BuildValidationException($"Category {categoryId} has subcategories; remove them first");

            await _categoryRepository.DeleteAsync(categoryId);
        }

        // ----- Mutating operations: global (marketplace) -----

        public async Task<CategoryInfo> InsertGlobalAsync(CategoryGlobalInsertInfo category)
        {
            await _globalInsertValidator.ValidateAndThrowAsync(category);

            long? parentId = null;
            if (category.ParentCategoryId.HasValue)
            {
                var parent = await AssertParentExistsAsync(category.ParentCategoryId.Value, null);
                await AssertDepthOkAsync(null, parent.CategoryId);
                parentId = parent.CategoryId;
            }

            await AssertSiblingNameAvailableAsync(parentId, null, category.Name, null);

            var model = new CategoryModel
            {
                Name = category.Name,
                StoreId = null,
                ParentId = parentId
            };
            model.Slug = await ComputeFullSlugAsync(parentId, category.Name, null);

            var inserted = await _categoryRepository.InsertAsync(model);
            return CategoryMapper.ToInfo(inserted);
        }

        public async Task<CategoryInfo> UpdateGlobalAsync(CategoryGlobalUpdateInfo category)
        {
            await _globalUpdateValidator.ValidateAndThrowAsync(category);

            var existing = await _categoryRepository.GetByIdAsync(category.CategoryId)
                ?? throw BuildValidationException($"Category {category.CategoryId} not found");

            if (existing.StoreId != null)
                throw BuildValidationException($"Category {category.CategoryId} is not global");

            long? newParentId = category.ParentCategoryId;
            if (newParentId.HasValue)
            {
                var parent = await AssertParentExistsAsync(newParentId.Value, null);
                await AssertNoCycleAsync(existing.CategoryId, parent.CategoryId);
                await AssertDepthOkAsync(existing.CategoryId, parent.CategoryId);
                newParentId = parent.CategoryId;
            }

            var nameChanged = !string.Equals(existing.Name, category.Name, StringComparison.Ordinal);
            var parentChanged = existing.ParentId != newParentId;

            if (nameChanged || parentChanged)
            {
                await AssertSiblingNameAvailableAsync(newParentId, null, category.Name, existing.CategoryId);
            }

            existing.Name = category.Name;
            existing.ParentId = newParentId;
            existing.Slug = await ComputeFullSlugAsync(newParentId, category.Name, existing.CategoryId);

            var updated = await _categoryRepository.UpdateAsync(existing);

            if (parentChanged || nameChanged)
            {
                await RecomputeSlugCascadeAsync(existing);
            }

            return CategoryMapper.ToInfo(updated);
        }

        public async Task DeleteGlobalAsync(long categoryId)
        {
            var existing = await _categoryRepository.GetByIdAsync(categoryId)
                ?? throw BuildValidationException($"Category {categoryId} not found");

            if (existing.StoreId != null)
                throw BuildValidationException($"Category {categoryId} is not global");

            if (await _categoryRepository.HasChildrenAsync(categoryId))
                throw BuildValidationException($"Category {categoryId} has subcategories; remove them first");

            await _categoryRepository.DeleteAsync(categoryId);
        }

        // ----- Tree assembly (US2) -----

        public async Task<IList<CategoryTreeNodeInfo>> GetTreeAsync(long? storeId)
        {
            var rows = await _categoryRepository.ListByScopeAsync(storeId);
            var byParent = rows.GroupBy(r => r.ParentId)
                .ToDictionary(g => g.Key ?? 0L, g => g.ToList());

            var roots = rows.Where(r => r.ParentId == null).ToList();
            return BuildLevel(roots, byParent);
        }

        private static IList<CategoryTreeNodeInfo> BuildLevel(
            IEnumerable<CategoryModel> level,
            IDictionary<long, List<CategoryModel>> byParent)
        {
            var comparer = StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);

            return level
                .OrderBy(c => c.Name, comparer)
                .Select(c => new CategoryTreeNodeInfo
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Slug = c.Slug,
                    ParentCategoryId = c.ParentId,
                    IsGlobal = c.StoreId == null,
                    Children = byParent.TryGetValue(c.CategoryId, out var children)
                        ? BuildLevel(children, byParent)
                        : new List<CategoryTreeNodeInfo>()
                })
                .ToList();
        }

        // ----- Cascade slug recompute (US3) -----

        private async Task RecomputeSlugCascadeAsync(CategoryModel root)
        {
            var descendants = await _categoryRepository.GetDescendantsAsync(root.CategoryId)
                              ?? new List<CategoryModel>();
            if (descendants.Count == 0) return;

            var slugByCategoryId = new Dictionary<long, string> { [root.CategoryId] = root.Slug };
            var ordered = TopologicalOrderByParent(descendants);

            var rewritten = new List<CategoryModel>();
            foreach (var node in ordered)
            {
                if (!node.ParentId.HasValue)
                    continue;

                if (!slugByCategoryId.TryGetValue(node.ParentId.Value, out var parentSlug))
                {
                    var parent = await _categoryRepository.GetByIdAsync(node.ParentId.Value);
                    if (parent == null) continue;
                    parentSlug = parent.Slug;
                }

                var segment = _slugGenerator.Generate(node.Name);
                var newSlug = $"{parentSlug}/{segment}";
                slugByCategoryId[node.CategoryId] = newSlug;
                node.Slug = newSlug;
                rewritten.Add(node);
            }

            if (rewritten.Count > 0)
            {
                await _categoryRepository.UpdateManyAsync(rewritten);
            }
        }

        private static IList<CategoryModel> TopologicalOrderByParent(IList<CategoryModel> nodes)
        {
            // Order so parents always precede children. Since BFS from GetDescendantsAsync already
            // emits in level order, we can rely on the natural order — but to be safe we explicitly
            // sort by depth using a parent dictionary.
            var byParent = nodes.GroupBy(n => n.ParentId ?? 0L)
                .ToDictionary(g => g.Key, g => g.ToList());

            var ordered = new List<CategoryModel>();
            var visited = new HashSet<long>();

            void Visit(long parentId)
            {
                if (!byParent.TryGetValue(parentId, out var children)) return;
                foreach (var child in children)
                {
                    if (!visited.Add(child.CategoryId)) continue;
                    ordered.Add(child);
                    Visit(child.CategoryId);
                }
            }

            // Start from each unique parent id present in the descendants set
            var seedParents = nodes.Select(n => n.ParentId ?? 0L).Distinct().ToList();
            foreach (var p in seedParents)
            {
                Visit(p);
            }
            // Fallback: append any orphans not yet visited
            foreach (var n in nodes)
            {
                if (visited.Add(n.CategoryId))
                    ordered.Add(n);
            }
            return ordered;
        }

        private static ValidationException BuildValidationException(string message)
        {
            return new ValidationException(message, new[] { new ValidationFailure(string.Empty, message) });
        }
    }
}
