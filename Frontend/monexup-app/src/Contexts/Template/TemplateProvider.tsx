import { useState } from "react";
import ProviderResult from "../../DTO/Contexts/ProviderResult";
import UserProfileInfo from "../../DTO/Domain/UserProfileInfo";
import IProfileProvider from "../../DTO/Contexts/IProfileProvider";
import ProfileFactory from "../../Business/Factory/ProfileFactory";
import TemplatePageInfo from "../../DTO/Domain/TemplatePageInfo";
import TemplatePartInfo from "../../DTO/Domain/TemplatePartInfo";
import ITemplateProvider from "../../DTO/Contexts/ITemplateProvider";
import TemplateContext from "./TemplateContext";
import TemplateVarInfo from "../../DTO/Domain/TemplateVarInfo";
import { LanguageEnum } from "../../DTO/Enum/LanguageEnum";
import TemplateFactory from "../../Business/Factory/TemplateFactory";

export default function TemplateProvider(props: any) {

    const [loading, setLoading] = useState<boolean>(false);
    const [loadingUpdate, setLoadingUpdate] = useState<boolean>(false);
    const [loadingVariable, setLoadingVariable] = useState<boolean>(false);

    const [editMode, _setEditMode] = useState<boolean>(false);

    const [page, setPage] = useState<TemplatePageInfo>(null);
    const [part, _setPart] = useState<TemplatePartInfo>(null);
    const [variable, _setVariable] = useState<TemplateVarInfo>(null);

    const templateProviderValue: ITemplateProvider = {
        loading: loading,
        loadingUpdate: loadingUpdate,
        loadingVariable: loadingVariable,

        page: page,
        part: part,
        variable: variable,


        editMode: editMode,
        setEditMode: (edit: boolean) => {
            _setEditMode(edit);
        },

        setPart: (value: TemplatePartInfo) => {
            _setPart(value);
        },
        setVariable: (value: TemplateVarInfo) => {
            _setVariable(value);
        },

        getNetworkPage: async (networkSlug: string, pageSlug: string, lang: LanguageEnum) => {
            let ret: Promise<ProviderResult>;
            setLoading(true);
            setPage(null);
            //try {
            let brt = await TemplateFactory.TemplateBusiness.getNetworkPage(networkSlug, pageSlug, lang);
            if (brt.sucesso) {
                setLoading(false);
                setPage(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    mensagemSucesso: "Load Page"
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
        },
        getPageById: async (pageId: number, lang: LanguageEnum) => {
            let ret: Promise<ProviderResult>;
            setLoading(true);
            setPage(null);
            //try {
            let brt = await TemplateFactory.TemplateBusiness.getPageById(pageId, lang);
            if (brt.sucesso) {
                setLoading(false);
                setPage(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    mensagemSucesso: "Load Page"
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
        },
        insertPart: async (part: TemplatePartInfo) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            //try {
            let brt = await TemplateFactory.TemplateBusiness.insertPart(part);
            if (brt.sucesso) {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: true,
                    mensagemSucesso: "Load Page"
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
        updatePart: async (part: TemplatePartInfo) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            //try {
            let brt = await TemplateFactory.TemplateBusiness.updatePart(part);
            if (brt.sucesso) {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: true,
                    mensagemSucesso: "Load Page"
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
        deletePart: async (partId: number) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            //try {
            let brt = await TemplateFactory.TemplateBusiness.deletePart(partId);
            if (brt.sucesso) {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: true,
                    mensagemSucesso: "Load Page"
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
        movePartUp: async (partId: number) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            //try {
            let brt = await TemplateFactory.TemplateBusiness.movePartUp(partId);
            if (brt.sucesso) {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: true,
                    mensagemSucesso: "Load Page"
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
        movePartDown: async (partId: number) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            //try {
            let brt = await TemplateFactory.TemplateBusiness.movePartDown(partId);
            if (brt.sucesso) {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: true,
                    mensagemSucesso: "Load Page"
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
        getVariable: async (pageId: number, key: string) => {
            let ret: Promise<ProviderResult>;
            setLoadingVariable(true);
            //try {
            let brt = await TemplateFactory.TemplateBusiness.getVariable(pageId, key);
            if (brt.sucesso) {
                setLoadingVariable(false);
                _setVariable(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    mensagemSucesso: "Load Page"
                };
            }
            else {
                setLoadingVariable(false);
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
        saveVariable: async (variable: TemplateVarInfo) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            //try {
            let brt = await TemplateFactory.TemplateBusiness.saveVariable(variable);
            if (brt.sucesso) {
                setLoadingUpdate(false);
                _setVariable(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    mensagemSucesso: "Load Page"
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
        <TemplateContext.Provider value={templateProviderValue}>
            {props.children}
        </TemplateContext.Provider>
    );
}