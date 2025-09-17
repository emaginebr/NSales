import { useContext, useEffect, useState } from "react";
import Col from "react-bootstrap/esm/Col";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import { Link, useNavigate, useParams } from "react-router-dom";
import AuthContext from "../../Contexts/Auth/AuthContext";
import NetworkContext from "../../Contexts/Network/NetworkContext";
import UserContext from "../../Contexts/User/UserContext";
import Skeleton from "react-loading-skeleton";
import { MessageToastEnum } from "../../DTO/Enum/MessageToastEnum";
import MessageToast from "../../Components/MessageToast";
import PlanPart from "../_WebParts/Plan3ColsPart";
import ProfilePart from "../_WebParts/ProfilePart";
import NetworkFooter from "../NetworkPage/NetworkFooter";
import ProductListPart from "../_WebParts/ProductListPart";
import ProductContext from "../../Contexts/Product/ProductContext";
import ProductSearchParam from "../../DTO/Domain/ProductSearchParam";
import TemplateContext from "../../Contexts/Template/TemplateContext";
import { strToPartEnum, WebsitePartEnum } from "../../Components/TemplateUtils";
import TemplatePartInfo from "../../DTO/Domain/TemplatePartInfo";
import EditMode from "../../Components/EditMode";
import Plan3ColsPart from "../_WebParts/Plan3ColsPart";
import SkeletonPage from "../../Components/SkeletonPage";

export default function SellerPage() {

    let { networkSlug, sellerSlug } = useParams();

    const PAGE_SLUG = (networkSlug) ? "network-seller" : "seller";

    const authContext = useContext(AuthContext);
    const networkContext = useContext(NetworkContext);
    const userContext = useContext(UserContext);
    const productContext = useContext(ProductContext);
    const templateContext = useContext(TemplateContext);

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
            userId: 0,
            networkSlug: networkSlug,
            userSlug: sellerSlug,
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
        WebsitePartEnum.PROFILE01,
        WebsitePartEnum.PLAN_3_COLS, WebsitePartEnum.PRODUCT_LIST_WITH_3_COLS
    ];

    const TemplatePart = (part: TemplatePartInfo) => {
        switch (strToPartEnum(part.partKey)) {
            case WebsitePartEnum.PROFILE01:
                return (
                    <>
                        <EditMode part={part} isEditing={templateContext.editMode} acceptableParts={ACCEPTABLE_PARTS}>
                            <ProfilePart
                                loading={networkContext.loadingSeller}
                                user={networkSlug ? networkContext.seller?.user : userContext.user}
                                userNetwork={networkContext.seller}
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
        }
    };

    useEffect(() => {
        if (networkSlug) {
            templateContext.getNetworkPage(networkSlug, PAGE_SLUG, authContext.language).then((ret) => {
                if (!ret.sucesso) {
                    throwError(ret.mensagemErro);
                    return;
                }
            });
            networkContext.getSellerBySlug(networkSlug, sellerSlug).then((ret) => {
                if (!ret.sucesso) {
                    throwError(ret.mensagemErro);
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
        }
        else {
            userContext.getBySlug(sellerSlug).then((ret) => {
                if (!ret.sucesso) {
                    throwError(ret.mensagemErro);
                }
            });
            searchProducts(1);
        }
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