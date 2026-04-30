using FluentValidation;
using FluentValidation.Results;
using Lofn.Domain.Core;
using Lofn.Domain.Models;
using Lofn.Domain.Services;
using Lofn.DTO.Category;
using Lofn.Infra.Interfaces.Repository;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Lofn.Tests.Domain.Services
{
    /// <summary>
    /// Tests for the 002-category-subcategories hierarchy support: parent guards,
    /// tree assembly, slug-cascade on rename / move.
    /// </summary>
    public class CategoryServiceHierarchyTest
    {
        private readonly Mock<ISlugGenerator> _slugGeneratorMock = new();
        private readonly Mock<ICategoryRepository<CategoryModel>> _categoryRepositoryMock = new();
        private readonly Mock<IStoreRepository<StoreModel>> _storeRepositoryMock = new();
        private readonly Mock<IProductTypeRepository<ProductTypeModel, ProductTypeFilterModel, ProductTypeCustomizationGroupModel, ProductTypeCustomizationOptionModel>> _productTypeRepositoryMock = new();
        private readonly Mock<IValidator<CategoryInsertInfo>> _insertValidatorMock = new();
        private readonly Mock<IValidator<CategoryUpdateInfo>> _updateValidatorMock = new();
        private readonly Mock<IValidator<CategoryGlobalInsertInfo>> _globalInsertValidatorMock = new();
        private readonly Mock<IValidator<CategoryGlobalUpdateInfo>> _globalUpdateValidatorMock = new();
        private readonly CategoryService _sut;

        private readonly StoreModel _store = new() { StoreId = 1, OwnerId = 1, Name = "Loja", Slug = "loja" };

        public CategoryServiceHierarchyTest()
        {
            _insertValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryInsertInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _updateValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryUpdateInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _globalInsertValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryGlobalInsertInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _globalUpdateValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryGlobalUpdateInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _storeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(_store);

            _slugGeneratorMock
                .Setup(x => x.Generate(It.IsAny<string>()))
                .Returns((string s) => (s ?? string.Empty)
                    .ToLowerInvariant()
                    .Replace(" ", "-")
                    .Replace("á", "a").Replace("ã", "a").Replace("â", "a").Replace("à", "a")
                    .Replace("é", "e").Replace("ê", "e")
                    .Replace("í", "i")
                    .Replace("ó", "o").Replace("ô", "o").Replace("õ", "o")
                    .Replace("ú", "u").Replace("ü", "u")
                    .Replace("ç", "c"));

            _sut = new CategoryService(
                _slugGeneratorMock.Object,
                _categoryRepositoryMock.Object,
                _storeRepositoryMock.Object,
                _productTypeRepositoryMock.Object,
                _insertValidatorMock.Object,
                _updateValidatorMock.Object,
                _globalInsertValidatorMock.Object,
                _globalUpdateValidatorMock.Object);
        }

        // ---------- US1: insert with parent ----------

        [Fact]
        public async Task InsertAsync_ShouldRejectParentNotFound()
        {
            var info = new CategoryInsertInfo { Name = "Sub", ParentCategoryId = 99 };
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((CategoryModel)null);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.InsertAsync(info, 1, 1));
            Assert.Contains("Parent category 99 not found", ex.Message);
        }

        [Fact]
        public async Task InsertAsync_ShouldRejectScopeMismatch_WhenParentIsGlobal()
        {
            var info = new CategoryInsertInfo { Name = "Sub", ParentCategoryId = 7 };
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(7))
                .ReturnsAsync(new CategoryModel { CategoryId = 7, StoreId = null, Name = "Global", Slug = "global" });

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.InsertAsync(info, 1, 1));
            Assert.Contains("same scope", ex.Message);
        }

        [Fact]
        public async Task InsertAsync_ShouldRejectSiblingNameCollision()
        {
            var info = new CategoryInsertInfo { Name = "Calças" };
            _categoryRepositoryMock
                .Setup(x => x.ExistSiblingNameAsync(null, 1, "Calças", null))
                .ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.InsertAsync(info, 1, 1));
            Assert.Contains("already exists under this parent", ex.Message);
        }

        [Fact]
        public async Task InsertAsync_ShouldComputePathSlug_FromParent()
        {
            var info = new CategoryInsertInfo { Name = "Camisetas", ParentCategoryId = 5 };
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(5))
                .ReturnsAsync(new CategoryModel { CategoryId = 5, StoreId = 1, Name = "Roupas", Slug = "roupas" });
            _categoryRepositoryMock.Setup(x => x.GetAncestorChainAsync(5))
                .ReturnsAsync(new List<CategoryModel>
                {
                    new() { CategoryId = 5, StoreId = 1, Name = "Roupas", Slug = "roupas" }
                });
            _categoryRepositoryMock.Setup(x => x.ExistSiblingNameAsync(5, 1, "Camisetas", null)).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.ExistSlugInTenantAsync(null, "roupas/camisetas")).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<CategoryModel>()))
                .ReturnsAsync((CategoryModel m) => { m.CategoryId = 11; return m; });

            var result = await _sut.InsertAsync(info, 1, 1);

            Assert.Equal("roupas/camisetas", result.Slug);
            Assert.Equal((long?)5, result.ParentId);
        }

        [Fact]
        public async Task InsertAsync_ShouldRejectDepth_WhenChainExceedsFive()
        {
            var info = new CategoryInsertInfo { Name = "L6", ParentCategoryId = 50 };
            // Parent chain of 5 ancestors -> child would be depth 6
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(50))
                .ReturnsAsync(new CategoryModel { CategoryId = 50, StoreId = 1, Name = "P", Slug = "p" });
            _categoryRepositoryMock.Setup(x => x.GetAncestorChainAsync(50))
                .ReturnsAsync(Enumerable.Range(1, 5)
                    .Select(i => new CategoryModel { CategoryId = i, StoreId = 1, Name = $"A{i}", Slug = $"a{i}" })
                    .ToList());

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.InsertAsync(info, 1, 1));
            Assert.Contains("Maximum nesting depth", ex.Message);
        }

        // ---------- US1: update guards ----------

        [Fact]
        public async Task UpdateAsync_ShouldRejectCycle_WhenProspectiveParentIsDescendant()
        {
            var info = new CategoryUpdateInfo { CategoryId = 10, Name = "X", ParentCategoryId = 20 };
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(new CategoryModel { CategoryId = 10, StoreId = 1, Name = "Self", Slug = "self" });
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(20))
                .ReturnsAsync(new CategoryModel { CategoryId = 20, StoreId = 1, Name = "Inside", Slug = "self/inside", ParentId = 10 });
            // Walking ancestors of 20 hits 10 -> cycle
            _categoryRepositoryMock.Setup(x => x.GetAncestorChainAsync(20))
                .ReturnsAsync(new List<CategoryModel>
                {
                    new() { CategoryId = 20, StoreId = 1, Name = "Inside", ParentId = 10 },
                    new() { CategoryId = 10, StoreId = 1, Name = "Self" }
                });

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(info, 1, 1));
            Assert.Contains("cycle", ex.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRejectWhenHasChildren()
        {
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new CategoryModel { CategoryId = 1, StoreId = 1, Name = "Parent" });
            _categoryRepositoryMock.Setup(x => x.HasChildrenAsync(1)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.DeleteAsync(1, 1, 1));
            Assert.Contains("subcategories", ex.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDelete_WhenLeaf()
        {
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new CategoryModel { CategoryId = 1, StoreId = 1, Name = "Leaf" });
            _categoryRepositoryMock.Setup(x => x.HasChildrenAsync(1)).ReturnsAsync(false);

            await _sut.DeleteAsync(1, 1, 1);

            _categoryRepositoryMock.Verify(x => x.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteGlobalAsync_ShouldRejectWhenHasChildren()
        {
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new CategoryModel { CategoryId = 1, StoreId = null, Name = "GlobalParent" });
            _categoryRepositoryMock.Setup(x => x.HasChildrenAsync(1)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.DeleteGlobalAsync(1));
            Assert.Contains("subcategories", ex.Message);
        }

        // ---------- US1: global insert with parent ----------

        [Fact]
        public async Task InsertGlobalAsync_ShouldRejectScopeMismatch_WhenParentIsStoreScoped()
        {
            var info = new CategoryGlobalInsertInfo { Name = "Sub", ParentCategoryId = 7 };
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(7))
                .ReturnsAsync(new CategoryModel { CategoryId = 7, StoreId = 1, Name = "Loja", Slug = "loja-cat" });

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.InsertGlobalAsync(info));
            Assert.Contains("same scope", ex.Message);
        }

        [Fact]
        public async Task InsertGlobalAsync_ShouldComputePathSlug_FromGlobalParent()
        {
            var info = new CategoryGlobalInsertInfo { Name = "Bermudas", ParentCategoryId = 30 };
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(30))
                .ReturnsAsync(new CategoryModel { CategoryId = 30, StoreId = null, Name = "Roupas", Slug = "roupas" });
            _categoryRepositoryMock.Setup(x => x.GetAncestorChainAsync(30))
                .ReturnsAsync(new List<CategoryModel>
                {
                    new() { CategoryId = 30, StoreId = null, Name = "Roupas", Slug = "roupas" }
                });
            _categoryRepositoryMock.Setup(x => x.ExistSiblingNameAsync(30, null, "Bermudas", null)).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.ExistSlugInTenantAsync(null, "roupas/bermudas")).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<CategoryModel>()))
                .ReturnsAsync((CategoryModel m) => { m.CategoryId = 31; return m; });

            var result = await _sut.InsertGlobalAsync(info);

            Assert.Equal("roupas/bermudas", result.Slug);
            Assert.Equal((long?)30, result.ParentCategoryId);
            Assert.True(result.IsGlobal);
        }

        // ---------- US2: tree assembly ----------

        [Fact]
        public async Task GetTreeAsync_BuildsThreeLevelHierarchy_OrderedAlphabetically()
        {
            var rows = new List<CategoryModel>
            {
                new() { CategoryId = 1, Name = "Roupas", Slug = "roupas", StoreId = 1 },
                new() { CategoryId = 2, Name = "Camisetas", Slug = "roupas/camisetas", StoreId = 1, ParentId = 1 },
                new() { CategoryId = 3, Name = "Calças", Slug = "roupas/calcas", StoreId = 1, ParentId = 1 },
                new() { CategoryId = 4, Name = "Skinny", Slug = "roupas/calcas/skinny", StoreId = 1, ParentId = 3 }
            };
            _categoryRepositoryMock.Setup(x => x.ListByScopeAsync(1)).ReturnsAsync(rows);

            var tree = await _sut.GetTreeAsync(1);

            Assert.Single(tree);
            var root = tree[0];
            Assert.Equal("Roupas", root.Name);
            Assert.Equal(2, root.Children.Count);
            // Accent-aware: "Calças" precedes "Camisetas"
            Assert.Equal("Calças", root.Children[0].Name);
            Assert.Equal("Camisetas", root.Children[1].Name);
            Assert.Single(root.Children[0].Children);
            Assert.Equal("Skinny", root.Children[0].Children[0].Name);
        }

        [Fact]
        public async Task GetTreeAsync_OnEmptyTenant_ReturnsEmpty()
        {
            _categoryRepositoryMock.Setup(x => x.ListByScopeAsync(It.IsAny<long?>()))
                .ReturnsAsync(new List<CategoryModel>());

            var tree = await _sut.GetTreeAsync(1);

            Assert.Empty(tree);
        }

        [Fact]
        public async Task GetTreeAsync_GlobalScope_FlagsAllNodesAsGlobal()
        {
            var rows = new List<CategoryModel>
            {
                new() { CategoryId = 1, Name = "A", Slug = "a", StoreId = null },
                new() { CategoryId = 2, Name = "B", Slug = "a/b", StoreId = null, ParentId = 1 }
            };
            _categoryRepositoryMock.Setup(x => x.ListByScopeAsync(null)).ReturnsAsync(rows);

            var tree = await _sut.GetTreeAsync(null);

            Assert.True(tree[0].IsGlobal);
            Assert.True(tree[0].Children[0].IsGlobal);
        }

        // ---------- US3: cascade ----------

        [Fact]
        public async Task UpdateAsync_RenameRoot_CascadesNewSlugToDescendants()
        {
            var existing = new CategoryModel { CategoryId = 1, StoreId = 1, Name = "Old", Slug = "old", ParentId = null };
            var info = new CategoryUpdateInfo { CategoryId = 1, Name = "New", ParentCategoryId = null };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existing);
            _categoryRepositoryMock.Setup(x => x.ExistSiblingNameAsync(null, 1, "New", 1L)).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.ExistSlugInTenantAsync(1, "new")).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<CategoryModel>()))
                .ReturnsAsync((CategoryModel m) => m);

            // Descendants: child (10) and grandchild (20)
            var descendants = new List<CategoryModel>
            {
                new() { CategoryId = 10, StoreId = 1, Name = "Child", Slug = "old/child", ParentId = 1 },
                new() { CategoryId = 20, StoreId = 1, Name = "Grand", Slug = "old/child/grand", ParentId = 10 }
            };
            _categoryRepositoryMock.Setup(x => x.GetDescendantsAsync(1)).ReturnsAsync(descendants);

            List<CategoryModel> updatedMany = null;
            _categoryRepositoryMock.Setup(x => x.UpdateManyAsync(It.IsAny<IEnumerable<CategoryModel>>()))
                .Callback<IEnumerable<CategoryModel>>(xs => updatedMany = xs.ToList())
                .Returns(Task.CompletedTask);

            await _sut.UpdateAsync(info, 1, 1);

            Assert.NotNull(updatedMany);
            Assert.Equal(2, updatedMany.Count);
            Assert.Equal("new/child", updatedMany.First(x => x.CategoryId == 10).Slug);
            Assert.Equal("new/child/grand", updatedMany.First(x => x.CategoryId == 20).Slug);
        }

        [Fact]
        public async Task UpdateAsync_DetachToRoot_CascadesNewSlugWithoutAncestorPrefix()
        {
            var existing = new CategoryModel { CategoryId = 5, StoreId = 1, Name = "Mid", Slug = "root/mid", ParentId = 1 };
            var info = new CategoryUpdateInfo { CategoryId = 5, Name = "Mid", ParentCategoryId = null };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(existing);
            _categoryRepositoryMock.Setup(x => x.ExistSiblingNameAsync(null, 1, "Mid", 5L)).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.ExistSlugInTenantAsync(5, "mid")).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<CategoryModel>()))
                .ReturnsAsync((CategoryModel m) => m);

            var descendants = new List<CategoryModel>
            {
                new() { CategoryId = 50, StoreId = 1, Name = "Leaf", Slug = "root/mid/leaf", ParentId = 5 }
            };
            _categoryRepositoryMock.Setup(x => x.GetDescendantsAsync(5)).ReturnsAsync(descendants);

            List<CategoryModel> updatedMany = null;
            _categoryRepositoryMock.Setup(x => x.UpdateManyAsync(It.IsAny<IEnumerable<CategoryModel>>()))
                .Callback<IEnumerable<CategoryModel>>(xs => updatedMany = xs.ToList())
                .Returns(Task.CompletedTask);

            await _sut.UpdateAsync(info, 1, 1);

            Assert.NotNull(updatedMany);
            Assert.Single(updatedMany);
            Assert.Equal("mid/leaf", updatedMany[0].Slug);
        }
    }
}
