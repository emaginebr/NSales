import OrderInfo from "../Domain/OrderInfo";
import OrderListPagedInfo from "../Domain/OrderListPagedInfo";
import OrderProviderResult from "./OrderProviderResult";
import ProviderResult from "./ProviderResult";


interface IOrderProvider {
    loading: boolean;
    loadingUpdate: boolean;
    loadingSearch: boolean;

    order: OrderInfo;
    searchResult: OrderListPagedInfo;
    clientSecret: string;
    
    createSubscription: (productSlug: string, networkSlug?: string, sellerSlug?: string) => Promise<OrderProviderResult>;
    createInvoice: (productSlug: string) => Promise<OrderProviderResult>;
    search: (networkId: number, userId: number, sellerId: number, pageNum: number) => Promise<ProviderResult>;
    getById: (orderId: number) => Promise<OrderProviderResult>;
}

export default IOrderProvider;