import { useContext, useEffect, useState } from "react";
import Col from "react-bootstrap/esm/Col";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import { Link, useNavigate, useParams } from "react-router-dom";
import ProductContext from "../../Contexts/Product/ProductContext";
import { MessageToastEnum } from "../../DTO/Enum/MessageToastEnum";
import MessageToast from "../../Components/MessageToast";
import NetworkContext from "../../Contexts/Network/NetworkContext";
import Skeleton from "react-loading-skeleton";
import AuthContext from "../../Contexts/Auth/AuthContext";
import SubscriptionForm from "./SubscriptionForm";
import NetworkFooter from "../NetworkPage/NetworkFooter";
import UserContext from "../../Contexts/User/UserContext";
import UserForm from "./UserForm";
import TemplateContext from "../../Contexts/Template/TemplateContext";
import { strToPartEnum, WebsitePartEnum } from "../../Components/TemplateUtils";
import TemplatePartInfo from "../../DTO/Domain/TemplatePartInfo";
import EditMode from "../../Components/EditMode";
import Product01Part from "../_WebParts/Product01Part";
import SkeletonPage from "../../Components/SkeletonPage";

export default function ProductPage() {

    let navigate = useNavigate();

    let { networkSlug, sellerSlug, productSlug } = useParams();

    const authContext = useContext(AuthContext);
    const networkContext = useContext(NetworkContext);
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

    const getUrl = () => {
        return "/" + networkSlug + ((sellerSlug) ? "/@/" + sellerSlug : "") + "/" + productSlug;
    };

    const ACCEPTABLE_PARTS: WebsitePartEnum[] = [
        WebsitePartEnum.PRODUCT01
    ];

    const TemplatePart = (part: TemplatePartInfo) => {
        switch (strToPartEnum(part.partKey)) {
            case WebsitePartEnum.PRODUCT01:
                return (
                    <>
                        <EditMode part={part} isEditing={templateContext.editMode} acceptableParts={ACCEPTABLE_PARTS}>
                            <Product01Part
                                loading={templateContext.loading}
                                product={productContext.product}
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
        //authContext.loadUserSession();
        if (networkSlug) {
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
            networkContext.getBySlug(networkSlug).then((ret) => {
                if (!ret.sucesso) {
                    throwError(ret.mensagemErro);
                }
            });
            templateContext.getNetworkPage(networkSlug, "network-product", authContext.language).then((ret) => {
                if (!ret.sucesso) {
                    throwError(ret.mensagemErro);
                    return;
                }
            });
        }
        productContext.getBySlug(productSlug).then((retProd) => {
            if (!retProd.sucesso) {
                throwError(retProd.mensagemErro);
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
            <Container>
                <Row>
                    <Col md="12" className="py-4">
                        <Row>
                            <Col md={8}>
                                <EditMode.New isEditing={templateContext.editMode} acceptableParts={ACCEPTABLE_PARTS} />
                                {templateContext.loading && <SkeletonPage />}
                                {templateContext.page?.parts.map((part) => {
                                    return (
                                        <>
                                            {TemplatePart(part)}
                                        </>
                                    )
                                })}
                            </Col>
                            <Col md={4}>
                                {authContext.sessionInfo ?
                                    <SubscriptionForm 
                                        productSlug={productSlug} 
                                        networkSlug={networkSlug}
                                        sellerSlug={sellerSlug} 
                                    />
                                    :
                                    <UserForm url={getUrl()} onSuccess={(msgSuccess) => {
                                        showSuccessMessage(msgSuccess);
                                    }} onThrowError={(msgError) =>
                                        throwError(msgError)
                                    } />
                                }
                            </Col>
                        </Row>

                    </Col>
                </Row>
            </Container>
        </>
    );
}