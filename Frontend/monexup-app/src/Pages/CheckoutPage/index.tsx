import { useContext, useEffect, useState } from "react";
import Col from "react-bootstrap/esm/Col";
import { useTranslation } from "react-i18next";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import Form from 'react-bootstrap/Form';
import InputGroup from 'react-bootstrap/InputGroup';
import AuthContext from "../../Contexts/Auth/AuthContext";
import Button from "react-bootstrap/esm/Button";
import Card from 'react-bootstrap/Card';
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faBitcoinSign, faBoltLightning, faCheck, faClose, faEnvelope, faLock, faMailBulk, faSave, faSign, faSignIn, faSignInAlt, faTrash, faUser } from '@fortawesome/free-solid-svg-icons';
import Table from "react-bootstrap/esm/Table";
import { Link, useNavigate, useParams } from "react-router-dom";
import NetworkContext from "../../Contexts/Network/NetworkContext";
import Skeleton from "react-loading-skeleton";
import { MessageToastEnum } from "../../DTO/Enum/MessageToastEnum";
import { UserNetworkStatusEnum } from "../../DTO/Enum/UserNetworkStatusEnum";
import MessageToast from "../../Components/MessageToast";

export default function CheckoutPage() {

    const { t } = useTranslation();

    let navigate = useNavigate();

    const authContext = useContext(AuthContext);
    const networkContext = useContext(NetworkContext);

    let { networkSlug } = useParams();

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

    useEffect(() => {
        //authContext.loadUserSession();
        networkContext.getBySlug(networkSlug).then((ret) => {
            if (ret.sucesso) {
                if (authContext.sessionInfo) {
                    networkContext.getUserNetwork(ret.network.networkId).then((retUserNetwork) => {
                        if (!retUserNetwork.sucesso) {
                            throwError(retUserNetwork.mensagemErro);
                        }
                    });
                }
            }
            else {
                throwError(ret.mensagemErro);
            }
        });
    }, []);

    const MessageActive = () => {
        return (
            <>
                <Card>
                    <Card.Body className="text-center">
                        <h3 className="text-center">{t("requestAccessPage.youAreApproved")}</h3>
                        {networkContext.loading ?
                            <p><Skeleton /></p>
                            :
                            <p>{t("requestAccessPage.welcomeTo_part1")}<strong>{networkContext.network?.name}</strong>{t("requestAccessPage.welcomeTo_part2")}</p>
                        }
                        <p>
                            {t("requestAccessPage.requestApprovedInfo")}
                        </p>
                        <p>
                            {t("requestAccessPage.youCanNow")}
                        </p>
                        <p>
                            {t("requestAccessPage.everythingReady")}
                        </p>
                        <div className="lc-block d-grid gap-3 d-md-block">
                            <Button variant="success" size="lg" className="me-md-2" onClick={(e) => {
                                e.preventDefault();
                                navigate("/admin/dashboard");
                            }}><FontAwesomeIcon icon={faBoltLightning} fixedWidth />{t("requestAccessPage.goToDashboard")}</Button>
                        </div>
                    </Card.Body>
                </Card>
            </>
        );
    };

    const MessageInactive = () => {
        return (
            <>
                <Card>
                    <Card.Body className="text-center">
                        <h3 className="text-center">{t("requestAccessPage.accessDenied")}</h3>
                        {networkContext.loading ?
                            <p><Skeleton /></p>
                            :
                            <p>
                                {t("requestAccessPage.accessDeniedMessage_part1")}<strong>{networkContext.network?.name}</strong>
                                {t("requestAccessPage.accessDeniedMessage_part2")}<strong>{t("requestAccessPage.notApproved")}</strong>.
                            </p>
                        }
                        <p>
                            {t("requestAccessPage.accessDeniedReason")}
                        </p>
                    </Card.Body>
                </Card>
            </>
        );
    };

    const MessageWaitForApproval = () => {
        return (
            <>
                <Card>
                    <Card.Body>
                        <h3 className="text-center">{t("requestAccessPage.requestSentSuccessfully")}</h3>
                        <p>{t("requestAccessPage.requestSentSuccessInfo_part1")}<strong>{networkContext.network?.name}</strong>{t("requestAccessPage.requestSentSuccessInfo_part2")}</p>
                        <p>
                            {t("requestAccessPage.waitForApprovalInfo")}
                        </p>
                        <p>
                            <strong>{t("requestAccessPage.importantTag")}</strong>{t("requestAccessPage.pendingRequestInfo")}
                        </p>
                    </Card.Body>
                </Card>
            </>
        );
    };

    const MessageBlocked = () => {
        return (
            <>
                <Card>
                    <Card.Body className="text-center">
                        <h3 className="text-center">{t("requestAccessPage.accessBlocked")}</h3>
                        {networkContext.loading ?
                            <p><Skeleton /></p>
                            :
                            <p>
                                {t("requestAccessPage.accessBlockedMessage_part1")}<strong>{networkContext.network?.name}</strong>{t("requestAccessPage.accessBlockedMessage_part2")}<strong>{t("requestAccessPage.blockedWord")}</strong>.
                            </p>
                        }
                        <p>
                            {t("requestAccessPage.accessBlockedReason")}
                        </p>
                        <p>
                            {t("requestAccessPage.accessBlockedConsequences")}
                        </p>
                        <p>
                            {t("requestAccessPage.contactManagerForBlocked")}
                        </p>
                    </Card.Body>
                </Card>
            </>
        );
    };


    const MessageWithUserNetwork = (status: UserNetworkStatusEnum) => {
        switch (status) {
            case UserNetworkStatusEnum.Active:
                return MessageActive();
                break;
            case UserNetworkStatusEnum.Inactive:
                return MessageInactive();
                break;
            case UserNetworkStatusEnum.Blocked:
                return MessageBlocked();
                break;
            case UserNetworkStatusEnum.WaitForApproval:
                return MessageWaitForApproval();
                break;
        }
    };

    return (
        <>
            <MessageToast
                dialog={dialog}
                showMessage={showMessage}
                messageText={messageText}
                onClose={() => setShowMessage(false)}
            ></MessageToast>
            <Container className="py-4">
                <Row>
                    <Col md="6" className='offset-md-3'>
                        {networkContext.userNetwork ?
                            <>
                                {MessageWithUserNetwork(networkContext.userNetwork?.status)}
                            </>
                            :
                            <>
                                <h3 className="text-center">{networkContext.loading ? <Skeleton></Skeleton> : t("requestAccessPage.joinNetworkQuestion", { networkName: networkContext.network?.name })}</h3>
                                <Card>
                                    <Card.Body>
                                        {networkContext.loading ?
                                            <>
                                                <p><Skeleton count={3}></Skeleton></p>
                                                <p><Skeleton count={1}></Skeleton></p>
                                                <p><Skeleton count={1}></Skeleton></p>
                                            </>
                                            :
                                            <>
                                                <p>
                                                    {t("requestAccessPage.byJoiningInfo")}
                                                </p>
                                                <p><strong>{t("requestAccessPage.noteTag")}</strong>{t("requestAccessPage.approvalInfo")}</p>
                                                <p>{t("requestAccessPage.confirmRequestAccess")}</p>
                                            </>
                                        }

                                        <div className="d-grid gap-2 d-md-flex justify-content-md-end">
                                            <Button variant="success" size="lg" onClick={async (e) => {
                                                e.preventDefault();
                                                let ret = await networkContext.requestAccess(networkContext.network?.networkId);
                                                if (ret.sucesso) {
                                                    showSuccessMessage(ret.mensagemSucesso);
                                                }
                                                else {
                                                    throwError(ret.mensagemErro);
                                                }

                                            }} disabled={networkContext.loadingRequestAccess}>{networkContext.loadingRequestAccess ?
                                                t("loading")
                                                :
                                                <>
                                                    <FontAwesomeIcon icon={faCheck} fixedWidth /> {t("requestAccessPage.yesIWantToJoin")}
                                                </>
                                                }</Button>
                                            <Button variant="danger" size="lg" onClick={(e) => {
                                                e.preventDefault();
                                                navigate("/" + networkContext.network?.slug);
                                            }}><FontAwesomeIcon icon={faClose} fixedWidth /> {t("requestAccessPage.noGoBack")} </Button>
                                        </div>
                                    </Card.Body>
                                </Card>
                            </>
                        }
                    </Col>
                </Row>
            </Container>
        </>
    );
}