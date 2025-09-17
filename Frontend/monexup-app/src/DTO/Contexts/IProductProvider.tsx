import ProductInfo from "../Domain/ProductInfo";
import ProductListPagedInfo from "../Domain/ProductListPagedInfo";
import ProductSearchParam from "../Domain/ProductSearchParam";
import UserProfileInfo from "../Domain/UserProfileInfo";
import ProductResult from "../Services/ProductResult";
import ProductProviderResult from "./ProductProviderResult";
import ProviderResult from "./ProviderResult";


interface IProductProvider {
    loading: boolean;
    loadingList: boolean;
    loadingUpdate: boolean;
    loadingSearch: boolean;

    product: ProductInfo;
    products: ProductInfo[];

    searchResult: ProductListPagedInfo;

    setProduct: (product: ProductInfo) => void;
    
    insert: (product: ProductInfo) => Promise<ProviderResult>;
    update: (product: ProductInfo) => Promise<ProviderResult>;
    search: (param: ProductSearchParam) => Promise<ProviderResult>;
    listByNetwork: (networkId: number) => Promise<ProviderResult>; 
    listByNetworkSlug: (networkSlug: string) => Promise<ProviderResult>; 
    getById: (productId: number) => Promise<ProductProviderResult>;
    getBySlug: (productSlug: string) => Promise<ProductProviderResult>;
}

export default IProductProvider;