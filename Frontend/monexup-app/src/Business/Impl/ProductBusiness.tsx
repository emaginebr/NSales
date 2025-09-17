import BusinessResult from "../../DTO/Business/BusinessResult";
import AuthSession from "../../DTO/Domain/AuthSession";
import ProductInfo from "../../DTO/Domain/ProductInfo";
import ProductListPagedInfo from "../../DTO/Domain/ProductListPagedInfo";
import ProductSearchParam from "../../DTO/Domain/ProductSearchParam";
import IProductService from "../../Services/Interfaces/IProductService";
import AuthFactory from "../Factory/AuthFactory";
import IProductBusiness from "../Interfaces/IProductBusiness";

let _productService: IProductService;

const ProductBusiness: IProductBusiness = {
  init: function (productService: IProductService): void {
    _productService = productService;
  },
  insert: async (product: ProductInfo) => {
    //try {
        let ret: BusinessResult<ProductInfo>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _productService.insert(product, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.product,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      /*  
      } catch {
        throw new Error("Failed to insert");
      }
      */
  },
  update: async (product: ProductInfo) => {
    try {
        let ret: BusinessResult<ProductInfo>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _productService.update(product, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.product,
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
        throw new Error("Failed to update");
      }
  },
  search: async (param: ProductSearchParam) => {
    try {
        let ret: BusinessResult<ProductListPagedInfo>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _productService.search(param, session.token);
        if (retServ.sucesso) {
          let search: ProductListPagedInfo;
          search = {
            ...search,
            pageNum: retServ.pageNum,
            pageCount: retServ.pageCount,
            products: retServ.products
          };
          return {
            ...ret,
            dataResult: search,
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
  listByNetwork: async (networkId: number) => {
    try {
        let ret: BusinessResult<ProductInfo[]>;
        let retServ = await _productService.listByNetwork(networkId);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.products,
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
  listByNetworkSlug: async (networkSlug: string) => {
    try {
        let ret: BusinessResult<ProductInfo[]>;
        let retServ = await _productService.listByNetworkSlug(networkSlug);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.products,
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
  getById: async (productId: number) => {
    try {
        let ret: BusinessResult<ProductInfo>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _productService.getById(productId, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.product,
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
  getBySlug: async (productSlug: string) => {
    try {
        let ret: BusinessResult<ProductInfo>;
        let retServ = await _productService.getBySlug(productSlug);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.product,
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

export default ProductBusiness;