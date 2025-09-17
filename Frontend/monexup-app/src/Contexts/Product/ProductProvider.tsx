import { useState } from "react";
import ProviderResult from "../../DTO/Contexts/ProviderResult";
import UserProfileInfo from "../../DTO/Domain/UserProfileInfo";
import ProfileFactory from "../../Business/Factory/ProfileFactory";
import IProductProvider from "../../DTO/Contexts/IProductProvider";
import ProductInfo from "../../DTO/Domain/ProductInfo";
import ProductListPagedInfo from "../../DTO/Domain/ProductListPagedInfo";
import ProductFactory from "../../Business/Factory/ProductFactory";
import ProductContext from "./ProductContext";
import ProductProviderResult from "../../DTO/Contexts/ProductProviderResult";
import ProductSearchParam from "../../DTO/Domain/ProductSearchParam";

export default function ProductProvider(props: any) {

    const [loading, setLoading] = useState<boolean>(false);
    const [loadingList, setLoadingList] = useState<boolean>(false);
    const [loadingUpdate, setLoadingUpdate] = useState<boolean>(false);
    const [loadingSearch, setLoadingSearch] = useState<boolean>(false);

    const [product, _setProduct] = useState<ProductInfo>(null);
    const [products, setProducts] = useState<ProductInfo[]>([]);
    const [searchResult, setSearchResult] = useState<ProductListPagedInfo>(null);

    const productProviderValue: IProductProvider = {
        loading: loading,
        loadingList: loadingList,
        loadingUpdate: loadingUpdate,
        loadingSearch: loadingSearch,

        product: product,
        products: products,
        searchResult: searchResult,

        setProduct: (product: ProductInfo) => {
            _setProduct(product);
        },

        insert: async (product: ProductInfo) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            //try {
            let brt = await ProductFactory.ProductBusiness.insert(product);
            if (brt.sucesso) {
                setLoadingUpdate(false);
                _setProduct(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    mensagemSucesso: "product_added_successfully" // Use translation key
                };
            }
            else {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: brt.mensagem
                };
            }
            /*
            }
            catch (err) {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
            */
        },
        update: async (product: ProductInfo) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            try {
                let brt = await ProductFactory.ProductBusiness.update(product);
                if (brt.sucesso) {
                    setLoadingUpdate(false);
                    _setProduct(brt.dataResult);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "product_updated_successfully" // Use translation key
                    };
                }
                else {
                    setLoadingUpdate(false);
                    return {
                        ...ret,
                        sucesso: false,
                        mensagemErro: brt.mensagem
                    };
                }
            }
            catch (err) {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
        },
        search: async (param: ProductSearchParam) => {
            let ret: Promise<ProviderResult>;
            setLoadingSearch(true);
            setSearchResult(null);
            //try {
                let brt = await ProductFactory.ProductBusiness.search(param);
                if (brt.sucesso) {
                    setLoadingSearch(false);
                    setSearchResult(brt.dataResult);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "search_executed_successfully" // Use translation key
                    };
                }
                else {
                    setLoadingSearch(false);
                    return {
                        ...ret,
                        sucesso: false,
                        mensagemErro: brt.mensagem
                    };
                }
            /*
            }
            catch (err) {
                setLoadingSearch(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
            */
        },
        listByNetwork: async (networkId: number) => {
            let ret: Promise<ProviderResult>;
            setLoadingList(true);
            try {
                let brt = await ProductFactory.ProductBusiness.listByNetwork(networkId);
                if (brt.sucesso) {
                    setLoadingList(false);
                    setProducts(brt.dataResult);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "profile_loaded_successfully" // Use translation key (or a more specific one)
                    };
                }
                else {
                    setLoadingList(false);
                    return {
                        ...ret,
                        sucesso: false,
                        mensagemErro: brt.mensagem
                    };
                }
            }
            catch (err) {
                setLoadingList(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
        },
        listByNetworkSlug: async (networkSlug: string) => {
            let ret: Promise<ProviderResult>;
            setLoadingList(true);
            try {
                let brt = await ProductFactory.ProductBusiness.listByNetworkSlug(networkSlug);
                if (brt.sucesso) {
                    setLoadingList(false);
                    setProducts(brt.dataResult);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "profile_loaded_successfully" // Use translation key
                    };
                }
                else {
                    setLoadingList(false);
                    return {
                        ...ret,
                        sucesso: false,
                        mensagemErro: brt.mensagem
                    };
                }
            }
            catch (err) {
                setLoadingList(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
        },
        getById: async (productId: number) => {
            let ret: Promise<ProductProviderResult>;
            setLoading(true);
            _setProduct(null);
            try {
                let brt = await ProductFactory.ProductBusiness.getById(productId);
                if (brt.sucesso) {
                    setLoading(false);
                    _setProduct(brt.dataResult);
                    return {
                        ...ret,
                        product: brt.dataResult,
                        sucesso: true,
                        mensagemSucesso: "profile_loaded_successfully" // Use translation key
                    };
                }
                else {
                    setLoading(false);
                    return {
                        ...ret,
                        sucesso: false,
                        mensagemErro: brt.mensagem
                    };
                }
            }
            catch (err) {
                setLoading(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
        },
        getBySlug: async (productSlug: string) => {
            let ret: Promise<ProductProviderResult>;
            setLoading(true);
            _setProduct(null);
            try {
                let brt = await ProductFactory.ProductBusiness.getBySlug(productSlug);
                if (brt.sucesso) {
                    setLoading(false);
                    _setProduct(brt.dataResult);
                    return {
                        ...ret,
                        product: brt.dataResult,
                        sucesso: true,
                        mensagemSucesso: "profile_loaded_successfully" // Use translation key
                    };
                }
                else {
                    setLoading(false);
                    return {
                        ...ret,
                        sucesso: false,
                        mensagemErro: brt.mensagem
                    };
                }
            }
            catch (err) {
                setLoading(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
        }
    }

    return (
        <ProductContext.Provider value={productProviderValue}>
            {props.children}
        </ProductContext.Provider>
    );
}