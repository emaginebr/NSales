import { useState } from "react";
import ProviderResult from "../../DTO/Contexts/ProviderResult";
import UserProfileInfo from "../../DTO/Domain/UserProfileInfo";
import IProfileProvider from "../../DTO/Contexts/IProfileProvider";
import ProfileFactory from "../../Business/Factory/ProfileFactory";
import OrderInfo from "../../DTO/Domain/OrderInfo";
import IOrderProvider from "../../DTO/Contexts/IOrderProvider";
import OrderContext from "./OrderContext";
import OrderProviderResult from "../../DTO/Contexts/OrderProviderResult";
import OrderFactory from "../../Business/Factory/OrderFactory";
import OrderListPagedResult from "../../DTO/Services/OrderListPagedResult";
import OrderListPagedInfo from "../../DTO/Domain/OrderListPagedInfo";

export default function OrderProvider(props: any) {

    const [loading, setLoading] = useState<boolean>(false);
    const [loadingUpdate, setLoadingUpdate] = useState<boolean>(false);
    const [loadingSearch, setLoadingSearch] = useState<boolean>(false);

    const [order, setOrder] = useState<OrderInfo>(null);
    const [searchResult, setSearchResult] = useState<OrderListPagedInfo>(null);
    const [clientSecret, setClientSecret] = useState<string>("");

    const orderProviderValue: IOrderProvider = {
        loading: loading,
        loadingUpdate: loadingUpdate,
        loadingSearch: loadingSearch,

        order: order,
        searchResult: searchResult,
        clientSecret: clientSecret,

        createSubscription: async (productSlug: string, networkSlug?: string, sellerSlug?: string) => {
            let ret: Promise<OrderProviderResult>;
            setLoadingUpdate(true);
            //try {
            let brt = await OrderFactory.OrderBusiness.createSubscription(productSlug, networkSlug, sellerSlug);
            if (brt.sucesso) {
                setLoadingUpdate(false);
                setClientSecret(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    clientSecret: brt.dataResult,
                    mensagemSucesso: "Profile added"
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
        createInvoice: async (productSlug: string) => {
            let ret: Promise<OrderProviderResult>;
            setLoadingUpdate(true);
            //try {
            let brt = await OrderFactory.OrderBusiness.createInvoice(productSlug);
            if (brt.sucesso) {
                setLoadingUpdate(false);
                setClientSecret(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    clientSecret: brt.dataResult,
                    mensagemSucesso: "Profile added"
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
        search: async (networkId: number, userId: number, sellerId: number, pageNum: number) => {
            let ret: Promise<ProviderResult>;
            setLoadingSearch(true);
            //try {
            let brt = await OrderFactory.OrderBusiness.search(networkId, userId, sellerId, pageNum);
            if (brt.sucesso) {
                setLoadingSearch(false);
                setSearchResult(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    clientSecret: brt.dataResult,
                    mensagemSucesso: "Profile added"
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
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
            */
        },
        getById: async (orderId: number) => {
            let ret: Promise<OrderProviderResult>;
            setLoading(true);
            //try {
            let brt = await OrderFactory.OrderBusiness.getById(orderId);
            if (brt.sucesso) {
                setLoading(false);
                setOrder(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    order: brt.dataResult,
                    mensagemSucesso: "Load Order"
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
        }
    }

    return (
        <OrderContext.Provider value={orderProviderValue}>
            {props.children}
        </OrderContext.Provider>
    );
}