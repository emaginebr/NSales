import BusinessResult from "../../DTO/Business/BusinessResult";
import AuthSession from "../../DTO/Domain/AuthSession";
import InvoiceInfo from "../../DTO/Domain/InvoiceInfo";
import InvoiceListPagedInfo from "../../DTO/Domain/InvoiceListPagedInfo";
import StatementListPagedInfo from "../../DTO/Domain/StatementListPagedInfo";
import StatementSearchParam from "../../DTO/Domain/StatementSearchParam";
import IInvoiceService from "../../Services/Interfaces/IInvoiceService";
import AuthFactory from "../Factory/AuthFactory";
import IInvoiceBusiness from "../Interfaces/IInvoiceBusiness";

let _invoiceService: IInvoiceService;

const InvoiceBusiness: IInvoiceBusiness = {
  init: function (invoiceService: IInvoiceService): void {
    _invoiceService = invoiceService;
  },
  search: async (networkId: number, userId: number, sellerId: number, pageNum: number) => {
    try {
      let ret: BusinessResult<InvoiceListPagedInfo>;
      let session: AuthSession = AuthFactory.AuthBusiness.getSession();
      if (!session) {
        return {
          ...ret,
          sucesso: false,
          mensagem: "Not logged"
        };
      }
      let retServ = await _invoiceService.search(networkId, userId, sellerId, pageNum, session.token);
      if (retServ.sucesso) {
        let orderPaged: InvoiceListPagedInfo;
        orderPaged = {
          ...orderPaged,
          invoices: retServ.invoices,
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
  searchStatement: async (param: StatementSearchParam) => {
    try {
      let ret: BusinessResult<StatementListPagedInfo>;
      let session: AuthSession = AuthFactory.AuthBusiness.getSession();
      if (!session) {
        return {
          ...ret,
          sucesso: false,
          mensagem: "Not logged"
        };
      }
      let retServ = await _invoiceService.searchStatement(param, session.token);
      if (retServ.sucesso) {
        let orderPaged: StatementListPagedInfo;
        orderPaged = {
          ...orderPaged,
          statements: retServ.statements,
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
  getBalance: async (networkId?: number) => {
    try {
      let ret: BusinessResult<number>;
      let session: AuthSession = AuthFactory.AuthBusiness.getSession();
      if (!session) {
        return {
          ...ret,
          sucesso: false,
          mensagem: "Not logged"
        };
      }
      let retServ = await _invoiceService.getBalance(session.token, networkId);
      if (retServ.sucesso) {
        return {
          ...ret,
          dataResult: retServ.value,
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
  getAvailableBalance: async () => {
    try {
      let ret: BusinessResult<number>;
      let session: AuthSession = AuthFactory.AuthBusiness.getSession();
      if (!session) {
        return {
          ...ret,
          sucesso: false,
          mensagem: "Not logged"
        };
      }
      let retServ = await _invoiceService.getAvailableBalance(session.token);
      if (retServ.sucesso) {
        return {
          ...ret,
          dataResult: retServ.value,
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
  syncronize: async () => {
    try {
      let ret: BusinessResult<boolean>;
      let session: AuthSession = AuthFactory.AuthBusiness.getSession();
      if (!session) {
        return {
          ...ret,
          sucesso: false,
          mensagem: "Not logged"
        };
      }
      let retServ = await _invoiceService.syncronize(session.token);
      if (retServ.sucesso) {
        return {
          ...ret,
          dataResult: retServ.sucesso,
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
  checkout: async (checkoutSessionId: string) => {
    try {
      let ret: BusinessResult<InvoiceInfo>;
      let retServ = await _invoiceService.checkout(checkoutSessionId);
      if (retServ.sucesso) {
        return {
          ...ret,
          dataResult: retServ.invoice,
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

export default InvoiceBusiness;