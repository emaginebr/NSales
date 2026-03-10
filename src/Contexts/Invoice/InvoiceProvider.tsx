import { useState } from "react";
import ProviderResult from "../../DTO/Contexts/ProviderResult";
import InvoiceListPagedInfo from "../../DTO/Domain/InvoiceListPagedInfo";
import IInvoiceProvider from "../../DTO/Contexts/IInvoiceProvider";
import InvoiceContext from "./InvoiceContext";
import InvoiceFactory from "../../Business/Factory/InvoiceFactory";
import StatementListPagedInfo from "../../DTO/Domain/StatementListPagedInfo";
import StatementSearchParam from "../../DTO/Domain/StatementSearchParam";

export default function InvoiceProvider(props: any) {

    const [loadingUpdate, setLoadingUpdate] = useState<boolean>(false);
    const [loadingSearch, setLoadingSearch] = useState<boolean>(false);
    const [loadingBalance, setLoadingBalance] = useState<boolean>(false);
    const [loadingAvailableBalance, setLoadingAvailableBalance] = useState<boolean>(false);

    const [balance, setBalance] = useState<number>(0);
    const [availableBalance, setAvailableBalance] = useState<number>(0);

    const [searchResult, setSearchResult] = useState<InvoiceListPagedInfo>(null);
    const [statementResult, setStatementResult] = useState<StatementListPagedInfo>(null);

    const invoiceProviderValue: IInvoiceProvider = {
        loadingUpdate: loadingUpdate,
        loadingSearch: loadingSearch,
        loadingBalance: loadingBalance,
        loadingAvailableBalance: loadingAvailableBalance,

        balance: balance,
        availableBalance: availableBalance,

        searchResult: searchResult,
        statementResult: statementResult,

        search: async (networkId: number, userId: number, sellerId: number, pageNum: number) => {
            let ret: Promise<ProviderResult>;
            setLoadingSearch(true);
            //try {
            let brt = await InvoiceFactory.InvoiceBusiness.search(networkId, userId, sellerId, pageNum);
            if (brt.sucesso) {
                setLoadingSearch(false);
                console.log(JSON.stringify(brt.dataResult));
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
        searchStatement: async (param: StatementSearchParam) => {
            let ret: Promise<ProviderResult>;
            setLoadingSearch(true);
            //try {
            let brt = await InvoiceFactory.InvoiceBusiness.searchStatement(param);
            if (brt.sucesso) {
                setLoadingSearch(false);
                setStatementResult(brt.dataResult);
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
        getBalance: async (networkId?: number) => {
            let ret: Promise<ProviderResult>;
            setLoadingBalance(true);
            //try {
            let brt = await InvoiceFactory.InvoiceBusiness.getBalance(networkId);
            if (brt.sucesso) {
                setLoadingBalance(false);
                setBalance(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    clientSecret: brt.dataResult,
                    mensagemSucesso: "Profile added"
                };
            }
            else {
                setLoadingBalance(false);
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
        getAvailableBalance: async () => {
            let ret: Promise<ProviderResult>;
            setLoadingAvailableBalance(true);
            //try {
            let brt = await InvoiceFactory.InvoiceBusiness.getAvailableBalance();
            if (brt.sucesso) {
                setLoadingAvailableBalance(false);
                setAvailableBalance(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    clientSecret: brt.dataResult,
                    mensagemSucesso: "Profile added"
                };
            }
            else {
                setLoadingAvailableBalance(false);
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
        syncronize: async () => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            //try {
            let brt = await InvoiceFactory.InvoiceBusiness.syncronize();
            if (brt.sucesso) {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: true,
                    clientSecret: brt.dataResult,
                    mensagemSucesso: "Syncronized"
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
        checkout: async (checkoutSessionId: string) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            //try {
            let brt = await InvoiceFactory.InvoiceBusiness.checkout(checkoutSessionId);
            if (brt.sucesso) {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: true,
                    clientSecret: brt.dataResult,
                    mensagemSucesso: "Checkout"
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
        }
    }

    return (
        <InvoiceContext.Provider value={invoiceProviderValue}>
            {props.children}
        </InvoiceContext.Provider>
    );
}