namespace Lofn.Domain.Models
{
    public class CategoryModel
    {
        public long CategoryId { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public long? StoreId { get; set; }
        public long? ParentId { get; set; }
        public long? ProductTypeId { get; set; }
        public bool IsGlobal => StoreId == null;
    }
}
