import TemplatePageInfo from "../Domain/TemplatePageInfo";
import TemplatePartInfo from "../Domain/TemplatePartInfo";
import TemplateVarInfo from "../Domain/TemplateVarInfo";
import { LanguageEnum } from "../Enum/LanguageEnum";
import ProviderResult from "./ProviderResult";


interface ITemplateProvider {
    loading: boolean;
    loadingUpdate: boolean;
    loadingVariable: boolean;

    page: TemplatePageInfo;
    part: TemplatePartInfo;
    variable: TemplateVarInfo;

    editMode: boolean;
    setEditMode: (edit: boolean) => void;

    setPart: (value: TemplatePartInfo) => void;
    setVariable: (value: TemplateVarInfo) => void;

    getNetworkPage: (networkSlug: string, pageSlug: string, lang: LanguageEnum) => Promise<ProviderResult>;
    getPageById: (pageId: number, lang: LanguageEnum) => Promise<ProviderResult>;
    insertPart: (part: TemplatePartInfo) => Promise<ProviderResult>;
    updatePart: (part: TemplatePartInfo) => Promise<ProviderResult>;
    deletePart: (partId: number) => Promise<ProviderResult>;
    movePartUp: (partId: number) => Promise<ProviderResult>;
    movePartDown: (partId: number) => Promise<ProviderResult>;
    getVariable: (pageId: number, key: string) => Promise<ProviderResult>;
    saveVariable: (variable: TemplateVarInfo) => Promise<ProviderResult>;
}

export default ITemplateProvider;