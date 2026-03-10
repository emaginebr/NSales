import BusinessResult from "../../DTO/Business/BusinessResult";
import OrderInfo from "../../DTO/Domain/OrderInfo";
import OrderListPagedInfo from "../../DTO/Domain/OrderListPagedInfo";
import IOrderService from "../../Services/Interfaces/IOrderService";

export default interface IOrderBusiness {
  init: (orderService: IOrderService) => void;
  createSubscription: (productSlug: string, networkSlug?: string, sellerSlug?: string) => Promise<BusinessResult<string>>;
  createInvoice: (productSlug: string) => Promise<BusinessResult<string>>;
  search: (networkId: number, userId: number, sellerId: number, pageNum: number) => Promise<BusinessResult<OrderListPagedInfo>>;
  getById: (orderId: number) => Promise<BusinessResult<OrderInfo>>;
}