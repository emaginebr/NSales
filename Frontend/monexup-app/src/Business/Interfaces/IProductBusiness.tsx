import BusinessResult from "../../DTO/Business/BusinessResult";
import ProductInfo from "../../DTO/Domain/ProductInfo";
import ProductListPagedInfo from "../../DTO/Domain/ProductListPagedInfo";
import ProductSearchParam from "../../DTO/Domain/ProductSearchParam";
import IProductService from "../../Services/Interfaces/IProductService";

export default interface IProductBusiness {
  init: (productService: IProductService) => void;
  insert: (product: ProductInfo) => Promise<BusinessResult<ProductInfo>>;
  update: (product: ProductInfo) => Promise<BusinessResult<ProductInfo>>;
  search: (param: ProductSearchParam) => Promise<BusinessResult<ProductListPagedInfo>>;
  listByNetwork: (networkId: number) => Promise<BusinessResult<ProductInfo[]>>;
  listByNetworkSlug: (networkSlug: string) => Promise<BusinessResult<ProductInfo[]>>;
  getById: (productId: number) => Promise<BusinessResult<ProductInfo>>;
  getBySlug: (productSlug: string) => Promise<BusinessResult<ProductInfo>>;
}