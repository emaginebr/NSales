import ProductInfo from "../../DTO/Domain/ProductInfo";
import ProductSearchParam from "../../DTO/Domain/ProductSearchParam";
import ProductListPagedResult from "../../DTO/Services/ProductListPagedResult";
import ProductListResult from "../../DTO/Services/ProductListResult";
import ProductResult from "../../DTO/Services/ProductResult";
import IHttpClient from "../../Infra/Interface/IHttpClient";

export default interface IProductService {
    init: (httpClient : IHttpClient) => void;
    insert: (profile: ProductInfo, token: string) => Promise<ProductResult>;
    update: (profile: ProductInfo, token: string) => Promise<ProductResult>;
    search: (param: ProductSearchParam, token: string) => Promise<ProductListPagedResult>;
    listByNetwork: (networkId: number) => Promise<ProductListResult>;
    listByNetworkSlug: (networkSlug: string) => Promise<ProductListResult>;
    getById: (productId: number, token: string) => Promise<ProductResult>;
    getBySlug: (productSlug: string) => Promise<ProductResult>;
}