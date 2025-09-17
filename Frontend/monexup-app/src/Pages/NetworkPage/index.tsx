import { useContext, useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import AuthContext from "../../Contexts/Auth/AuthContext";
import NetworkContext from "../../Contexts/Network/NetworkContext";
import { MessageToastEnum } from "../../DTO/Enum/MessageToastEnum";
import MessageToast from "../../Components/MessageToast";
import Plan3ColsPart from "../_WebParts/Plan3ColsPart";
import TeamPart from "../_WebParts/TeamPart";
import Hero01Part from "../_WebParts/Hero01Part";
import EditMode from "../../Components/EditMode";
import TemplateContext from "../../Contexts/Template/TemplateContext";
import { strToPartEnum, WebsitePartEnum } from "../../Components/TemplateUtils";
import TemplatePartInfo from "../../DTO/Domain/TemplatePartInfo";
import Hero02Part from "../_WebParts/Hero02Part";
import SkeletonPage from "../../Components/SkeletonPage";
import ProductContext from "../../Contexts/Product/ProductContext";
import ProductListPart from "../_WebParts/ProductListPart";
import ProductSearchParam from "../../DTO/Domain/ProductSearchParam";
import Plan4ColsPart from "../_WebParts/Plan4ColsPart";

export default function NetworkPage() {

    let { networkSlug } = useParams();

    const authContext = useContext(AuthContext);
    const networkContext = useContext(NetworkContext);
    const templateContext = useContext(TemplateContext);
    const productContext = useContext(ProductContext);

    const [dialog, setDialog] = useState<MessageToastEnum>(MessageToastEnum.Error);
    const [showMessage, setShowMessage] = useState<boolean>(false);
    const [messageText, setMessageText] = useState<string>("");

    const throwError = (message: string) => {
        setDialog(MessageToastEnum.Error);
        setMessageText(message);
        setShowMessage(true);
    };
    const showSuccessMessage = (message: string) => {
        setDialog(MessageToastEnum.Success);
        setMessageText(message);
        setShowMessage(true);
    };

    const searchProducts = (pageNum: number) => {
        let param: ProductSearchParam;
        param = {
            ...param,
            networkId: 0,
            networkSlug: networkSlug,
            userId: 0,
            keyword: "",
            onlyActive: true,
            pageNum: pageNum
        };
        productContext.search(param).then((ret) => {
            if (!ret.sucesso) {
                throwError(ret.mensagemErro);
            }
        });
    };

    const ACCEPTABLE_PARTS: WebsitePartEnum[] = [
        WebsitePartEnum.HERO01, WebsitePartEnum.HERO02, WebsitePartEnum.PLAN_3_COLS,
        WebsitePartEnum.PLAN_4_COLS, WebsitePartEnum.TEAM_3_COLS, WebsitePartEnum.PRODUCT_LIST_WITH_3_COLS
    ];

    const TemplatePart = (part: TemplatePartInfo) => {
        switch (strToPartEnum(part.partKey)) {
            case WebsitePartEnum.HERO01:
                return (
                    <>
                        <EditMode part={part} isEditing={templateContext.editMode} acceptableParts={ACCEPTABLE_PARTS}>
                            <Hero01Part
                                loading={templateContext.loading}
                                isEditing={templateContext.editMode}
                                variables={templateContext.page?.variables}
                            />
                        </EditMode>
                        <hr />
                    </>
                );
                break;
            case WebsitePartEnum.HERO02:
                return (
                    <>
                        <EditMode part={part} isEditing={templateContext.editMode} acceptableParts={ACCEPTABLE_PARTS}>
                            <Hero02Part
                                loading={networkContext.loading}
                                isEditing={templateContext.editMode}
                                variables={templateContext.page?.variables}
                            />
                        </EditMode>
                        <hr />
                    </>
                );
                break;
            case WebsitePartEnum.PLAN_3_COLS:
                return (
                    <>
                        <EditMode part={part} isEditing={templateContext.editMode} acceptableParts={ACCEPTABLE_PARTS}>
                            <Plan3ColsPart
                                loading={productContext.loadingList}
                                products={productContext.searchResult?.products}
                                isEditing={templateContext.editMode}
                                variables={templateContext.page?.variables}
                            />
                        </EditMode>
                        <hr />
                    </>
                );
                break;
            case WebsitePartEnum.PLAN_4_COLS:
                return (
                    <>
                        <EditMode part={part} isEditing={templateContext.editMode} acceptableParts={ACCEPTABLE_PARTS}>
                            <Plan4ColsPart
                                loading={productContext.loadingList}
                                products={productContext.searchResult?.products}
                                isEditing={templateContext.editMode}
                                variables={templateContext.page?.variables}
                            />
                        </EditMode>
                        <hr />
                    </>
                );
                break;
            case WebsitePartEnum.PRODUCT_LIST_WITH_3_COLS:
                return (
                    <>
                        <EditMode part={part} isEditing={templateContext.editMode} acceptableParts={ACCEPTABLE_PARTS}>
                            <ProductListPart
                                loading={productContext.loadingSearch}
                                networkSlug={networkSlug}
                                isEditing={templateContext.editMode}
                                variables={templateContext.page?.variables}
                                ProductResult={productContext.searchResult}
                                onChangePage={(pageNum) => searchProducts(pageNum)}
                            />
                        </EditMode>
                        <hr />
                    </>
                );
                break;
            case WebsitePartEnum.TEAM_3_COLS:
                return (
                    <>
                        <EditMode part={part} isEditing={templateContext.editMode} acceptableParts={ACCEPTABLE_PARTS}>
                            <TeamPart
                                loading={networkContext.loadingTeam}
                                teams={networkContext.teams}
                                isEditing={templateContext.editMode}
                                variables={templateContext.page?.variables}
                            />
                        </EditMode>
                        <hr />
                    </>
                );
                break;
        }
    };

    useEffect(() => {
        /*
        networkContext.getBySlug(networkSlug).then((ret) => {
            if (!ret.sucesso) {
                throwError(ret.mensagemErro);
            }
        });
        */
        authContext.loadUserSession().then((authRet) => {
            if (authRet.sucesso) {
                networkContext.getUserNetworkBySlug(networkSlug).then((retUserNetwork) => {
                    if (!retUserNetwork.sucesso) {
                        throwError(retUserNetwork.mensagemErro);
                        return;
                    }
                });
            }
        });
        templateContext.getNetworkPage(networkSlug, "network-home", authContext.language).then((ret) => {
            if (!ret.sucesso) {
                throwError(ret.mensagemErro);
                return;
            }
        });
        /*
        productContext.listByNetworkSlug(networkSlug).then((ret) => {
            if (!ret.sucesso) {
                throwError(ret.mensagemErro);
                return;
            }
        });
        */
        searchProducts(1);
        networkContext.listByNetwork(networkSlug).then((ret) => {
            if (!ret.sucesso) {
                throwError(ret.mensagemErro);
                return;
            }
        });
    }, []);

    return (
        <>
            <MessageToast
                dialog={dialog}
                showMessage={showMessage}
                messageText={messageText}
                onClose={() => setShowMessage(false)}
            ></MessageToast>
            <EditMode.New isEditing={templateContext.editMode} acceptableParts={ACCEPTABLE_PARTS} />
            {templateContext.loading && <SkeletonPage />}
            {templateContext.page?.parts.map((part) => {
                return (
                    <>
                        {TemplatePart(part)}
                    </>
                )
            })}
        </>
    );
}