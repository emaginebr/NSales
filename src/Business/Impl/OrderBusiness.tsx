import BusinessResult from "../../DTO/Business/BusinessResult";
import AuthSession from "../../DTO/Domain/AuthSession";
import OrderInfo from "../../DTO/Domain/OrderInfo";
import OrderListPagedInfo from "../../DTO/Domain/OrderListPagedInfo";
import IOrderService from "../../Services/Interfaces/IOrderService";
import AuthFactory from "../Factory/AuthFactory";
import IOrderBusiness from "../Interfaces/IOrderBusiness";

let _orderService: IOrderService;

const OrderBusiness: IOrderBusiness = {
  init: function (orderService: IOrderService): void {
    _orderService = orderService;
  },
  createSubscription: async (productSlug: string, networkSlug?: string, sellerSlug?: string) => {
    try {
        let ret: BusinessResult<string>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _orderService.createSubscription(productSlug, session.token, networkSlug, sellerSlug);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.clientSecret,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      } catch {
        throw new Error("Failed to get user by email");
      }
  }
  ,
  createInvoice: async (productSlug: string) => {
    try {
        let ret: BusinessResult<string>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _orderService.createInvoice(productSlug, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.clientSecret,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      } catch {
        throw new Error("Failed to get user by email");
      }
  },
  search: async (networkId: number, userId: number, sellerId: number, pageNum: number) => {
    try {
      let ret: BusinessResult<OrderListPagedInfo>;
      let session: AuthSession = AuthFactory.AuthBusiness.getSession();
      if (!session) {
        return {
          ...ret,
          sucesso: false,
          mensagem: "Not logged"
        };
      }
      let retServ = await _orderService.search(networkId, userId, sellerId, pageNum, session.token);
      if (retServ.sucesso) {
        let orderPaged: OrderListPagedInfo;
        orderPaged = {
          ...orderPaged,
          orders: retServ.orders,
          pageNum: retServ.pageNum,
          pageCount: retServ.pageCount
        }
        return {
          ...ret,
          dataResult: orderPaged,
          sucesso: true
        };
      } else {
        return {
          ...ret,
          sucesso: false,
          mensagem: retServ.mensagem
        };
      }
    } catch {
      throw new Error("Failed to get user by email");
    }
  },
  getById: async (orderId: number) => {
    try {
      let ret: BusinessResult<OrderInfo>;
      let session: AuthSession = AuthFactory.AuthBusiness.getSession();
      if (!session) {
        return {
          ...ret,
          sucesso: false,
          mensagem: "Not logged"
        };
      }
      let retServ = await _orderService.getById(orderId, session.token);
      if (retServ.sucesso) {
        return {
          ...ret,
          dataResult: retServ.order,
          sucesso: true
        };
      } else {
        return {
          ...ret,
          sucesso: false,
          mensagem: retServ.mensagem
        };
      }
    } catch {
      throw new Error("Failed to get user by email");
    }
  }
}

export default OrderBusiness;